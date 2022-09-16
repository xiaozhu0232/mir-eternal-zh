using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.CryptoPro;

public class Gost28147Parameters : Asn1Encodable
{
	private readonly Asn1OctetString iv;

	private readonly DerObjectIdentifier paramSet;

	public static Gost28147Parameters GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static Gost28147Parameters GetInstance(object obj)
	{
		if (obj == null || obj is Gost28147Parameters)
		{
			return (Gost28147Parameters)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new Gost28147Parameters((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid GOST3410Parameter: " + Platform.GetTypeName(obj));
	}

	private Gost28147Parameters(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Wrong number of elements in sequence", "seq");
		}
		iv = Asn1OctetString.GetInstance(seq[0]);
		paramSet = DerObjectIdentifier.GetInstance(seq[1]);
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(iv, paramSet);
	}
}
