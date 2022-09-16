using System;
using System.IO;

namespace LumiSoft.Net.IO;

public class MemoryStreamEx : Stream
{
	private static int m_DefaultMemorySize = 64000;

	private bool m_IsDisposed;

	private Stream m_pStream;

	private int m_MaxMemSize = 64000;

	public static int DefaultMemorySize
	{
		get
		{
			return m_DefaultMemorySize;
		}
		set
		{
			if (value < 32000)
			{
				throw new ArgumentException("Property 'DefaultMemorySize' value must be >= 32k.", "value");
			}
			m_DefaultMemorySize = value;
		}
	}

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
			return true;
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
			return m_pStream.Length;
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
			return m_pStream.Position;
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
			m_pStream.Position = value;
		}
	}

	public MemoryStreamEx()
		: this(m_DefaultMemorySize)
	{
	}

	public MemoryStreamEx(int memSize)
	{
		m_MaxMemSize = memSize;
		m_pStream = new MemoryStream();
	}

	~MemoryStreamEx()
	{
		Dispose();
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			if (m_pStream != null)
			{
				m_pStream.Close();
			}
			m_pStream = null;
			base.Dispose();
		}
	}

	public override void Flush()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		m_pStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		return m_pStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		m_pStream.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		return m_pStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (m_pStream is MemoryStream && m_pStream.Position + count > m_MaxMemSize)
		{
			FileStream fileStream = new FileStream(Path.GetTempPath() + "ls-" + Guid.NewGuid().ToString().Replace("-", "") + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 32000, FileOptions.DeleteOnClose);
			m_pStream.Position = 0L;
			Net_Utils.StreamCopy(m_pStream, fileStream, 8000);
			m_pStream.Close();
			m_pStream = fileStream;
		}
		m_pStream.Write(buffer, offset, count);
	}
}
