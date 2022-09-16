using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

public abstract class DerGenerator : Asn1Generator
{
	private bool _tagged = false;

	private bool _isExplicit;

	private int _tagNo;

	protected DerGenerator(Stream outStream)
		: base(outStream)
	{
	}

	protected DerGenerator(Stream outStream, int tagNo, bool isExplicit)
		: base(outStream)
	{
		_tagged = true;
		_isExplicit = isExplicit;
		_tagNo = tagNo;
	}

	private static void WriteLength(Stream outStr, int length)
	{
		if (length > 127)
		{
			int num = 1;
			int num2 = length;
			while ((num2 >>= 8) != 0)
			{
				num++;
			}
			outStr.WriteByte((byte)((uint)num | 0x80u));
			for (int num3 = (num - 1) * 8; num3 >= 0; num3 -= 8)
			{
				outStr.WriteByte((byte)(length >> num3));
			}
		}
		else
		{
			outStr.WriteByte((byte)length);
		}
	}

	internal static void WriteDerEncoded(Stream outStream, int tag, byte[] bytes)
	{
		outStream.WriteByte((byte)tag);
		WriteLength(outStream, bytes.Length);
		outStream.Write(bytes, 0, bytes.Length);
	}

	internal void WriteDerEncoded(int tag, byte[] bytes)
	{
		if (_tagged)
		{
			int num = _tagNo | 0x80;
			if (_isExplicit)
			{
				int tag2 = _tagNo | 0x20 | 0x80;
				MemoryStream memoryStream = new MemoryStream();
				WriteDerEncoded(memoryStream, tag, bytes);
				WriteDerEncoded(base.Out, tag2, memoryStream.ToArray());
			}
			else
			{
				if (((uint)tag & 0x20u) != 0)
				{
					num |= 0x20;
				}
				WriteDerEncoded(base.Out, num, bytes);
			}
		}
		else
		{
			WriteDerEncoded(base.Out, tag, bytes);
		}
	}

	internal static void WriteDerEncoded(Stream outStr, int tag, Stream inStr)
	{
		WriteDerEncoded(outStr, tag, Streams.ReadAll(inStr));
	}
}
