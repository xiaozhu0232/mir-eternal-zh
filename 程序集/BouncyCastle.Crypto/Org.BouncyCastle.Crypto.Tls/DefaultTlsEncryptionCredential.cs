using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class DefaultTlsEncryptionCredentials : AbstractTlsEncryptionCredentials
{
	protected readonly TlsContext mContext;

	protected readonly Certificate mCertificate;

	protected readonly AsymmetricKeyParameter mPrivateKey;

	public override Certificate Certificate => mCertificate;

	public DefaultTlsEncryptionCredentials(TlsContext context, Certificate certificate, AsymmetricKeyParameter privateKey)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (certificate.IsEmpty)
		{
			throw new ArgumentException("cannot be empty", "certificate");
		}
		if (privateKey == null)
		{
			throw new ArgumentNullException("'privateKey' cannot be null");
		}
		if (!privateKey.IsPrivate)
		{
			throw new ArgumentException("must be private", "privateKey");
		}
		if (!(privateKey is RsaKeyParameters))
		{
			throw new ArgumentException("type not supported: " + Platform.GetTypeName(privateKey), "privateKey");
		}
		mContext = context;
		mCertificate = certificate;
		mPrivateKey = privateKey;
	}

	public override byte[] DecryptPreMasterSecret(byte[] encryptedPreMasterSecret)
	{
		return TlsRsaUtilities.SafeDecryptPreMasterSecret(mContext, (RsaKeyParameters)mPrivateKey, encryptedPreMasterSecret);
	}
}
