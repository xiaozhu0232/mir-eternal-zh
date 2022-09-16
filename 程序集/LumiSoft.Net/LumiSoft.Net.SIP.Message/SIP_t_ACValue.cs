using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ACValue : SIP_t_ValueWithParams
{
	public bool Require
	{
		get
		{
			if (base.Parameters["require"] != null)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (!value)
			{
				base.Parameters.Remove("require");
			}
			else
			{
				base.Parameters.Set("require", null);
			}
		}
	}

	public bool Explicit
	{
		get
		{
			if (base.Parameters["explicit"] != null)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (!value)
			{
				base.Parameters.Remove("explicit");
			}
			else
			{
				base.Parameters.Set("explicit", null);
			}
		}
	}

	public SIP_t_ACValue()
	{
	}

	public SIP_t_ACValue(string value)
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
		if (reader.ReadWord() == null)
		{
			throw new SIP_ParseException("Invalid 'ac-value', '*' is missing !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("*");
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
