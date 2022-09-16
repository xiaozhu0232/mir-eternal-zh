using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_ContentDisposition : SIP_t_ValueWithParams
{
	private string m_DispositionType = "";

	public string DispositionType
	{
		get
		{
			return m_DispositionType;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DispositionType");
			}
			if (!TextUtils.IsToken(value))
			{
				throw new ArgumentException("Invalid DispositionType value, value must be 'token' !");
			}
			m_DispositionType = value;
		}
	}

	public string Handling
	{
		get
		{
			return base.Parameters["handling"]?.Value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.Parameters.Remove("handling");
			}
			else
			{
				base.Parameters.Set("handling", value);
			}
		}
	}

	public SIP_t_ContentDisposition(string value)
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
			throw new SIP_ParseException("SIP Content-Disposition 'disp-type' value is missing !");
		}
		m_DispositionType = text;
		ParseParameters(reader);
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_DispositionType);
		stringBuilder.Append(ParametersToString());
		return stringBuilder.ToString();
	}
}
