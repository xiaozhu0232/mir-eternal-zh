using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509;

public class Admissions : Asn1Encodable
{
	private readonly GeneralName admissionAuthority;

	private readonly NamingAuthority namingAuthority;

	private readonly Asn1Sequence professionInfos;

	public virtual GeneralName AdmissionAuthority => admissionAuthority;

	public virtual NamingAuthority NamingAuthority => namingAuthority;

	public static Admissions GetInstance(object obj)
	{
		if (obj == null || obj is Admissions)
		{
			return (Admissions)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new Admissions((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private Admissions(Asn1Sequence seq)
	{
		if (seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		Asn1Encodable asn1Encodable = (Asn1Encodable)enumerator.Current;
		if (asn1Encodable is Asn1TaggedObject)
		{
			switch (((Asn1TaggedObject)asn1Encodable).TagNo)
			{
			case 0:
				admissionAuthority = GeneralName.GetInstance((Asn1TaggedObject)asn1Encodable, explicitly: true);
				break;
			case 1:
				namingAuthority = NamingAuthority.GetInstance((Asn1TaggedObject)asn1Encodable, isExplicit: true);
				break;
			default:
				throw new ArgumentException("Bad tag number: " + ((Asn1TaggedObject)asn1Encodable).TagNo);
			}
			enumerator.MoveNext();
			asn1Encodable = (Asn1Encodable)enumerator.Current;
		}
		if (asn1Encodable is Asn1TaggedObject)
		{
			int tagNo = ((Asn1TaggedObject)asn1Encodable).TagNo;
			if (tagNo != 1)
			{
				throw new ArgumentException("Bad tag number: " + ((Asn1TaggedObject)asn1Encodable).TagNo);
			}
			namingAuthority = NamingAuthority.GetInstance((Asn1TaggedObject)asn1Encodable, isExplicit: true);
			enumerator.MoveNext();
			asn1Encodable = (Asn1Encodable)enumerator.Current;
		}
		professionInfos = Asn1Sequence.GetInstance(asn1Encodable);
		if (enumerator.MoveNext())
		{
			throw new ArgumentException("Bad object encountered: " + Platform.GetTypeName(enumerator.Current));
		}
	}

	public Admissions(GeneralName admissionAuthority, NamingAuthority namingAuthority, ProfessionInfo[] professionInfos)
	{
		this.admissionAuthority = admissionAuthority;
		this.namingAuthority = namingAuthority;
		this.professionInfos = new DerSequence(professionInfos);
	}

	public ProfessionInfo[] GetProfessionInfos()
	{
		ProfessionInfo[] array = new ProfessionInfo[professionInfos.Count];
		int num = 0;
		foreach (Asn1Encodable professionInfo in professionInfos)
		{
			array[num++] = ProfessionInfo.GetInstance(professionInfo);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, admissionAuthority);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, namingAuthority);
		asn1EncodableVector.Add(professionInfos);
		return new DerSequence(asn1EncodableVector);
	}
}
