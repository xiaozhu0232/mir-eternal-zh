using System.Timers;

namespace LumiSoft.Net;

public class TimerEx : Timer
{
	public TimerEx()
	{
	}

	public TimerEx(double interval)
		: base(interval)
	{
	}

	public TimerEx(double interval, bool autoReset)
		: base(interval)
	{
		base.AutoReset = autoReset;
	}
}
