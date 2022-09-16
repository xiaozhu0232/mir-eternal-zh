using System;

namespace LumiSoft.Net.SIP.Message;

public class SIP_SingleValueHF<T> : SIP_HeaderField where T : SIP_t_Value
{
	private T m_pValue;

	public override string Value
	{
		get
		{
			return ToStringValue();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Property Value value may not be null !");
			}
			Parse(new StringReader(value));
		}
	}

	public T ValueX
	{
		get
		{
			return m_pValue;
		}
		set
		{
			m_pValue = value;
		}
	}

	public SIP_SingleValueHF(string name, T value)
		: base(name, "")
	{
		m_pValue = value;
	}

	public void Parse(StringReader reader)
	{
		m_pValue.Parse(reader);
	}

	public string ToStringValue()
	{
		return m_pValue.ToStringValue();
	}
}
