using System;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Utilities;

[Obsolete("Use Org.BouncyCastle.Utilities.IO.FilterStream")]
public class FilterStream : Stream
{
	protected readonly Stream s;

	public override bool CanRead => s.CanRead;

	public override bool CanSeek => s.CanSeek;

	public override bool CanWrite => s.CanWrite;

	public override long Length => s.Length;

	public override long Position
	{
		get
		{
			return s.Position;
		}
		set
		{
			s.Position = value;
		}
	}

	[Obsolete("Use Org.BouncyCastle.Utilities.IO.FilterStream")]
	public FilterStream(Stream s)
	{
		this.s = s;
	}

	public override void Close()
	{
		Platform.Dispose(s);
		base.Close();
	}

	public override void Flush()
	{
		s.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return s.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		s.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return s.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		return s.ReadByte();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		s.Write(buffer, offset, count);
	}

	public override void WriteByte(byte value)
	{
		s.WriteByte(value);
	}
}
