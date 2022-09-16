using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf;

public class CertTemplateBuilder
{
	private DerInteger version;

	private DerInteger serialNumber;

	private AlgorithmIdentifier signingAlg;

	private X509Name issuer;

	private OptionalValidity validity;

	private X509Name subject;

	private SubjectPublicKeyInfo publicKey;

	private DerBitString issuerUID;

	private DerBitString subjectUID;

	private X509Extensions extensions;

	public virtual CertTemplateBuilder SetVersion(int ver)
	{
		version = new DerInteger(ver);
		return this;
	}

	public virtual CertTemplateBuilder SetSerialNumber(DerInteger ser)
	{
		serialNumber = ser;
		return this;
	}

	public virtual CertTemplateBuilder SetSigningAlg(AlgorithmIdentifier aid)
	{
		signingAlg = aid;
		return this;
	}

	public virtual CertTemplateBuilder SetIssuer(X509Name name)
	{
		issuer = name;
		return this;
	}

	public virtual CertTemplateBuilder SetValidity(OptionalValidity v)
	{
		validity = v;
		return this;
	}

	public virtual CertTemplateBuilder SetSubject(X509Name name)
	{
		subject = name;
		return this;
	}

	public virtual CertTemplateBuilder SetPublicKey(SubjectPublicKeyInfo spki)
	{
		publicKey = spki;
		return this;
	}

	public virtual CertTemplateBuilder SetIssuerUID(DerBitString uid)
	{
		issuerUID = uid;
		return this;
	}

	public virtual CertTemplateBuilder SetSubjectUID(DerBitString uid)
	{
		subjectUID = uid;
		return this;
	}

	public virtual CertTemplateBuilder SetExtensions(X509Extensions extens)
	{
		extensions = extens;
		return this;
	}

	public virtual CertTemplate Build()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		AddOptional(asn1EncodableVector, 0, isExplicit: false, version);
		AddOptional(asn1EncodableVector, 1, isExplicit: false, serialNumber);
		AddOptional(asn1EncodableVector, 2, isExplicit: false, signingAlg);
		AddOptional(asn1EncodableVector, 3, isExplicit: true, issuer);
		AddOptional(asn1EncodableVector, 4, isExplicit: false, validity);
		AddOptional(asn1EncodableVector, 5, isExplicit: true, subject);
		AddOptional(asn1EncodableVector, 6, isExplicit: false, publicKey);
		AddOptional(asn1EncodableVector, 7, isExplicit: false, issuerUID);
		AddOptional(asn1EncodableVector, 8, isExplicit: false, subjectUID);
		AddOptional(asn1EncodableVector, 9, isExplicit: false, extensions);
		return CertTemplate.GetInstance(new DerSequence(asn1EncodableVector));
	}

	private void AddOptional(Asn1EncodableVector v, int tagNo, bool isExplicit, Asn1Encodable obj)
	{
		if (obj != null)
		{
			v.Add(new DerTaggedObject(isExplicit, tagNo, obj));
		}
	}
}
