using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Operators;

public class CmsKeyTransRecipientInfoGenerator : KeyTransRecipientInfoGenerator
{
	private readonly IKeyWrapper keyWrapper;

	protected override AlgorithmIdentifier AlgorithmDetails => (AlgorithmIdentifier)keyWrapper.AlgorithmDetails;

	public CmsKeyTransRecipientInfoGenerator(X509Certificate recipCert, IKeyWrapper keyWrapper)
		: base(new IssuerAndSerialNumber(recipCert.IssuerDN, new DerInteger(recipCert.SerialNumber)))
	{
		this.keyWrapper = keyWrapper;
		base.RecipientCert = recipCert;
		base.RecipientPublicKey = recipCert.GetPublicKey();
	}

	public CmsKeyTransRecipientInfoGenerator(byte[] subjectKeyID, IKeyWrapper keyWrapper)
		: base(subjectKeyID)
	{
		this.keyWrapper = keyWrapper;
	}

	protected override byte[] GenerateWrappedKey(KeyParameter contentKey)
	{
		return keyWrapper.Wrap(contentKey.GetKey()).Collect();
	}
}
