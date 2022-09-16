using System;
using System.Diagnostics;

namespace LumiSoft.Net;

public class Error_EventArgs
{
	private Exception m_pException;

	private StackTrace m_pStackTrace;

	private string m_Text = "";

	public Exception Exception => m_pException;

	public StackTrace StackTrace => m_pStackTrace;

	public string Text => m_Text;

	public Error_EventArgs(Exception x, StackTrace stackTrace)
	{
		m_pException = x;
		m_pStackTrace = stackTrace;
	}
}
