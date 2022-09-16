using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1;

public class BerSequence : DerSequence
{
	public new static readonly BerSequence Empty = new BerSequence();

	public new static BerSequence FromVector(Asn1EncodableVector elementVector)
	{
		if (elementVector.Count >= 1)
		{
			return new BerSequence(elementVector);
		}
		return Empty;
	}

	public BerSequence()
	{
	}

	public BerSequence(Asn1Encodable element)
		: base(element)
	{
	}

	public BerSequence(params Asn1Encodable[] elements)
		: base(elements)
	{
	}

	public BerSequence(Asn1EncodableVector elementVector)
		: base(elementVector)
	{
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (derOut is Asn1OutputStream || derOut is BerOutputStream)
		{
			derOut.WriteByte(48);
			derOut.WriteByte(128);
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Asn1Encodable obj = (Asn1Encodable)enumerator.Current;
						derOut.WriteObject(obj);
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
			}
			derOut.WriteByte(0);
			derOut.WriteByte(0);
		}
		else
		{
			base.Encode(derOut);
		}
	}
}
