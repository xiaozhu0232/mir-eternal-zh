using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X9;

public class DHDomainParameters : Asn1Encodable
{
	private readonly DerInteger p;

	private readonly DerInteger g;

	private readonly DerInteger q;

	private readonly DerInteger j;

	private readonly DHValidationParms validationParms;

	public DerInteger P => p;

	public DerInteger G => g;

	public DerInteger Q => q;

	public DerInteger J => j;

	public DHValidationParms ValidationParms => validationParms;

	public static DHDomainParameters GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static DHDomainParameters GetInstance(object obj)
	{
		if (obj == null || obj is DHDomainParameters)
		{
			return (DHDomainParameters)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new DHDomainParameters((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid DHDomainParameters: " + Platform.GetTypeName(obj), "obj");
	}

	public DHDomainParameters(DerInteger p, DerInteger g, DerInteger q, DerInteger j, DHValidationParms validationParms)
	{
		if (p == null)
		{
			throw new ArgumentNullException("p");
		}
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (q == null)
		{
			throw new ArgumentNullException("q");
		}
		this.p = p;
		this.g = g;
		this.q = q;
		this.j = j;
		this.validationParms = validationParms;
	}

	private DHDomainParameters(Asn1Sequence seq)
	{
		if (seq.Count < 3 || seq.Count > 5)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		IEnumerator enumerator = seq.GetEnumerator();
		p = DerInteger.GetInstance(GetNext(enumerator));
		g = DerInteger.GetInstance(GetNext(enumerator));
		q = DerInteger.GetInstance(GetNext(enumerator));
		Asn1Encodable next = GetNext(enumerator);
		if (next != null && next is DerInteger)
		{
			j = DerInteger.GetInstance(next);
			next = GetNext(enumerator);
		}
		if (next != null)
		{
			validationParms = DHValidationParms.GetInstance(next.ToAsn1Object());
		}
	}

	private static Asn1Encodable GetNext(IEnumerator e)
	{
		if (!e.MoveNext())
		{
			return null;
		}
		return (Asn1Encodable)e.Current;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(p, g, q);
		asn1EncodableVector.AddOptional(j, validationParms);
		return new DerSequence(asn1EncodableVector);
	}
}
