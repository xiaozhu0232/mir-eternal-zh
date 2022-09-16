using System;
using System.Text;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Pkix;

public class PkixCertPathBuilderResult : PkixCertPathValidatorResult
{
	private PkixCertPath certPath;

	public PkixCertPath CertPath => certPath;

	public PkixCertPathBuilderResult(PkixCertPath certPath, TrustAnchor trustAnchor, PkixPolicyNode policyTree, AsymmetricKeyParameter subjectPublicKey)
		: base(trustAnchor, policyTree, subjectPublicKey)
	{
		if (certPath == null)
		{
			throw new ArgumentNullException("certPath");
		}
		this.certPath = certPath;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SimplePKIXCertPathBuilderResult: [\n");
		stringBuilder.Append("  Certification Path: ").Append(CertPath).Append('\n');
		stringBuilder.Append("  Trust Anchor: ").Append(base.TrustAnchor.TrustedCert.IssuerDN.ToString()).Append('\n');
		stringBuilder.Append("  Subject Public Key: ").Append(base.SubjectPublicKey).Append("\n]");
		return stringBuilder.ToString();
	}
}
