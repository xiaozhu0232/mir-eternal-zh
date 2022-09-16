using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerSet : Asn1Set
{
	public static readonly DerSet Empty = new DerSet();

	public static DerSet FromVector(Asn1EncodableVector elementVector)
	{
		if (elementVector.Count >= 1)
		{
			return new DerSet(elementVector);
		}
		return Empty;
	}

	internal static DerSet FromVector(Asn1EncodableVector elementVector, bool needsSorting)
	{
		if (elementVector.Count >= 1)
		{
			return new DerSet(elementVector, needsSorting);
		}
		return Empty;
	}

	public DerSet()
	{
	}

	public DerSet(Asn1Encodable element)
		: base(element)
	{
	}

	public DerSet(params Asn1Encodable[] elements)
		: base(elements)
	{
		Sort();
	}

	public DerSet(Asn1EncodableVector elementVector)
		: this(elementVector, needsSorting: true)
	{
	}

	internal DerSet(Asn1EncodableVector elementVector, bool needsSorting)
		: base(elementVector)
	{
		if (needsSorting)
		{
			Sort();
		}
	}

	internal override void Encode(DerOutputStream derOut)
	{
		MemoryStream memoryStream = new MemoryStream();
		DerOutputStream derOutputStream = new DerOutputStream(memoryStream);
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Asn1Encodable obj = (Asn1Encodable)enumerator.Current;
				derOutputStream.WriteObject(obj);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		Platform.Dispose(derOutputStream);
		byte[] bytes = memoryStream.ToArray();
		derOut.WriteEncoded(49, bytes);
	}
}
