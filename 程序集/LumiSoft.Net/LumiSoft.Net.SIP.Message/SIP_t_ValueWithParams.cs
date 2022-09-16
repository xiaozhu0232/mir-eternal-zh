using System.Text;

namespace LumiSoft.Net.SIP.Message;

public abstract class SIP_t_ValueWithParams : SIP_t_Value
{
	private SIP_ParameterCollection m_pParameters;

	public SIP_ParameterCollection Parameters => m_pParameters;

	public SIP_t_ValueWithParams()
	{
		m_pParameters = new SIP_ParameterCollection();
	}

	protected void ParseParameters(StringReader reader)
	{
		m_pParameters.Clear();
		while (reader.Available > 0)
		{
			reader.ReadToFirstChar();
			if (reader.SourceString.StartsWith(";"))
			{
				reader.ReadSpecifiedLength(1);
				string text = reader.QuotedReadToDelimiter(new char[2] { ';', ',' }, removeDelimiter: false);
				if (text != "")
				{
					string[] array = text.Split(new char[1] { '=' }, 2);
					if (array.Length == 2)
					{
						Parameters.Add(array[0], TextUtils.UnQuoteString(array[1]));
					}
					else
					{
						Parameters.Add(array[0], null);
					}
				}
				continue;
			}
			if (!reader.SourceString.StartsWith(","))
			{
				throw new SIP_ParseException("Unexpected value '" + reader.SourceString + "' !");
			}
			break;
		}
	}

	protected string ParametersToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (SIP_Parameter pParameter in m_pParameters)
		{
			if (!string.IsNullOrEmpty(pParameter.Value))
			{
				if (TextUtils.IsToken(pParameter.Value))
				{
					stringBuilder.Append(";" + pParameter.Name + "=" + pParameter.Value);
				}
				else
				{
					stringBuilder.Append(";" + pParameter.Name + "=" + TextUtils.QuoteString(pParameter.Value));
				}
			}
			else
			{
				stringBuilder.Append(";" + pParameter.Name);
			}
		}
		return stringBuilder.ToString();
	}
}
