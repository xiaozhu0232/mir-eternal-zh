using System;
using System.IO;
using System.Text;

namespace LumiSoft.Net;

public class StreamLineReader
{
	private Stream m_StrmSource;

	private string m_Encoding = "";

	private bool m_CRLF_LinesOnly = true;

	private int m_ReadBufferSize = 1024;

	private byte[] m_Buffer = new byte[1024];

	public string Encoding
	{
		get
		{
			return m_Encoding;
		}
		set
		{
			System.Text.Encoding.GetEncoding(value);
			m_Encoding = value;
		}
	}

	public bool CRLF_LinesOnly
	{
		get
		{
			return m_CRLF_LinesOnly;
		}
		set
		{
			m_CRLF_LinesOnly = value;
		}
	}

	public StreamLineReader(Stream strmSource)
	{
		m_StrmSource = strmSource;
	}

	public byte[] ReadLine()
	{
		byte[] array = m_Buffer;
		int num = 0;
		int num2 = m_StrmSource.ReadByte();
		int num3 = m_StrmSource.ReadByte();
		while (num2 > -1)
		{
			if (num2 == 13 && (byte)num3 == 10)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, array2, num);
				return array2;
			}
			if (!m_CRLF_LinesOnly && num3 == 10)
			{
				byte[] array3 = new byte[num + 1];
				Array.Copy(array, array3, num + 1);
				array3[num] = (byte)num2;
				return array3;
			}
			if (num == array.Length)
			{
				byte[] array4 = new byte[array.Length + m_ReadBufferSize];
				Array.Copy(array, array4, array.Length);
				array = array4;
			}
			array[num] = (byte)num2;
			num++;
			num2 = num3;
			num3 = m_StrmSource.ReadByte();
		}
		if (num > 0)
		{
			byte[] array5 = new byte[num];
			Array.Copy(array, array5, num);
			return array5;
		}
		return null;
	}

	public string ReadLineString()
	{
		byte[] array = ReadLine();
		if (array != null)
		{
			if (m_Encoding == null || m_Encoding == "")
			{
				return System.Text.Encoding.Default.GetString(array);
			}
			return System.Text.Encoding.GetEncoding(m_Encoding).GetString(array);
		}
		return null;
	}
}
