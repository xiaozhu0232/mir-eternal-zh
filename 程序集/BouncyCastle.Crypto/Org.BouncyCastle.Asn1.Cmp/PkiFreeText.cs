using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiFreeText : Asn1Encodable
{
	internal Asn1Sequence strings;

	[Obsolete("Use 'Count' property instead")]
	public int Size => strings.Count;

	public int Count => strings.Count;

	public DerUtf8String this[int index] => (DerUtf8String)strings[index];

	public static PkiFreeText GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static PkiFreeText GetInstance(object obj)
	{
		if (obj is PkiFreeText)
		{
			return (PkiFreeText)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PkiFreeText((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public PkiFreeText(Asn1Sequence seq)
	{
		foreach (object item in seq)
		{
			if (!(item is DerUtf8String))
			{
				throw new ArgumentException("attempt to insert non UTF8 STRING into PkiFreeText");
			}
		}
		strings = seq;
	}

	public PkiFreeText(DerUtf8String p)
	{
		strings = new DerSequence(p);
	}

	[Obsolete("Use 'object[index]' syntax instead")]
	public DerUtf8String GetStringAt(int index)
	{
		return this[index];
	}

	public override Asn1Object ToAsn1Object()
	{
		return strings;
	}
}
