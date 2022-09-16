using System;
using System.Text;

namespace LumiSoft.Net.Mime.vCard;

internal class vCard_Utils
{
	public static string Encode(string version, string value)
	{
		return Encode(version, Encoding.UTF8, value);
	}

	public static string Encode(string version, Encoding charset, string value)
	{
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		value = ((!version.StartsWith("3")) ? QPEncode(charset.GetBytes(value)) : value.Replace("\r", "").Replace("\n", "\\n").Replace(",", "\\,"));
		return value;
	}

	public static string QPEncode(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < data.Length; i++)
		{
			byte b = data[i];
			string text = null;
			if (b > 127 || b == 61 || b == 63 || b == 95 || char.IsControl((char)b))
			{
				text = "=" + b.ToString("X2");
			}
			else
			{
				char c = (char)b;
				text = c.ToString();
			}
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}
}
