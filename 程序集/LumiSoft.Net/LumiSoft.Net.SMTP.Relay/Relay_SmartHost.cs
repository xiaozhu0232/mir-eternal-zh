using System;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_SmartHost
{
	private string m_Host = "";

	private int m_Port = 25;

	private SslMode m_SslMode;

	private string m_UserName;

	private string m_Password;

	public string Host => m_Host;

	public int Port => m_Port;

	public SslMode SslMode => m_SslMode;

	public string UserName => m_UserName;

	public string Password => m_Password;

	public Relay_SmartHost(string host, int port)
		: this(host, port, SslMode.None, null, null)
	{
	}

	public Relay_SmartHost(string host, int port, SslMode sslMode, string userName, string password)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (host == "")
		{
			throw new ArgumentException("Argument 'host' value must be specified.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Argument 'port' value is invalid.");
		}
		m_Host = host;
		m_Port = port;
		m_SslMode = sslMode;
		m_UserName = userName;
		m_Password = password;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Relay_SmartHost))
		{
			return false;
		}
		Relay_SmartHost relay_SmartHost = (Relay_SmartHost)obj;
		if (m_Host != relay_SmartHost.Host)
		{
			return false;
		}
		if (m_Port != relay_SmartHost.Port)
		{
			return false;
		}
		if (m_SslMode != relay_SmartHost.SslMode)
		{
			return false;
		}
		if (m_UserName != relay_SmartHost.UserName)
		{
			return false;
		}
		if (m_Password != relay_SmartHost.Password)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
