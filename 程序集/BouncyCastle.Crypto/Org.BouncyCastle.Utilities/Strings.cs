using System;
using System.Text;

namespace Org.BouncyCastle.Utilities;

public abstract class Strings
{
	public static string ToUpperCase(string original)
	{
		bool flag = false;
		char[] array = original.ToCharArray();
		for (int i = 0; i != array.Length; i++)
		{
			char c = array[i];
			if ('a' <= c && 'z' >= c)
			{
				flag = true;
				array[i] = (char)(c - 97 + 65);
			}
		}
		if (flag)
		{
			return new string(array);
		}
		return original;
	}

	internal static bool IsOneOf(string s, params string[] candidates)
	{
		foreach (string text in candidates)
		{
			if (s == text)
			{
				return true;
			}
		}
		return false;
	}

	public static string FromByteArray(byte[] bs)
	{
		char[] array = new char[bs.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToChar(bs[i]);
		}
		return new string(array);
	}

	public static byte[] ToByteArray(char[] cs)
	{
		byte[] array = new byte[cs.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(cs[i]);
		}
		return array;
	}

	public static byte[] ToByteArray(string s)
	{
		byte[] array = new byte[s.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(s[i]);
		}
		return array;
	}

	public static string FromAsciiByteArray(byte[] bytes)
	{
		return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
	}

	public static byte[] ToAsciiByteArray(char[] cs)
	{
		return Encoding.ASCII.GetBytes(cs);
	}

	public static byte[] ToAsciiByteArray(string s)
	{
		return Encoding.ASCII.GetBytes(s);
	}

	public static string FromUtf8ByteArray(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	public static byte[] ToUtf8ByteArray(char[] cs)
	{
		return Encoding.UTF8.GetBytes(cs);
	}

	public static byte[] ToUtf8ByteArray(string s)
	{
		return Encoding.UTF8.GetBytes(s);
	}
}
