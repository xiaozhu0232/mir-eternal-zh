using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_ParseException : Exception
{
	public SIP_ParseException(string message)
		: base(message)
	{
	}
}
