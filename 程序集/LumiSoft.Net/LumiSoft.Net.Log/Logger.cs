using System;
using System.Net;
using System.Security.Principal;

namespace LumiSoft.Net.Log;

public class Logger : IDisposable
{
	public event EventHandler<WriteLogEventArgs> WriteLog;

	public void Dispose()
	{
	}

	public void AddRead(long size, string text)
	{
		OnWriteLog(new LogEntry(LogEntryType.Read, "", size, text));
	}

	public void AddRead(string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP)
	{
		OnWriteLog(new LogEntry(LogEntryType.Read, id, userIdentity, size, text, localEP, remoteEP, (byte[])null));
	}

	public void AddRead(string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP, byte[] data)
	{
		OnWriteLog(new LogEntry(LogEntryType.Read, id, userIdentity, size, text, localEP, remoteEP, data));
	}

	public void AddWrite(long size, string text)
	{
		OnWriteLog(new LogEntry(LogEntryType.Write, "", size, text));
	}

	public void AddWrite(string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP)
	{
		OnWriteLog(new LogEntry(LogEntryType.Write, id, userIdentity, size, text, localEP, remoteEP, (byte[])null));
	}

	public void AddWrite(string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP, byte[] data)
	{
		OnWriteLog(new LogEntry(LogEntryType.Write, id, userIdentity, size, text, localEP, remoteEP, data));
	}

	public void AddText(string text)
	{
		OnWriteLog(new LogEntry(LogEntryType.Text, "", 0L, text));
	}

	public void AddText(string id, string text)
	{
		OnWriteLog(new LogEntry(LogEntryType.Text, id, 0L, text));
	}

	public void AddText(string id, GenericIdentity userIdentity, string text, IPEndPoint localEP, IPEndPoint remoteEP)
	{
		OnWriteLog(new LogEntry(LogEntryType.Text, id, userIdentity, 0L, text, localEP, remoteEP, (byte[])null));
	}

	public void AddException(string id, GenericIdentity userIdentity, string text, IPEndPoint localEP, IPEndPoint remoteEP, Exception exception)
	{
		OnWriteLog(new LogEntry(LogEntryType.Exception, id, userIdentity, 0L, text, localEP, remoteEP, exception));
	}

	private void OnWriteLog(LogEntry entry)
	{
		if (this.WriteLog != null)
		{
			this.WriteLog(this, new WriteLogEventArgs(entry));
		}
	}
}
