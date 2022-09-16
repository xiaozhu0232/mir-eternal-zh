using System;

namespace LumiSoft.Net;

public class ParseException : Exception
{
	public ParseException(string message)
		: base(message)
	{
	}
}
