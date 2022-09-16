using System;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class OcspIdentifier : Asn1Encodable
{
	private readonly ResponderID ocspResponderID;

	private readonly DerGeneralizedTime producedAt;

	public ResponderID OcspResponderID => ocspResponderID;

	public DateTime ProducedAt => producedAt.ToDateTime();

	public static OcspIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is OcspIdentifier)
		{
			return (OcspIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OcspIdentifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'OcspIdentifier' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private OcspIdentifier(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		ocspResponderID = ResponderID.GetInstance(seq[0].ToAsn1Object());
		producedAt = (DerGeneralizedTime)seq[1].ToAsn1Object();
	}

	public OcspIdentifier(ResponderID ocspResponderID, DateTime producedAt)
	{
		if (ocspResponderID == null)
		{
			throw new ArgumentNullException();
		}
		this.ocspResponderID = ocspResponderID;
		this.producedAt = new DerGeneralizedTime(producedAt);
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(ocspResponderID, producedAt);
	}
}
