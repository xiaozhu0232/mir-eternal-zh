using System;
using System.Globalization;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_SecMechanism : SIP_t_ValueWithParams
{
	private string m_Mechanism = "";

	public string Mechanism
	{
		get
		{
			return m_Mechanism;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Mechanism");
			}
			if (value == "")
			{
				throw new ArgumentException("Property Mechanism value may not be '' !");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Property Mechanism value must be 'token' !");
			}
			m_Mechanism = value;
		}
	}

	public double Q
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
			if (value < 0.0 || value > 2.0)
			{
				throw new ArgumentException("Property QValue value must be between 0.0 and 2.0 !");
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

	public string D_Alg
	{
		get
		{
			return base.Parameters["d-alg"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("d-alg");
			}
			else
			{
				base.Parameters.Set("d-alg", value);
			}
		}
	}

	public string D_Qop
	{
		get
		{
			return base.Parameters["d-qop"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("d-qop");
			}
			else
			{
				base.Parameters.Set("d-qop", value);
			}
		}
	}

	public string D_Ver
	{
		get
		{
			return base.Parameters["d-ver"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("d-ver");
			}
			else
			{
				base.Parameters.Set("d-ver", value);
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
		if (reader.ReadWord() == null)
		{
			throw new SIP_ParseException("Invalid 'sec-mechanism', 'mechanism-name' is missing !");
		}
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_Mechanism);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
