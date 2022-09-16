using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_SubscriptionState : SIP_t_ValueWithParams
{
	public class SubscriptionState
	{
		public const string active = "active";

		public const string pending = "pending";

		public const string terminated = "terminated";
	}

	public class EventReason
	{
		public const string deactivated = "deactivated";

		public const string probation = "probation";

		public const string rejected = "rejected";

		public const string timeout = "timeout";

		public const string giveup = "giveup";

		public const string noresource = "noresource";
	}

	private string m_Value = "";

	public string Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Value");
			}
			if (value == "")
			{
				throw new ArgumentException("Property 'Value' value may not be '' !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property 'Value' value must be 'token' !");
			}
			m_Value = value;
		}
	}

	public string Reason
	{
		get
		{
			return base.Parameters["reason"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("reason");
			}
			else
			{
				base.Parameters.Set("reason", value);
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
			if (value == -1)
			{
				base.Parameters.Remove("expires");
				return;
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'Expires' value must >= 0 !");
			}
			base.Parameters.Set("expires", value.ToString());
		}
	}

	public int RetryAfter
	{
		get
		{
			SIP_Parameter sIP_Parameter = base.Parameters["retry-after"];
			if (sIP_Parameter != null)
			{
				return Convert.ToInt32(sIP_Parameter.Value);
			}
			return -1;
		}
		set
		{
			if (value == -1)
			{
				base.Parameters.Remove("retry-after");
				return;
			}
			if (value < 0)
			{
				throw new ArgumentException("Property 'RetryAfter' value must >= 0 !");
			}
			base.Parameters.Set("retry-after", value.ToString());
		}
	}

	public SIP_t_SubscriptionState(string value)
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
		string text = reader.ReadWord();
		if (text == null)
		{
			throw new SIP_ParseException("SIP Event 'substate-value' value is missing !");
		}
		m_Value = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Value);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
