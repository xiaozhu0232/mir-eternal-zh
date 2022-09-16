using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders;

public sealed class Hex
{
	private static readonly HexEncoder encoder = new HexEncoder();

	private Hex()
	{
	}

	public static string ToHexString(byte[] data)
	{
		return ToHexString(data, 0, data.Length);
	}

	public static string ToHexString(byte[] data, int off, int length)
	{
		byte[] bytes = Encode(data, off, length);
		return Strings.FromAsciiByteArray(bytes);
	}

	public static byte[] Encode(byte[] data)
	{
		return Encode(data, 0, data.Length);
	}

	public static byte[] Encode(byte[] data, int off, int length)
	{
		MemoryStream memoryStream = new MemoryStream(length * 2);
		encoder.Encode(data, off, length, memoryStream);
		return memoryStream.ToArray();
	}

	public static int Encode(byte[] data, Stream outStream)
	{
		return encoder.Encode(data, 0, data.Length, outStream);
	}

	public static int Encode(byte[] data, int off, int length, Stream outStream)
	{
		return encoder.Encode(data, off, length, outStream);
	}

	public static byte[] Decode(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream((data.Length + 1) / 2);
		encoder.Decode(data, 0, data.Length, memoryStream);
		return memoryStream.ToArray();
	}

	public static byte[] Decode(string data)
	{
		MemoryStream memoryStream = new MemoryStream((data.Length + 1) / 2);
		encoder.DecodeString(data, memoryStream);
		return memoryStream.ToArray();
	}

	public static int Decode(string data, Stream outStream)
	{
		return encoder.DecodeString(data, outStream);
	}

	public static byte[] DecodeStrict(string str)
	{
		return encoder.DecodeStrict(str, 0, str.Length);
	}

	public static byte[] DecodeStrict(string str, int off, int len)
	{
		return encoder.DecodeStrict(str, off, len);
	}
}
