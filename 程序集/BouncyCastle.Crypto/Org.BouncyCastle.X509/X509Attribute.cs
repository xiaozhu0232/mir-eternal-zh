using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.X509;

public class X509Attribute : Asn1Encodable
{
	private readonly AttributeX509 attr;

	public string Oid => attr.AttrType.Id;

	internal X509Attribute(Asn1Encodable at)
	{
		attr = AttributeX509.GetInstance(at);
	}

	public X509Attribute(string oid, Asn1Encodable value)
	{
		attr = new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value));
	}

	public X509Attribute(string oid, Asn1EncodableVector value)
	{
		attr = new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value));
	}

	public Asn1Encodable[] GetValues()
	{
		Asn1Set attrValues = attr.AttrValues;
		Asn1Encodable[] array = new Asn1Encodable[attrValues.Count];
		for (int i = 0; i != attrValues.Count; i++)
		{
			array[i] = attrValues[i];
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return attr.ToAsn1Object();
	}
}
