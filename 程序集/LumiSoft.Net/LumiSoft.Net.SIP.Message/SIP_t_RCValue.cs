using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_RCValue : SIP_t_ValueWithParams
{
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
			throw new SIP_ParseException("Invalid 'rc-value', '*' is missing !");
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
