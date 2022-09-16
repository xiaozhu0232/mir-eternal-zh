using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LumiSoft.Net;

public class IPBindInfo
{
	private string m_HostName = "";

	private BindInfoProtocol m_Protocol;

	private IPEndPoint m_pEndPoint;

	private SslMode m_SslMode;

	private X509Certificate2 m_pCertificate;

	private object m_Tag;

	public string HostName => m_HostName;

	public BindInfoProtocol Protocol => m_Protocol;

	public IPEndPoint EndPoint => m_pEndPoint;

	public IPAddress IP => m_pEndPoint.Address;

	public int Port => m_pEndPoint.Port;

	public SslMode SslMode => m_SslMode;

	[Obsolete("Use property Certificate instead.")]
	public X509Certificate2 SSL_Certificate => m_pCertificate;

	public X509Certificate2 Certificate => m_pCertificate;

	public object Tag
	{
		get
		{
			return m_Tag;
		}
		set
		{
			m_Tag = value;
		}
	}

	public IPBindInfo(string hostName, BindInfoProtocol protocol, IPAddress ip, int port)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		m_HostName = hostName;
		m_Protocol = protocol;
		m_pEndPoint = new IPEndPoint(ip, port);
	}

	public IPBindInfo(string hostName, IPAddress ip, int port, SslMode sslMode, X509Certificate2 sslCertificate)
		: this(hostName, BindInfoProtocol.TCP, ip, port, sslMode, sslCertificate)
	{
	}

	public IPBindInfo(string hostName, BindInfoProtocol protocol, IPAddress ip, int port, SslMode sslMode, X509Certificate2 sslCertificate)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		m_HostName = hostName;
		m_Protocol = protocol;
		m_pEndPoint = new IPEndPoint(ip, port);
		m_SslMode = sslMode;
		m_pCertificate = sslCertificate;
		if ((sslMode == SslMode.SSL || sslMode == SslMode.TLS) && sslCertificate == null)
		{
			throw new ArgumentException("SSL requested, but argument 'sslCertificate' is not provided.");
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is IPBindInfo))
		{
			return false;
		}
		IPBindInfo iPBindInfo = (IPBindInfo)obj;
		if (iPBindInfo.HostName != m_HostName)
		{
			return false;
		}
		if (iPBindInfo.Protocol != m_Protocol)
		{
			return false;
		}
		if (!iPBindInfo.EndPoint.Equals(m_pEndPoint))
		{
			return false;
		}
		if (iPBindInfo.SslMode != m_SslMode)
		{
			return false;
		}
		if (!object.Equals(iPBindInfo.Certificate, m_pCertificate))
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
