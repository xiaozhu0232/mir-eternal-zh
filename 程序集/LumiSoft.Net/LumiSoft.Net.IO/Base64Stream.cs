using System;
using System.IO;

namespace LumiSoft.Net.IO;

public class Base64Stream : Stream, IDisposable
{
	private static readonly byte[] BASE64_ENCODE_TABLE = new byte[64]
	{
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
		75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
		85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
		101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
		111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
		121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
		56, 57, 43, 47
	};

	private static readonly short[] BASE64_DECODE_TABLE = new short[128]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
		54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
		-1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
		5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
		15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
		25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
		29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
		39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
		49, 50, 51, -1, -1, -1, -1, -1
	};

	private bool m_IsDisposed;

	private bool m_IsFinished;

	private Stream m_pStream;

	private bool m_IsOwner;

	private bool m_AddLineBreaks = true;

	private FileAccess m_AccessMode = FileAccess.ReadWrite;

	private int m_EncodeBufferOffset;

	private int m_OffsetInEncode3x8Block;

	private byte[] m_pEncode3x8Block = new byte[3];

	private byte[] m_pEncodeBuffer = new byte[78];

	private byte[] m_pDecodedBlock;

	private int m_DecodedBlockOffset;

	private int m_DecodedBlockCount;

	private Base64 m_pBase64;

	private bool m_IgnoreInvalidPadding;

	public bool IsDisposed => m_IsDisposed;

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
			throw new NotSupportedException();
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

	public bool IgnoreInvalidPadding
	{
		get
		{
			return m_IgnoreInvalidPadding;
		}
		set
		{
			m_IgnoreInvalidPadding = value;
		}
	}

	public Base64Stream(Stream stream, bool owner, bool addLineBreaks)
		: this(stream, owner, addLineBreaks, FileAccess.ReadWrite)
	{
	}

	public Base64Stream(Stream stream, bool owner, bool addLineBreaks, FileAccess access)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
		m_IsOwner = owner;
		m_AddLineBreaks = addLineBreaks;
		m_AccessMode = access;
		m_pDecodedBlock = new byte[8000];
		m_pBase64 = new Base64();
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			try
			{
				Finish();
			}
			catch
			{
			}
			m_IsDisposed = true;
			if (m_IsOwner)
			{
				m_pStream.Close();
			}
		}
	}

	public override void Flush()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("Base64Stream");
		}
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
		if ((m_AccessMode & FileAccess.Read) == 0)
		{
			throw new NotSupportedException();
		}
		if (m_DecodedBlockCount - m_DecodedBlockOffset == 0)
		{
			byte[] array = new byte[m_pDecodedBlock.Length + 3];
			int num = m_pStream.Read(array, 0, array.Length - 3);
			if (num == 0)
			{
				return 0;
			}
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				byte b = array[i];
				if (b == 61 || BASE64_DECODE_TABLE[b] != -1)
				{
					num2++;
				}
			}
			while (num2 % 4 != 0)
			{
				int num3 = m_pStream.ReadByte();
				if (num3 == -1)
				{
					if (!m_IgnoreInvalidPadding)
					{
						break;
					}
					if (num2 % 4 == 1)
					{
						array[num++] = 65;
						num2++;
					}
					else
					{
						array[num++] = 61;
						num2++;
					}
				}
				else if (num3 == 61 || BASE64_DECODE_TABLE[num3] != -1)
				{
					array[num++] = (byte)num3;
					num2++;
				}
			}
			m_DecodedBlockCount = m_pBase64.Decode(array, 0, num, m_pDecodedBlock, 0, ignoreNonBase64Chars: true);
			m_DecodedBlockOffset = 0;
		}
		int val = m_DecodedBlockCount - m_DecodedBlockOffset;
		int num4 = Math.Min(count, val);
		Array.Copy(m_pDecodedBlock, m_DecodedBlockOffset, buffer, offset, num4);
		m_DecodedBlockOffset += num4;
		return num4;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_IsFinished)
		{
			throw new InvalidOperationException("Stream is marked as finished by calling Finish method.");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentException("Invalid argument 'offset' value.");
		}
		if (count < 0 || count > buffer.Length - offset)
		{
			throw new ArgumentException("Invalid argument 'count' value.");
		}
		if ((m_AccessMode & FileAccess.Write) == 0)
		{
			throw new NotSupportedException();
		}
		int num = m_pEncodeBuffer.Length;
		for (int i = 0; i < count; i++)
		{
			m_pEncode3x8Block[m_OffsetInEncode3x8Block++] = buffer[offset + i];
			if (m_OffsetInEncode3x8Block != 3)
			{
				continue;
			}
			m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
			m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[((m_pEncode3x8Block[0] & 3) << 4) | (m_pEncode3x8Block[1] >> 4)];
			m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[((m_pEncode3x8Block[1] & 0xF) << 2) | (m_pEncode3x8Block[2] >> 6)];
			m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[2] & 0x3F];
			if (m_EncodeBufferOffset >= num - 2)
			{
				if (m_AddLineBreaks)
				{
					m_pEncodeBuffer[m_EncodeBufferOffset++] = 13;
					m_pEncodeBuffer[m_EncodeBufferOffset++] = 10;
				}
				m_pStream.Write(m_pEncodeBuffer, 0, m_EncodeBufferOffset);
				m_EncodeBufferOffset = 0;
			}
			m_OffsetInEncode3x8Block = 0;
		}
	}

	public void Finish()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsFinished)
		{
			m_IsFinished = true;
			if (m_OffsetInEncode3x8Block == 1)
			{
				m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
				m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[(m_pEncode3x8Block[0] & 3) << 4];
				m_pEncodeBuffer[m_EncodeBufferOffset++] = 61;
				m_pEncodeBuffer[m_EncodeBufferOffset++] = 61;
			}
			else if (m_OffsetInEncode3x8Block == 2)
			{
				m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[m_pEncode3x8Block[0] >> 2];
				m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[((m_pEncode3x8Block[0] & 3) << 4) | (m_pEncode3x8Block[1] >> 4)];
				m_pEncodeBuffer[m_EncodeBufferOffset++] = BASE64_ENCODE_TABLE[(m_pEncode3x8Block[1] & 0xF) << 2];
				m_pEncodeBuffer[m_EncodeBufferOffset++] = 61;
			}
			if (m_EncodeBufferOffset > 0)
			{
				m_pStream.Write(m_pEncodeBuffer, 0, m_EncodeBufferOffset);
			}
		}
	}
}
