using System;

namespace Org.BouncyCastle.Asn1.X509;

public class V2Form : Asn1Encodable
{
	internal GeneralNames issuerName;

	internal IssuerSerial baseCertificateID;

	internal ObjectDigestInfo objectDigestInfo;

	public GeneralNames IssuerName => issuerName;

	public IssuerSerial BaseCertificateID => baseCertificateID;

	public ObjectDigestInfo ObjectDigestInfo => objectDigestInfo;

	public static V2Form GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static V2Form GetInstance(object obj)
	{
		if (obj is V2Form)
		{
			return (V2Form)obj;
		}
		if (obj != null)
		{
			return new V2Form(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public V2Form(GeneralNames issuerName)
		: this(issuerName, null, null)
	{
	}

	public V2Form(GeneralNames issuerName, IssuerSerial baseCertificateID)
		: this(issuerName, baseCertificateID, null)
	{
	}

	public V2Form(GeneralNames issuerName, ObjectDigestInfo objectDigestInfo)
		: this(issuerName, null, objectDigestInfo)
	{
	}

	public V2Form(GeneralNames issuerName, IssuerSerial baseCertificateID, ObjectDigestInfo objectDigestInfo)
	{
		this.issuerName = issuerName;
		this.baseCertificateID = baseCertificateID;
		this.objectDigestInfo = objectDigestInfo;
	}

	private V2Form(Asn1Sequence seq)
	{
		if (seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		int num = 0;
		if (!(seq[0] is Asn1TaggedObject))
		{
			num++;
			issuerName = GeneralNames.GetInstance(seq[0]);
		}
		for (int i = num; i != seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			if (instance.TagNo == 0)
			{
				baseCertificateID = IssuerSerial.GetInstance(instance, explicitly: false);
				continue;
			}
			if (instance.TagNo == 1)
			{
				objectDigestInfo = ObjectDigestInfo.GetInstance(instance, isExplicit: false);
				continue;
			}
			throw new ArgumentException("Bad tag number: " + instance.TagNo);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(issuerName);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, baseCertificateID);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, objectDigestInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
