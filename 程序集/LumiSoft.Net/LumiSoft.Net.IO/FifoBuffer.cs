using System;

namespace LumiSoft.Net.IO;

public class FifoBuffer
{
	private object m_pLock = new object();

	private byte[] m_pBuffer;

	private int m_ReadOffset;

	private int m_WriteOffset;

	public int MaxSize => m_pBuffer.Length;

	public int Available => m_WriteOffset - m_ReadOffset;

	public FifoBuffer(int maxSize)
	{
		if (maxSize < 1)
		{
			throw new ArgumentException("Argument 'maxSize' value must be >= 1.");
		}
		m_pBuffer = new byte[maxSize];
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' value must be >= 0.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is bigger than than argument 'buffer' can store.");
		}
		lock (m_pLock)
		{
			int num = Math.Min(count, m_WriteOffset - m_ReadOffset);
			if (num > 0)
			{
				Array.Copy(m_pBuffer, m_ReadOffset, buffer, offset, num);
				m_ReadOffset += num;
			}
			return num;
		}
	}

	public void Write(byte[] buffer, int offset, int count, bool ignoreBufferFull)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (count < 0 || count + offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		lock (m_pLock)
		{
			if (m_pBuffer.Length - m_WriteOffset < count)
			{
				TrimStart();
				if (m_pBuffer.Length - m_WriteOffset >= count)
				{
					Array.Copy(buffer, offset, m_pBuffer, m_WriteOffset, count);
					m_WriteOffset += count;
				}
				else if (!ignoreBufferFull)
				{
					throw new DataSizeExceededException();
				}
			}
			else
			{
				Array.Copy(buffer, offset, m_pBuffer, m_WriteOffset, count);
				m_WriteOffset += count;
			}
		}
	}

	public void Clear()
	{
		lock (m_pLock)
		{
			m_ReadOffset = 0;
			m_WriteOffset = 0;
		}
	}

	private void TrimStart()
	{
		if (m_ReadOffset > 0)
		{
			byte[] array = new byte[Available];
			Array.Copy(m_pBuffer, m_ReadOffset, array, 0, array.Length);
			Array.Copy(array, m_pBuffer, array.Length);
			m_ReadOffset = 0;
			m_WriteOffset = array.Length;
		}
	}
}
