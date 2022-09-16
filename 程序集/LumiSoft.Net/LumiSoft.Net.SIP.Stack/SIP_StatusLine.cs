using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_StatusLine
{
	private string m_Version = "";

	private int m_StatusCode;

	private string m_Reason = "";

	public string Version
	{
		get
		{
			return m_Version;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Version");
			}
			if (value == "")
			{
				throw new ArgumentException("Property 'Version' value must be specified.");
			}
			m_Version = value;
		}
	}

	public int StatusCode
	{
		get
		{
			return m_StatusCode;
		}
		set
		{
			if (value < 100 || value > 699)
			{
				throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
			}
			m_StatusCode = value;
		}
	}

	public string Reason
	{
		get
		{
			return m_Reason;
		}
		set
		{
			if (Reason == null)
			{
				throw new ArgumentNullException("Reason");
			}
			m_Reason = value;
		}
	}

	public SIP_StatusLine(int statusCode, string reason)
	{
		if (statusCode < 100 || statusCode > 699)
		{
			throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
		}
		if (reason == null)
		{
			throw new ArgumentNullException("reason");
		}
		m_Version = "SIP/2.0";
		m_StatusCode = statusCode;
		m_Reason = reason;
	}

	public override string ToString()
	{
		return m_Version + " " + m_StatusCode + " " + m_Reason + "\r\n";
	}
}
