using System;
using System.Globalization;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ContactParam : SIP_t_ValueWithParams
{
	private SIP_t_NameAddress m_pAddress;

	public bool IsStarContact
	{
		get
		{
			if (m_pAddress.Uri.Value.StartsWith("*"))
			{
				return true;
			}
			return false;
		}
	}

	public SIP_t_NameAddress Address => m_pAddress;

	public double QValue
	{
		get
		{
			if (!base.Parameters.Contains("qvalue"))
			{
				return -1.0;
			}
			return double.Parse(base.Parameters["qvalue"].Value, NumberStyles.Any);
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentException("Property QValue value must be between 0.0 and 1.0 !");
			}
			if (value < 0.0)
			{
				base.Parameters.Remove("qvalue");
			}
			else
			{
				base.Parameters.Set("qvalue", value.ToString());
			}
		}
	}

	public int Expires
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["expires"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value < 0)
			{
				base.Parameters.Remove("expires");
			}
			else
			{
				base.Parameters.Set("expires", value.ToString());
			}
		}
	}

	public SIP_t_ContactParam()
	{
		m_pAddress = new SIP_t_NameAddress();
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
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
