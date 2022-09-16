using System;
using System.Text;
using LumiSoft.Net.MIME;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net;

public class SIP_Uri : AbsoluteUri
{
	private bool m_IsSecure;

	private string m_User;

	private string m_Host = "";

	private int m_Port = -1;

	private SIP_ParameterCollection m_pParameters;

	private string m_Header;

	public override string Scheme
	{
		get
		{
			if (IsSecure)
			{
				return "sips";
			}
			return "sip";
		}
	}

	public bool IsSecure
	{
		get
		{
			return m_IsSecure;
		}
		set
		{
			m_IsSecure = value;
		}
	}

	public string Address => m_User + "@" + m_Host;

	public string User
	{
		get
		{
			return m_User;
		}
		set
		{
			m_User = value;
		}
	}

	public string Host
	{
		get
		{
			return m_Host;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Host value can't be null or '' !");
			}
			m_Host = value;
		}
	}

	public int Port
	{
		get
		{
			return m_Port;
		}
		set
		{
			m_Port = value;
		}
	}

	public string HostPort
	{
		get
		{
			if (m_Port == -1)
			{
				return m_Host;
			}
			return m_Host + ":" + m_Port;
		}
	}

	public SIP_ParameterCollection Parameters => m_pParameters;

	public int Param_Cause
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["cause"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				Parameters.Remove("cause");
			}
			else
			{
				Parameters.Set("cause", value.ToString());
			}
		}
	}

	public string Param_Comp
	{
		get
		{
			return Parameters["comp"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("comp");
			}
			else
			{
				Parameters.Set("comp", value);
			}
		}
	}

	public string Param_ContentType
	{
		get
		{
			return Parameters["content-type"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("content-type");
			}
			else
			{
				Parameters.Set("content-type", value);
			}
		}
	}

	public int Param_Delay
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["delay"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				Parameters.Remove("delay");
			}
			else
			{
				Parameters.Set("delay", value.ToString());
			}
		}
	}

	public int Param_Duration
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["duration"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				Parameters.Remove("duration");
			}
			else
			{
				Parameters.Set("duration", value.ToString());
			}
		}
	}

	public string Param_Locale
	{
		get
		{
			return Parameters["locale"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("locale");
			}
			else
			{
				Parameters.Set("locale", value);
			}
		}
	}

	public bool Param_Lr
	{
		get
		{
			if (Parameters["lr"] != null)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (!value)
			{
				Parameters.Remove("lr");
			}
			else
			{
				Parameters.Set("lr", null);
			}
		}
	}

	public string Param_Maddr
	{
		get
		{
			return Parameters["maddr"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("maddr");
			}
			else
			{
				Parameters.Set("maddr", value);
			}
		}
	}

	public string Param_Method
	{
		get
		{
			return Parameters["method"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("method");
			}
			else
			{
				Parameters.Set("method", value);
			}
		}
	}

	public string Param_Play
	{
		get
		{
			return Parameters["play"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("play");
			}
			else
			{
				Parameters.Set("play", value);
			}
		}
	}

	public int Param_Repeat
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["ttl"];
			if (sIP_Parameter != null)
			{
				if (sIP_Parameter.Value.ToLower() == "forever")
				{
					return int.MaxValue;
				}
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			switch (value)
			{
			case -1:
				Parameters.Remove("ttl");
				break;
			case int.MaxValue:
				Parameters.Set("ttl", "forever");
				break;
			default:
				Parameters.Set("ttl", value.ToString());
				break;
			}
		}
	}

	public string Param_Target
	{
		get
		{
			return Parameters["target"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("target");
			}
			else
			{
				Parameters.Set("target", value);
			}
		}
	}

	public string Param_Transport
	{
		get
		{
			return Parameters["transport"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("transport");
			}
			else
			{
				Parameters.Set("transport", value);
			}
		}
	}

	public int Param_Ttl
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["ttl"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				Parameters.Remove("ttl");
			}
			else
			{
				Parameters.Set("ttl", value.ToString());
			}
		}
	}

	public string Param_User
	{
		get
		{
			return Parameters["user"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("user");
			}
			else
			{
				Parameters.Set("user", value);
			}
		}
	}

	public string Param_Voicexml
	{
		get
		{
			return Parameters["voicexml"]?.Value;
		}
		set
		{
			if (value == null)
			{
				Parameters.Remove("voicexml");
			}
			else
			{
				Parameters.Set("voicexml", value);
			}
		}
	}

	public string Header
	{
		get
		{
			return m_Header;
		}
		set
		{
			m_Header = value;
		}
	}

	public SIP_Uri()
	{
		m_pParameters = new SIP_ParameterCollection();
	}

	public new static SIP_Uri Parse(string value)
	{
		AbsoluteUri absoluteUri = AbsoluteUri.Parse(value);
		if (absoluteUri is SIP_Uri)
		{
			return (SIP_Uri)absoluteUri;
		}
		throw new ArgumentException("Argument 'value' is not valid SIP or SIPS URI.");
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is SIP_Uri))
		{
			return false;
		}
		SIP_Uri sIP_Uri = (SIP_Uri)obj;
		if (IsSecure && !sIP_Uri.IsSecure)
		{
			return false;
		}
		if (User != sIP_Uri.User)
		{
			return false;
		}
		if (Host.ToLower() != sIP_Uri.Host.ToLower())
		{
			return false;
		}
		if (Port != sIP_Uri.Port)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	protected override void ParseInternal(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value = Uri.UnescapeDataString(value);
		if (!value.ToLower().StartsWith("sip:") && !value.ToLower().StartsWith("sips:"))
		{
			throw new SIP_ParseException("Specified value is invalid SIP-URI !");
		}
		StringReader stringReader = new StringReader(value);
		IsSecure = stringReader.QuotedReadToDelimiter(':').ToLower() == "sips";
		if (stringReader.SourceString.IndexOf('@') > -1)
		{
			User = stringReader.QuotedReadToDelimiter('@');
		}
		string[] array = stringReader.QuotedReadToDelimiter(new char[2] { ';', '?' }, removeDelimiter: false).Split(':');
		Host = array[0];
		if (array.Length == 2)
		{
			Port = Convert.ToInt32(array[1]);
		}
		if (stringReader.Available <= 0)
		{
			return;
		}
		string[] array2 = TextUtils.SplitQuotedString(stringReader.QuotedReadToDelimiter('?'), ';');
		foreach (string text in array2)
		{
			if (text.Trim() != "")
			{
				string[] array3 = text.Trim().Split(new char[1] { '=' }, 2);
				if (array3.Length == 2)
				{
					Parameters.Add(array3[0], TextUtils.UnQuoteString(array3[1]));
				}
				else
				{
					Parameters.Add(array3[0], null);
				}
			}
		}
		if (stringReader.Available > 0)
		{
			m_Header = stringReader.ReadToEnd();
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (IsSecure)
		{
			stringBuilder.Append("sips:");
		}
		else
		{
			stringBuilder.Append("sip:");
		}
		if (User != null)
		{
			stringBuilder.Append(User + "@");
		}
		stringBuilder.Append(Host);
		if (Port > -1)
		{
			stringBuilder.Append(":" + Port);
		}
		foreach (SIP_Parameter pParameter in m_pParameters)
		{
			if (pParameter.Value != null)
			{
				if (MIME_Reader.IsToken(pParameter.Value))
				{
					stringBuilder.Append(";" + pParameter.Name + "=" + pParameter.Value);
				}
				else
				{
					stringBuilder.Append(";" + pParameter.Name + "=" + TextUtils.QuoteString(pParameter.Value));
				}
			}
			else
			{
				stringBuilder.Append(";" + pParameter.Name);
			}
		}
		if (Header != null)
		{
			stringBuilder.Append("?" + Header);
		}
		return stringBuilder.ToString();
	}
}
