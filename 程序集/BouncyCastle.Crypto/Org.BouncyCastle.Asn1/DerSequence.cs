using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerSequence : Asn1Sequence
{
	public static readonly DerSequence Empty = new DerSequence();

	public static DerSequence FromVector(Asn1EncodableVector elementVector)
	{
		if (elementVector.Count >= 1)
		{
			return new DerSequence(elementVector);
		}
		return Empty;
	}

	public DerSequence()
	{
	}

	public DerSequence(Asn1Encodable element)
		: base(element)
	{
	}

	public DerSequence(params Asn1Encodable[] elements)
		: base(elements)
	{
	}

	public DerSequence(Asn1EncodableVector elementVector)
		: base(elementVector)
	{
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
		derOut.WriteEncoded(48, bytes);
	}
}
