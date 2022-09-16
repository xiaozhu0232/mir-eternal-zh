using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsPskKeyExchange : AbstractTlsKeyExchange
{
	protected TlsPskIdentity mPskIdentity;

	protected TlsPskIdentityManager mPskIdentityManager;

	protected TlsDHVerifier mDHVerifier;

	protected DHParameters mDHParameters;

	protected int[] mNamedCurves;

	protected byte[] mClientECPointFormats;

	protected byte[] mServerECPointFormats;

	protected byte[] mPskIdentityHint = null;

	protected byte[] mPsk = null;

	protected DHPrivateKeyParameters mDHAgreePrivateKey = null;

	protected DHPublicKeyParameters mDHAgreePublicKey = null;

	protected ECPrivateKeyParameters mECAgreePrivateKey = null;

	protected ECPublicKeyParameters mECAgreePublicKey = null;

	protected AsymmetricKeyParameter mServerPublicKey = null;

	protected RsaKeyParameters mRsaServerPublicKey = null;

	protected TlsEncryptionCredentials mServerCredentials = null;

	protected byte[] mPremasterSecret;

	public override bool RequiresServerKeyExchange
	{
		get
		{
			int num = mKeyExchange;
			if (num == 14 || num == 24)
			{
				return true;
			}
			return false;
		}
	}

	[Obsolete("Use constructor that takes a TlsDHVerifier")]
	public TlsPskKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, TlsPskIdentity pskIdentity, TlsPskIdentityManager pskIdentityManager, DHParameters dhParameters, int[] namedCurves, byte[] clientECPointFormats, byte[] serverECPointFormats)
		: this(keyExchange, supportedSignatureAlgorithms, pskIdentity, pskIdentityManager, new DefaultTlsDHVerifier(), dhParameters, namedCurves, clientECPointFormats, serverECPointFormats)
	{
	}

	public TlsPskKeyExchange(int keyExchange, IList supportedSignatureAlgorithms, TlsPskIdentity pskIdentity, TlsPskIdentityManager pskIdentityManager, TlsDHVerifier dhVerifier, DHParameters dhParameters, int[] namedCurves, byte[] clientECPointFormats, byte[] serverECPointFormats)
		: base(keyExchange, supportedSignatureAlgorithms)
	{
		switch (keyExchange)
		{
		default:
			throw new InvalidOperationException("unsupported key exchange algorithm");
		case 13:
		case 14:
		case 15:
		case 24:
			mPskIdentity = pskIdentity;
			mPskIdentityManager = pskIdentityManager;
			mDHVerifier = dhVerifier;
			mDHParameters = dhParameters;
			mNamedCurves = namedCurves;
			mClientECPointFormats = clientECPointFormats;
			mServerECPointFormats = serverECPointFormats;
			break;
		}
	}

	public override void SkipServerCredentials()
	{
		if (mKeyExchange == 15)
		{
			throw new TlsFatalAlert(10);
		}
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

	public override byte[] GenerateServerKeyExchange()
	{
		mPskIdentityHint = mPskIdentityManager.GetHint();
		if (mPskIdentityHint == null && !RequiresServerKeyExchange)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		if (mPskIdentityHint == null)
		{
			TlsUtilities.WriteOpaque16(TlsUtilities.EmptyBytes, memoryStream);
		}
		else
		{
			TlsUtilities.WriteOpaque16(mPskIdentityHint, memoryStream);
		}
		if (mKeyExchange == 14)
		{
			if (mDHParameters == null)
			{
				throw new TlsFatalAlert(80);
			}
			mDHAgreePrivateKey = TlsDHUtilities.GenerateEphemeralServerKeyExchange(mContext.SecureRandom, mDHParameters, memoryStream);
		}
		else if (mKeyExchange == 24)
		{
			mECAgreePrivateKey = TlsEccUtilities.GenerateEphemeralServerKeyExchange(mContext.SecureRandom, mNamedCurves, mClientECPointFormats, memoryStream);
		}
		return memoryStream.ToArray();
	}

	public override void ProcessServerCertificate(Certificate serverCertificate)
	{
		if (mKeyExchange != 15)
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
		if (mServerPublicKey.IsPrivate)
		{
			throw new TlsFatalAlert(80);
		}
		mRsaServerPublicKey = ValidateRsaPublicKey((RsaKeyParameters)mServerPublicKey);
		TlsUtilities.ValidateKeyUsage(certificateAt, 32);
		base.ProcessServerCertificate(serverCertificate);
	}

	public override void ProcessServerKeyExchange(Stream input)
	{
		mPskIdentityHint = TlsUtilities.ReadOpaque16(input);
		if (mKeyExchange == 14)
		{
			mDHParameters = TlsDHUtilities.ReceiveDHParameters(mDHVerifier, input);
			mDHAgreePublicKey = new DHPublicKeyParameters(TlsDHUtilities.ReadDHParameter(input), mDHParameters);
		}
		else if (mKeyExchange == 24)
		{
			ECDomainParameters curve_params = TlsEccUtilities.ReadECParameters(mNamedCurves, mClientECPointFormats, input);
			byte[] encoding = TlsUtilities.ReadOpaque8(input);
			mECAgreePublicKey = TlsEccUtilities.ValidateECPublicKey(TlsEccUtilities.DeserializeECPublicKey(mClientECPointFormats, curve_params, encoding));
		}
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
		if (mPskIdentityHint == null)
		{
			mPskIdentity.SkipIdentityHint();
		}
		else
		{
			mPskIdentity.NotifyIdentityHint(mPskIdentityHint);
		}
		byte[] pskIdentity = mPskIdentity.GetPskIdentity();
		if (pskIdentity == null)
		{
			throw new TlsFatalAlert(80);
		}
		mPsk = mPskIdentity.GetPsk();
		if (mPsk == null)
		{
			throw new TlsFatalAlert(80);
		}
		TlsUtilities.WriteOpaque16(pskIdentity, output);
		mContext.SecurityParameters.pskIdentity = pskIdentity;
		if (mKeyExchange == 14)
		{
			mDHAgreePrivateKey = TlsDHUtilities.GenerateEphemeralClientKeyExchange(mContext.SecureRandom, mDHParameters, output);
		}
		else if (mKeyExchange == 24)
		{
			mECAgreePrivateKey = TlsEccUtilities.GenerateEphemeralClientKeyExchange(mContext.SecureRandom, mServerECPointFormats, mECAgreePublicKey.Parameters, output);
		}
		else if (mKeyExchange == 15)
		{
			mPremasterSecret = TlsRsaUtilities.GenerateEncryptedPreMasterSecret(mContext, mRsaServerPublicKey, output);
		}
	}

	public override void ProcessClientKeyExchange(Stream input)
	{
		byte[] array = TlsUtilities.ReadOpaque16(input);
		mPsk = mPskIdentityManager.GetPsk(array);
		if (mPsk == null)
		{
			throw new TlsFatalAlert(115);
		}
		mContext.SecurityParameters.pskIdentity = array;
		if (mKeyExchange == 14)
		{
			mDHAgreePublicKey = new DHPublicKeyParameters(TlsDHUtilities.ReadDHParameter(input), mDHParameters);
		}
		else if (mKeyExchange == 24)
		{
			byte[] encoding = TlsUtilities.ReadOpaque8(input);
			ECDomainParameters parameters = mECAgreePrivateKey.Parameters;
			mECAgreePublicKey = TlsEccUtilities.ValidateECPublicKey(TlsEccUtilities.DeserializeECPublicKey(mServerECPointFormats, parameters, encoding));
		}
		else if (mKeyExchange == 15)
		{
			byte[] encryptedPreMasterSecret = ((!TlsUtilities.IsSsl(mContext)) ? TlsUtilities.ReadOpaque16(input) : Streams.ReadAll(input));
			mPremasterSecret = mServerCredentials.DecryptPreMasterSecret(encryptedPreMasterSecret);
		}
	}

	public override byte[] GeneratePremasterSecret()
	{
		byte[] array = GenerateOtherSecret(mPsk.Length);
		MemoryStream memoryStream = new MemoryStream(4 + array.Length + mPsk.Length);
		TlsUtilities.WriteOpaque16(array, memoryStream);
		TlsUtilities.WriteOpaque16(mPsk, memoryStream);
		Arrays.Fill(mPsk, 0);
		mPsk = null;
		return memoryStream.ToArray();
	}

	protected virtual byte[] GenerateOtherSecret(int pskLength)
	{
		if (mKeyExchange == 14)
		{
			if (mDHAgreePrivateKey != null)
			{
				return TlsDHUtilities.CalculateDHBasicAgreement(mDHAgreePublicKey, mDHAgreePrivateKey);
			}
			throw new TlsFatalAlert(80);
		}
		if (mKeyExchange == 24)
		{
			if (mECAgreePrivateKey != null)
			{
				return TlsEccUtilities.CalculateECDHBasicAgreement(mECAgreePublicKey, mECAgreePrivateKey);
			}
			throw new TlsFatalAlert(80);
		}
		if (mKeyExchange == 15)
		{
			return mPremasterSecret;
		}
		return new byte[pskLength];
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
