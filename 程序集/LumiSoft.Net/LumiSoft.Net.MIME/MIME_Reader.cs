using System;
using System.Text;
using System.Text.RegularExpressions;

namespace LumiSoft.Net.MIME;

public class MIME_Reader
{
	private string m_Source = "";

	private int m_Offset;

	private static readonly char[] atextChars = new char[19]
	{
		'!', '#', '$', '%', '&', '\'', '*', '+', '-', '/',
		'=', '?', '^', '_', '`', '{', '|', '}', '~'
	};

	private static readonly char[] specials = new char[13]
	{
		'(', ')', '<', '>', '[', ']', ':', ';', '@', '\\',
		',', '.', '"'
	};

	private static readonly char[] tspecials = new char[15]
	{
		'(', ')', '<', '>', '@', ',', ';', ':', '\\', '"',
		'/', '[', ']', '?', '='
	};

	private static readonly Regex encodedword_regex = new Regex("=\\?(?<charset>.*?)\\?(?<encoding>[qQbB])\\?(?<value>.*?)\\?=", RegexOptions.IgnoreCase);

	public int Available => m_Source.Length - m_Offset;

	public int Position => m_Offset;

	public MIME_Reader(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_Source = value;
	}

	public string Atom()
	{
		ToFirstChar();
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			int num = Peek(readToFirstChar: false);
			if (num == -1 || !IsAText((char)num))
			{
				break;
			}
			stringBuilder.Append((char)Char(readToFirstChar: false));
		}
		if (stringBuilder.Length > 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public string DotAtom()
	{
		ToFirstChar();
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			string text = Atom();
			if (text == null)
			{
				break;
			}
			stringBuilder.Append(text);
			if (Peek(readToFirstChar: false) != 46)
			{
				break;
			}
			stringBuilder.Append((char)Char(readToFirstChar: false));
		}
		if (stringBuilder.Length > 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public string Token()
	{
		ToFirstChar();
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			int num = Peek(readToFirstChar: false);
			if (num == -1 || !IsToken((char)num))
			{
				break;
			}
			stringBuilder.Append((char)Char(readToFirstChar: false));
		}
		if (stringBuilder.Length > 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public string Comment()
	{
		ToFirstChar();
		if (Peek(readToFirstChar: false) != 40)
		{
			throw new InvalidOperationException("No 'comment' value available.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		Char(readToFirstChar: false);
		int num = 0;
		while (true)
		{
			int num2 = Char(readToFirstChar: false);
			switch (num2)
			{
			case -1:
				throw new ArgumentException("Invalid 'comment' value, no closing ')'.");
			case 40:
				num++;
				break;
			case 41:
				if (num != 0)
				{
					num--;
					break;
				}
				return stringBuilder.ToString();
			default:
				stringBuilder.Append((char)num2);
				break;
			}
		}
	}

	public string Word()
	{
		if (Peek(readToFirstChar: true) == 34)
		{
			return QuotedString();
		}
		return DotAtom();
	}

	public string EncodedWord()
	{
		ToFirstChar();
		if (Peek(readToFirstChar: false) != 61)
		{
			throw new InvalidOperationException("No encoded-word available.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			Match match = encodedword_regex.Match(m_Source, m_Offset);
			if (match.Success && match.Index == m_Offset)
			{
				string value = m_Source.Substring(m_Offset, match.Length);
				m_Offset += match.Length;
				try
				{
					if (string.Equals(match.Groups["encoding"].Value, "Q", StringComparison.InvariantCultureIgnoreCase))
					{
						stringBuilder.Append(MIME_Utils.QDecode(Encoding.GetEncoding(match.Groups["charset"].Value), match.Groups["value"].Value));
					}
					else if (string.Equals(match.Groups["encoding"].Value, "B", StringComparison.InvariantCultureIgnoreCase))
					{
						stringBuilder.Append(Encoding.GetEncoding(match.Groups["charset"].Value).GetString(Net_Utils.FromBase64(Encoding.Default.GetBytes(match.Groups["value"].Value))));
					}
					else
					{
						stringBuilder.Append(value);
					}
				}
				catch
				{
					stringBuilder.Append(value);
				}
			}
			else
			{
				stringBuilder.Append(Atom());
			}
			match = encodedword_regex.Match(m_Source, m_Offset);
			if (!match.Success || match.Index != m_Offset)
			{
				break;
			}
			ToFirstChar();
		}
		return stringBuilder.ToString();
	}

	public string QuotedString()
	{
		ToFirstChar();
		if (Peek(readToFirstChar: false) != 34)
		{
			throw new InvalidOperationException("No quoted-string available.");
		}
		Char(readToFirstChar: false);
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		while (true)
		{
			int num = Char(readToFirstChar: false);
			if (num == -1)
			{
				break;
			}
			if (flag)
			{
				flag = false;
				stringBuilder.Append((char)num);
				continue;
			}
			switch (num)
			{
			case 10:
			case 13:
				break;
			case 92:
				flag = true;
				break;
			default:
				stringBuilder.Append((char)num);
				break;
			case 34:
				return stringBuilder.ToString();
			}
		}
		throw new ArgumentException("Invalid quoted-string, end quote is missing.");
	}

	public string Value()
	{
		if (Peek(readToFirstChar: true) == 34)
		{
			return QuotedString();
		}
		return Token();
	}

	public string Phrase()
	{
		switch (Peek(readToFirstChar: true))
		{
		case -1:
			return null;
		case 34:
			return "\"" + QuotedString() + "\"";
		case 61:
			return EncodedWord();
		default:
		{
			string text = Atom();
			if (text == null)
			{
				return null;
			}
			return encodedword_regex.Replace(text, delegate(Match m)
			{
				string value = m.Value;
				try
				{
					if (string.Equals(m.Groups["encoding"].Value, "Q", StringComparison.InvariantCultureIgnoreCase))
					{
						return MIME_Utils.QDecode(Encoding.GetEncoding(m.Groups["charset"].Value), m.Groups["value"].Value);
					}
					if (string.Equals(m.Groups["encoding"].Value, "B", StringComparison.InvariantCultureIgnoreCase))
					{
						return Encoding.GetEncoding(m.Groups["charset"].Value).GetString(Net_Utils.FromBase64(Encoding.Default.GetBytes(m.Groups["value"].Value)));
					}
					return value;
				}
				catch
				{
					return value;
				}
			});
		}
		}
	}

	public string Text()
	{
		throw new NotImplementedException();
	}

	public string ToFirstChar()
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			int num = -1;
			switch ((m_Offset <= m_Source.Length - 1) ? m_Source[m_Offset] : (-1))
			{
			case 9:
			case 10:
			case 13:
			case 32:
				break;
			default:
				return stringBuilder.ToString();
			}
			stringBuilder.Append(m_Source[m_Offset++]);
		}
	}

	public int Char(bool readToFirstChar)
	{
		if (readToFirstChar)
		{
			ToFirstChar();
		}
		if (m_Offset > m_Source.Length - 1)
		{
			return -1;
		}
		return m_Source[m_Offset++];
	}

	public int Peek(bool readToFirstChar)
	{
		if (readToFirstChar)
		{
			ToFirstChar();
		}
		if (m_Offset > m_Source.Length - 1)
		{
			return -1;
		}
		return m_Source[m_Offset];
	}

	public bool StartsWith(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return m_Source.Substring(m_Offset).StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
	}

	public string ToEnd()
	{
		if (m_Offset >= m_Source.Length)
		{
			return null;
		}
		string result = m_Source.Substring(m_Offset);
		m_Offset = m_Source.Length;
		return result;
	}

	public static bool IsAlpha(char c)
	{
		if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
		{
			return true;
		}
		return false;
	}

	public static bool IsAText(char c)
	{
		if (IsAlpha(c) || char.IsDigit(c))
		{
			return true;
		}
		char[] array = atextChars;
		foreach (char c2 in array)
		{
			if (c == c2)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDotAtom(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		foreach (char c in value)
		{
			if (c != '.' && !IsAText(c))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsToken(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text == "")
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!IsToken(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsToken(char c)
	{
		if (c > '\u001f')
		{
			switch (c)
			{
			case '\u007f':
				break;
			case ' ':
				return false;
			default:
			{
				char[] array = tspecials;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == c)
					{
						return false;
					}
				}
				return true;
			}
			}
		}
		return false;
	}

	public static bool IsAttributeChar(char c)
	{
		if (c <= '\u001f' || c > '\u007f')
		{
			return false;
		}
		if (c == ' ' || c == '*' || c == '\'' || c == '%')
		{
			return false;
		}
		char[] array = tspecials;
		foreach (char c2 in array)
		{
			if (c == c2)
			{
				return false;
			}
		}
		return true;
	}

	public string ReadParenthesized()
	{
		ToFirstChar();
		char c = ' ';
		char c2 = ' ';
		if (m_Source[m_Offset] == '{')
		{
			c = '{';
			c2 = '}';
		}
		else if (m_Source[m_Offset] == '(')
		{
			c = '(';
			c2 = ')';
		}
		else if (m_Source[m_Offset] == '[')
		{
			c = '[';
			c2 = ']';
		}
		else
		{
			if (m_Source[m_Offset] != '<')
			{
				throw new Exception("No parenthesized value '" + m_Source.Substring(m_Offset) + "' !");
			}
			c = '<';
			c2 = '>';
		}
		m_Offset++;
		bool flag = false;
		char c3 = '\0';
		int num = 0;
		for (int i = m_Offset; i < m_Source.Length; i++)
		{
			if (c3 != '\\' && m_Source[i] == '"')
			{
				flag = !flag;
			}
			else if (!flag)
			{
				if (m_Source[i] == c)
				{
					num++;
				}
				else if (m_Source[i] == c2)
				{
					if (num == 0)
					{
						string result = m_Source.Substring(m_Offset, i - m_Offset);
						m_Offset = i + 1;
						return result;
					}
					num--;
				}
			}
			c3 = m_Source[i];
		}
		throw new ArgumentException("There is no closing parenthesize for '" + m_Source.Substring(m_Offset) + "' !");
	}

	public string QuotedReadToDelimiter(char[] delimiters)
	{
		if (delimiters == null)
		{
			throw new ArgumentNullException("delimiters");
		}
		if (Available == 0)
		{
			return null;
		}
		ToFirstChar();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		char c = '\0';
		for (int i = m_Offset; i < m_Source.Length; i++)
		{
			char c2 = (char)Peek(readToFirstChar: false);
			if (c != '\\' && c2 == '"')
			{
				flag = !flag;
			}
			bool flag2 = false;
			foreach (char c3 in delimiters)
			{
				if (c2 == c3)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag && flag2)
			{
				return stringBuilder.ToString();
			}
			stringBuilder.Append(c2);
			m_Offset++;
			c = c2;
		}
		return stringBuilder.ToString();
	}
}
