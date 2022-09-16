using System.Collections;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class ArmoredInputStream : BaseInputStream
{
	private static readonly byte[] decodingTable;

	private Stream input;

	private bool start = true;

	private int[] outBuf = new int[3];

	private int bufPtr = 3;

	private Crc24 crc = new Crc24();

	private bool crcFound = false;

	private bool hasHeaders = true;

	private string header = null;

	private bool newLineFound = false;

	private bool clearText = false;

	private bool restart = false;

	private IList headerList = Platform.CreateArrayList();

	private int lastC = 0;

	private bool isEndOfStream;

	static ArmoredInputStream()
	{
		decodingTable = new byte[128];
		for (int i = 65; i <= 90; i++)
		{
			decodingTable[i] = (byte)(i - 65);
		}
		for (int j = 97; j <= 122; j++)
		{
			decodingTable[j] = (byte)(j - 97 + 26);
		}
		for (int k = 48; k <= 57; k++)
		{
			decodingTable[k] = (byte)(k - 48 + 52);
		}
		decodingTable[43] = 62;
		decodingTable[47] = 63;
	}

	private int Decode(int in0, int in1, int in2, int in3, int[] result)
	{
		if (in3 < 0)
		{
			throw new EndOfStreamException("unexpected end of file in armored stream.");
		}
		int num;
		int num2;
		if (in2 == 61)
		{
			num = decodingTable[in0] & 0xFF;
			num2 = decodingTable[in1] & 0xFF;
			result[2] = ((num << 2) | (num2 >> 4)) & 0xFF;
			return 2;
		}
		int num3;
		if (in3 == 61)
		{
			num = decodingTable[in0];
			num2 = decodingTable[in1];
			num3 = decodingTable[in2];
			result[1] = ((num << 2) | (num2 >> 4)) & 0xFF;
			result[2] = ((num2 << 4) | (num3 >> 2)) & 0xFF;
			return 1;
		}
		num = decodingTable[in0];
		num2 = decodingTable[in1];
		num3 = decodingTable[in2];
		int num4 = decodingTable[in3];
		result[0] = ((num << 2) | (num2 >> 4)) & 0xFF;
		result[1] = ((num2 << 4) | (num3 >> 2)) & 0xFF;
		result[2] = ((num3 << 6) | num4) & 0xFF;
		return 0;
	}

	public ArmoredInputStream(Stream input)
		: this(input, hasHeaders: true)
	{
	}

	public ArmoredInputStream(Stream input, bool hasHeaders)
	{
		this.input = input;
		this.hasHeaders = hasHeaders;
		if (hasHeaders)
		{
			ParseHeaders();
		}
		start = false;
	}

	private bool ParseHeaders()
	{
		header = null;
		int num = 0;
		bool flag = false;
		headerList = Platform.CreateArrayList();
		if (restart)
		{
			flag = true;
		}
		else
		{
			int num2;
			while ((num2 = input.ReadByte()) >= 0)
			{
				if (num2 == 45 && (num == 0 || num == 10 || num == 13))
				{
					flag = true;
					break;
				}
				num = num2;
			}
		}
		if (flag)
		{
			StringBuilder stringBuilder = new StringBuilder("-");
			bool flag2 = false;
			bool flag3 = false;
			if (restart)
			{
				stringBuilder.Append('-');
			}
			int num2;
			while ((num2 = input.ReadByte()) >= 0)
			{
				if (num == 13 && num2 == 10)
				{
					flag3 = true;
				}
				if ((flag2 && num != 13 && num2 == 10) || (flag2 && num2 == 13))
				{
					break;
				}
				if (num2 == 13 || (num != 13 && num2 == 10))
				{
					string text = stringBuilder.ToString();
					if (text.Trim().Length < 1)
					{
						break;
					}
					headerList.Add(text);
					stringBuilder.Length = 0;
				}
				if (num2 != 10 && num2 != 13)
				{
					stringBuilder.Append((char)num2);
					flag2 = false;
				}
				else if (num2 == 13 || (num != 13 && num2 == 10))
				{
					flag2 = true;
				}
				num = num2;
			}
			if (flag3)
			{
				input.ReadByte();
			}
		}
		if (headerList.Count > 0)
		{
			header = (string)headerList[0];
		}
		clearText = "-----BEGIN PGP SIGNED MESSAGE-----".Equals(header);
		newLineFound = true;
		return flag;
	}

	public bool IsClearText()
	{
		return clearText;
	}

	public bool IsEndOfStream()
	{
		return isEndOfStream;
	}

	public string GetArmorHeaderLine()
	{
		return header;
	}

	public string[] GetArmorHeaders()
	{
		if (headerList.Count <= 1)
		{
			return null;
		}
		string[] array = new string[headerList.Count - 1];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = (string)headerList[i + 1];
		}
		return array;
	}

	private int ReadIgnoreSpace()
	{
		int num;
		do
		{
			num = input.ReadByte();
		}
		while (num == 32 || num == 9);
		return num;
	}

	private int ReadIgnoreWhitespace()
	{
		int num;
		do
		{
			num = input.ReadByte();
		}
		while (num == 32 || num == 9 || num == 13 || num == 10);
		return num;
	}

	private int ReadByteClearText()
	{
		int num = input.ReadByte();
		if (num == 13 || (num == 10 && lastC != 13))
		{
			newLineFound = true;
		}
		else if (newLineFound && num == 45)
		{
			num = input.ReadByte();
			if (num == 45)
			{
				clearText = false;
				start = true;
				restart = true;
			}
			else
			{
				num = input.ReadByte();
			}
			newLineFound = false;
		}
		else if (num != 10 && lastC != 13)
		{
			newLineFound = false;
		}
		lastC = num;
		if (num < 0)
		{
			isEndOfStream = true;
		}
		return num;
	}

	private int ReadClearText(byte[] buffer, int offset, int count)
	{
		int num = offset;
		try
		{
			int num2 = offset + count;
			while (num < num2)
			{
				int num3 = ReadByteClearText();
				if (num3 != -1)
				{
					buffer[num++] = (byte)num3;
					continue;
				}
				break;
			}
		}
		catch (IOException ex)
		{
			if (num == offset)
			{
				throw ex;
			}
		}
		return num - offset;
	}

	private int DoReadByte()
	{
		if (bufPtr > 2 || crcFound)
		{
			int num = ReadIgnoreSpace();
			if (num == 10 || num == 13)
			{
				num = ReadIgnoreWhitespace();
				if (num == 61)
				{
					bufPtr = Decode(ReadIgnoreSpace(), ReadIgnoreSpace(), ReadIgnoreSpace(), ReadIgnoreSpace(), outBuf);
					if (bufPtr != 0)
					{
						throw new IOException("no crc found in armored message.");
					}
					crcFound = true;
					int num2 = ((outBuf[0] & 0xFF) << 16) | ((outBuf[1] & 0xFF) << 8) | (outBuf[2] & 0xFF);
					if (num2 != crc.Value)
					{
						throw new IOException("crc check failed in armored message.");
					}
					return ReadByte();
				}
				if (num == 45)
				{
					while ((num = input.ReadByte()) >= 0 && num != 10 && num != 13)
					{
					}
					if (!crcFound)
					{
						throw new IOException("crc check not found.");
					}
					crcFound = false;
					start = true;
					bufPtr = 3;
					if (num < 0)
					{
						isEndOfStream = true;
					}
					return -1;
				}
			}
			if (num < 0)
			{
				isEndOfStream = true;
				return -1;
			}
			bufPtr = Decode(num, ReadIgnoreSpace(), ReadIgnoreSpace(), ReadIgnoreSpace(), outBuf);
		}
		return outBuf[bufPtr++];
	}

	public override int ReadByte()
	{
		if (start)
		{
			if (hasHeaders)
			{
				ParseHeaders();
			}
			crc.Reset();
			start = false;
		}
		if (clearText)
		{
			return ReadByteClearText();
		}
		int num = DoReadByte();
		crc.Update(num);
		return num;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (start && count > 0)
		{
			if (hasHeaders)
			{
				ParseHeaders();
			}
			start = false;
		}
		if (clearText)
		{
			return ReadClearText(buffer, offset, count);
		}
		int num = offset;
		try
		{
			int num2 = offset + count;
			while (num < num2)
			{
				int num3 = DoReadByte();
				crc.Update(num3);
				if (num3 != -1)
				{
					buffer[num++] = (byte)num3;
					continue;
				}
				break;
			}
		}
		catch (IOException ex)
		{
			if (num == offset)
			{
				throw ex;
			}
		}
		return num - offset;
	}

	public override void Close()
	{
		Platform.Dispose(input);
		base.Close();
	}
}
