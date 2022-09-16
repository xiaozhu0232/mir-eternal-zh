using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsRsaKeyExchange : AbstractTlsKeyExchange
{
	protected AsymmetricKeyParameter mServerPublicKey = null;

	protected RsaKeyParameters mRsaServerPublicKey = null;

	protected TlsEncryptionCredentials mServerCredentials = null;

	protected byte[] mPremasterSecret;

	public TlsRsaKeyExchange(IList supportedSignatureAlgorithms)
		: base(1, supportedSignatureAlgorithms)
	{
	}

	public override void SkipServerCredentials()
	{
		throw new TlsFatalAlert(10);
	}

	public override void ProcessServerCredentials(TlsCredentials serverCredentials)
	{
		if (!(serverCredentials is TlsEncryptionCredentials))
		{
			throw new TlsFatalAlert(80);
		}
		ProcessServerCertificate(serverCredentials.Certificate);
		mServerCredentials = (TlsEncryptionCredentials)serverCredentials;
	}

	public override void ProcessServerCertificate(Certificate serverCertificate)
	{
		if (serverCertificate.IsEmpty)
		{
			throw new TlsFatalAlert(50);
		}
		X509CertificateStructure certificateAt = serverCertificate.GetCertificateAt(0);
		SubjectPublicKeyInfo subjectPublicKeyInfo = certificateAt.SubjectPublicKeyInfo;
		try
		{
			mServerPublicKey = PublicKeyFactory.CreateKey(subjectPublicKeyInfo);
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(43, alertCause);
		}
		if (mServerPublicKey.IsPrivate)
		{
			throw new TlsFatalAlert(80);
		}
		mRsaServerPublicKey = ValidateRsaPublicKey((RsaKeyParameters)mServerPublicKey);
		TlsUtilities.ValidateKeyUsage(certificateAt, 32);
		base.ProcessServerCertificate(serverCertificate);
	}

	public override void ValidateCertificateRequest(CertificateRequest certificateRequest)
	{
		byte[] certificateTypes = certificateRequest.CertificateTypes;
		for (int i = 0; i < certificateTypes.Length; i++)
		{
			switch (certificateTypes[i])
			{
			case 1:
			case 2:
			case 64:
				continue;
			}
			throw new TlsFatalAlert(47);
		}
	}

	public override void ProcessClientCredentials(TlsCredentials clientCredentials)
	{
		if (!(clientCredentials is TlsSignerCredentials))
		{
			throw new TlsFatalAlert(80);
		}
	}

	public override void GenerateClientKeyExchange(Stream output)
	{
		mPremasterSecret = TlsRsaUtilities.GenerateEncryptedPreMasterSecret(mContext, mRsaServerPublicKey, output);
	}

	public override void ProcessClientKeyExchange(Stream input)
	{
		byte[] encryptedPreMasterSecret = ((!TlsUtilities.IsSsl(mContext)) ? TlsUtilities.ReadOpaque16(input) : Streams.ReadAll(input));
		mPremasterSecret = mServerCredentials.DecryptPreMasterSecret(encryptedPreMasterSecret);
	}

	public override byte[] GeneratePremasterSecret()
	{
		if (mPremasterSecret == null)
		{
			throw new TlsFatalAlert(80);
		}
		byte[] result = mPremasterSecret;
		mPremasterSecret = null;
		return result;
	}

	protected virtual RsaKeyParameters ValidateRsaPublicKey(RsaKeyParameters key)
	{
		if (!key.Exponent.IsProbablePrime(2))
		{
			throw new TlsFatalAlert(47);
		}
		return key;
	}
}
