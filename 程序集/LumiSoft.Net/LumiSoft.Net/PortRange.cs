using System;

namespace LumiSoft.Net;

public class PortRange
{
	private int m_Start = 1000;

	private int m_End = 1100;

	public int Start => m_Start;

	public int End => m_End;

	public PortRange(int start, int end)
	{
		if (start < 1 || start > 65535)
		{
			throw new ArgumentOutOfRangeException("Argument 'start' value must be > 0 and << 65 535.");
		}
		if (end < 1 || end > 65535)
		{
			throw new ArgumentOutOfRangeException("Argument 'end' value must be > 0 and << 65 535.");
		}
		if (start > end)
		{
			throw new ArgumentOutOfRangeException("Argumnet 'start' value must be >= argument 'end' value.");
		}
		m_Start = start;
		m_End = end;
	}
}
