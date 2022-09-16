using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class BerOctetString : DerOctetString, IEnumerable
{
	private class ChunkEnumerator : IEnumerator
	{
		private readonly byte[] octets;

		private readonly int chunkSize;

		private DerOctetString currentChunk = null;

		private int nextChunkPos = 0;

		public object Current
		{
			get
			{
				if (currentChunk == null)
				{
					throw new InvalidOperationException();
				}
				return currentChunk;
			}
		}

		internal ChunkEnumerator(byte[] octets, int chunkSize)
		{
			this.octets = octets;
			this.chunkSize = chunkSize;
		}

		public bool MoveNext()
		{
			if (nextChunkPos >= octets.Length)
			{
				currentChunk = null;
				return false;
			}
			int num = System.Math.Min(octets.Length - nextChunkPos, chunkSize);
			byte[] array = new byte[num];
			Array.Copy(octets, nextChunkPos, array, 0, num);
			currentChunk = new DerOctetString(array);
			nextChunkPos += num;
			return true;
		}

		public void Reset()
		{
			currentChunk = null;
			nextChunkPos = 0;
		}
	}

	private static readonly int DefaultChunkSize = 1000;

	private readonly int chunkSize;

	private readonly Asn1OctetString[] octs;

	public static BerOctetString FromSequence(Asn1Sequence seq)
	{
		int count = seq.Count;
		Asn1OctetString[] array = new Asn1OctetString[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = Asn1OctetString.GetInstance(seq[i]);
		}
		return new BerOctetString(array);
	}

	private static byte[] ToBytes(Asn1OctetString[] octs)
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (Asn1OctetString asn1OctetString in octs)
		{
			byte[] octets = asn1OctetString.GetOctets();
			memoryStream.Write(octets, 0, octets.Length);
		}
		return memoryStream.ToArray();
	}

	private static Asn1OctetString[] ToOctetStringArray(IEnumerable e)
	{
		IList list = Platform.CreateArrayList(e);
		int count = list.Count;
		Asn1OctetString[] array = new Asn1OctetString[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = Asn1OctetString.GetInstance(list[i]);
		}
		return array;
	}

	[Obsolete("Will be removed")]
	public BerOctetString(IEnumerable e)
		: this(ToOctetStringArray(e))
	{
	}

	public BerOctetString(byte[] str)
		: this(str, DefaultChunkSize)
	{
	}

	public BerOctetString(Asn1OctetString[] octs)
		: this(octs, DefaultChunkSize)
	{
	}

	public BerOctetString(byte[] str, int chunkSize)
		: this(str, null, chunkSize)
	{
	}

	public BerOctetString(Asn1OctetString[] octs, int chunkSize)
		: this(ToBytes(octs), octs, chunkSize)
	{
	}

	private BerOctetString(byte[] str, Asn1OctetString[] octs, int chunkSize)
		: base(str)
	{
		this.octs = octs;
		this.chunkSize = chunkSize;
	}

	public IEnumerator GetEnumerator()
	{
		if (octs == null)
		{
			return new ChunkEnumerator(str, chunkSize);
		}
		return octs.GetEnumerator();
	}

	[Obsolete("Use GetEnumerator() instead")]
	public IEnumerator GetObjects()
	{
		return GetEnumerator();
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (derOut is Asn1OutputStream || derOut is BerOutputStream)
		{
			derOut.WriteByte(36);
			derOut.WriteByte(128);
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Asn1OctetString obj = (Asn1OctetString)enumerator.Current;
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
