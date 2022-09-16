using System;

namespace Newtonsoft.Json.Utilities;

internal struct DateTimeParser
{
	public int Year;

	public int Month;

	public int Day;

	public int Hour;

	public int Minute;

	public int Second;

	public int Fraction;

	public int ZoneHour;

	public int ZoneMinute;

	public ParserTimeZone Zone;

	private char[] _text;

	private int _end;

	private static readonly int[] Power10;

	private static readonly int Lzyyyy;

	private static readonly int Lzyyyy_;

	private static readonly int Lzyyyy_MM;

	private static readonly int Lzyyyy_MM_;

	private static readonly int Lzyyyy_MM_dd;

	private static readonly int Lzyyyy_MM_ddT;

	private static readonly int LzHH;

	private static readonly int LzHH_;

	private static readonly int LzHH_mm;

	private static readonly int LzHH_mm_;

	private static readonly int LzHH_mm_ss;

	private static readonly int Lz_;

	private static readonly int Lz_zz;

	private const short MaxFractionDigits = 7;

	static DateTimeParser()
	{
		Power10 = new int[7] { -1, 10, 100, 1000, 10000, 100000, 1000000 };
		Lzyyyy = "yyyy".Length;
		Lzyyyy_ = "yyyy-".Length;
		Lzyyyy_MM = "yyyy-MM".Length;
		Lzyyyy_MM_ = "yyyy-MM-".Length;
		Lzyyyy_MM_dd = "yyyy-MM-dd".Length;
		Lzyyyy_MM_ddT = "yyyy-MM-ddT".Length;
		LzHH = "HH".Length;
		LzHH_ = "HH:".Length;
		LzHH_mm = "HH:mm".Length;
		LzHH_mm_ = "HH:mm:".Length;
		LzHH_mm_ss = "HH:mm:ss".Length;
		Lz_ = "-".Length;
		Lz_zz = "-zz".Length;
	}

	public bool Parse(char[] text, int startIndex, int length)
	{
		_text = text;
		_end = startIndex + length;
		if (ParseDate(startIndex) && ParseChar(Lzyyyy_MM_dd + startIndex, 'T') && ParseTimeAndZoneAndWhitespace(Lzyyyy_MM_ddT + startIndex))
		{
			return true;
		}
		return false;
	}

	private bool ParseDate(int start)
	{
		if (Parse4Digit(start, out Year) && 1 <= Year && ParseChar(start + Lzyyyy, '-') && Parse2Digit(start + Lzyyyy_, out Month) && 1 <= Month && Month <= 12 && ParseChar(start + Lzyyyy_MM, '-') && Parse2Digit(start + Lzyyyy_MM_, out Day) && 1 <= Day)
		{
			return Day <= DateTime.DaysInMonth(Year, Month);
		}
		return false;
	}

	private bool ParseTimeAndZoneAndWhitespace(int start)
	{
		if (ParseTime(ref start))
		{
			return ParseZone(start);
		}
		return false;
	}

	private bool ParseTime(ref int start)
	{
		if (!Parse2Digit(start, out Hour) || Hour > 24 || !ParseChar(start + LzHH, ':') || !Parse2Digit(start + LzHH_, out Minute) || Minute >= 60 || !ParseChar(start + LzHH_mm, ':') || !Parse2Digit(start + LzHH_mm_, out Second) || Second >= 60 || (Hour == 24 && (Minute != 0 || Second != 0)))
		{
			return false;
		}
		start += LzHH_mm_ss;
		if (ParseChar(start, '.'))
		{
			Fraction = 0;
			int num = 0;
			while (++start < _end && num < 7)
			{
				int num2 = _text[start] - 48;
				if (num2 < 0 || num2 > 9)
				{
					break;
				}
				Fraction = Fraction * 10 + num2;
				num++;
			}
			if (num < 7)
			{
				if (num == 0)
				{
					return false;
				}
				Fraction *= Power10[7 - num];
			}
			if (Hour == 24 && Fraction != 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool ParseZone(int start)
	{
		if (start < _end)
		{
			char c = _text[start];
			if (c == 'Z' || c == 'z')
			{
				Zone = ParserTimeZone.Utc;
				start++;
			}
			else
			{
				if (start + 2 < _end && Parse2Digit(start + Lz_, out ZoneHour) && ZoneHour <= 99)
				{
					switch (c)
					{
					case '-':
						Zone = ParserTimeZone.LocalWestOfUtc;
						start += Lz_zz;
						break;
					case '+':
						Zone = ParserTimeZone.LocalEastOfUtc;
						start += Lz_zz;
						break;
					}
				}
				if (start < _end)
				{
					if (ParseChar(start, ':'))
					{
						start++;
						if (start + 1 < _end && Parse2Digit(start, out ZoneMinute) && ZoneMinute <= 99)
						{
							start += 2;
						}
					}
					else if (start + 1 < _end && Parse2Digit(start, out ZoneMinute) && ZoneMinute <= 99)
					{
						start += 2;
					}
				}
			}
		}
		return start == _end;
	}

	private bool Parse4Digit(int start, out int num)
	{
		if (start + 3 < _end)
		{
			int num2 = _text[start] - 48;
			int num3 = _text[start + 1] - 48;
			int num4 = _text[start + 2] - 48;
			int num5 = _text[start + 3] - 48;
			if (0 <= num2 && num2 < 10 && 0 <= num3 && num3 < 10 && 0 <= num4 && num4 < 10 && 0 <= num5 && num5 < 10)
			{
				num = ((num2 * 10 + num3) * 10 + num4) * 10 + num5;
				return true;
			}
		}
		num = 0;
		return false;
	}

	private bool Parse2Digit(int start, out int num)
	{
		if (start + 1 < _end)
		{
			int num2 = _text[start] - 48;
			int num3 = _text[start + 1] - 48;
			if (0 <= num2 && num2 < 10 && 0 <= num3 && num3 < 10)
			{
				num = num2 * 10 + num3;
				return true;
			}
		}
		num = 0;
		return false;
	}

	private bool ParseChar(int start, char ch)
	{
		if (start < _end)
		{
			return _text[start] == ch;
		}
		return false;
	}
}
