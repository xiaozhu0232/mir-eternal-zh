using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_AuthenticationInfo : SIP_t_Value
{
	private string m_NextNonce;

	private string m_Qop;

	private string m_ResponseAuth;

	private string m_CNonce;

	private int m_NonceCount = -1;

	public string NextNonce
	{
		get
		{
			return m_NextNonce;
		}
		set
		{
			m_NextNonce = value;
		}
	}

	public string Qop
	{
		get
		{
			return m_Qop;
		}
		set
		{
			m_Qop = value;
		}
	}

	public string ResponseAuth
	{
		get
		{
			return m_ResponseAuth;
		}
		set
		{
			m_ResponseAuth = value;
		}
	}

	public string CNonce
	{
		get
		{
			return m_CNonce;
		}
		set
		{
			m_CNonce = value;
		}
	}

	public int NonceCount
	{
		get
		{
			return m_NonceCount;
		}
		set
		{
			if (value < 0)
			{
				m_NonceCount = -1;
			}
			else
			{
				m_NonceCount = value;
			}
		}
	}

	public SIP_t_AuthenticationInfo(string value)
	{
		Parse(new StringReader(value));
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Parse(new StringReader(value));
	}

	public override void Parse(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.Available > 0)
		{
			string text = reader.QuotedReadToDelimiter(',');
			if (text == null || text.Length <= 0)
			{
				continue;
			}
			string[] array = text.Split(new char[1] { '=' }, 2);
			if (array[0].ToLower() == "nextnonce")
			{
				NextNonce = array[1];
				continue;
			}
			if (array[0].ToLower() == "qop")
			{
				Qop = array[1];
				continue;
			}
			if (array[0].ToLower() == "rspauth")
			{
				ResponseAuth = array[1];
				continue;
			}
			if (array[0].ToLower() == "cnonce")
			{
				CNonce = array[1];
				continue;
			}
			if (array[0].ToLower() == "nc")
			{
				NonceCount = Convert.ToInt32(array[1]);
				continue;
			}
			throw new SIP_ParseException("Invalid Authentication-Info value !");
		}
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_NextNonce != null)
		{
			stringBuilder.Append("nextnonce=" + m_NextNonce);
		}
		if (m_Qop != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("qop=" + m_Qop);
		}
		if (m_ResponseAuth != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("rspauth=" + TextUtils.QuoteString(m_ResponseAuth));
		}
		if (m_CNonce != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("cnonce=" + m_CNonce);
		}
		if (m_NonceCount != -1)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("nc=" + m_NonceCount.ToString("X8"));
		}
		return stringBuilder.ToString();
	}
}
