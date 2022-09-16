using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_RAck : SIP_t_Value
{
	private int m_ResponseNumber = 1;

	private int m_CSeqNumber = 1;

	private string m_Method = "";

	public int ResponseNumber
	{
		get
		{
			return m_ResponseNumber;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("ResponseNumber value must be >= 1 !");
			}
			m_ResponseNumber = value;
		}
	}

	public int CSeqNumber
	{
		get
		{
			return m_CSeqNumber;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("CSeqNumber value must be >= 1 !");
			}
			m_CSeqNumber = value;
		}
	}

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
			m_Method = value;
		}
	}

	public SIP_t_RAck(string value)
	{
		Parse(value);
	}

	public SIP_t_RAck(int responseNo, int cseqNo, string method)
	{
		ResponseNumber = responseNo;
		CSeqNumber = cseqNo;
		Method = method;
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
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("RAck response-num value is missing !");
		}
		try
		{
			m_ResponseNumber = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid RAck response-num value !");
		}
		text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("RAck CSeq-num value is missing !");
		}
		try
		{
			m_CSeqNumber = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid RAck CSeq-num value !");
		}
		text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("RAck Method value is missing !");
		}
		m_Method = text;
	}

	public override string ToStringValue()
	{
		return m_ResponseNumber + " " + m_CSeqNumber + " " + m_Method;
	}
}
