using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class SignaturePolicyIdentifier : Asn1Encodable, IAsn1Choice
{
	private readonly SignaturePolicyId sigPolicy;

	public SignaturePolicyId SignaturePolicyId => sigPolicy;

	public static SignaturePolicyIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is SignaturePolicyIdentifier)
		{
			return (SignaturePolicyIdentifier)obj;
		}
		if (obj is SignaturePolicyId)
		{
			return new SignaturePolicyIdentifier((SignaturePolicyId)obj);
		}
		if (obj is Asn1Null)
		{
			return new SignaturePolicyIdentifier();
		}
		throw new ArgumentException("Unknown object in 'SignaturePolicyIdentifier' factory: " + Platform.GetTypeName(obj), "obj");
	}

	public SignaturePolicyIdentifier()
	{
		sigPolicy = null;
	}

	public SignaturePolicyIdentifier(SignaturePolicyId signaturePolicyId)
	{
		if (signaturePolicyId == null)
		{
			throw new ArgumentNullException("signaturePolicyId");
		}
		sigPolicy = signaturePolicyId;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (sigPolicy != null)
		{
			return sigPolicy.ToAsn1Object();
		}
		return DerNull.Instance;
	}
}
