using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.IO;

public class MacStream : Stream
{
	protected readonly Stream stream;

	protected readonly IMac inMac;

	protected readonly IMac outMac;

	public override bool CanRead => stream.CanRead;

	public override bool CanWrite => stream.CanWrite;

	public override bool CanSeek => stream.CanSeek;

	public override long Length => stream.Length;

	public override long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			stream.Position = value;
		}
	}

	public MacStream(Stream stream, IMac readMac, IMac writeMac)
	{
		this.stream = stream;
		inMac = readMac;
		outMac = writeMac;
	}

	public virtual IMac ReadMac()
	{
		return inMac;
	}

	public virtual IMac WriteMac()
	{
		return outMac;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = stream.Read(buffer, offset, count);
		if (inMac != null && num > 0)
		{
			inMac.BlockUpdate(buffer, offset, num);
		}
		return num;
	}

	public override int ReadByte()
	{
		int num = stream.ReadByte();
		if (inMac != null && num >= 0)
		{
			inMac.Update((byte)num);
		}
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (outMac != null && count > 0)
		{
			outMac.BlockUpdate(buffer, offset, count);
		}
		stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte b)
	{
		if (outMac != null)
		{
			outMac.Update(b);
		}
		stream.WriteByte(b);
	}

	public override void Close()
	{
		Platform.Dispose(stream);
		base.Close();
	}

	public override void Flush()
	{
		stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long length)
	{
		stream.SetLength(length);
	}
}
