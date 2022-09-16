using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Dialog_Subscribe
{
	internal SIP_Dialog_Subscribe()
	{
	}

	public void Notify(SIP_Request notify)
	{
		if (notify == null)
		{
			throw new ArgumentNullException("notify");
		}
	}
}
