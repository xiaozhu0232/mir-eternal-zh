using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_TransportException : Exception
{
	public SIP_TransportException(string errorText)
		: base(errorText)
	{
	}
}
