using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

internal class TlsStream : Stream
{
	private readonly TlsProtocol handler;

	public override bool CanRead => !handler.IsClosed;

	public override bool CanSeek => false;

	public override bool CanWrite => !handler.IsClosed;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal TlsStream(TlsProtocol handler)
	{
		this.handler = handler;
	}

	public override void Close()
	{
		handler.Close();
		base.Close();
	}

	public override void Flush()
	{
		handler.Flush();
	}

	public override int Read(byte[] buf, int off, int len)
	{
		return handler.ReadApplicationData(buf, off, len);
	}

	public override int ReadByte()
	{
		byte[] array = new byte[1];
		if (Read(array, 0, 1) <= 0)
		{
			return -1;
		}
		return array[0];
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buf, int off, int len)
	{
		handler.WriteData(buf, off, len);
	}

	public override void WriteByte(byte b)
	{
		handler.WriteData(new byte[1] { b }, 0, 1);
	}
}
