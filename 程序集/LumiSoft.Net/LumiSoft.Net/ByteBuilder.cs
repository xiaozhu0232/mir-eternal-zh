using System;
using System.Text;

namespace LumiSoft.Net;

public class ByteBuilder
{
	private int m_BlockSize = 1024;

	private byte[] m_pBuffer;

	private int m_Count;

	private Encoding m_pCharset;

	public int Count => m_Count;

	public Encoding Charset
	{
		get
		{
			return m_pCharset;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pCharset = value;
		}
	}

	public ByteBuilder()
	{
		m_pBuffer = new byte[m_BlockSize];
		m_pCharset = Encoding.UTF8;
	}

	public void Append(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Append(m_pCharset.GetBytes(value));
	}

	public void Append(Encoding charset, string value)
	{
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Append(charset.GetBytes(value));
	}

	public void Append(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Append(value, 0, value.Length);
	}

	public void Append(byte[] value, int offset, int count)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		while (m_pBuffer.Length - m_Count < count)
		{
			byte[] array = new byte[m_pBuffer.Length + m_BlockSize];
			Array.Copy(m_pBuffer, array, m_Count);
			m_pBuffer = array;
		}
		Array.Copy(value, offset, m_pBuffer, m_Count, count);
		m_Count += value.Length;
	}

	public byte[] ToByte()
	{
		byte[] array = new byte[m_Count];
		Array.Copy(m_pBuffer, array, m_Count);
		return array;
	}
}
