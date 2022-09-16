using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf;

public class EncKeyWithID : Asn1Encodable
{
	private readonly PrivateKeyInfo privKeyInfo;

	private readonly Asn1Encodable identifier;

	public virtual PrivateKeyInfo PrivateKey => privKeyInfo;

	public virtual bool HasIdentifier => identifier != null;

	public virtual bool IsIdentifierUtf8String => identifier is DerUtf8String;

	public virtual Asn1Encodable Identifier => identifier;

	public static EncKeyWithID GetInstance(object obj)
	{
		if (obj is EncKeyWithID)
		{
			return (EncKeyWithID)obj;
		}
		if (obj != null)
		{
			return new EncKeyWithID(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	private EncKeyWithID(Asn1Sequence seq)
	{
		privKeyInfo = PrivateKeyInfo.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			if (!(seq[1] is DerUtf8String))
			{
				identifier = GeneralName.GetInstance(seq[1]);
			}
			else
			{
				identifier = seq[1];
			}
		}
		else
		{
			identifier = null;
		}
	}

	public EncKeyWithID(PrivateKeyInfo privKeyInfo)
	{
		this.privKeyInfo = privKeyInfo;
		identifier = null;
	}

	public EncKeyWithID(PrivateKeyInfo privKeyInfo, DerUtf8String str)
	{
		this.privKeyInfo = privKeyInfo;
		identifier = str;
	}

	public EncKeyWithID(PrivateKeyInfo privKeyInfo, GeneralName generalName)
	{
		this.privKeyInfo = privKeyInfo;
		identifier = generalName;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(privKeyInfo);
		asn1EncodableVector.AddOptional(identifier);
		return new DerSequence(asn1EncodableVector);
	}
}
