using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ess;

[Obsolete("Use version in Asn1.Esf instead")]
public class OtherSigningCertificate : Asn1Encodable
{
	private Asn1Sequence certs;

	private Asn1Sequence policies;

	public static OtherSigningCertificate GetInstance(object o)
	{
		if (o == null || o is OtherSigningCertificate)
		{
			return (OtherSigningCertificate)o;
		}
		if (o is Asn1Sequence)
		{
			return new OtherSigningCertificate((Asn1Sequence)o);
		}
		throw new ArgumentException("unknown object in 'OtherSigningCertificate' factory : " + Platform.GetTypeName(o) + ".");
	}

	public OtherSigningCertificate(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		certs = Asn1Sequence.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			policies = Asn1Sequence.GetInstance(seq[1]);
		}
	}

	public OtherSigningCertificate(OtherCertID otherCertID)
	{
		certs = new DerSequence(otherCertID);
	}

	public OtherCertID[] GetCerts()
	{
		OtherCertID[] array = new OtherCertID[certs.Count];
		for (int i = 0; i != certs.Count; i++)
		{
			array[i] = OtherCertID.GetInstance(certs[i]);
		}
		return array;
	}

	public PolicyInformation[] GetPolicies()
	{
		if (policies == null)
		{
			return null;
		}
		PolicyInformation[] array = new PolicyInformation[policies.Count];
		for (int i = 0; i != policies.Count; i++)
		{
			array[i] = PolicyInformation.GetInstance(policies[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certs);
		asn1EncodableVector.AddOptional(policies);
		return new DerSequence(asn1EncodableVector);
	}
}
