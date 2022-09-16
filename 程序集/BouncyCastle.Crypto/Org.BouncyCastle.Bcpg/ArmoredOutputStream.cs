using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class ArmoredOutputStream : BaseOutputStream
{
	public static readonly string HeaderVersion = "Version";

	private static readonly byte[] encodingTable = new byte[64]
	{
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
		75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
		85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
		101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
		111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
		121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
		56, 57, 43, 47
	};

	private readonly Stream outStream;

	private int[] buf = new int[3];

	private int bufPtr = 0;

	private Crc24 crc = new Crc24();

	private int chunkCount = 0;

	private int lastb;

	private bool start = true;

	private bool clearText = false;

	private bool newLine = false;

	private string type;

	private static readonly string nl = Platform.NewLine;

	private static readonly string headerStart = "-----BEGIN PGP ";

	private static readonly string headerTail = "-----";

	private static readonly string footerStart = "-----END PGP ";

	private static readonly string footerTail = "-----";

	private static readonly string Version = "BCPG C# v" + AssemblyInfo.Version;

	private readonly IDictionary headers;

	private static void Encode(Stream outStream, int[] data, int len)
	{
		byte[] array = new byte[4];
		int num = data[0];
		array[0] = encodingTable[(num >> 2) & 0x3F];
		switch (len)
		{
		case 1:
			array[1] = encodingTable[(num << 4) & 0x3F];
			array[2] = 61;
			array[3] = 61;
			break;
		case 2:
		{
			int num4 = data[1];
			array[1] = encodingTable[((num << 4) | (num4 >> 4)) & 0x3F];
			array[2] = encodingTable[(num4 << 2) & 0x3F];
			array[3] = 61;
			break;
		}
		case 3:
		{
			int num2 = data[1];
			int num3 = data[2];
			array[1] = encodingTable[((num << 4) | (num2 >> 4)) & 0x3F];
			array[2] = encodingTable[((num2 << 2) | (num3 >> 6)) & 0x3F];
			array[3] = encodingTable[num3 & 0x3F];
			break;
		}
		}
		outStream.Write(array, 0, array.Length);
	}

	public ArmoredOutputStream(Stream outStream)
	{
		this.outStream = outStream;
		headers = Platform.CreateHashtable(1);
		SetHeader(HeaderVersion, Version);
	}

	public ArmoredOutputStream(Stream outStream, IDictionary headers)
		: this(outStream)
	{
		foreach (string key in headers.Keys)
		{
			IList list = Platform.CreateArrayList(1);
			list.Add(headers[key]);
			this.headers[key] = list;
		}
	}

	public void SetHeader(string name, string val)
	{
		if (val == null)
		{
			headers.Remove(name);
			return;
		}
		IList list = (IList)headers[name];
		if (list == null)
		{
			list = Platform.CreateArrayList(1);
			headers[name] = list;
		}
		else
		{
			list.Clear();
		}
		list.Add(val);
	}

	public void AddHeader(string name, string val)
	{
		if (val != null && name != null)
		{
			IList list = (IList)headers[name];
			if (list == null)
			{
				list = Platform.CreateArrayList(1);
				headers[name] = list;
			}
			list.Add(val);
		}
	}

	public void ResetHeaders()
	{
		IList list = (IList)headers[HeaderVersion];
		headers.Clear();
		if (list != null)
		{
			headers[HeaderVersion] = list;
		}
	}

	public void BeginClearText(HashAlgorithmTag hashAlgorithm)
	{
		string text = hashAlgorithm switch
		{
			HashAlgorithmTag.Sha1 => "SHA1", 
			HashAlgorithmTag.Sha256 => "SHA256", 
			HashAlgorithmTag.Sha384 => "SHA384", 
			HashAlgorithmTag.Sha512 => "SHA512", 
			HashAlgorithmTag.MD2 => "MD2", 
			HashAlgorithmTag.MD5 => "MD5", 
			HashAlgorithmTag.RipeMD160 => "RIPEMD160", 
			_ => throw new IOException("unknown hash algorithm tag in beginClearText: " + hashAlgorithm), 
		};
		DoWrite("-----BEGIN PGP SIGNED MESSAGE-----" + nl);
		DoWrite("Hash: " + text + nl + nl);
		clearText = true;
		newLine = true;
		lastb = 0;
	}

	public void EndClearText()
	{
		clearText = false;
	}

	public override void WriteByte(byte b)
	{
		if (clearText)
		{
			outStream.WriteByte(b);
			if (newLine)
			{
				if (b != 10 || lastb != 13)
				{
					newLine = false;
				}
				if (b == 45)
				{
					outStream.WriteByte(32);
					outStream.WriteByte(45);
				}
			}
			if (b == 13 || (b == 10 && lastb != 13))
			{
				newLine = true;
			}
			lastb = b;
			return;
		}
		if (start)
		{
			switch (((b & 0x40) == 0) ? ((b & 0x3F) >> 2) : (b & 0x3F))
			{
			case 6:
				type = "PUBLIC KEY BLOCK";
				break;
			case 5:
				type = "PRIVATE KEY BLOCK";
				break;
			case 2:
				type = "SIGNATURE";
				break;
			default:
				type = "MESSAGE";
				break;
			}
			DoWrite(headerStart + type + headerTail + nl);
			IList list = (IList)headers[HeaderVersion];
			if (list != null)
			{
				WriteHeaderEntry(HeaderVersion, list[0].ToString());
			}
			foreach (object header in headers)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)header;
				string text = (string)dictionaryEntry.Key;
				if (!(text != HeaderVersion))
				{
					continue;
				}
				IList list2 = (IList)dictionaryEntry.Value;
				foreach (string item in list2)
				{
					WriteHeaderEntry(text, item);
				}
			}
			DoWrite(nl);
			start = false;
		}
		if (bufPtr == 3)
		{
			Encode(outStream, buf, bufPtr);
			bufPtr = 0;
			if ((++chunkCount & 0xF) == 0)
			{
				DoWrite(nl);
			}
		}
		crc.Update(b);
		buf[bufPtr++] = b & 0xFF;
	}

	public override void Close()
	{
		if (type != null)
		{
			DoClose();
			type = null;
			start = true;
			base.Close();
		}
	}

	private void DoClose()
	{
		if (bufPtr > 0)
		{
			Encode(outStream, buf, bufPtr);
		}
		DoWrite(nl + '=');
		int value = crc.Value;
		buf[0] = (value >> 16) & 0xFF;
		buf[1] = (value >> 8) & 0xFF;
		buf[2] = value & 0xFF;
		Encode(outStream, buf, 3);
		DoWrite(nl);
		DoWrite(footerStart);
		DoWrite(type);
		DoWrite(footerTail);
		DoWrite(nl);
		outStream.Flush();
	}

	private void WriteHeaderEntry(string name, string v)
	{
		DoWrite(name + ": " + v + nl);
	}

	private void DoWrite(string s)
	{
		byte[] array = Strings.ToAsciiByteArray(s);
		outStream.Write(array, 0, array.Length);
	}
}
