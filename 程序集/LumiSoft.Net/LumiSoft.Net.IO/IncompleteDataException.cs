using System;

namespace LumiSoft.Net.IO;

public class IncompleteDataException : Exception
{
	public IncompleteDataException()
	{
	}

	public IncompleteDataException(string message)
		: base(message)
	{
	}
}
