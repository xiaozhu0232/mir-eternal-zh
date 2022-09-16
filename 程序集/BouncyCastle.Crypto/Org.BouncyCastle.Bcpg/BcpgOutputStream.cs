using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class BcpgOutputStream : BaseOutputStream
{
	private const int BufferSizePower = 16;

	private Stream outStr;

	private byte[] partialBuffer;

	private int partialBufferLength;

	private int partialPower;

	private int partialOffset;

	internal static BcpgOutputStream Wrap(Stream outStr)
	{
		if (outStr is BcpgOutputStream)
		{
			return (BcpgOutputStream)outStr;
		}
		return new BcpgOutputStream(outStr);
	}

	public BcpgOutputStream(Stream outStr)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		this.outStr = outStr;
	}

	public BcpgOutputStream(Stream outStr, PacketTag tag)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		this.outStr = outStr;
		WriteHeader(tag, oldPackets: true, partial: true, 0L);
	}

	public BcpgOutputStream(Stream outStr, PacketTag tag, long length, bool oldFormat)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		this.outStr = outStr;
		if (length > uint.MaxValue)
		{
			WriteHeader(tag, oldPackets: false, partial: true, 0L);
			partialBufferLength = 65536;
			partialBuffer = new byte[partialBufferLength];
			partialPower = 16;
			partialOffset = 0;
		}
		else
		{
			WriteHeader(tag, oldFormat, partial: false, length);
		}
	}

	public BcpgOutputStream(Stream outStr, PacketTag tag, long length)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		this.outStr = outStr;
		WriteHeader(tag, oldPackets: false, partial: false, length);
	}

	public BcpgOutputStream(Stream outStr, PacketTag tag, byte[] buffer)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		this.outStr = outStr;
		WriteHeader(tag, oldPackets: false, partial: true, 0L);
		partialBuffer = buffer;
		uint num = (uint)partialBuffer.Length;
		partialPower = 0;
		while (num != 1)
		{
			num >>= 1;
			partialPower++;
		}
		if (partialPower > 30)
		{
			throw new IOException("Buffer cannot be greater than 2^30 in length.");
		}
		partialBufferLength = 1 << partialPower;
		partialOffset = 0;
	}

	private void WriteNewPacketLength(long bodyLen)
	{
		if (bodyLen < 192)
		{
			outStr.WriteByte((byte)bodyLen);
		}
		else if (bodyLen <= 8383)
		{
			bodyLen -= 192;
			outStr.WriteByte((byte)(((bodyLen >> 8) & 0xFF) + 192));
			outStr.WriteByte((byte)bodyLen);
		}
		else
		{
			outStr.WriteByte(byte.MaxValue);
			outStr.WriteByte((byte)(bodyLen >> 24));
			outStr.WriteByte((byte)(bodyLen >> 16));
			outStr.WriteByte((byte)(bodyLen >> 8));
			outStr.WriteByte((byte)bodyLen);
		}
	}

	private void WriteHeader(PacketTag tag, bool oldPackets, bool partial, long bodyLen)
	{
		int num = 128;
		if (partialBuffer != null)
		{
			PartialFlush(isLast: true);
			partialBuffer = null;
		}
		if (oldPackets)
		{
			num |= (int)tag << 2;
			if (partial)
			{
				WriteByte((byte)((uint)num | 3u));
			}
			else if (bodyLen <= 255)
			{
				WriteByte((byte)num);
				WriteByte((byte)bodyLen);
			}
			else if (bodyLen <= 65535)
			{
				WriteByte((byte)((uint)num | 1u));
				WriteByte((byte)(bodyLen >> 8));
				WriteByte((byte)bodyLen);
			}
			else
			{
				WriteByte((byte)((uint)num | 2u));
				WriteByte((byte)(bodyLen >> 24));
				WriteByte((byte)(bodyLen >> 16));
				WriteByte((byte)(bodyLen >> 8));
				WriteByte((byte)bodyLen);
			}
		}
		else
		{
			num |= (int)((PacketTag)64 | tag);
			WriteByte((byte)num);
			if (partial)
			{
				partialOffset = 0;
			}
			else
			{
				WriteNewPacketLength(bodyLen);
			}
		}
	}

	private void PartialFlush(bool isLast)
	{
		if (isLast)
		{
			WriteNewPacketLength(partialOffset);
			outStr.Write(partialBuffer, 0, partialOffset);
		}
		else
		{
			outStr.WriteByte((byte)(0xE0u | (uint)partialPower));
			outStr.Write(partialBuffer, 0, partialBufferLength);
		}
		partialOffset = 0;
	}

	private void WritePartial(byte b)
	{
		if (partialOffset == partialBufferLength)
		{
			PartialFlush(isLast: false);
		}
		partialBuffer[partialOffset++] = b;
	}

	private void WritePartial(byte[] buffer, int off, int len)
	{
		if (partialOffset == partialBufferLength)
		{
			PartialFlush(isLast: false);
		}
		if (len <= partialBufferLength - partialOffset)
		{
			Array.Copy(buffer, off, partialBuffer, partialOffset, len);
			partialOffset += len;
			return;
		}
		int num = partialBufferLength - partialOffset;
		Array.Copy(buffer, off, partialBuffer, partialOffset, num);
		off += num;
		len -= num;
		PartialFlush(isLast: false);
		while (len > partialBufferLength)
		{
			Array.Copy(buffer, off, partialBuffer, 0, partialBufferLength);
			off += partialBufferLength;
			len -= partialBufferLength;
			PartialFlush(isLast: false);
		}
		Array.Copy(buffer, off, partialBuffer, 0, len);
		partialOffset += len;
	}

	public override void WriteByte(byte value)
	{
		if (partialBuffer != null)
		{
			WritePartial(value);
		}
		else
		{
			outStr.WriteByte(value);
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (partialBuffer != null)
		{
			WritePartial(buffer, offset, count);
		}
		else
		{
			outStr.Write(buffer, offset, count);
		}
	}

	internal virtual void WriteShort(short n)
	{
		Write((byte)(n >> 8), (byte)n);
	}

	internal virtual void WriteInt(int n)
	{
		Write((byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n);
	}

	internal virtual void WriteLong(long n)
	{
		Write((byte)(n >> 56), (byte)(n >> 48), (byte)(n >> 40), (byte)(n >> 32), (byte)(n >> 24), (byte)(n >> 16), (byte)(n >> 8), (byte)n);
	}

	public void WritePacket(ContainedPacket p)
	{
		p.Encode(this);
	}

	internal void WritePacket(PacketTag tag, byte[] body, bool oldFormat)
	{
		WriteHeader(tag, oldFormat, partial: false, body.Length);
		Write(body);
	}

	public void WriteObject(BcpgObject bcpgObject)
	{
		bcpgObject.Encode(this);
	}

	public void WriteObjects(params BcpgObject[] v)
	{
		foreach (BcpgObject bcpgObject in v)
		{
			bcpgObject.Encode(this);
		}
	}

	public override void Flush()
	{
		outStr.Flush();
	}

	public void Finish()
	{
		if (partialBuffer != null)
		{
			PartialFlush(isLast: true);
			Array.Clear(partialBuffer, 0, partialBuffer.Length);
			partialBuffer = null;
		}
	}

	public override void Close()
	{
		Finish();
		outStr.Flush();
		Platform.Dispose(outStr);
		base.Close();
	}
}
