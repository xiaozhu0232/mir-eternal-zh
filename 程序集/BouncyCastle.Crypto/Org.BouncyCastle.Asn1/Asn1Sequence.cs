using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1;

public abstract class Asn1Sequence : Asn1Object, IEnumerable
{
	private class Asn1SequenceParserImpl : Asn1SequenceParser, IAsn1Convertible
	{
		private readonly Asn1Sequence outer;

		private readonly int max;

		private int index;

		public Asn1SequenceParserImpl(Asn1Sequence outer)
		{
			this.outer = outer;
			max = outer.Count;
		}

		public IAsn1Convertible ReadObject()
		{
			if (index == max)
			{
				return null;
			}
			Asn1Encodable asn1Encodable = outer[index++];
			if (asn1Encodable is Asn1Sequence)
			{
				return ((Asn1Sequence)asn1Encodable).Parser;
			}
			if (asn1Encodable is Asn1Set)
			{
				return ((Asn1Set)asn1Encodable).Parser;
			}
			return asn1Encodable;
		}

		public Asn1Object ToAsn1Object()
		{
			return outer;
		}
	}

	internal Asn1Encodable[] elements;

	public virtual Asn1SequenceParser Parser => new Asn1SequenceParserImpl(this);

	public virtual Asn1Encodable this[int index] => elements[index];

	public virtual int Count => elements.Length;

	public static Asn1Sequence GetInstance(object obj)
	{
		if (obj == null || obj is Asn1Sequence)
		{
			return (Asn1Sequence)obj;
		}
		if (obj is Asn1SequenceParser)
		{
			return GetInstance(((Asn1SequenceParser)obj).ToAsn1Object());
		}
		if (obj is byte[])
		{
			try
			{
				return GetInstance(Asn1Object.FromByteArray((byte[])obj));
			}
			catch (IOException ex)
			{
				throw new ArgumentException("failed to construct sequence from byte[]: " + ex.Message);
			}
		}
		if (obj is Asn1Encodable)
		{
			Asn1Object asn1Object = ((Asn1Encodable)obj).ToAsn1Object();
			if (asn1Object is Asn1Sequence)
			{
				return (Asn1Sequence)asn1Object;
			}
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static Asn1Sequence GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		Asn1Object @object = obj.GetObject();
		if (explicitly)
		{
			if (!obj.IsExplicit())
			{
				throw new ArgumentException("object implicit - explicit expected.");
			}
			return (Asn1Sequence)@object;
		}
		if (obj.IsExplicit())
		{
			if (obj is BerTaggedObject)
			{
				return new BerSequence(@object);
			}
			return new DerSequence(@object);
		}
		if (@object is Asn1Sequence)
		{
			return (Asn1Sequence)@object;
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	protected internal Asn1Sequence()
	{
		elements = Asn1EncodableVector.EmptyElements;
	}

	protected internal Asn1Sequence(Asn1Encodable element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		elements = new Asn1Encodable[1] { element };
	}

	protected internal Asn1Sequence(params Asn1Encodable[] elements)
	{
		if (Arrays.IsNullOrContainsNull(elements))
		{
			throw new NullReferenceException("'elements' cannot be null, or contain null");
		}
		this.elements = Asn1EncodableVector.CloneElements(elements);
	}

	protected internal Asn1Sequence(Asn1EncodableVector elementVector)
	{
		if (elementVector == null)
		{
			throw new ArgumentNullException("elementVector");
		}
		elements = elementVector.TakeElements();
	}

	public virtual IEnumerator GetEnumerator()
	{
		return elements.GetEnumerator();
	}

	public virtual Asn1Encodable[] ToArray()
	{
		return Asn1EncodableVector.CloneElements(elements);
	}

	protected override int Asn1GetHashCode()
	{
		int num = elements.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= elements[num].ToAsn1Object().CallAsn1GetHashCode();
		}
		return num2;
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is Asn1Sequence asn1Sequence))
		{
			return false;
		}
		int count = Count;
		if (asn1Sequence.Count != count)
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			Asn1Object asn1Object2 = elements[i].ToAsn1Object();
			Asn1Object asn1Object3 = asn1Sequence.elements[i].ToAsn1Object();
			if (asn1Object2 != asn1Object3 && !asn1Object2.CallAsn1Equals(asn1Object3))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return CollectionUtilities.ToString(elements);
	}
}
