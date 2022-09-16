using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf;

public class CertTemplate : Asn1Encodable
{
	private readonly Asn1Sequence seq;

	private readonly DerInteger version;

	private readonly DerInteger serialNumber;

	private readonly AlgorithmIdentifier signingAlg;

	private readonly X509Name issuer;

	private readonly OptionalValidity validity;

	private readonly X509Name subject;

	private readonly SubjectPublicKeyInfo publicKey;

	private readonly DerBitString issuerUID;

	private readonly DerBitString subjectUID;

	private readonly X509Extensions extensions;

	public virtual int Version => version.IntValueExact;

	public virtual DerInteger SerialNumber => serialNumber;

	public virtual AlgorithmIdentifier SigningAlg => signingAlg;

	public virtual X509Name Issuer => issuer;

	public virtual OptionalValidity Validity => validity;

	public virtual X509Name Subject => subject;

	public virtual SubjectPublicKeyInfo PublicKey => publicKey;

	public virtual DerBitString IssuerUID => issuerUID;

	public virtual DerBitString SubjectUID => subjectUID;

	public virtual X509Extensions Extensions => extensions;

	private CertTemplate(Asn1Sequence seq)
	{
		this.seq = seq;
		foreach (Asn1TaggedObject item in seq)
		{
			switch (item.TagNo)
			{
			case 0:
				version = DerInteger.GetInstance(item, isExplicit: false);
				break;
			case 1:
				serialNumber = DerInteger.GetInstance(item, isExplicit: false);
				break;
			case 2:
				signingAlg = AlgorithmIdentifier.GetInstance(item, explicitly: false);
				break;
			case 3:
				issuer = X509Name.GetInstance(item, explicitly: true);
				break;
			case 4:
				validity = OptionalValidity.GetInstance(Asn1Sequence.GetInstance(item, explicitly: false));
				break;
			case 5:
				subject = X509Name.GetInstance(item, explicitly: true);
				break;
			case 6:
				publicKey = SubjectPublicKeyInfo.GetInstance(item, explicitly: false);
				break;
			case 7:
				issuerUID = DerBitString.GetInstance(item, isExplicit: false);
				break;
			case 8:
				subjectUID = DerBitString.GetInstance(item, isExplicit: false);
				break;
			case 9:
				extensions = X509Extensions.GetInstance(item, explicitly: false);
				break;
			default:
				throw new ArgumentException("unknown tag: " + item.TagNo, "seq");
			}
		}
	}

	public static CertTemplate GetInstance(object obj)
	{
		if (obj is CertTemplate)
		{
			return (CertTemplate)obj;
		}
		if (obj != null)
		{
			return new CertTemplate(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}
}
