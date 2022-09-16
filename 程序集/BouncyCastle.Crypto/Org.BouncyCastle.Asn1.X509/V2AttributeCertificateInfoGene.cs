using System;

namespace Org.BouncyCastle.Asn1.X509;

public class V2AttributeCertificateInfoGenerator
{
	internal DerInteger version;

	internal Holder holder;

	internal AttCertIssuer issuer;

	internal AlgorithmIdentifier signature;

	internal DerInteger serialNumber;

	internal Asn1EncodableVector attributes;

	internal DerBitString issuerUniqueID;

	internal X509Extensions extensions;

	internal DerGeneralizedTime startDate;

	internal DerGeneralizedTime endDate;

	public V2AttributeCertificateInfoGenerator()
	{
		version = new DerInteger(1);
		attributes = new Asn1EncodableVector();
	}

	public void SetHolder(Holder holder)
	{
		this.holder = holder;
	}

	public void AddAttribute(string oid, Asn1Encodable value)
	{
		attributes.Add(new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value)));
	}

	public void AddAttribute(AttributeX509 attribute)
	{
		attributes.Add(attribute);
	}

	public void SetSerialNumber(DerInteger serialNumber)
	{
		this.serialNumber = serialNumber;
	}

	public void SetSignature(AlgorithmIdentifier signature)
	{
		this.signature = signature;
	}

	public void SetIssuer(AttCertIssuer issuer)
	{
		this.issuer = issuer;
	}

	public void SetStartDate(DerGeneralizedTime startDate)
	{
		this.startDate = startDate;
	}

	public void SetEndDate(DerGeneralizedTime endDate)
	{
		this.endDate = endDate;
	}

	public void SetIssuerUniqueID(DerBitString issuerUniqueID)
	{
		this.issuerUniqueID = issuerUniqueID;
	}

	public void SetExtensions(X509Extensions extensions)
	{
		this.extensions = extensions;
	}

	public AttributeCertificateInfo GenerateAttributeCertificateInfo()
	{
		if (serialNumber == null || signature == null || issuer == null || startDate == null || endDate == null || holder == null || attributes == null)
		{
			throw new InvalidOperationException("not all mandatory fields set in V2 AttributeCertificateInfo generator");
		}
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, holder, issuer, signature, serialNumber);
		asn1EncodableVector.Add(new AttCertValidityPeriod(startDate, endDate));
		asn1EncodableVector.Add(new DerSequence(attributes));
		if (issuerUniqueID != null)
		{
			asn1EncodableVector.Add(issuerUniqueID);
		}
		if (extensions != null)
		{
			asn1EncodableVector.Add(extensions);
		}
		return AttributeCertificateInfo.GetInstance(new DerSequence(asn1EncodableVector));
	}
}
