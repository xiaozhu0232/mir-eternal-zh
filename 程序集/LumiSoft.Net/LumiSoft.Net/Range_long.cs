namespace LumiSoft.Net;

public class Range_long
{
	private long m_Start;

	private long m_End;

	public long Start => m_Start;

	public long End => m_End;

	public Range_long(long value)
	{
		m_Start = value;
		m_End = value;
	}

	public Range_long(long start, long end)
	{
		m_Start = start;
		m_End = end;
	}

	public bool Contains(long value)
	{
		if (value >= m_Start && value <= m_End)
		{
			return true;
		}
		return false;
	}
}
