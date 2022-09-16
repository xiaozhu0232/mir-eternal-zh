using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_AddressParam : SIP_t_ValueWithParams
{
	private SIP_t_NameAddress m_pAddress;

	public SIP_t_NameAddress Address => m_pAddress;

	public SIP_t_AddressParam()
	{
	}

	public SIP_t_AddressParam(string value)
	{
		Parse(value);
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
		SIP_t_NameAddress sIP_t_NameAddress = new SIP_t_NameAddress();
		sIP_t_NameAddress.Parse(reader);
		m_pAddress = sIP_t_NameAddress;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_pAddress.ToStringValue());
		foreach (SIP_Parameter parameter in base.Parameters)
		{
			if (parameter.Value != null)
			{
				stringBuilder.Append(";" + parameter.Name + "=" + parameter.Value);
			}
			else
			{
				stringBuilder.Append(";" + parameter.Name);
			}
		}
		return stringBuilder.ToString();
	}
}
