using System;

namespace LumiSoft.Net.Log;

public class WriteLogEventArgs : EventArgs
{
	private LogEntry m_pLogEntry;

	public LogEntry LogEntry => m_pLogEntry;

	public WriteLogEventArgs(LogEntry logEntry)
	{
		if (logEntry == null)
		{
			throw new ArgumentNullException("logEntry");
		}
		m_pLogEntry = logEntry;
	}
}
