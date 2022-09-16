using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_NameAddress
{
	private string m_DisplayName = "";

	private AbsoluteUri m_pUri;

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_DisplayName = value;
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
				throw new ArgumentNullException("value");
			}
			m_pUri = value;
		}
	}

	public bool IsSipOrSipsUri
	{
		get
		{
			if (!IsSipUri)
			{
				return IsSecureSipUri;
			}
			return true;
		}
	}

	public bool IsSipUri
	{
		get
		{
			if (m_pUri.Scheme == "sip")
			{
				return true;
			}
			return false;
		}
	}

	public bool IsSecureSipUri
	{
		get
		{
			if (m_pUri.Scheme == "sips")
			{
				return true;
			}
			return false;
		}
	}

	public bool IsMailToUri
	{
		get
		{
			if (m_pUri.Scheme == "mailto")
			{
				return true;
			}
			return false;
		}
	}

	public SIP_t_NameAddress()
	{
	}

	public SIP_t_NameAddress(string value)
	{
		Parse(value);
	}

	public SIP_t_NameAddress(string displayName, AbsoluteUri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		DisplayName = displayName;
		Uri = uri;
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("reader");
		}
		Parse(new StringReader(value));
	}

	public void Parse(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.ReadToFirstChar();
		if (reader.StartsWith("<"))
		{
			m_pUri = AbsoluteUri.Parse(reader.ReadParenthesized());
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			stringBuilder.Append(reader.ReadToFirstChar());
			string value = reader.ReadWord();
			if (string.IsNullOrEmpty(value))
			{
				break;
			}
			stringBuilder.Append(value);
		}
		reader.ReadToFirstChar();
		if (reader.StartsWith("<"))
		{
			m_DisplayName = stringBuilder.ToString().Trim();
			m_pUri = AbsoluteUri.Parse(reader.ReadParenthesized());
		}
		else
		{
			m_pUri = AbsoluteUri.Parse(stringBuilder.ToString());
		}
	}

	public string ToStringValue()
	{
		if (string.IsNullOrEmpty(m_DisplayName))
		{
			return "<" + m_pUri.ToString() + ">";
		}
		return TextUtils.QuoteString(m_DisplayName) + " <" + m_pUri.ToString() + ">";
	}
}
