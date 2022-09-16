using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_RequestLine
{
	private string m_Method = "";

	private AbsoluteUri m_pUri;

	private string m_Version = "";

	public string Method
	{
		get
		{
			return m_Method;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Method");
			}
			if (!SIP_Utils.IsToken(value))
			{
				throw new ArgumentException("Property 'Method' value must be token.");
			}
			m_Method = value.ToUpper();
		}
	}

	public AbsoluteUri Uri
	{
		get
		{
			return m_pUri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Uri");
			}
			m_pUri = value;
		}
	}

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

	public SIP_RequestLine(string method, AbsoluteUri uri)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (!SIP_Utils.IsToken(method))
		{
			throw new ArgumentException("Argument 'method' value must be token.");
		}
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		m_Method = method.ToUpper();
		m_pUri = uri;
		m_Version = "SIP/2.0";
	}

	public override string ToString()
	{
		return m_Method + " " + m_pUri.ToString() + " " + m_Version + "\r\n";
	}
}
