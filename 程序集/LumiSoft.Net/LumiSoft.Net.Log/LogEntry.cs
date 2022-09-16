using System;
using System.Net;
using System.Security.Principal;

namespace LumiSoft.Net.Log;

public class LogEntry
{
	private LogEntryType m_Type = LogEntryType.Text;

	private string m_ID = "";

	private DateTime m_Time;

	private GenericIdentity m_pUserIdentity;

	private long m_Size;

	private string m_Text = "";

	private Exception m_pException;

	private IPEndPoint m_pLocalEP;

	private IPEndPoint m_pRemoteEP;

	private byte[] m_pData;

	public LogEntryType EntryType => m_Type;

	public string ID => m_ID;

	public DateTime Time => m_Time;

	public GenericIdentity UserIdentity => m_pUserIdentity;

	public long Size => m_Size;

	public string Text => m_Text;

	public Exception Exception => m_pException;

	public IPEndPoint LocalEndPoint => m_pLocalEP;

	public IPEndPoint RemoteEndPoint => m_pRemoteEP;

	public byte[] Data => m_pData;

	public LogEntry(LogEntryType type, string id, long size, string text)
	{
		m_Type = type;
		m_ID = id;
		m_Size = size;
		m_Text = text;
		m_Time = DateTime.Now;
	}

	public LogEntry(LogEntryType type, string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP, byte[] data)
	{
		m_Type = type;
		m_ID = id;
		m_pUserIdentity = userIdentity;
		m_Size = size;
		m_Text = text;
		m_pLocalEP = localEP;
		m_pRemoteEP = remoteEP;
		m_pData = data;
		m_Time = DateTime.Now;
	}

	public LogEntry(LogEntryType type, string id, GenericIdentity userIdentity, long size, string text, IPEndPoint localEP, IPEndPoint remoteEP, Exception exception)
	{
		m_Type = type;
		m_ID = id;
		m_pUserIdentity = userIdentity;
		m_Size = size;
		m_Text = text;
		m_pLocalEP = localEP;
		m_pRemoteEP = remoteEP;
		m_pException = exception;
		m_Time = DateTime.Now;
	}
}
