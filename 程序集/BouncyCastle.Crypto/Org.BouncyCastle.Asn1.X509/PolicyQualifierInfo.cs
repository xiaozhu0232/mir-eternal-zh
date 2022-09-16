using System;

namespace Org.BouncyCastle.Asn1.X509;

public class PolicyQualifierInfo : Asn1Encodable
{
	private readonly DerObjectIdentifier policyQualifierId;

	private readonly Asn1Encodable qualifier;

	public virtual DerObjectIdentifier PolicyQualifierId => policyQualifierId;

	public virtual Asn1Encodable Qualifier => qualifier;

	public PolicyQualifierInfo(DerObjectIdentifier policyQualifierId, Asn1Encodable qualifier)
	{
		this.policyQualifierId = policyQualifierId;
		this.qualifier = qualifier;
	}

	public PolicyQualifierInfo(string cps)
	{
		policyQualifierId = PolicyQualifierID.IdQtCps;
		qualifier = new DerIA5String(cps);
	}

	private PolicyQualifierInfo(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		policyQualifierId = DerObjectIdentifier.GetInstance(seq[0]);
		qualifier = seq[1];
	}

	public static PolicyQualifierInfo GetInstance(object obj)
	{
		if (obj is PolicyQualifierInfo)
		{
			return (PolicyQualifierInfo)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new PolicyQualifierInfo(Asn1Sequence.GetInstance(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(policyQualifierId, qualifier);
	}
}
