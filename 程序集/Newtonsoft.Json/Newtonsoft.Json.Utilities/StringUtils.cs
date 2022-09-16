using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Utilities;

internal static class StringUtils
{
	private enum SeparatedCaseState
	{
		Start,
		Lower,
		Upper,
		NewWord
	}

	public const string CarriageReturnLineFeed = "\r\n";

	public const string Empty = "";

	public const char CarriageReturn = '\r';

	public const char LineFeed = '\n';

	public const char Tab = '\t';

	public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
	{
		return string.IsNullOrEmpty(value);
	}

	public static string FormatWith(this string format, IFormatProvider provider, object? arg0)
	{
		return format.FormatWith(provider, new object[1] { arg0 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1)
	{
		return format.FormatWith(provider, new object[2] { arg0, arg1 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2)
	{
		return format.FormatWith(provider, new object[3] { arg0, arg1, arg2 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2, object? arg3)
	{
		return format.FormatWith(provider, new object[4] { arg0, arg1, arg2, arg3 });
	}

	private static string FormatWith(this string format, IFormatProvider provider, params object?[] args)
	{
		ValidationUtils.ArgumentNotNull(format, "format");
		return string.Format(provider, format, args);
	}

	public static bool IsWhiteSpace(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (s.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (!char.IsWhiteSpace(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static StringWriter CreateStringWriter(int capacity)
	{
		return new StringWriter(new StringBuilder(capacity), CultureInfo.InvariantCulture);
	}

	public static void ToCharAsUnicode(char c, char[] buffer)
	{
		buffer[0] = '\\';
		buffer[1] = 'u';
		buffer[2] = MathUtils.IntToHex(((int)c >> 12) & 0xF);
		buffer[3] = MathUtils.IntToHex(((int)c >> 8) & 0xF);
		buffer[4] = MathUtils.IntToHex(((int)c >> 4) & 0xF);
		buffer[5] = MathUtils.IntToHex(c & 0xF);
	}

	public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
	{
		Func<TSource, string> valueSelector2 = valueSelector;
		string testValue2 = testValue;
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (valueSelector2 == null)
		{
			throw new ArgumentNullException("valueSelector");
		}
		IEnumerable<TSource> source2 = source.Where<TSource>((TSource s) => string.Equals(valueSelector2(s), testValue2, StringComparison.OrdinalIgnoreCase));
		if (source2.Count() <= 1)
		{
			return source2.SingleOrDefault();
		}
		return source.Where<TSource>((TSource s) => string.Equals(valueSelector2(s), testValue2, StringComparison.Ordinal)).SingleOrDefault();
	}

	public static string ToCamelCase(string s)
	{
		if (IsNullOrEmpty(s) || !char.IsUpper(s[0]))
		{
			return s;
		}
		char[] array = s.ToCharArray();
		for (int i = 0; i < array.Length && (i != 1 || char.IsUpper(array[i])); i++)
		{
			bool flag = i + 1 < array.Length;
			if (i > 0 && flag && !char.IsUpper(array[i + 1]))
			{
				if (char.IsSeparator(array[i + 1]))
				{
					array[i] = ToLower(array[i]);
				}
				break;
			}
			array[i] = ToLower(array[i]);
		}
		return new string(array);
	}

	private static char ToLower(char c)
	{
		c = char.ToLower(c, CultureInfo.InvariantCulture);
		return c;
	}

	public static string ToSnakeCase(string s)
	{
		return ToSeparatedCase(s, '_');
	}

	public static string ToKebabCase(string s)
	{
		return ToSeparatedCase(s, '-');
	}

	private static string ToSeparatedCase(string s, char separator)
	{
		if (IsNullOrEmpty(s))
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder();
		SeparatedCaseState separatedCaseState = SeparatedCaseState.Start;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == ' ')
			{
				if (separatedCaseState != 0)
				{
					separatedCaseState = SeparatedCaseState.NewWord;
				}
			}
			else if (char.IsUpper(s[i]))
			{
				switch (separatedCaseState)
				{
				case SeparatedCaseState.Upper:
				{
					bool flag = i + 1 < s.Length;
					if (i > 0 && flag)
					{
						char c = s[i + 1];
						if (!char.IsUpper(c) && c != separator)
						{
							stringBuilder.Append(separator);
						}
					}
					break;
				}
				case SeparatedCaseState.Lower:
				case SeparatedCaseState.NewWord:
					stringBuilder.Append(separator);
					break;
				}
				char value = char.ToLower(s[i], CultureInfo.InvariantCulture);
				stringBuilder.Append(value);
				separatedCaseState = SeparatedCaseState.Upper;
			}
			else if (s[i] == separator)
			{
				stringBuilder.Append(separator);
				separatedCaseState = SeparatedCaseState.Start;
			}
			else
			{
				if (separatedCaseState == SeparatedCaseState.NewWord)
				{
					stringBuilder.Append(separator);
				}
				stringBuilder.Append(s[i]);
				separatedCaseState = SeparatedCaseState.Lower;
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsHighSurrogate(char c)
	{
		return char.IsHighSurrogate(c);
	}

	public static bool IsLowSurrogate(char c)
	{
		return char.IsLowSurrogate(c);
	}

	public static bool StartsWith(this string source, char value)
	{
		if (source.Length > 0)
		{
			return source[0] == value;
		}
		return false;
	}

	public static bool EndsWith(this string source, char value)
	{
		if (source.Length > 0)
		{
			return source[source.Length - 1] == value;
		}
		return false;
	}

	public static string Trim(this string s, int start, int length)
	{
		if (s == null)
		{
			throw new ArgumentNullException();
		}
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		int num = start + length - 1;
		if (num >= s.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		while (start < num && char.IsWhiteSpace(s[start]))
		{
			start++;
		}
		while (num >= start && char.IsWhiteSpace(s[num]))
		{
			num--;
		}
		return s.Substring(start, num - start + 1);
	}
}
