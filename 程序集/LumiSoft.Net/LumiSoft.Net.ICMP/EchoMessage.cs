using System;
using System.Net;

namespace LumiSoft.Net.ICMP;

public class EchoMessage
{
	private IPAddress m_pIP;

	private int m_TTL;

	private int m_Time;

	public IPAddress IPAddress => m_pIP;

	public int ReplyTime => m_Time;

	internal EchoMessage(IPAddress ip, int ttl, int time)
	{
		m_pIP = ip;
		m_TTL = ttl;
		m_Time = time;
	}

	[Obsolete("Will be removed !")]
	public string ToStringEx()
	{
		return "TTL=" + m_TTL + "\tTime=" + m_Time + "ms\tIP=" + m_pIP;
	}

	[Obsolete("Will be removed !")]
	public static string ToStringEx(EchoMessage[] messages)
	{
		string text = "";
		foreach (EchoMessage echoMessage in messages)
		{
			text = text + echoMessage.ToStringEx() + "\r\n";
		}
		return text;
	}
}
