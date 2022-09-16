using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class AttributeCertificateInfo : Asn1Encodable
{
	internal readonly DerInteger version;

	internal readonly Holder holder;

	internal readonly AttCertIssuer issuer;

	internal readonly AlgorithmIdentifier signature;

	internal readonly DerInteger serialNumber;

	internal readonly AttCertValidityPeriod attrCertValidityPeriod;

	internal readonly Asn1Sequence attributes;

	internal readonly DerBitString issuerUniqueID;

	internal readonly X509Extensions extensions;

	public DerInteger Version => version;

	public Holder Holder => holder;

	public AttCertIssuer Issuer => issuer;

	public AlgorithmIdentifier Signature => signature;

	public DerInteger SerialNumber => serialNumber;

	public AttCertValidityPeriod AttrCertValidityPeriod => attrCertValidityPeriod;

	public Asn1Sequence Attributes => attributes;

	public DerBitString IssuerUniqueID => issuerUniqueID;

	public X509Extensions Extensions => extensions;

	public static AttributeCertificateInfo GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static AttributeCertificateInfo GetInstance(object obj)
	{
		if (obj is AttributeCertificateInfo)
		{
			return (AttributeCertificateInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new AttributeCertificateInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private AttributeCertificateInfo(Asn1Sequence seq)
	{
		if (seq.Count < 7 || seq.Count > 9)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		version = DerInteger.GetInstance(seq[0]);
		holder = Holder.GetInstance(seq[1]);
		issuer = AttCertIssuer.GetInstance(seq[2]);
		signature = AlgorithmIdentifier.GetInstance(seq[3]);
		serialNumber = DerInteger.GetInstance(seq[4]);
		attrCertValidityPeriod = AttCertValidityPeriod.GetInstance(seq[5]);
		attributes = Asn1Sequence.GetInstance(seq[6]);
		for (int i = 7; i < seq.Count; i++)
		{
			Asn1Encodable asn1Encodable = seq[i];
			if (asn1Encodable is DerBitString)
			{
				issuerUniqueID = DerBitString.GetInstance(seq[i]);
			}
			else if (asn1Encodable is Asn1Sequence || asn1Encodable is X509Extensions)
			{
				extensions = X509Extensions.GetInstance(seq[i]);
			}
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, holder, issuer, signature, serialNumber, attrCertValidityPeriod, attributes);
		asn1EncodableVector.AddOptional(issuerUniqueID, extensions);
		return new DerSequence(asn1EncodableVector);
	}
}
