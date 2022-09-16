using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1;

public class BerSet : DerSet
{
	public new static readonly BerSet Empty = new BerSet();

	public new static BerSet FromVector(Asn1EncodableVector elementVector)
	{
		if (elementVector.Count >= 1)
		{
			return new BerSet(elementVector);
		}
		return Empty;
	}

	internal new static BerSet FromVector(Asn1EncodableVector elementVector, bool needsSorting)
	{
		if (elementVector.Count >= 1)
		{
			return new BerSet(elementVector, needsSorting);
		}
		return Empty;
	}

	public BerSet()
	{
	}

	public BerSet(Asn1Encodable element)
		: base(element)
	{
	}

	public BerSet(Asn1EncodableVector elementVector)
		: base(elementVector, needsSorting: false)
	{
	}

	internal BerSet(Asn1EncodableVector elementVector, bool needsSorting)
		: base(elementVector, needsSorting)
	{
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (derOut is Asn1OutputStream || derOut is BerOutputStream)
		{
			derOut.WriteByte(49);
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
