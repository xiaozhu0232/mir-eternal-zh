using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class SignerAttribute : Asn1Encodable
{
	private Asn1Sequence claimedAttributes;

	private AttributeCertificate certifiedAttributes;

	public virtual Asn1Sequence ClaimedAttributes => claimedAttributes;

	public virtual AttributeCertificate CertifiedAttributes => certifiedAttributes;

	public static SignerAttribute GetInstance(object obj)
	{
		if (obj == null || obj is SignerAttribute)
		{
			return (SignerAttribute)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SignerAttribute(obj);
		}
		throw new ArgumentException("Unknown object in 'SignerAttribute' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private SignerAttribute(object obj)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)obj;
		DerTaggedObject derTaggedObject = (DerTaggedObject)asn1Sequence[0];
		if (derTaggedObject.TagNo == 0)
		{
			claimedAttributes = Asn1Sequence.GetInstance(derTaggedObject, explicitly: true);
			return;
		}
		if (derTaggedObject.TagNo == 1)
		{
			certifiedAttributes = AttributeCertificate.GetInstance(derTaggedObject);
			return;
		}
		throw new ArgumentException("illegal tag.", "obj");
	}

	public SignerAttribute(Asn1Sequence claimedAttributes)
	{
		this.claimedAttributes = claimedAttributes;
	}

	public SignerAttribute(AttributeCertificate certifiedAttributes)
	{
		this.certifiedAttributes = certifiedAttributes;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (claimedAttributes != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(0, claimedAttributes));
		}
		else
		{
			asn1EncodableVector.Add(new DerTaggedObject(1, certifiedAttributes));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
