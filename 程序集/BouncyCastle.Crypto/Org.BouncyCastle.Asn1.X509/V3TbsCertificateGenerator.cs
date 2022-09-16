using System;

namespace Org.BouncyCastle.Asn1.X509;

public class V3TbsCertificateGenerator
{
	internal DerTaggedObject version = new DerTaggedObject(0, new DerInteger(2));

	internal DerInteger serialNumber;

	internal AlgorithmIdentifier signature;

	internal X509Name issuer;

	internal Time startDate;

	internal Time endDate;

	internal X509Name subject;

	internal SubjectPublicKeyInfo subjectPublicKeyInfo;

	internal X509Extensions extensions;

	private bool altNamePresentAndCritical;

	private DerBitString issuerUniqueID;

	private DerBitString subjectUniqueID;

	public void SetSerialNumber(DerInteger serialNumber)
	{
		this.serialNumber = serialNumber;
	}

	public void SetSignature(AlgorithmIdentifier signature)
	{
		this.signature = signature;
	}

	public void SetIssuer(X509Name issuer)
	{
		this.issuer = issuer;
	}

	public void SetStartDate(DerUtcTime startDate)
	{
		this.startDate = new Time(startDate);
	}

	public void SetStartDate(Time startDate)
	{
		this.startDate = startDate;
	}

	public void SetEndDate(DerUtcTime endDate)
	{
		this.endDate = new Time(endDate);
	}

	public void SetEndDate(Time endDate)
	{
		this.endDate = endDate;
	}

	public void SetSubject(X509Name subject)
	{
		this.subject = subject;
	}

	public void SetIssuerUniqueID(DerBitString uniqueID)
	{
		issuerUniqueID = uniqueID;
	}

	public void SetSubjectUniqueID(DerBitString uniqueID)
	{
		subjectUniqueID = uniqueID;
	}

	public void SetSubjectPublicKeyInfo(SubjectPublicKeyInfo pubKeyInfo)
	{
		subjectPublicKeyInfo = pubKeyInfo;
	}

	public void SetExtensions(X509Extensions extensions)
	{
		this.extensions = extensions;
		if (extensions != null)
		{
			X509Extension extension = extensions.GetExtension(X509Extensions.SubjectAlternativeName);
			if (extension != null && extension.IsCritical)
			{
				altNamePresentAndCritical = true;
			}
		}
	}

	public TbsCertificateStructure GenerateTbsCertificate()
	{
		if (serialNumber == null || signature == null || issuer == null || startDate == null || endDate == null || (subject == null && !altNamePresentAndCritical) || subjectPublicKeyInfo == null)
		{
			throw new InvalidOperationException("not all mandatory fields set in V3 TBScertificate generator");
		}
		DerSequence derSequence = new DerSequence(startDate, endDate);
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, serialNumber, signature, issuer, derSequence);
		if (subject != null)
		{
			asn1EncodableVector.Add(subject);
		}
		else
		{
			asn1EncodableVector.Add(DerSequence.Empty);
		}
		asn1EncodableVector.Add(subjectPublicKeyInfo);
		if (issuerUniqueID != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 1, issuerUniqueID));
		}
		if (subjectUniqueID != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 2, subjectUniqueID));
		}
		if (extensions != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(3, extensions));
		}
		return new TbsCertificateStructure(new DerSequence(asn1EncodableVector));
	}
}
