using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_HiEntry : SIP_t_ValueWithParams
{
	private SIP_t_NameAddress m_pAddress;

	public SIP_t_NameAddress Address
	{
		get
		{
			return m_pAddress;
		}
		set
		{
			if (m_pAddress == null)
			{
				throw new ArgumentNullException("m_pAddress");
			}
			m_pAddress = value;
		}
	}

	public double Index
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["index"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1.0;
		}
		set
		{
			if (value == -1.0)
			{
				base.Parameters.Remove("index");
			}
			else
			{
				base.Parameters.Set("index", value.ToString());
			}
		}
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
		m_pAddress = new SIP_t_NameAddress();
		m_pAddress.Parse(reader);
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_pAddress.ToStringValue());
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
