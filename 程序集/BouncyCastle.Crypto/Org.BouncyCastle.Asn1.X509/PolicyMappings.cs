using System.Collections;

namespace Org.BouncyCastle.Asn1.X509;

public class PolicyMappings : Asn1Encodable
{
	private readonly Asn1Sequence seq;

	public PolicyMappings(Asn1Sequence seq)
	{
		this.seq = seq;
	}

	public PolicyMappings(Hashtable mappings)
		: this((IDictionary)mappings)
	{
	}

	public PolicyMappings(IDictionary mappings)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (string key in mappings.Keys)
		{
			string identifier = (string)mappings[key];
			asn1EncodableVector.Add(new DerSequence(new DerObjectIdentifier(key), new DerObjectIdentifier(identifier)));
		}
		seq = new DerSequence(asn1EncodableVector);
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}
}
