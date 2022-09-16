using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LumiSoft.Net.MIME;

public class MIME_Encoding_EncodedWord
{
	private MIME_EncodedWordEncoding m_Encoding;

	private Encoding m_pCharset;

	private bool m_Split = true;

	private static readonly Regex encodedword_regex = new Regex("\\=\\?(?<charset>\\S+?)\\?(?<encoding>[qQbB])\\?(?<value>.+?)\\?\\=(?<whitespaces>\\s*)", RegexOptions.IgnoreCase);

	public bool Split
	{
		get
		{
			return m_Split;
		}
		set
		{
			m_Split = value;
		}
	}

	public MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding encoding, Encoding charset)
	{
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		m_Encoding = encoding;
		m_pCharset = charset;
	}

	public string Encode(string text)
	{
		if (MustEncode(text))
		{
			return EncodeS(m_Encoding, m_pCharset, m_Split, text);
		}
		return text;
	}

	public string Decode(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return DecodeS(text);
	}

	public static bool MustEncode(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] > '\u007f')
			{
				return true;
			}
		}
		return false;
	}

	public static string EncodeS(MIME_EncodedWordEncoding encoding, Encoding charset, bool split, string text)
	{
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (MustEncode(text))
		{
			List<string> list = new List<string>();
			if (split)
			{
				int num;
				for (int i = 0; i < text.Length; i += num)
				{
					num = Math.Min(30, text.Length - i);
					list.Add(text.Substring(i, num));
				}
			}
			else
			{
				list.Add(text);
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < list.Count; j++)
			{
				string s = list[j];
				byte[] bytes = charset.GetBytes(s);
				if (encoding == MIME_EncodedWordEncoding.B)
				{
					stringBuilder.Append("=?" + charset.WebName + "?B?" + Convert.ToBase64String(bytes) + "?=");
				}
				else
				{
					stringBuilder.Append("=?" + charset.WebName + "?Q?");
					int num2 = 0;
					byte[] array = bytes;
					for (int k = 0; k < array.Length; k++)
					{
						byte b = array[k];
						string text2 = null;
						if (b > 127 || b == 61 || b == 63 || b == 95 || b == 32)
						{
							text2 = "=" + b.ToString("X2");
						}
						else
						{
							char c = (char)b;
							text2 = c.ToString();
						}
						stringBuilder.Append(text2);
						num2 += text2.Length;
					}
					stringBuilder.Append("?=");
				}
				if (j < list.Count - 1)
				{
					stringBuilder.Append("\r\n ");
				}
			}
			return stringBuilder.ToString();
		}
		return text;
	}

	public static string DecodeS(string word)
	{
		if (word == null)
		{
			throw new ArgumentNullException("word");
		}
		return DecodeTextS(word);
	}

	public static string DecodeTextS(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("word");
		}
		string retVal = text;
		retVal = encodedword_regex.Replace(retVal, delegate(Match m)
		{
			string text2 = m.Value;
			try
			{
				if (string.Equals(m.Groups["encoding"].Value, "Q", StringComparison.InvariantCultureIgnoreCase))
				{
					text2 = MIME_Utils.QDecode(Encoding.GetEncoding(m.Groups["charset"].Value), m.Groups["value"].Value);
				}
				else if (string.Equals(m.Groups["encoding"].Value, "B", StringComparison.InvariantCultureIgnoreCase))
				{
					text2 = Encoding.GetEncoding(m.Groups["charset"].Value).GetString(Net_Utils.FromBase64(Encoding.Default.GetBytes(m.Groups["value"].Value)));
				}
				Match match = encodedword_regex.Match(retVal, m.Index + m.Length);
				if (!match.Success || match.Index != m.Index + m.Length)
				{
					text2 += m.Groups["whitespaces"].Value;
				}
				return text2;
			}
			catch
			{
				return text2;
			}
		});
		return retVal;
	}
}
