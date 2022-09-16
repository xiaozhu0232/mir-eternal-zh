using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class SigPolicyQualifierInfo : Asn1Encodable
{
	private readonly DerObjectIdentifier sigPolicyQualifierId;

	private readonly Asn1Object sigQualifier;

	public DerObjectIdentifier SigPolicyQualifierId => sigPolicyQualifierId;

	public Asn1Object SigQualifier => sigQualifier;

	public static SigPolicyQualifierInfo GetInstance(object obj)
	{
		if (obj == null || obj is SigPolicyQualifierInfo)
		{
			return (SigPolicyQualifierInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SigPolicyQualifierInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'SigPolicyQualifierInfo' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private SigPolicyQualifierInfo(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		sigPolicyQualifierId = (DerObjectIdentifier)seq[0].ToAsn1Object();
		sigQualifier = seq[1].ToAsn1Object();
	}

	public SigPolicyQualifierInfo(DerObjectIdentifier sigPolicyQualifierId, Asn1Encodable sigQualifier)
	{
		this.sigPolicyQualifierId = sigPolicyQualifierId;
		this.sigQualifier = sigQualifier.ToAsn1Object();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(sigPolicyQualifierId, sigQualifier);
	}
}
