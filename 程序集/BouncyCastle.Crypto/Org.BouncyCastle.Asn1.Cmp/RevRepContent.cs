using System;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class RevRepContent : Asn1Encodable
{
	private readonly Asn1Sequence status;

	private readonly Asn1Sequence revCerts;

	private readonly Asn1Sequence crls;

	private RevRepContent(Asn1Sequence seq)
	{
		status = Asn1Sequence.GetInstance(seq[0]);
		for (int i = 1; i < seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			if (instance.TagNo == 0)
			{
				revCerts = Asn1Sequence.GetInstance(instance, explicitly: true);
			}
			else
			{
				crls = Asn1Sequence.GetInstance(instance, explicitly: true);
			}
		}
	}

	public static RevRepContent GetInstance(object obj)
	{
		if (obj is RevRepContent)
		{
			return (RevRepContent)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RevRepContent((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual PkiStatusInfo[] GetStatus()
	{
		PkiStatusInfo[] array = new PkiStatusInfo[status.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = PkiStatusInfo.GetInstance(status[i]);
		}
		return array;
	}

	public virtual CertId[] GetRevCerts()
	{
		if (revCerts == null)
		{
			return null;
		}
		CertId[] array = new CertId[revCerts.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertId.GetInstance(revCerts[i]);
		}
		return array;
	}

	public virtual CertificateList[] GetCrls()
	{
		if (crls == null)
		{
			return null;
		}
		CertificateList[] array = new CertificateList[crls.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertificateList.GetInstance(crls[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(status);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, revCerts);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, crls);
		return new DerSequence(asn1EncodableVector);
	}
}
