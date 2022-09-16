using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.BC;

public class LinkedCertificate : Asn1Encodable
{
	private readonly DigestInfo mDigest;

	private readonly GeneralName mCertLocation;

	private X509Name mCertIssuer;

	private GeneralNames mCACerts;

	public virtual DigestInfo Digest => mDigest;

	public virtual GeneralName CertLocation => mCertLocation;

	public virtual X509Name CertIssuer => mCertIssuer;

	public virtual GeneralNames CACerts => mCACerts;

	public LinkedCertificate(DigestInfo digest, GeneralName certLocation)
		: this(digest, certLocation, null, null)
	{
	}

	public LinkedCertificate(DigestInfo digest, GeneralName certLocation, X509Name certIssuer, GeneralNames caCerts)
	{
		mDigest = digest;
		mCertLocation = certLocation;
		mCertIssuer = certIssuer;
		mCACerts = caCerts;
	}

	private LinkedCertificate(Asn1Sequence seq)
	{
		mDigest = DigestInfo.GetInstance(seq[0]);
		mCertLocation = GeneralName.GetInstance(seq[1]);
		for (int i = 2; i < seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			switch (instance.TagNo)
			{
			case 0:
				mCertIssuer = X509Name.GetInstance(instance, explicitly: false);
				break;
			case 1:
				mCACerts = GeneralNames.GetInstance(instance, explicitly: false);
				break;
			default:
				throw new ArgumentException("unknown tag in tagged field");
			}
		}
	}

	public static LinkedCertificate GetInstance(object obj)
	{
		if (obj is LinkedCertificate)
		{
			return (LinkedCertificate)obj;
		}
		if (obj != null)
		{
			return new LinkedCertificate(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(mDigest, mCertLocation);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, mCertIssuer);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, mCACerts);
		return new DerSequence(asn1EncodableVector);
	}
}
