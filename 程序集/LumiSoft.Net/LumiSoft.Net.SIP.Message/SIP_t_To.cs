using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_To : SIP_t_ValueWithParams
{
	private SIP_t_NameAddress m_pAddress;

	public SIP_t_NameAddress Address => m_pAddress;

	public string Tag
	{
		get
		{
			return base.Parameters["tag"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("tag");
			}
			else
			{
				base.Parameters.Set("tag", value);
			}
		}
	}

	public SIP_t_To(string value)
	{
		m_pAddress = new SIP_t_NameAddress();
		Parse(new StringReader(value));
	}

	public SIP_t_To(SIP_t_NameAddress address)
	{
		m_pAddress = address;
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
