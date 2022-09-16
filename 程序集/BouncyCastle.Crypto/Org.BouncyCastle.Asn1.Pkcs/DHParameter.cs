using System.Collections;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class DHParameter : Asn1Encodable
{
	internal DerInteger p;

	internal DerInteger g;

	internal DerInteger l;

	public BigInteger P => p.PositiveValue;

	public BigInteger G => g.PositiveValue;

	public BigInteger L
	{
		get
		{
			if (l != null)
			{
				return l.PositiveValue;
			}
			return null;
		}
	}

	public DHParameter(BigInteger p, BigInteger g, int l)
	{
		this.p = new DerInteger(p);
		this.g = new DerInteger(g);
		if (l != 0)
		{
			this.l = new DerInteger(l);
		}
	}

	public DHParameter(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		p = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		g = (DerInteger)enumerator.Current;
		if (enumerator.MoveNext())
		{
			l = (DerInteger)enumerator.Current;
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(p, g);
		asn1EncodableVector.AddOptional(l);
		return new DerSequence(asn1EncodableVector);
	}
}
