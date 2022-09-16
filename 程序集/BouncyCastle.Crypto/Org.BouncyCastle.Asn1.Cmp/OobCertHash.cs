using System;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class OobCertHash : Asn1Encodable
{
	private readonly AlgorithmIdentifier hashAlg;

	private readonly CertId certId;

	private readonly DerBitString hashVal;

	public virtual AlgorithmIdentifier HashAlg => hashAlg;

	public virtual CertId CertID => certId;

	private OobCertHash(Asn1Sequence seq)
	{
		int num = seq.Count - 1;
		hashVal = DerBitString.GetInstance(seq[num--]);
		for (int num2 = num; num2 >= 0; num2--)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[num2];
			if (asn1TaggedObject.TagNo == 0)
			{
				hashAlg = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: true);
			}
			else
			{
				certId = CertId.GetInstance(asn1TaggedObject, isExplicit: true);
			}
		}
	}

	public static OobCertHash GetInstance(object obj)
	{
		if (obj is OobCertHash)
		{
			return (OobCertHash)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OobCertHash((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, hashAlg);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, certId);
		asn1EncodableVector.Add(hashVal);
		return new DerSequence(asn1EncodableVector);
	}
}
