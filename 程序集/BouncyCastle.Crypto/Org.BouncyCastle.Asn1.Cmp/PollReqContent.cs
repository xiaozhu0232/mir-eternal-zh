using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PollReqContent : Asn1Encodable
{
	private readonly Asn1Sequence content;

	private PollReqContent(Asn1Sequence seq)
	{
		content = seq;
	}

	public static PollReqContent GetInstance(object obj)
	{
		if (obj is PollReqContent)
		{
			return (PollReqContent)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PollReqContent((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual DerInteger[][] GetCertReqIDs()
	{
		DerInteger[][] array = new DerInteger[content.Count][];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = SequenceToDerIntegerArray((Asn1Sequence)content[i]);
		}
		return array;
	}

	private static DerInteger[] SequenceToDerIntegerArray(Asn1Sequence seq)
	{
		DerInteger[] array = new DerInteger[seq.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = DerInteger.GetInstance(seq[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return content;
	}
}
