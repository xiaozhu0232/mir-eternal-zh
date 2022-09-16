using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class CertificatePair : Asn1Encodable
{
	private X509CertificateStructure forward;

	private X509CertificateStructure reverse;

	public X509CertificateStructure Forward => forward;

	public X509CertificateStructure Reverse => reverse;

	public static CertificatePair GetInstance(object obj)
	{
		if (obj == null || obj is CertificatePair)
		{
			return (CertificatePair)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertificatePair((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CertificatePair(Asn1Sequence seq)
	{
		if (seq.Count != 1 && seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		foreach (object item in seq)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(item);
			if (instance.TagNo == 0)
			{
				forward = X509CertificateStructure.GetInstance(instance, explicitly: true);
				continue;
			}
			if (instance.TagNo == 1)
			{
				reverse = X509CertificateStructure.GetInstance(instance, explicitly: true);
				continue;
			}
			throw new ArgumentException("Bad tag number: " + instance.TagNo);
		}
	}

	public CertificatePair(X509CertificateStructure forward, X509CertificateStructure reverse)
	{
		this.forward = forward;
		this.reverse = reverse;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, forward);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, reverse);
		return new DerSequence(asn1EncodableVector);
	}
}
