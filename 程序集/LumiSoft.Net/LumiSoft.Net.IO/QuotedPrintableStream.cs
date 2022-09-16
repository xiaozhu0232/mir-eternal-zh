using System;
using System.Globalization;
using System.IO;

namespace LumiSoft.Net.IO;

public class QuotedPrintableStream : Stream
{
	private SmartStream m_pStream;

	private FileAccess m_AccessMode = FileAccess.ReadWrite;

	private byte[] m_pDecodedBuffer;

	private int m_DecodedOffset;

	private int m_DecodedCount;

	private byte[] m_pEncodedBuffer;

	private int m_EncodedCount;

	public override bool CanRead => (m_AccessMode & FileAccess.Read) != 0;

	public override bool CanSeek => false;

	public override bool CanWrite => (m_AccessMode & FileAccess.Write) != 0;

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

	public QuotedPrintableStream(SmartStream stream, FileAccess access)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
		m_AccessMode = access;
		m_pDecodedBuffer = new byte[32000];
		m_pEncodedBuffer = new byte[78];
	}

	public override void Flush()
	{
		if (m_EncodedCount > 0)
		{
			m_pStream.Write(m_pEncodedBuffer, 0, m_EncodedCount);
			m_EncodedCount = 0;
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
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
		do
		{
			if (m_DecodedOffset < m_DecodedCount)
			{
				continue;
			}
			m_DecodedOffset = 0;
			m_DecodedCount = 0;
			SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.ThrowException);
			m_pStream.ReadLine(readLineAsyncOP, async: false);
			if (readLineAsyncOP.Error != null)
			{
				throw readLineAsyncOP.Error;
			}
			if (readLineAsyncOP.BytesInBuffer == 0)
			{
				return 0;
			}
			bool flag = false;
			int lineBytesInBuffer = readLineAsyncOP.LineBytesInBuffer;
			for (int i = 0; i < readLineAsyncOP.LineBytesInBuffer; i++)
			{
				byte b = readLineAsyncOP.Buffer[i];
				if (b == 61 && i == lineBytesInBuffer - 1)
				{
					flag = true;
				}
				else if (b == 61)
				{
					byte b2 = readLineAsyncOP.Buffer[++i];
					byte b3 = readLineAsyncOP.Buffer[++i];
					byte result = 0;
					if (byte.TryParse(new string(new char[2]
					{
						(char)b2,
						(char)b3
					}), NumberStyles.HexNumber, null, out result))
					{
						m_pDecodedBuffer[m_DecodedCount++] = result;
						continue;
					}
					m_pDecodedBuffer[m_DecodedCount++] = 61;
					m_pDecodedBuffer[m_DecodedCount++] = b2;
					m_pDecodedBuffer[m_DecodedCount++] = b3;
				}
				else
				{
					m_pDecodedBuffer[m_DecodedCount++] = b;
				}
			}
			if (readLineAsyncOP.LineBytesInBuffer != readLineAsyncOP.BytesInBuffer && !flag)
			{
				m_pDecodedBuffer[m_DecodedCount++] = 13;
				m_pDecodedBuffer[m_DecodedCount++] = 10;
			}
		}
		while (m_DecodedOffset >= m_DecodedCount);
		int num = Math.Min(count, m_DecodedCount - m_DecodedOffset);
		Array.Copy(m_pDecodedBuffer, m_DecodedOffset, buffer, offset, num);
		m_DecodedOffset += num;
		return num;
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
		for (int i = 0; i < count; i++)
		{
			byte b = buffer[offset + i];
			if ((b >= 33 && b <= 60) || (b >= 62 && b <= 126))
			{
				if (m_EncodedCount >= 75)
				{
					m_pEncodedBuffer[m_EncodedCount++] = 61;
					m_pEncodedBuffer[m_EncodedCount++] = 13;
					m_pEncodedBuffer[m_EncodedCount++] = 10;
					Flush();
				}
				m_pEncodedBuffer[m_EncodedCount++] = b;
				continue;
			}
			if (m_EncodedCount >= 73)
			{
				m_pEncodedBuffer[m_EncodedCount++] = 61;
				m_pEncodedBuffer[m_EncodedCount++] = 13;
				m_pEncodedBuffer[m_EncodedCount++] = 10;
				Flush();
			}
			m_pEncodedBuffer[m_EncodedCount++] = 61;
			m_pEncodedBuffer[m_EncodedCount++] = (byte)(b >> 4).ToString("X")[0];
			m_pEncodedBuffer[m_EncodedCount++] = (byte)(b & 0xF).ToString("X")[0];
		}
	}
}
