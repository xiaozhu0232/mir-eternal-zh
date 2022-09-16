using System;
using System.Text;

namespace LumiSoft.Net;

public class StringReader
{
	private string m_OriginalString = "";

	private string m_SourceString = "";

	public long Available => m_SourceString.Length;

	public string OriginalString => m_OriginalString;

	public string SourceString => m_SourceString;

	public int Position => m_OriginalString.Length - m_SourceString.Length;

	public StringReader(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_OriginalString = source;
		m_SourceString = source;
	}

	public void AppendString(string value)
	{
		m_SourceString += value;
	}

	public string ReadToFirstChar()
	{
		int num = 0;
		for (int i = 0; i < m_SourceString.Length && char.IsWhiteSpace(m_SourceString[i]); i++)
		{
			num++;
		}
		string result = m_SourceString.Substring(0, num);
		m_SourceString = m_SourceString.Substring(num);
		return result;
	}

	public string ReadSpecifiedLength(int length)
	{
		if (m_SourceString.Length >= length)
		{
			string result = m_SourceString.Substring(0, length);
			m_SourceString = m_SourceString.Substring(length);
			return result;
		}
		throw new Exception("Read length can't be bigger than source string !");
	}

	public string QuotedReadToDelimiter(char delimiter)
	{
		return QuotedReadToDelimiter(new char[1] { delimiter });
	}

	public string QuotedReadToDelimiter(char[] delimiters)
	{
		return QuotedReadToDelimiter(delimiters, removeDelimiter: true);
	}

	public string QuotedReadToDelimiter(char[] delimiters, bool removeDelimiter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < m_SourceString.Length; i++)
		{
			char c = m_SourceString[i];
			if (flag2)
			{
				stringBuilder.Append(c);
				flag2 = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				stringBuilder.Append(c);
				flag2 = true;
				continue;
			case '"':
				flag = !flag;
				break;
			}
			bool flag3 = false;
			foreach (char c2 in delimiters)
			{
				if (c == c2)
				{
					flag3 = true;
					break;
				}
			}
			if (!flag && flag3)
			{
				string result = stringBuilder.ToString();
				if (removeDelimiter)
				{
					m_SourceString = m_SourceString.Substring(i + 1);
					return result;
				}
				m_SourceString = m_SourceString.Substring(i);
				return result;
			}
			stringBuilder.Append(c);
		}
		m_SourceString = "";
		return stringBuilder.ToString();
	}

	public string ReadWord()
	{
		return ReadWord(unQuote: true);
	}

	public string ReadWord(bool unQuote)
	{
		return ReadWord(unQuote, new char[13]
		{
			' ', ',', ';', '{', '}', '(', ')', '[', ']', '<',
			'>', '\r', '\n'
		}, removeWordTerminator: false);
	}

	public string ReadWord(bool unQuote, char[] wordTerminatorChars, bool removeWordTerminator)
	{
		ReadToFirstChar();
		if (Available == 0L)
		{
			return null;
		}
		if (m_SourceString.StartsWith("\""))
		{
			if (unQuote)
			{
				return TextUtils.UnQuoteString(QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator));
			}
			return QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator);
		}
		int num = 0;
		for (int i = 0; i < m_SourceString.Length; i++)
		{
			char c = m_SourceString[i];
			bool flag = false;
			foreach (char c2 in wordTerminatorChars)
			{
				if (c == c2)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num++;
		}
		string result = m_SourceString.Substring(0, num);
		if (removeWordTerminator)
		{
			if (m_SourceString.Length >= num + 1)
			{
				m_SourceString = m_SourceString.Substring(num + 1);
				return result;
			}
		}
		else
		{
			m_SourceString = m_SourceString.Substring(num);
		}
		return result;
	}

	public string ReadParenthesized()
	{
		ReadToFirstChar();
		char c = ' ';
		char c2 = ' ';
		if (m_SourceString.StartsWith("{"))
		{
			c = '{';
			c2 = '}';
		}
		else if (m_SourceString.StartsWith("("))
		{
			c = '(';
			c2 = ')';
		}
		else if (m_SourceString.StartsWith("["))
		{
			c = '[';
			c2 = ']';
		}
		else
		{
			if (!m_SourceString.StartsWith("<"))
			{
				throw new Exception("No parenthesized value '" + m_SourceString + "' !");
			}
			c = '<';
			c2 = '>';
		}
		bool flag = false;
		bool flag2 = false;
		int num = -1;
		int num2 = 0;
		for (int i = 1; i < m_SourceString.Length; i++)
		{
			if (flag2)
			{
				flag2 = false;
			}
			else if (m_SourceString[i] == '\\')
			{
				flag2 = true;
			}
			else if (m_SourceString[i] == '"')
			{
				flag = !flag;
			}
			else
			{
				if (flag)
				{
					continue;
				}
				if (m_SourceString[i] == c)
				{
					num2++;
				}
				else if (m_SourceString[i] == c2)
				{
					if (num2 == 0)
					{
						num = i;
						break;
					}
					num2--;
				}
			}
		}
		if (num == -1)
		{
			throw new Exception("There is no closing parenthesize for '" + m_SourceString + "' !");
		}
		string result = m_SourceString.Substring(1, num - 1);
		m_SourceString = m_SourceString.Substring(num + 1);
		return result;
	}

	public string ReadToEnd()
	{
		if (Available == 0L)
		{
			return null;
		}
		string sourceString = m_SourceString;
		m_SourceString = "";
		return sourceString;
	}

	public void RemoveFromEnd(int count)
	{
		if (count < 0)
		{
			throw new ArgumentException("Argument 'count' value must be >= 0.", "count");
		}
		m_SourceString = m_SourceString.Substring(0, m_SourceString.Length - count);
	}

	public bool StartsWith(string value)
	{
		return m_SourceString.StartsWith(value);
	}

	public bool StartsWith(string value, bool case_sensitive)
	{
		if (case_sensitive)
		{
			return m_SourceString.StartsWith(value, StringComparison.InvariantCulture);
		}
		return m_SourceString.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
	}

	public bool EndsWith(string value)
	{
		return m_SourceString.EndsWith(value);
	}

	public bool EndsWith(string value, bool case_sensitive)
	{
		if (case_sensitive)
		{
			return m_SourceString.EndsWith(value, StringComparison.InvariantCulture);
		}
		return m_SourceString.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
	}

	public bool StartsWithWord()
	{
		if (m_SourceString.Length == 0)
		{
			return false;
		}
		if (char.IsWhiteSpace(m_SourceString[0]))
		{
			return false;
		}
		if (char.IsSeparator(m_SourceString[0]))
		{
			return false;
		}
		char[] array = new char[13]
		{
			' ', ',', ';', '{', '}', '(', ')', '[', ']', '<',
			'>', '\r', '\n'
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == m_SourceString[0])
			{
				return false;
			}
		}
		return true;
	}
}
