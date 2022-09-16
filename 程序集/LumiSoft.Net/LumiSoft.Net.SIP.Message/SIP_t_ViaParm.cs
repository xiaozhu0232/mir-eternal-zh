using System;
using System.Net;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ViaParm : SIP_t_ValueWithParams
{
	private string m_ProtocolName = "";

	private string m_ProtocolVersion = "";

	private string m_ProtocolTransport = "";

	private HostEndPoint m_pSentBy;

	public string ProtocolName
	{
		get
		{
			return m_ProtocolName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property ProtocolName can't be null or empty !");
			}
			m_ProtocolName = value;
		}
	}

	public string ProtocolVersion
	{
		get
		{
			return m_ProtocolVersion;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property ProtocolVersion can't be null or empty !");
			}
			m_ProtocolVersion = value;
		}
	}

	public string ProtocolTransport
	{
		get
		{
			return m_ProtocolTransport.ToUpper();
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property ProtocolTransport can't be null or empty !");
			}
			m_ProtocolTransport = value;
		}
	}

	public HostEndPoint SentBy
	{
		get
		{
			return m_pSentBy;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pSentBy = value;
		}
	}

	public int SentByPortWithDefault
	{
		get
		{
			if (m_pSentBy.Port != -1)
			{
				return m_pSentBy.Port;
			}
			if (ProtocolTransport == "TLS")
			{
				return 5061;
			}
			return 5060;
		}
	}

	public string Branch
	{
		get
		{
			return base.Parameters["branch"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("branch");
				return;
			}
			if (!value.StartsWith("z9hG4bK"))
			{
				throw new ArgumentException("Property Branch value must start with magic cookie 'z9hG4bK' !");
			}
			base.Parameters.Set("branch", value);
		}
	}

	public string Comp
	{
		get
		{
			return base.Parameters["comp"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("comp");
			}
			else
			{
				base.Parameters.Set("comp", value);
			}
		}
	}

	public string Maddr
	{
		get
		{
			return base.Parameters["maddr"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("maddr");
			}
			else
			{
				base.Parameters.Set("maddr", value);
			}
		}
	}

	public IPAddress Received
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["received"];
			if (sIP_Parameter != null)
			{
				return IPAddress.Parse(sIP_Parameter.Value);
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				base.Parameters.Remove("received");
			}
			else
			{
				base.Parameters.Set("received", value.ToString());
			}
		}
	}

	public int RPort
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["rport"];
			if (sIP_Parameter != null)
			{
				if (sIP_Parameter.Value == "")
				{
					return 0;
				}
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				base.Parameters.Remove("rport");
			}
			else if (value == 0)
			{
				base.Parameters.Set("rport", "");
			}
			else
			{
				base.Parameters.Set("rport", value.ToString());
			}
		}
	}

	public int Ttl
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["ttl"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				base.Parameters.Remove("ttl");
			}
			else
			{
				base.Parameters.Set("ttl", value.ToString());
			}
		}
	}

	public SIP_t_ViaParm()
	{
		m_ProtocolName = "SIP";
		m_ProtocolVersion = "2.0";
		m_ProtocolTransport = "UDP";
		m_pSentBy = new HostEndPoint("localhost", -1);
	}

	public static string CreateBranch()
	{
		return "z9hG4bK-" + Guid.NewGuid().ToString().Replace("-", "");
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("reader");
		}
		Parse(new StringReader(value));
	}

	public override void Parse(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = reader.QuotedReadToDelimiter('/');
		if (text == null)
		{
			throw new SIP_ParseException("Via header field protocol-name is missing !");
		}
		ProtocolName = text.Trim();
		text = reader.QuotedReadToDelimiter('/');
		if (text == null)
		{
			throw new SIP_ParseException("Via header field protocol-version is missing !");
		}
		ProtocolVersion = text.Trim();
		text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Via header field transport is missing !");
		}
		ProtocolTransport = text.Trim();
		text = reader.QuotedReadToDelimiter(new char[2] { ';', ',' }, removeDelimiter: false);
		if (text == null)
		{
			throw new SIP_ParseException("Via header field sent-by is missing !");
		}
		SentBy = HostEndPoint.Parse(text.Trim());
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ProtocolName + "/" + ProtocolVersion + "/" + ProtocolTransport + " ");
		stringBuilder.Append(SentBy.ToString());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
