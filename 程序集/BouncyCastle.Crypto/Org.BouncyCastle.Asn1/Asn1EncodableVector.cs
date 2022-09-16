using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1;

public class Asn1EncodableVector : IEnumerable
{
	private const int DefaultCapacity = 10;

	internal static readonly Asn1Encodable[] EmptyElements = new Asn1Encodable[0];

	private Asn1Encodable[] elements;

	private int elementCount;

	private bool copyOnWrite;

	public Asn1Encodable this[int index]
	{
		get
		{
			if (index >= elementCount)
			{
				throw new IndexOutOfRangeException(index + " >= " + elementCount);
			}
			return elements[index];
		}
	}

	public int Count => elementCount;

	public static Asn1EncodableVector FromEnumerable(IEnumerable e)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (Asn1Encodable item in e)
		{
			asn1EncodableVector.Add(item);
		}
		return asn1EncodableVector;
	}

	public Asn1EncodableVector()
		: this(10)
	{
	}

	public Asn1EncodableVector(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentException("must not be negative", "initialCapacity");
		}
		elements = ((initialCapacity == 0) ? EmptyElements : new Asn1Encodable[initialCapacity]);
		elementCount = 0;
		copyOnWrite = false;
	}

	public Asn1EncodableVector(params Asn1Encodable[] v)
		: this()
	{
		Add(v);
	}

	public void Add(Asn1Encodable element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		int num = elements.Length;
		int num2 = elementCount + 1;
		if ((num2 > num) | copyOnWrite)
		{
			Reallocate(num2);
		}
		elements[elementCount] = element;
		elementCount = num2;
	}

	public void Add(params Asn1Encodable[] objs)
	{
		foreach (Asn1Encodable element in objs)
		{
			Add(element);
		}
	}

	public void AddOptional(params Asn1Encodable[] objs)
	{
		if (objs == null)
		{
			return;
		}
		foreach (Asn1Encodable asn1Encodable in objs)
		{
			if (asn1Encodable != null)
			{
				Add(asn1Encodable);
			}
		}
	}

	public void AddOptionalTagged(bool isExplicit, int tagNo, Asn1Encodable obj)
	{
		if (obj != null)
		{
			Add(new DerTaggedObject(isExplicit, tagNo, obj));
		}
	}

	public void AddAll(Asn1EncodableVector other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		int count = other.Count;
		if (count < 1)
		{
			return;
		}
		int num = elements.Length;
		int num2 = elementCount + count;
		if ((num2 > num) | copyOnWrite)
		{
			Reallocate(num2);
		}
		int num3 = 0;
		do
		{
			Asn1Encodable asn1Encodable = other[num3];
			if (asn1Encodable == null)
			{
				throw new NullReferenceException("'other' elements cannot be null");
			}
			elements[elementCount + num3] = asn1Encodable;
		}
		while (++num3 < count);
		elementCount = num2;
	}

	public IEnumerator GetEnumerator()
	{
		return CopyElements().GetEnumerator();
	}

	internal Asn1Encodable[] CopyElements()
	{
		if (elementCount == 0)
		{
			return EmptyElements;
		}
		Asn1Encodable[] array = new Asn1Encodable[elementCount];
		Array.Copy(elements, 0, array, 0, elementCount);
		return array;
	}

	internal Asn1Encodable[] TakeElements()
	{
		if (elementCount == 0)
		{
			return EmptyElements;
		}
		if (elements.Length == elementCount)
		{
			copyOnWrite = true;
			return elements;
		}
		Asn1Encodable[] array = new Asn1Encodable[elementCount];
		Array.Copy(elements, 0, array, 0, elementCount);
		return array;
	}

	private void Reallocate(int minCapacity)
	{
		int val = elements.Length;
		int num = System.Math.Max(val, minCapacity + (minCapacity >> 1));
		Asn1Encodable[] destinationArray = new Asn1Encodable[num];
		Array.Copy(elements, 0, destinationArray, 0, elementCount);
		elements = destinationArray;
		copyOnWrite = false;
	}

	internal static Asn1Encodable[] CloneElements(Asn1Encodable[] elements)
	{
		if (elements.Length >= 1)
		{
			return (Asn1Encodable[])elements.Clone();
		}
		return EmptyElements;
	}
}
