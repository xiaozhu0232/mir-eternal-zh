using System;

namespace LumiSoft.Net.TCP;

public class TCP_ServerSessionEventArgs<T> : EventArgs where T : TCP_ServerSession, new()
{
	private TCP_Server<T> m_pServer;

	private T m_pSession;

	public TCP_Server<T> Server => m_pServer;

	public T Session => m_pSession;

	internal TCP_ServerSessionEventArgs(TCP_Server<T> server, T session)
	{
		m_pServer = server;
		m_pSession = session;
	}
}
