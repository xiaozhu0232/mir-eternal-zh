using System;
using System.Collections.Generic;
using System.IO;

namespace LumiSoft.Net.IO;

public class MultiStream : Stream
{
	private bool m_IsDisposed;

	private Queue<Stream> m_pStreams;

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
			return false;
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
			long num = 0L;
			Stream[] array = m_pStreams.ToArray();
			foreach (Stream stream in array)
			{
				num += stream.Length;
			}
			return num;
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
			throw new NotSupportedException();
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			throw new NotSupportedException();
		}
	}

	public MultiStream()
	{
		m_pStreams = new Queue<Stream>();
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pStreams = null;
			base.Dispose();
		}
	}

	public void AppendStream(Stream stream)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStreams.Enqueue(stream);
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
		throw new NotSupportedException();
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
		int num;
		while (true)
		{
			if (m_pStreams.Count == 0)
			{
				return 0;
			}
			num = m_pStreams.Peek().Read(buffer, offset, count);
			if (num != 0)
			{
				break;
			}
			m_pStreams.Dequeue();
		}
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
