using System;
using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

public class DerOutputStream : FilterStream
{
	public DerOutputStream(Stream os)
		: base(os)
	{
	}

	private void WriteLength(int length)
	{
		if (length > 127)
		{
			int num = 1;
			uint num2 = (uint)length;
			while ((num2 >>= 8) != 0)
			{
				num++;
			}
			WriteByte((byte)((uint)num | 0x80u));
			for (int num3 = (num - 1) * 8; num3 >= 0; num3 -= 8)
			{
				WriteByte((byte)(length >> num3));
			}
		}
		else
		{
			WriteByte((byte)length);
		}
	}

	internal void WriteEncoded(int tag, byte[] bytes)
	{
		WriteByte((byte)tag);
		WriteLength(bytes.Length);
		Write(bytes, 0, bytes.Length);
	}

	internal void WriteEncoded(int tag, byte first, byte[] bytes)
	{
		WriteByte((byte)tag);
		WriteLength(bytes.Length + 1);
		WriteByte(first);
		Write(bytes, 0, bytes.Length);
	}

	internal void WriteEncoded(int tag, byte[] bytes, int offset, int length)
	{
		WriteByte((byte)tag);
		WriteLength(length);
		Write(bytes, offset, length);
	}

	internal void WriteTag(int flags, int tagNo)
	{
		if (tagNo < 31)
		{
			WriteByte((byte)(flags | tagNo));
			return;
		}
		WriteByte((byte)((uint)flags | 0x1Fu));
		if (tagNo < 128)
		{
			WriteByte((byte)tagNo);
			return;
		}
		byte[] array = new byte[5];
		int num = array.Length;
		array[--num] = (byte)((uint)tagNo & 0x7Fu);
		do
		{
			tagNo >>= 7;
			array[--num] = (byte)(((uint)tagNo & 0x7Fu) | 0x80u);
		}
		while (tagNo > 127);
		Write(array, num, array.Length - num);
	}

	internal void WriteEncoded(int flags, int tagNo, byte[] bytes)
	{
		WriteTag(flags, tagNo);
		WriteLength(bytes.Length);
		Write(bytes, 0, bytes.Length);
	}

	protected void WriteNull()
	{
		WriteByte(5);
		WriteByte(0);
	}

	[Obsolete("Use version taking an Asn1Encodable arg instead")]
	public virtual void WriteObject(object obj)
	{
		if (obj == null)
		{
			WriteNull();
			return;
		}
		if (obj is Asn1Object)
		{
			((Asn1Object)obj).Encode(this);
			return;
		}
		if (obj is Asn1Encodable)
		{
			((Asn1Encodable)obj).ToAsn1Object().Encode(this);
			return;
		}
		throw new IOException("object not Asn1Object");
	}

	public virtual void WriteObject(Asn1Encodable obj)
	{
		if (obj == null)
		{
			WriteNull();
		}
		else
		{
			obj.ToAsn1Object().Encode(this);
		}
	}

	public virtual void WriteObject(Asn1Object obj)
	{
		if (obj == null)
		{
			WriteNull();
		}
		else
		{
			obj.Encode(this);
		}
	}
}
