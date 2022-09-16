using System;
using System.Net;

namespace LumiSoft.Net.RTP;

public class RTP_Address
{
	private IPAddress m_pIP;

	private int m_DataPort;

	private int m_ControlPort;

	private int m_TTL;

	private IPEndPoint m_pRtpEP;

	private IPEndPoint m_pRtcpEP;

	public bool IsMulticast => Net_Utils.IsMulticastAddress(m_pIP);

	public IPAddress IP => m_pIP;

	public int DataPort => m_DataPort;

	public int ControlPort => m_ControlPort;

	public int TTL => m_TTL;

	public IPEndPoint RtpEP => m_pRtpEP;

	public IPEndPoint RtcpEP => m_pRtcpEP;

	public RTP_Address(IPAddress ip, int dataPort, int controlPort)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (dataPort < 0 || dataPort > 65535)
		{
			throw new ArgumentException("Argument 'dataPort' value must be between '" + 0 + "' and '" + 65535 + "'.");
		}
		if (controlPort < 0 || controlPort > 65535)
		{
			throw new ArgumentException("Argument 'controlPort' value must be between '" + 0 + "' and '" + 65535 + "'.");
		}
		if (dataPort == controlPort)
		{
			throw new ArgumentException("Arguments 'dataPort' and 'controlPort' values must be different.");
		}
		m_pIP = ip;
		m_DataPort = dataPort;
		m_ControlPort = controlPort;
		m_pRtpEP = new IPEndPoint(ip, dataPort);
		m_pRtcpEP = new IPEndPoint(ip, controlPort);
	}

	public RTP_Address(IPAddress ip, int dataPort, int controlPort, int ttl)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (!Net_Utils.IsMulticastAddress(ip))
		{
			throw new ArgumentException("Argument 'ip' is not multicast ip address.");
		}
		if (dataPort < 0 || dataPort > 65535)
		{
			throw new ArgumentException("Argument 'dataPort' value must be between '" + 0 + "' and '" + 65535 + "'.");
		}
		if (controlPort < 0 || controlPort > 65535)
		{
			throw new ArgumentException("Argument 'controlPort' value must be between '" + 0 + "' and '" + 65535 + "'.");
		}
		if (dataPort == controlPort)
		{
			throw new ArgumentException("Arguments 'dataPort' and 'controlPort' values must be different.");
		}
		if (ttl < 0 || ttl > 255)
		{
			throw new ArgumentException("Argument 'ttl' value must be between '0' and '255'.");
		}
		m_pIP = ip;
		m_DataPort = dataPort;
		m_ControlPort = controlPort;
		m_TTL = ttl;
		m_pRtpEP = new IPEndPoint(ip, dataPort);
		m_pRtcpEP = new IPEndPoint(ip, controlPort);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is RTP_Address)
		{
			RTP_Address rTP_Address = (RTP_Address)obj;
			if (rTP_Address.IP.Equals(IP) && rTP_Address.ControlPort == ControlPort && rTP_Address.DataPort == DataPort)
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
