using System;

namespace LumiSoft.Net;

public class ExceptionEventArgs : EventArgs
{
	private Exception m_pException;

	public Exception Exception => m_pException;

	public ExceptionEventArgs(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		m_pException = exception;
	}
}
