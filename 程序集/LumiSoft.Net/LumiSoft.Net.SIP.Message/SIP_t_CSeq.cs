using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_CSeq : SIP_t_Value
{
	private int m_SequenceNumber = 1;

	private string m_RequestMethod = "";

	public int SequenceNumber
	{
		get
		{
			return m_SequenceNumber;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Property SequenceNumber value must be >= 1 !");
			}
			m_SequenceNumber = value;
		}
	}

	public string RequestMethod
	{
		get
		{
			return m_RequestMethod;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property RequestMethod value can't be null or empty !");
			}
			m_RequestMethod = value;
		}
	}

	public SIP_t_CSeq(string value)
	{
		Parse(new StringReader(value));
	}

	public SIP_t_CSeq(int sequenceNumber, string requestMethod)
	{
		m_SequenceNumber = sequenceNumber;
		m_RequestMethod = requestMethod;
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
			throw new SIP_ParseException("Invalid 'CSeq' value, sequence number is missing !");
		}
		try
		{
			m_SequenceNumber = Convert.ToInt32(text);
		}
		catch
		{
			throw new SIP_ParseException("Invalid CSeq 'sequence number' value !");
		}
		text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("Invalid 'CSeq' value, request method is missing !");
		}
		m_RequestMethod = text;
	}

	public override string ToStringValue()
	{
		return m_SequenceNumber + " " + m_RequestMethod;
	}
}
