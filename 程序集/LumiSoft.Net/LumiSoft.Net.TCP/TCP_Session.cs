using System;
using System.Net;
using System.Security.Principal;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.TCP;

public abstract class TCP_Session : IDisposable
{
	public abstract bool IsConnected { get; }

	public abstract string ID { get; }

	public abstract DateTime ConnectTime { get; }

	public abstract DateTime LastActivity { get; }

	public abstract IPEndPoint LocalEndPoint { get; }

	public abstract IPEndPoint RemoteEndPoint { get; }

	public virtual bool IsSecureConnection => false;

	public bool IsAuthenticated => AuthenticatedUserIdentity != null;

	public virtual GenericIdentity AuthenticatedUserIdentity => null;

	public abstract SmartStream TcpStream { get; }

	public TCP_Session()
	{
	}

	public abstract void Dispose();

	public abstract void Disconnect();
}
