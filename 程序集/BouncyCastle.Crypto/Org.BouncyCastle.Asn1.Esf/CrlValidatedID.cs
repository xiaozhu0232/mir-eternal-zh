using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class CrlValidatedID : Asn1Encodable
{
	private readonly OtherHash crlHash;

	private readonly CrlIdentifier crlIdentifier;

	public OtherHash CrlHash => crlHash;

	public CrlIdentifier CrlIdentifier => crlIdentifier;

	public static CrlValidatedID GetInstance(object obj)
	{
		if (obj == null || obj is CrlValidatedID)
		{
			return (CrlValidatedID)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CrlValidatedID((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'CrlValidatedID' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CrlValidatedID(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		crlHash = OtherHash.GetInstance(seq[0].ToAsn1Object());
		if (seq.Count > 1)
		{
			crlIdentifier = CrlIdentifier.GetInstance(seq[1].ToAsn1Object());
		}
	}

	public CrlValidatedID(OtherHash crlHash)
		: this(crlHash, null)
	{
	}

	public CrlValidatedID(OtherHash crlHash, CrlIdentifier crlIdentifier)
	{
		if (crlHash == null)
		{
			throw new ArgumentNullException("crlHash");
		}
		this.crlHash = crlHash;
		this.crlIdentifier = crlIdentifier;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(crlHash.ToAsn1Object());
		if (crlIdentifier != null)
		{
			asn1EncodableVector.Add(crlIdentifier.ToAsn1Object());
		}
		return new DerSequence(asn1EncodableVector);
	}
}
