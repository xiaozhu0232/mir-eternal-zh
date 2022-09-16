using System;

namespace LumiSoft.Net.SDP;

public class SDP_Time
{
	private long m_StartTime;

	private long m_StopTime;

	public long StartTime
	{
		get
		{
			return m_StartTime;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Property StartTime value must be >= 0 !");
			}
			m_StopTime = value;
		}
	}

	public long StopTime
	{
		get
		{
			return m_StopTime;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Property StopTime value must be >= 0 !");
			}
			m_StopTime = value;
		}
	}

	public SDP_Time(long startTime, long stopTime)
	{
		if (startTime < 0)
		{
			throw new ArgumentException("Argument 'startTime' value must be >= 0.");
		}
		if (stopTime < 0)
		{
			throw new ArgumentException("Argument 'stopTime' value must be >= 0.");
		}
		m_StartTime = startTime;
		m_StopTime = stopTime;
	}

	public static SDP_Time Parse(string tValue)
	{
		long num = 0L;
		long num2 = 0L;
		StringReader stringReader = new StringReader(tValue);
		stringReader.QuotedReadToDelimiter('=');
		num = Convert.ToInt64(stringReader.ReadWord() ?? throw new Exception("SDP message \"t\" field <start-time> value is missing !"));
		num2 = Convert.ToInt64(stringReader.ReadWord() ?? throw new Exception("SDP message \"t\" field <stop-time> value is missing !"));
		return new SDP_Time(num, num2);
	}

	public string ToValue()
	{
		return "t=" + StartTime + " " + StopTime + "\r\n";
	}
}
