using System;
using System.IO;

namespace LumiSoft.Net.IO;

public class ReadWriteControlledStream : Stream
{
	private Stream m_pStream;

	private FileAccess m_AccessMode = FileAccess.ReadWrite;

	public override bool CanRead => (m_AccessMode & FileAccess.Read) != 0;

	public override bool CanSeek => m_pStream.CanSeek;

	public override bool CanWrite => (m_AccessMode & FileAccess.Write) != 0;

	public override long Length => m_pStream.Length;

	public override long Position
	{
		get
		{
			return m_pStream.Position;
		}
		set
		{
			m_pStream.Position = value;
		}
	}

	public ReadWriteControlledStream(Stream stream, FileAccess access)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
		m_AccessMode = access;
	}

	public override void Flush()
	{
		m_pStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return m_pStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		m_pStream.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentException("Invalid argument 'offset' value.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentException("Invalid argument 'count' value.");
		}
		if ((m_AccessMode & FileAccess.Read) == 0)
		{
			throw new NotSupportedException();
		}
		return m_pStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentException("Invalid argument 'offset' value.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentException("Invalid argument 'count' value.");
		}
		if ((m_AccessMode & FileAccess.Write) == 0)
		{
			throw new NotSupportedException();
		}
		m_pStream.Write(buffer, offset, count);
	}
}
