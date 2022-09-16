using System;
using System.Globalization;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerUtcTime : Asn1Object
{
	private readonly string time;

	public string TimeString
	{
		get
		{
			if (time.IndexOf('-') < 0 && time.IndexOf('+') < 0)
			{
				if (time.Length == 11)
				{
					return time.Substring(0, 10) + "00GMT+00:00";
				}
				return time.Substring(0, 12) + "GMT+00:00";
			}
			int num = time.IndexOf('-');
			if (num < 0)
			{
				num = time.IndexOf('+');
			}
			string text = time;
			if (num == time.Length - 3)
			{
				text += "00";
			}
			if (num == 10)
			{
				return text.Substring(0, 10) + "00GMT" + text.Substring(10, 3) + ":" + text.Substring(13, 2);
			}
			return text.Substring(0, 12) + "GMT" + text.Substring(12, 3) + ":" + text.Substring(15, 2);
		}
	}

	[Obsolete("Use 'AdjustedTimeString' property instead")]
	public string AdjustedTime => AdjustedTimeString;

	public string AdjustedTimeString
	{
		get
		{
			string timeString = TimeString;
			string text = ((timeString[0] < '5') ? "20" : "19");
			return text + timeString;
		}
	}

	public static DerUtcTime GetInstance(object obj)
	{
		if (obj == null || obj is DerUtcTime)
		{
			return (DerUtcTime)obj;
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
	}

	public static DerUtcTime GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		Asn1Object @object = obj.GetObject();
		if (isExplicit || @object is DerUtcTime)
		{
			return GetInstance(@object);
		}
		return new DerUtcTime(((Asn1OctetString)@object).GetOctets());
	}

	public DerUtcTime(string time)
	{
		if (time == null)
		{
			throw new ArgumentNullException("time");
		}
		this.time = time;
		try
		{
			ToDateTime();
		}
		catch (FormatException ex)
		{
			throw new ArgumentException("invalid date string: " + ex.Message);
		}
	}

	public DerUtcTime(DateTime time)
	{
		this.time = time.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture) + "Z";
	}

	internal DerUtcTime(byte[] bytes)
	{
		time = Strings.FromAsciiByteArray(bytes);
	}

	public DateTime ToDateTime()
	{
		return ParseDateString(TimeString, "yyMMddHHmmss'GMT'zzz");
	}

	public DateTime ToAdjustedDateTime()
	{
		return ParseDateString(AdjustedTimeString, "yyyyMMddHHmmss'GMT'zzz");
	}

	private DateTime ParseDateString(string dateStr, string formatStr)
	{
		return DateTime.ParseExact(dateStr, formatStr, DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
	}

	private byte[] GetOctets()
	{
		return Strings.ToAsciiByteArray(time);
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(23, GetOctets());
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerUtcTime derUtcTime))
		{
			return false;
		}
		return time.Equals(derUtcTime.time);
	}

	protected override int Asn1GetHashCode()
	{
		return time.GetHashCode();
	}

	public override string ToString()
	{
		return time;
	}
}
