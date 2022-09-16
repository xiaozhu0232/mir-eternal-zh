using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ReferredBy : SIP_t_ValueWithParams
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
			if (value == null)
			{
				throw new ArgumentNullException("Address");
			}
			m_pAddress = value;
		}
	}

	public string CID
	{
		get
		{
			return base.Parameters["cid"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("cid");
			}
			else
			{
				base.Parameters.Set("cid", value);
			}
		}
	}

	public SIP_t_ReferredBy(string value)
	{
		m_pAddress = new SIP_t_NameAddress();
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
