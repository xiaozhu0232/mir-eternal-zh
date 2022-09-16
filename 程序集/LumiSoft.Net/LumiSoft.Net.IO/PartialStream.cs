using System;
using System.IO;

namespace LumiSoft.Net.IO;

public class PartialStream : Stream
{
	private bool m_IsDisposed;

	private Stream m_pStream;

	private long m_Start;

	private long m_Length;

	private long m_Position;

	public override bool CanRead
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return true;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return true;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_Length;
		}
	}

	public override long Position
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_Position;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			if (value < 0 || value > Length)
			{
				throw new ArgumentException("Property 'Position' value must be >= 0 and <= this.Length.");
			}
			m_Position = value;
		}
	}

	public PartialStream(Stream stream, long start, long length)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanSeek)
		{
			throw new ArgumentException("Argument 'stream' does not support seeking.");
		}
		if (start < 0)
		{
			throw new ArgumentException("Argument 'start' value must be >= 0.");
		}
		if (start + length > stream.Length)
		{
			throw new ArgumentException("Argument 'length' value will exceed source stream length.");
		}
		m_pStream = stream;
		m_Start = start;
		m_Length = length;
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			base.Dispose();
		}
	}

	public override void Flush()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		switch (origin)
		{
		case SeekOrigin.Begin:
			m_Position = 0L;
			break;
		case SeekOrigin.End:
			m_Position = m_Length;
			break;
		}
		return m_Position;
	}

	public override void SetLength(long value)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		if (m_pStream.Position != m_Start + m_Position)
		{
			m_pStream.Position = m_Start + m_Position;
		}
		int num = m_pStream.Read(buffer, offset, Math.Min(count, (int)(Length - m_Position)));
		m_Position += num;
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		throw new NotSupportedException();
	}
}
