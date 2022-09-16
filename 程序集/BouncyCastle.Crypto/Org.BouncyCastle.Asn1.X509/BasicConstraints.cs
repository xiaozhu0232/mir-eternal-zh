using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509;

public class BasicConstraints : Asn1Encodable
{
	private readonly DerBoolean cA;

	private readonly DerInteger pathLenConstraint;

	public BigInteger PathLenConstraint
	{
		get
		{
			if (pathLenConstraint != null)
			{
				return pathLenConstraint.Value;
			}
			return null;
		}
	}

	public static BasicConstraints GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static BasicConstraints GetInstance(object obj)
	{
		if (obj is BasicConstraints)
		{
			return (BasicConstraints)obj;
		}
		if (obj is X509Extension)
		{
			return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		if (obj == null)
		{
			return null;
		}
		return new BasicConstraints(Asn1Sequence.GetInstance(obj));
	}

	public static BasicConstraints FromExtensions(X509Extensions extensions)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.BasicConstraints));
	}

	private BasicConstraints(Asn1Sequence seq)
	{
		if (seq.Count <= 0)
		{
			return;
		}
		if (seq[0] is DerBoolean)
		{
			cA = DerBoolean.GetInstance(seq[0]);
		}
		else
		{
			pathLenConstraint = DerInteger.GetInstance(seq[0]);
		}
		if (seq.Count > 1)
		{
			if (cA == null)
			{
				throw new ArgumentException("wrong sequence in constructor", "seq");
			}
			pathLenConstraint = DerInteger.GetInstance(seq[1]);
		}
	}

	public BasicConstraints(bool cA)
	{
		if (cA)
		{
			this.cA = DerBoolean.True;
		}
	}

	public BasicConstraints(int pathLenConstraint)
	{
		cA = DerBoolean.True;
		this.pathLenConstraint = new DerInteger(pathLenConstraint);
	}

	public bool IsCA()
	{
		if (cA != null)
		{
			return cA.IsTrue;
		}
		return false;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(2);
		asn1EncodableVector.AddOptional(cA, pathLenConstraint);
		return new DerSequence(asn1EncodableVector);
	}

	public override string ToString()
	{
		if (pathLenConstraint == null)
		{
			return "BasicConstraints: isCa(" + IsCA() + ")";
		}
		return "BasicConstraints: isCa(" + IsCA() + "), pathLenConstraint = " + pathLenConstraint.Value;
	}
}
