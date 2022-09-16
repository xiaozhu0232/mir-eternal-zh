using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsSrpKeyExchange : AbstractTlsKeyExchange
{
	protected TlsSigner mTlsSigner;

	protected TlsSrpGroupVerifier mGroupVerifier;

	protected byte[] mIdentity;

	protected byte[] mPassword;

	protected AsymmetricKeyParameter mServerPublicKey = null;

	protected Srp6GroupParameters mSrpGroup = null;

	protected Srp6Client mSrpClient = null;

	protected Srp6Server mSrpServer = null;

	protected BigInteger mSrpPeerCredentials = null;

	protected BigInteger mSrpVerifier = null;

	protected byte[] mSrpSalt = null;

	protected TlsSignerCredentials mServerCredentials = null;

	public override bool RequiresServerKeyExchange => true;

	protected static TlsSigner CreateSigner(int keyExchange)
	{
		return keyExchange switch
		{
			21 => null, 
			23 => new TlsRsaSigner(), 
			22 => new TlsDssSigner(), 
			_ => throw new ArgumentException("unsupported key exchange algorithm"), 
		};
	}

	[Obsolete("Use constructor taking an explicit 'groupVerifier' argument")]
	public TlsSrpKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, byte[] identity, byte[] password)
		: this(keyExchange, supportedSignatureAlgorithms, new DefaultTlsSrpGroupVerifier(), identity, password)
	{
	}

	public TlsSrpKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, TlsSrpGroupVerifier groupVerifier, byte[] identity, byte[] password)
		: base(keyExchange, supportedSignatureAlgorithms)
	{
		mTlsSigner = CreateSigner(keyExchange);
		mGroupVerifier = groupVerifier;
		mIdentity = identity;
		mPassword = password;
		mSrpClient = new Srp6Client();
	}

	public TlsSrpKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, byte[] identity, TlsSrpLoginParameters loginParameters)
		: base(keyExchange, supportedSignatureAlgorithms)
	{
		mTlsSigner = CreateSigner(keyExchange);
		mIdentity = identity;
		mSrpServer = new Srp6Server();
		mSrpGroup = loginParameters.Group;
		mSrpVerifier = loginParameters.Verifier;
		mSrpSalt = loginParameters.Salt;
	}

	public override void Init(TlsContext context)
	{
		base.Init(context);
		if (mTlsSigner != null)
		{
			mTlsSigner.Init(context);
		}
	}

	public override void SkipServerCredentials()
	{
		if (mTlsSigner != null)
		{
			throw new TlsFatalAlert(10);
		}
	}

	public override void ProcessServerCertificate(Certificate serverCertificate)
	{
		if (mTlsSigner == null)
		{
			throw new TlsFatalAlert(10);
		}
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
		if (!mTlsSigner.IsValidPublicKey(mServerPublicKey))
		{
			throw new TlsFatalAlert(46);
		}
		TlsUtilities.ValidateKeyUsage(certificateAt, 128);
		base.ProcessServerCertificate(serverCertificate);
	}

	public override void ProcessServerCredentials(TlsCredentials serverCredentials)
	{
		if (mKeyExchange == 21 || !(serverCredentials is TlsSignerCredentials))
		{
			throw new TlsFatalAlert(80);
		}
		ProcessServerCertificate(serverCredentials.Certificate);
		mServerCredentials = (TlsSignerCredentials)serverCredentials;
	}

	public override byte[] GenerateServerKeyExchange()
	{
		mSrpServer.Init(mSrpGroup, mSrpVerifier, TlsUtilities.CreateHash(2), mContext.SecureRandom);
		BigInteger b = mSrpServer.GenerateServerCredentials();
		ServerSrpParams serverSrpParams = new ServerSrpParams(mSrpGroup.N, mSrpGroup.G, mSrpSalt, b);
		DigestInputBuffer digestInputBuffer = new DigestInputBuffer();
		serverSrpParams.Encode(digestInputBuffer);
		if (mServerCredentials != null)
		{
			SignatureAndHashAlgorithm signatureAndHashAlgorithm = TlsUtilities.GetSignatureAndHashAlgorithm(mContext, mServerCredentials);
			IDigest digest = TlsUtilities.CreateHash(signatureAndHashAlgorithm);
			SecurityParameters securityParameters = mContext.SecurityParameters;
			digest.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
			digest.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
			digestInputBuffer.UpdateDigest(digest);
			byte[] array = new byte[digest.GetDigestSize()];
			digest.DoFinal(array, 0);
			byte[] signature = mServerCredentials.GenerateCertificateSignature(array);
			DigitallySigned digitallySigned = new DigitallySigned(signatureAndHashAlgorithm, signature);
			digitallySigned.Encode(digestInputBuffer);
		}
		return digestInputBuffer.ToArray();
	}

	public override void ProcessServerKeyExchange(Stream input)
	{
		SecurityParameters securityParameters = mContext.SecurityParameters;
		SignerInputBuffer signerInputBuffer = null;
		Stream input2 = input;
		if (mTlsSigner != null)
		{
			signerInputBuffer = new SignerInputBuffer();
			input2 = new TeeInputStream(input, signerInputBuffer);
		}
		ServerSrpParams serverSrpParams = ServerSrpParams.Parse(input2);
		if (signerInputBuffer != null)
		{
			DigitallySigned digitallySigned = ParseSignature(input);
			ISigner signer = InitVerifyer(mTlsSigner, digitallySigned.Algorithm, securityParameters);
			signerInputBuffer.UpdateSigner(signer);
			if (!signer.VerifySignature(digitallySigned.Signature))
			{
				throw new TlsFatalAlert(51);
			}
		}
		mSrpGroup = new Srp6GroupParameters(serverSrpParams.N, serverSrpParams.G);
		if (!mGroupVerifier.Accept(mSrpGroup))
		{
			throw new TlsFatalAlert(71);
		}
		mSrpSalt = serverSrpParams.S;
		try
		{
			mSrpPeerCredentials = Srp6Utilities.ValidatePublicValue(mSrpGroup.N, serverSrpParams.B);
		}
		catch (CryptoException alertCause)
		{
			throw new TlsFatalAlert(47, alertCause);
		}
		mSrpClient.Init(mSrpGroup, TlsUtilities.CreateHash(2), mContext.SecureRandom);
	}

	public override void ValidateCertificateRequest(CertificateRequest certificateRequest)
	{
		throw new TlsFatalAlert(10);
	}

	public override void ProcessClientCredentials(TlsCredentials clientCredentials)
	{
		throw new TlsFatalAlert(80);
	}

	public override void GenerateClientKeyExchange(Stream output)
	{
		BigInteger x = mSrpClient.GenerateClientCredentials(mSrpSalt, mIdentity, mPassword);
		TlsSrpUtilities.WriteSrpParameter(x, output);
		mContext.SecurityParameters.srpIdentity = Arrays.Clone(mIdentity);
	}

	public override void ProcessClientKeyExchange(Stream input)
	{
		try
		{
			mSrpPeerCredentials = Srp6Utilities.ValidatePublicValue(mSrpGroup.N, TlsSrpUtilities.ReadSrpParameter(input));
		}
		catch (CryptoException alertCause)
		{
			throw new TlsFatalAlert(47, alertCause);
		}
		mContext.SecurityParameters.srpIdentity = Arrays.Clone(mIdentity);
	}

	public override byte[] GeneratePremasterSecret()
	{
		try
		{
			BigInteger n = ((mSrpServer != null) ? mSrpServer.CalculateSecret(mSrpPeerCredentials) : mSrpClient.CalculateSecret(mSrpPeerCredentials));
			return BigIntegers.AsUnsignedByteArray(n);
		}
		catch (CryptoException alertCause)
		{
			throw new TlsFatalAlert(47, alertCause);
		}
	}

	protected virtual ISigner InitVerifyer(TlsSigner tlsSigner, SignatureAndHashAlgorithm algorithm, SecurityParameters securityParameters)
	{
		ISigner signer = tlsSigner.CreateVerifyer(algorithm, mServerPublicKey);
		signer.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
		signer.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
		return signer;
	}
}
