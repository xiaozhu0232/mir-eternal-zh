using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class SingleResponse : Asn1Encodable
{
	private readonly CertID certID;

	private readonly CertStatus certStatus;

	private readonly DerGeneralizedTime thisUpdate;

	private readonly DerGeneralizedTime nextUpdate;

	private readonly X509Extensions singleExtensions;

	public CertID CertId => certID;

	public CertStatus CertStatus => certStatus;

	public DerGeneralizedTime ThisUpdate => thisUpdate;

	public DerGeneralizedTime NextUpdate => nextUpdate;

	public X509Extensions SingleExtensions => singleExtensions;

	public SingleResponse(CertID certID, CertStatus certStatus, DerGeneralizedTime thisUpdate, DerGeneralizedTime nextUpdate, X509Extensions singleExtensions)
	{
		this.certID = certID;
		this.certStatus = certStatus;
		this.thisUpdate = thisUpdate;
		this.nextUpdate = nextUpdate;
		this.singleExtensions = singleExtensions;
	}

	public SingleResponse(Asn1Sequence seq)
	{
		certID = CertID.GetInstance(seq[0]);
		certStatus = CertStatus.GetInstance(seq[1]);
		thisUpdate = (DerGeneralizedTime)seq[2];
		if (seq.Count > 4)
		{
			nextUpdate = DerGeneralizedTime.GetInstance((Asn1TaggedObject)seq[3], isExplicit: true);
			singleExtensions = X509Extensions.GetInstance((Asn1TaggedObject)seq[4], explicitly: true);
		}
		else if (seq.Count > 3)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[3];
			if (asn1TaggedObject.TagNo == 0)
			{
				nextUpdate = DerGeneralizedTime.GetInstance(asn1TaggedObject, isExplicit: true);
			}
			else
			{
				singleExtensions = X509Extensions.GetInstance(asn1TaggedObject, explicitly: true);
			}
		}
	}

	public static SingleResponse GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static SingleResponse GetInstance(object obj)
	{
		if (obj == null || obj is SingleResponse)
		{
			return (SingleResponse)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SingleResponse((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certID, certStatus, thisUpdate);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, nextUpdate);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, singleExtensions);
		return new DerSequence(asn1EncodableVector);
	}
}
