using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1;

public abstract class Asn1Set : Asn1Object, IEnumerable
{
	private class Asn1SetParserImpl : Asn1SetParser, IAsn1Convertible
	{
		private readonly Asn1Set outer;

		private readonly int max;

		private int index;

		public Asn1SetParserImpl(Asn1Set outer)
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

		public virtual Asn1Object ToAsn1Object()
		{
			return outer;
		}
	}

	private class DerComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			byte[] array = (byte[])x;
			byte[] array2 = (byte[])y;
			int num = array[0] & -33;
			int num2 = array2[0] & -33;
			if (num != num2)
			{
				if (num >= num2)
				{
					return 1;
				}
				return -1;
			}
			int num3 = System.Math.Min(array.Length, array2.Length);
			for (int i = 1; i < num3; i++)
			{
				byte b = array[i];
				byte b2 = array2[i];
				if (b != b2)
				{
					if (b >= b2)
					{
						return 1;
					}
					return -1;
				}
			}
			return 0;
		}
	}

	internal Asn1Encodable[] elements;

	public virtual Asn1Encodable this[int index] => elements[index];

	public virtual int Count => elements.Length;

	public Asn1SetParser Parser => new Asn1SetParserImpl(this);

	public static Asn1Set GetInstance(object obj)
	{
		if (obj == null || obj is Asn1Set)
		{
			return (Asn1Set)obj;
		}
		if (obj is Asn1SetParser)
		{
			return GetInstance(((Asn1SetParser)obj).ToAsn1Object());
		}
		if (obj is byte[])
		{
			try
			{
				return GetInstance(Asn1Object.FromByteArray((byte[])obj));
			}
			catch (IOException ex)
			{
				throw new ArgumentException("failed to construct set from byte[]: " + ex.Message);
			}
		}
		if (obj is Asn1Encodable)
		{
			Asn1Object asn1Object = ((Asn1Encodable)obj).ToAsn1Object();
			if (asn1Object is Asn1Set)
			{
				return (Asn1Set)asn1Object;
			}
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static Asn1Set GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		Asn1Object @object = obj.GetObject();
		if (explicitly)
		{
			if (!obj.IsExplicit())
			{
				throw new ArgumentException("object implicit - explicit expected.");
			}
			return (Asn1Set)@object;
		}
		if (obj.IsExplicit())
		{
			return new DerSet(@object);
		}
		if (@object is Asn1Set)
		{
			return (Asn1Set)@object;
		}
		if (@object is Asn1Sequence)
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
			Asn1Sequence asn1Sequence = (Asn1Sequence)@object;
			foreach (Asn1Encodable item in asn1Sequence)
			{
				asn1EncodableVector.Add(item);
			}
			return new DerSet(asn1EncodableVector, needsSorting: false);
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	protected internal Asn1Set()
	{
		elements = Asn1EncodableVector.EmptyElements;
	}

	protected internal Asn1Set(Asn1Encodable element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		elements = new Asn1Encodable[1] { element };
	}

	protected internal Asn1Set(params Asn1Encodable[] elements)
	{
		if (Arrays.IsNullOrContainsNull(elements))
		{
			throw new NullReferenceException("'elements' cannot be null, or contain null");
		}
		this.elements = Asn1EncodableVector.CloneElements(elements);
	}

	protected internal Asn1Set(Asn1EncodableVector elementVector)
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
		if (!(asn1Object is Asn1Set asn1Set))
		{
			return false;
		}
		int count = Count;
		if (asn1Set.Count != count)
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			Asn1Object asn1Object2 = elements[i].ToAsn1Object();
			Asn1Object asn1Object3 = asn1Set.elements[i].ToAsn1Object();
			if (asn1Object2 != asn1Object3 && !asn1Object2.CallAsn1Equals(asn1Object3))
			{
				return false;
			}
		}
		return true;
	}

	protected internal void Sort()
	{
		if (elements.Length >= 2)
		{
			int num = elements.Length;
			byte[][] array = new byte[num][];
			for (int i = 0; i < num; i++)
			{
				array[i] = elements[i].GetEncoded("DER");
			}
			Array.Sort(array, elements, new DerComparer());
		}
	}

	public override string ToString()
	{
		return CollectionUtilities.ToString(elements);
	}
}
