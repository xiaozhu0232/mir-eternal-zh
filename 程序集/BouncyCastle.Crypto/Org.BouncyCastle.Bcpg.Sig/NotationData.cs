using System;
using System.IO;
using System.Text;

namespace Org.BouncyCastle.Bcpg.Sig;

public class NotationData : SignatureSubpacket
{
	public const int HeaderFlagLength = 4;

	public const int HeaderNameLength = 2;

	public const int HeaderValueLength = 2;

	public bool IsHumanReadable => data[0] == 128;

	public NotationData(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.NotationData, critical, isLongLength, data)
	{
	}

	public NotationData(bool critical, bool humanReadable, string notationName, string notationValue)
		: base(SignatureSubpacketTag.NotationData, critical, isLongLength: false, CreateData(humanReadable, notationName, notationValue))
	{
	}

	private static byte[] CreateData(bool humanReadable, string notationName, string notationValue)
	{
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.WriteByte((byte)(humanReadable ? 128 : 0));
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		memoryStream.WriteByte(0);
		byte[] array = null;
		byte[] bytes = Encoding.UTF8.GetBytes(notationName);
		int num = System.Math.Min(bytes.Length, 255);
		array = Encoding.UTF8.GetBytes(notationValue);
		int num2 = System.Math.Min(array.Length, 255);
		memoryStream.WriteByte((byte)(num >> 8));
		memoryStream.WriteByte((byte)num);
		memoryStream.WriteByte((byte)(num2 >> 8));
		memoryStream.WriteByte((byte)num2);
		memoryStream.Write(bytes, 0, num);
		memoryStream.Write(array, 0, num2);
		return memoryStream.ToArray();
	}

	public string GetNotationName()
	{
		int count = (data[4] << 8) + data[5];
		int index = 8;
		return Encoding.UTF8.GetString(data, index, count);
	}

	public string GetNotationValue()
	{
		int num = (data[4] << 8) + data[5];
		int count = (data[6] << 8) + data[7];
		int index = 8 + num;
		return Encoding.UTF8.GetString(data, index, count);
	}

	public byte[] GetNotationValueBytes()
	{
		int num = (data[4] << 8) + data[5];
		int num2 = (data[6] << 8) + data[7];
		int sourceIndex = 8 + num;
		byte[] array = new byte[num2];
		Array.Copy(data, sourceIndex, array, 0, num2);
		return array;
	}
}
