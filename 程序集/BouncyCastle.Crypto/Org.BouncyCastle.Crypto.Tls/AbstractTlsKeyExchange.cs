using System;
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class AbstractTlsKeyExchange : TlsKeyExchange
{
	protected readonly int mKeyExchange;

	protected IList mSupportedSignatureAlgorithms;

	protected TlsContext mContext;

	public virtual bool RequiresServerKeyExchange => false;

	protected AbstractTlsKeyExchange(int keyExchange, IList supportedSignatureAlgorithms)
	{
		mKeyExchange = keyExchange;
		mSupportedSignatureAlgorithms = supportedSignatureAlgorithms;
	}

	protected virtual DigitallySigned ParseSignature(Stream input)
	{
		DigitallySigned digitallySigned = DigitallySigned.Parse(mContext, input);
		SignatureAndHashAlgorithm algorithm = digitallySigned.Algorithm;
		if (algorithm != null)
		{
			TlsUtilities.VerifySupportedSignatureAlgorithm(mSupportedSignatureAlgorithms, algorithm);
		}
		return digitallySigned;
	}

	public virtual void Init(TlsContext context)
	{
		mContext = context;
		ProtocolVersion clientVersion = context.ClientVersion;
		if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(clientVersion))
		{
			if (mSupportedSignatureAlgorithms == null)
			{
				switch (mKeyExchange)
				{
				case 3:
				case 7:
				case 22:
					mSupportedSignatureAlgorithms = TlsUtilities.GetDefaultDssSignatureAlgorithms();
					break;
				case 16:
				case 17:
					mSupportedSignatureAlgorithms = TlsUtilities.GetDefaultECDsaSignatureAlgorithms();
					break;
				case 1:
				case 5:
				case 9:
				case 15:
				case 18:
				case 19:
				case 23:
					mSupportedSignatureAlgorithms = TlsUtilities.GetDefaultRsaSignatureAlgorithms();
					break;
				default:
					throw new InvalidOperationException("unsupported key exchange algorithm");
				case 13:
				case 14:
				case 21:
				case 24:
					break;
				}
			}
		}
		else if (mSupportedSignatureAlgorithms != null)
		{
			throw new InvalidOperationException("supported_signature_algorithms not allowed for " + clientVersion);
		}
	}

	public abstract void SkipServerCredentials();

	public virtual void ProcessServerCertificate(Certificate serverCertificate)
	{
		if (mSupportedSignatureAlgorithms != null)
		{
		}
	}

	public virtual void ProcessServerCredentials(TlsCredentials serverCredentials)
	{
		ProcessServerCertificate(serverCredentials.Certificate);
	}

	public virtual byte[] GenerateServerKeyExchange()
	{
		if (RequiresServerKeyExchange)
		{
			throw new TlsFatalAlert(80);
		}
		return null;
	}

	public virtual void SkipServerKeyExchange()
	{
		if (RequiresServerKeyExchange)
		{
			throw new TlsFatalAlert(10);
		}
	}

	public virtual void ProcessServerKeyExchange(Stream input)
	{
		if (!RequiresServerKeyExchange)
		{
			throw new TlsFatalAlert(10);
		}
	}

	public abstract void ValidateCertificateRequest(CertificateRequest certificateRequest);

	public virtual void SkipClientCredentials()
	{
	}

	public abstract void ProcessClientCredentials(TlsCredentials clientCredentials);

	public virtual void ProcessClientCertificate(Certificate clientCertificate)
	{
	}

	public abstract void GenerateClientKeyExchange(Stream output);

	public virtual void ProcessClientKeyExchange(Stream input)
	{
		throw new TlsFatalAlert(80);
	}

	public abstract byte[] GeneratePremasterSecret();
}
