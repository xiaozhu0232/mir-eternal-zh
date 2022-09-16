using System;
using System.Text;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Pkix;

public class PkixCertPathValidatorResult
{
	private TrustAnchor trustAnchor;

	private PkixPolicyNode policyTree;

	private AsymmetricKeyParameter subjectPublicKey;

	public PkixPolicyNode PolicyTree => policyTree;

	public TrustAnchor TrustAnchor => trustAnchor;

	public AsymmetricKeyParameter SubjectPublicKey => subjectPublicKey;

	public PkixCertPathValidatorResult(TrustAnchor trustAnchor, PkixPolicyNode policyTree, AsymmetricKeyParameter subjectPublicKey)
	{
		if (subjectPublicKey == null)
		{
			throw new NullReferenceException("subjectPublicKey must be non-null");
		}
		if (trustAnchor == null)
		{
			throw new NullReferenceException("trustAnchor must be non-null");
		}
		this.trustAnchor = trustAnchor;
		this.policyTree = policyTree;
		this.subjectPublicKey = subjectPublicKey;
	}

	public object Clone()
	{
		return new PkixCertPathValidatorResult(TrustAnchor, PolicyTree, SubjectPublicKey);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PKIXCertPathValidatorResult: [ \n");
		stringBuilder.Append("  Trust Anchor: ").Append(TrustAnchor).Append('\n');
		stringBuilder.Append("  Policy Tree: ").Append(PolicyTree).Append('\n');
		stringBuilder.Append("  Subject Public Key: ").Append(SubjectPublicKey).Append("\n]");
		return stringBuilder.ToString();
	}
}
