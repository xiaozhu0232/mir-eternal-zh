using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Newtonsoft.Json.Utilities;

internal static class DateTimeUtils
{
	internal static readonly long InitialJavaScriptDateTicks;

	private const string IsoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

	private const int DaysPer100Years = 36524;

	private const int DaysPer400Years = 146097;

	private const int DaysPer4Years = 1461;

	private const int DaysPerYear = 365;

	private const long TicksPerDay = 864000000000L;

	private static readonly int[] DaysToMonth365;

	private static readonly int[] DaysToMonth366;

	static DateTimeUtils()
	{
		InitialJavaScriptDateTicks = 621355968000000000L;
		DaysToMonth365 = new int[13]
		{
			0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
			304, 334, 365
		};
		DaysToMonth366 = new int[13]
		{
			0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
			305, 335, 366
		};
	}

	public static TimeSpan GetUtcOffset(this DateTime d)
	{
		return TimeZoneInfo.Local.GetUtcOffset(d);
	}

	public static XmlDateTimeSerializationMode ToSerializationMode(DateTimeKind kind)
	{
		return kind switch
		{
			DateTimeKind.Local => XmlDateTimeSerializationMode.Local, 
			DateTimeKind.Unspecified => XmlDateTimeSerializationMode.Unspecified, 
			DateTimeKind.Utc => XmlDateTimeSerializationMode.Utc, 
			_ => throw MiscellaneousUtils.CreateArgumentOutOfRangeException("kind", kind, "Unexpected DateTimeKind value."), 
		};
	}

	internal static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
	{
		switch (timeZone)
		{
		case DateTimeZoneHandling.Local:
			value = SwitchToLocalTime(value);
			break;
		case DateTimeZoneHandling.Utc:
			value = SwitchToUtcTime(value);
			break;
		case DateTimeZoneHandling.Unspecified:
			value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
			break;
		default:
			throw new ArgumentException("Invalid date time handling value.");
		case DateTimeZoneHandling.RoundtripKind:
			break;
		}
		return value;
	}

	private static DateTime SwitchToLocalTime(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Local), 
			DateTimeKind.Utc => value.ToLocalTime(), 
			DateTimeKind.Local => value, 
			_ => value, 
		};
	}

	private static DateTime SwitchToUtcTime(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Utc), 
			DateTimeKind.Utc => value, 
			DateTimeKind.Local => value.ToUniversalTime(), 
			_ => value, 
		};
	}

	private static long ToUniversalTicks(DateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKind.Utc)
		{
			return dateTime.Ticks;
		}
		return ToUniversalTicks(dateTime, dateTime.GetUtcOffset());
	}

	private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
	{
		if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
		{
			return dateTime.Ticks;
		}
		long num = dateTime.Ticks - offset.Ticks;
		if (num > 3155378975999999999L)
		{
			return 3155378975999999999L;
		}
		if (num < 0)
		{
			return 0L;
		}
		return num;
	}

	internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
	{
		return UniversalTicksToJavaScriptTicks(ToUniversalTicks(dateTime, offset));
	}

	internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
	{
		return ConvertDateTimeToJavaScriptTicks(dateTime, convertToUtc: true);
	}

	internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
	{
		return UniversalTicksToJavaScriptTicks(convertToUtc ? ToUniversalTicks(dateTime) : dateTime.Ticks);
	}

	private static long UniversalTicksToJavaScriptTicks(long universalTicks)
	{
		return (universalTicks - InitialJavaScriptDateTicks) / 10000;
	}

	internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
	{
		return new DateTime(javaScriptTicks * 10000 + InitialJavaScriptDateTicks, DateTimeKind.Utc);
	}

	internal static bool TryParseDateTimeIso(StringReference text, DateTimeZoneHandling dateTimeZoneHandling, out DateTime dt)
	{
		DateTimeParser dateTimeParser = default(DateTimeParser);
		if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
		{
			dt = default(DateTime);
			return false;
		}
		DateTime dateTime = CreateDateTime(dateTimeParser);
		switch (dateTimeParser.Zone)
		{
		case ParserTimeZone.Utc:
			dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
			break;
		case ParserTimeZone.LocalWestOfUtc:
		{
			TimeSpan timeSpan2 = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
			long num = dateTime.Ticks + timeSpan2.Ticks;
			if (num <= DateTime.MaxValue.Ticks)
			{
				dateTime = new DateTime(num, DateTimeKind.Utc).ToLocalTime();
				break;
			}
			num += dateTime.GetUtcOffset().Ticks;
			if (num > DateTime.MaxValue.Ticks)
			{
				num = DateTime.MaxValue.Ticks;
			}
			dateTime = new DateTime(num, DateTimeKind.Local);
			break;
		}
		case ParserTimeZone.LocalEastOfUtc:
		{
			TimeSpan timeSpan = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
			long num = dateTime.Ticks - timeSpan.Ticks;
			if (num >= DateTime.MinValue.Ticks)
			{
				dateTime = new DateTime(num, DateTimeKind.Utc).ToLocalTime();
				break;
			}
			num += dateTime.GetUtcOffset().Ticks;
			if (num < DateTime.MinValue.Ticks)
			{
				num = DateTime.MinValue.Ticks;
			}
			dateTime = new DateTime(num, DateTimeKind.Local);
			break;
		}
		}
		dt = EnsureDateTime(dateTime, dateTimeZoneHandling);
		return true;
	}

	internal static bool TryParseDateTimeOffsetIso(StringReference text, out DateTimeOffset dt)
	{
		DateTimeParser dateTimeParser = default(DateTimeParser);
		if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
		{
			dt = default(DateTimeOffset);
			return false;
		}
		DateTime dateTime = CreateDateTime(dateTimeParser);
		TimeSpan offset = dateTimeParser.Zone switch
		{
			ParserTimeZone.Utc => new TimeSpan(0L), 
			ParserTimeZone.LocalWestOfUtc => new TimeSpan(-dateTimeParser.ZoneHour, -dateTimeParser.ZoneMinute, 0), 
			ParserTimeZone.LocalEastOfUtc => new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0), 
			_ => TimeZoneInfo.Local.GetUtcOffset(dateTime), 
		};
		long num = dateTime.Ticks - offset.Ticks;
		if (num < 0 || num > 3155378975999999999L)
		{
			dt = default(DateTimeOffset);
			return false;
		}
		dt = new DateTimeOffset(dateTime, offset);
		return true;
	}

	private static DateTime CreateDateTime(DateTimeParser dateTimeParser)
	{
		bool flag;
		if (dateTimeParser.Hour == 24)
		{
			flag = true;
			dateTimeParser.Hour = 0;
		}
		else
		{
			flag = false;
		}
		DateTime result = new DateTime(dateTimeParser.Year, dateTimeParser.Month, dateTimeParser.Day, dateTimeParser.Hour, dateTimeParser.Minute, dateTimeParser.Second).AddTicks(dateTimeParser.Fraction);
		if (flag)
		{
			result = result.AddDays(1.0);
		}
		return result;
	}

	internal static bool TryParseDateTime(StringReference s, DateTimeZoneHandling dateTimeZoneHandling, string? dateFormatString, CultureInfo culture, out DateTime dt)
	{
		if (s.Length > 0)
		{
			int startIndex = s.StartIndex;
			if (s[startIndex] == '/')
			{
				if (s.Length >= 9 && s.StartsWith("/Date(") && s.EndsWith(")/") && TryParseDateTimeMicrosoft(s, dateTimeZoneHandling, out dt))
				{
					return true;
				}
			}
			else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[startIndex]) && s[startIndex + 10] == 'T' && TryParseDateTimeIso(s, dateTimeZoneHandling, out dt))
			{
				return true;
			}
			if (!StringUtils.IsNullOrEmpty(dateFormatString) && TryParseDateTimeExact(s.ToString(), dateTimeZoneHandling, dateFormatString, culture, out dt))
			{
				return true;
			}
		}
		dt = default(DateTime);
		return false;
	}

	internal static bool TryParseDateTime(string s, DateTimeZoneHandling dateTimeZoneHandling, string? dateFormatString, CultureInfo culture, out DateTime dt)
	{
		if (s.Length > 0)
		{
			if (s[0] == '/')
			{
				if (s.Length >= 9 && s.StartsWith("/Date(", StringComparison.Ordinal) && s.EndsWith(")/", StringComparison.Ordinal) && TryParseDateTimeMicrosoft(new StringReference(s.ToCharArray(), 0, s.Length), dateTimeZoneHandling, out dt))
				{
					return true;
				}
			}
			else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[0]) && s[10] == 'T' && DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
			{
				dt = EnsureDateTime(dt, dateTimeZoneHandling);
				return true;
			}
			if (!StringUtils.IsNullOrEmpty(dateFormatString) && TryParseDateTimeExact(s, dateTimeZoneHandling, dateFormatString, culture, out dt))
			{
				return true;
			}
		}
		dt = default(DateTime);
		return false;
	}

	internal static bool TryParseDateTimeOffset(StringReference s, string? dateFormatString, CultureInfo culture, out DateTimeOffset dt)
	{
		if (s.Length > 0)
		{
			int startIndex = s.StartIndex;
			if (s[startIndex] == '/')
			{
				if (s.Length >= 9 && s.StartsWith("/Date(") && s.EndsWith(")/") && TryParseDateTimeOffsetMicrosoft(s, out dt))
				{
					return true;
				}
			}
			else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[startIndex]) && s[startIndex + 10] == 'T' && TryParseDateTimeOffsetIso(s, out dt))
			{
				return true;
			}
			if (!StringUtils.IsNullOrEmpty(dateFormatString) && TryParseDateTimeOffsetExact(s.ToString(), dateFormatString, culture, out dt))
			{
				return true;
			}
		}
		dt = default(DateTimeOffset);
		return false;
	}

	internal static bool TryParseDateTimeOffset(string s, string? dateFormatString, CultureInfo culture, out DateTimeOffset dt)
	{
		if (s.Length > 0)
		{
			if (s[0] == '/')
			{
				if (s.Length >= 9 && s.StartsWith("/Date(", StringComparison.Ordinal) && s.EndsWith(")/", StringComparison.Ordinal) && TryParseDateTimeOffsetMicrosoft(new StringReference(s.ToCharArray(), 0, s.Length), out dt))
				{
					return true;
				}
			}
			else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[0]) && s[10] == 'T' && DateTimeOffset.TryParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt) && TryParseDateTimeOffsetIso(new StringReference(s.ToCharArray(), 0, s.Length), out dt))
			{
				return true;
			}
			if (!StringUtils.IsNullOrEmpty(dateFormatString) && TryParseDateTimeOffsetExact(s, dateFormatString, culture, out dt))
			{
				return true;
			}
		}
		dt = default(DateTimeOffset);
		return false;
	}

	private static bool TryParseMicrosoftDate(StringReference text, out long ticks, out TimeSpan offset, out DateTimeKind kind)
	{
		kind = DateTimeKind.Utc;
		int num = text.IndexOf('+', 7, text.Length - 8);
		if (num == -1)
		{
			num = text.IndexOf('-', 7, text.Length - 8);
		}
		if (num != -1)
		{
			kind = DateTimeKind.Local;
			if (!TryReadOffset(text, num + text.StartIndex, out offset))
			{
				ticks = 0L;
				return false;
			}
		}
		else
		{
			offset = TimeSpan.Zero;
			num = text.Length - 2;
		}
		return ConvertUtils.Int64TryParse(text.Chars, 6 + text.StartIndex, num - 6, out ticks) == ParseResult.Success;
	}

	private static bool TryParseDateTimeMicrosoft(StringReference text, DateTimeZoneHandling dateTimeZoneHandling, out DateTime dt)
	{
		if (!TryParseMicrosoftDate(text, out var ticks, out var _, out var kind))
		{
			dt = default(DateTime);
			return false;
		}
		DateTime dateTime = ConvertJavaScriptTicksToDateTime(ticks);
		switch (kind)
		{
		case DateTimeKind.Unspecified:
			dt = DateTime.SpecifyKind(dateTime.ToLocalTime(), DateTimeKind.Unspecified);
			break;
		case DateTimeKind.Local:
			dt = dateTime.ToLocalTime();
			break;
		default:
			dt = dateTime;
			break;
		}
		dt = EnsureDateTime(dt, dateTimeZoneHandling);
		return true;
	}

	private static bool TryParseDateTimeExact(string text, DateTimeZoneHandling dateTimeZoneHandling, string dateFormatString, CultureInfo culture, out DateTime dt)
	{
		if (DateTime.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out var result))
		{
			result = (dt = EnsureDateTime(result, dateTimeZoneHandling));
			return true;
		}
		dt = default(DateTime);
		return false;
	}

	private static bool TryParseDateTimeOffsetMicrosoft(StringReference text, out DateTimeOffset dt)
	{
		if (!TryParseMicrosoftDate(text, out var ticks, out var offset, out var _))
		{
			dt = default(DateTime);
			return false;
		}
		dt = new DateTimeOffset(ConvertJavaScriptTicksToDateTime(ticks).Add(offset).Ticks, offset);
		return true;
	}

	private static bool TryParseDateTimeOffsetExact(string text, string dateFormatString, CultureInfo culture, out DateTimeOffset dt)
	{
		if (DateTimeOffset.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out var result))
		{
			dt = result;
			return true;
		}
		dt = default(DateTimeOffset);
		return false;
	}

	private static bool TryReadOffset(StringReference offsetText, int startIndex, out TimeSpan offset)
	{
		bool flag = offsetText[startIndex] == '-';
		if (ConvertUtils.Int32TryParse(offsetText.Chars, startIndex + 1, 2, out var value) != ParseResult.Success)
		{
			offset = default(TimeSpan);
			return false;
		}
		int value2 = 0;
		if (offsetText.Length - startIndex > 5 && ConvertUtils.Int32TryParse(offsetText.Chars, startIndex + 3, 2, out value2) != ParseResult.Success)
		{
			offset = default(TimeSpan);
			return false;
		}
		offset = TimeSpan.FromHours(value) + TimeSpan.FromMinutes(value2);
		if (flag)
		{
			offset = offset.Negate();
		}
		return true;
	}

	internal static void WriteDateTimeString(TextWriter writer, DateTime value, DateFormatHandling format, string? formatString, CultureInfo culture)
	{
		if (StringUtils.IsNullOrEmpty(formatString))
		{
			char[] array = new char[64];
			int count = WriteDateTimeString(array, 0, value, null, value.Kind, format);
			writer.Write(array, 0, count);
		}
		else
		{
			writer.Write(value.ToString(formatString, culture));
		}
	}

	internal static int WriteDateTimeString(char[] chars, int start, DateTime value, TimeSpan? offset, DateTimeKind kind, DateFormatHandling format)
	{
		int num = start;
		if (format == DateFormatHandling.MicrosoftDateFormat)
		{
			TimeSpan offset2 = offset ?? value.GetUtcOffset();
			long num2 = ConvertDateTimeToJavaScriptTicks(value, offset2);
			"\\/Date(".CopyTo(0, chars, num, 7);
			num += 7;
			string text = num2.ToString(CultureInfo.InvariantCulture);
			text.CopyTo(0, chars, num, text.Length);
			num += text.Length;
			switch (kind)
			{
			case DateTimeKind.Unspecified:
				if (value != DateTime.MaxValue && value != DateTime.MinValue)
				{
					num = WriteDateTimeOffset(chars, num, offset2, format);
				}
				break;
			case DateTimeKind.Local:
				num = WriteDateTimeOffset(chars, num, offset2, format);
				break;
			}
			")\\/".CopyTo(0, chars, num, 3);
			num += 3;
		}
		else
		{
			num = WriteDefaultIsoDate(chars, num, value);
			switch (kind)
			{
			case DateTimeKind.Local:
				num = WriteDateTimeOffset(chars, num, offset ?? value.GetUtcOffset(), format);
				break;
			case DateTimeKind.Utc:
				chars[num++] = 'Z';
				break;
			}
		}
		return num;
	}

	internal static int WriteDefaultIsoDate(char[] chars, int start, DateTime dt)
	{
		int num = 19;
		GetDateValues(dt, out var year, out var month, out var day);
		CopyIntToCharArray(chars, start, year, 4);
		chars[start + 4] = '-';
		CopyIntToCharArray(chars, start + 5, month, 2);
		chars[start + 7] = '-';
		CopyIntToCharArray(chars, start + 8, day, 2);
		chars[start + 10] = 'T';
		CopyIntToCharArray(chars, start + 11, dt.Hour, 2);
		chars[start + 13] = ':';
		CopyIntToCharArray(chars, start + 14, dt.Minute, 2);
		chars[start + 16] = ':';
		CopyIntToCharArray(chars, start + 17, dt.Second, 2);
		int num2 = (int)(dt.Ticks % 10000000);
		if (num2 != 0)
		{
			int num3 = 7;
			while (num2 % 10 == 0)
			{
				num3--;
				num2 /= 10;
			}
			chars[start + 19] = '.';
			CopyIntToCharArray(chars, start + 20, num2, num3);
			num += num3 + 1;
		}
		return start + num;
	}

	private static void CopyIntToCharArray(char[] chars, int start, int value, int digits)
	{
		while (digits-- != 0)
		{
			chars[start + digits] = (char)(value % 10 + 48);
			value /= 10;
		}
	}

	internal static int WriteDateTimeOffset(char[] chars, int start, TimeSpan offset, DateFormatHandling format)
	{
		chars[start++] = ((offset.Ticks >= 0) ? '+' : '-');
		int value = Math.Abs(offset.Hours);
		CopyIntToCharArray(chars, start, value, 2);
		start += 2;
		if (format == DateFormatHandling.IsoDateFormat)
		{
			chars[start++] = ':';
		}
		int value2 = Math.Abs(offset.Minutes);
		CopyIntToCharArray(chars, start, value2, 2);
		start += 2;
		return start;
	}

	internal static void WriteDateTimeOffsetString(TextWriter writer, DateTimeOffset value, DateFormatHandling format, string? formatString, CultureInfo culture)
	{
		if (StringUtils.IsNullOrEmpty(formatString))
		{
			char[] array = new char[64];
			int count = WriteDateTimeString(array, 0, (format == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, format);
			writer.Write(array, 0, count);
		}
		else
		{
			writer.Write(value.ToString(formatString, culture));
		}
	}

	private static void GetDateValues(DateTime td, out int year, out int month, out int day)
	{
		int num = (int)(td.Ticks / 864000000000L);
		int num2 = num / 146097;
		num -= num2 * 146097;
		int num3 = num / 36524;
		if (num3 == 4)
		{
			num3 = 3;
		}
		num -= num3 * 36524;
		int num4 = num / 1461;
		num -= num4 * 1461;
		int num5 = num / 365;
		if (num5 == 4)
		{
			num5 = 3;
		}
		year = num2 * 400 + num3 * 100 + num4 * 4 + num5 + 1;
		num -= num5 * 365;
		int[] array = ((num5 == 3 && (num4 != 24 || num3 == 3)) ? DaysToMonth366 : DaysToMonth365);
		int i;
		for (i = num >> 6; num >= array[i]; i++)
		{
		}
		month = i;
		day = num - array[i - 1] + 1;
	}
}
