using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509;

public class AdmissionSyntax : Asn1Encodable
{
	private readonly GeneralName admissionAuthority;

	private readonly Asn1Sequence contentsOfAdmissions;

	public virtual GeneralName AdmissionAuthority => admissionAuthority;

	public static AdmissionSyntax GetInstance(object obj)
	{
		if (obj == null || obj is AdmissionSyntax)
		{
			return (AdmissionSyntax)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new AdmissionSyntax((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private AdmissionSyntax(Asn1Sequence seq)
	{
		switch (seq.Count)
		{
		case 1:
			contentsOfAdmissions = Asn1Sequence.GetInstance(seq[0]);
			break;
		case 2:
			admissionAuthority = GeneralName.GetInstance(seq[0]);
			contentsOfAdmissions = Asn1Sequence.GetInstance(seq[1]);
			break;
		default:
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
	}

	public AdmissionSyntax(GeneralName admissionAuthority, Asn1Sequence contentsOfAdmissions)
	{
		this.admissionAuthority = admissionAuthority;
		this.contentsOfAdmissions = contentsOfAdmissions;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(admissionAuthority);
		asn1EncodableVector.Add(contentsOfAdmissions);
		return new DerSequence(asn1EncodableVector);
	}

	public virtual Admissions[] GetContentsOfAdmissions()
	{
		Admissions[] array = new Admissions[contentsOfAdmissions.Count];
		for (int i = 0; i < contentsOfAdmissions.Count; i++)
		{
			array[i] = Admissions.GetInstance(contentsOfAdmissions[i]);
		}
		return array;
	}
}
