using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net;

public class TextUtils
{
	public static string QuoteString(string text)
	{
		if (text != null && text.StartsWith("\"") && text.EndsWith("\""))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in text)
		{
			switch (c)
			{
			case '\\':
				stringBuilder.Append("\\\\");
				break;
			case '"':
				stringBuilder.Append("\\\"");
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return "\"" + stringBuilder.ToString() + "\"";
	}

	public static string UnQuoteString(string text)
	{
		int num = 0;
		int num2 = text.Length;
		for (int i = 0; i < num2; i++)
		{
			char c = text[i];
			if (c != ' ' && c != '\t')
			{
				break;
			}
			num++;
		}
		for (int num3 = num2 - 1; num3 > 0; num3--)
		{
			char c2 = text[num3];
			if (c2 != ' ' && c2 != '\t')
			{
				break;
			}
			num2--;
		}
		if (num2 - num <= 0)
		{
			return "";
		}
		if (text[num] == '"')
		{
			num++;
		}
		if (text[num2 - 1] == '"')
		{
			num2--;
		}
		if (num2 == num - 1)
		{
			return "";
		}
		char[] array = new char[num2 - num];
		int num4 = 0;
		bool flag = false;
		for (int j = num; j < num2; j++)
		{
			char c3 = text[j];
			if (!flag && c3 == '\\')
			{
				flag = true;
			}
			else if (flag)
			{
				array[num4] = c3;
				num4++;
				flag = false;
			}
			else
			{
				array[num4] = c3;
				num4++;
				flag = false;
			}
		}
		return new string(array, 0, num4);
	}

	public static string EscapeString(string text, char[] charsToEscape)
	{
		char[] array = new char[text.Length * 2];
		int num = 0;
		foreach (char c in text)
		{
			foreach (char c2 in charsToEscape)
			{
				if (c == c2)
				{
					array[num] = '\\';
					num++;
					break;
				}
			}
			array[num] = c;
			num++;
		}
		return new string(array, 0, num);
	}

	public static string UnEscapeString(string text)
	{
		char[] array = new char[text.Length];
		int num = 0;
		bool flag = false;
		foreach (char c in text)
		{
			if (!flag && c == '\\')
			{
				flag = true;
				continue;
			}
			array[num] = c;
			num++;
			flag = false;
		}
		return new string(array, 0, num);
	}

	public static string[] SplitQuotedString(string text, char splitChar)
	{
		return SplitQuotedString(text, splitChar, unquote: false);
	}

	public static string[] SplitQuotedString(string text, char splitChar, bool unquote)
	{
		return SplitQuotedString(text, splitChar, unquote, int.MaxValue);
	}

	public static string[] SplitQuotedString(string text, char splitChar, bool unquote, int count)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		List<string> list = new List<string>();
		int num = 0;
		bool flag = false;
		char c = '0';
		for (int i = 0; i < text.Length; i++)
		{
			char c2 = text[i];
			if (list.Count + 1 >= count)
			{
				break;
			}
			if (c != '\\' && c2 == '"')
			{
				flag = !flag;
			}
			if (!flag && c2 == splitChar)
			{
				if (unquote)
				{
					list.Add(UnQuoteString(text.Substring(num, i - num)));
				}
				else
				{
					list.Add(text.Substring(num, i - num));
				}
				num = i + 1;
			}
			c = c2;
		}
		if (unquote)
		{
			list.Add(UnQuoteString(text.Substring(num, text.Length - num)));
		}
		else
		{
			list.Add(text.Substring(num, text.Length - num));
		}
		return list.ToArray();
	}

	public static int QuotedIndexOf(string text, char indexChar)
	{
		int result = -1;
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '"')
			{
				flag = !flag;
			}
			if (!flag && c == indexChar)
			{
				return i;
			}
		}
		return result;
	}

	public static string[] SplitString(string text, char splitChar)
	{
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			if (text[i] == splitChar)
			{
				arrayList.Add(text.Substring(num, i - num));
				num = i + 1;
			}
		}
		if (num <= length)
		{
			arrayList.Add(text.Substring(num));
		}
		string[] array = new string[arrayList.Count];
		arrayList.CopyTo(array, 0);
		return array;
	}

	public static bool IsToken(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(value);
		}
		char[] array = new char[10] { '-', '.', '!', '%', '*', '_', '+', '`', '\'', '~' };
		foreach (char c in value)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
			{
				continue;
			}
			bool flag = false;
			char[] array2 = array;
			foreach (char c2 in array2)
			{
				if (c == c2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
