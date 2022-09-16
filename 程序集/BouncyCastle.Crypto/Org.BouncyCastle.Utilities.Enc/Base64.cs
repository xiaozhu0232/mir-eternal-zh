using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders;

public sealed class Base64
{
	private Base64()
	{
	}

	public static string ToBase64String(byte[] data)
	{
		return Convert.ToBase64String(data, 0, data.Length);
	}

	public static string ToBase64String(byte[] data, int off, int length)
	{
		return Convert.ToBase64String(data, off, length);
	}

	public static byte[] Encode(byte[] data)
	{
		return Encode(data, 0, data.Length);
	}

	public static byte[] Encode(byte[] data, int off, int length)
	{
		string s = Convert.ToBase64String(data, off, length);
		return Strings.ToAsciiByteArray(s);
	}

	public static int Encode(byte[] data, Stream outStream)
	{
		byte[] array = Encode(data);
		outStream.Write(array, 0, array.Length);
		return array.Length;
	}

	public static int Encode(byte[] data, int off, int length, Stream outStream)
	{
		byte[] array = Encode(data, off, length);
		outStream.Write(array, 0, array.Length);
		return array.Length;
	}

	public static byte[] Decode(byte[] data)
	{
		string s = Strings.FromAsciiByteArray(data);
		return Convert.FromBase64String(s);
	}

	public static byte[] Decode(string data)
	{
		return Convert.FromBase64String(data);
	}

	public static int Decode(string data, Stream outStream)
	{
		byte[] array = Decode(data);
		outStream.Write(array, 0, array.Length);
		return array.Length;
	}
}
