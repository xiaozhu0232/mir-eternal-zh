using System;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_t_AcceptRange : SIP_t_Value
{
	private string m_MediaType = "";

	private SIP_ParameterCollection m_pMediaParameters;

	private SIP_ParameterCollection m_pParameters;

	public string MediaType
	{
		get
		{
			return m_MediaType;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property MediaType value can't be null or empty !");
			}
			if (value.IndexOf('/') == -1)
			{
				throw new ArgumentException("Invalid roperty MediaType value, syntax: mediaType / mediaSubType !");
			}
			m_MediaType = value;
		}
	}

	public SIP_ParameterCollection MediaParameters => m_pMediaParameters;

	public SIP_ParameterCollection Parameters => m_pParameters;

	public double QValue
	{
		get
		{
			SIP_Parameter sIP_Parameter = Parameters["qvalue"];
			if (sIP_Parameter != null)
			{
				return Convert.ToDouble(sIP_Parameter.Value);
			}
			return -1.0;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentException("Property QValue value must be between 0.0 and 1.0 !");
			}
			if (value < 0.0)
			{
				Parameters.Remove("qvalue");
			}
			else
			{
				Parameters.Set("qvalue", value.ToString());
			}
		}
	}

	public SIP_t_AcceptRange()
	{
		m_pMediaParameters = new SIP_ParameterCollection();
		m_pParameters = new SIP_ParameterCollection();
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
			throw new SIP_ParseException("Invalid 'accept-range' value, m-type is missing !");
		}
		MediaType = text;
		bool flag = true;
		while (reader.Available > 0)
		{
			reader.ReadToFirstChar();
			if (reader.SourceString.StartsWith(","))
			{
				break;
			}
			if (reader.SourceString.StartsWith(";"))
			{
				reader.ReadSpecifiedLength(1);
				string text2 = reader.QuotedReadToDelimiter(new char[2] { ';', ',' }, removeDelimiter: false);
				if (text2 != "")
				{
					string[] array = text2.Split(new char[1] { '=' }, 2);
					string text3 = array[0].Trim();
					string value = "";
					if (array.Length == 2)
					{
						value = array[1];
					}
					if (text3.ToLower() == "q")
					{
						flag = false;
					}
					if (flag)
					{
						MediaParameters.Add(text3, value);
					}
					else
					{
						Parameters.Add(text3, value);
					}
				}
				continue;
			}
			throw new SIP_ParseException("SIP_t_AcceptRange unexpected prarameter value !");
		}
	}

	public override string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_MediaType);
		foreach (SIP_Parameter pMediaParameter in m_pMediaParameters)
		{
			if (pMediaParameter.Value != null)
			{
				stringBuilder.Append(";" + pMediaParameter.Name + "=" + pMediaParameter.Value);
			}
			else
			{
				stringBuilder.Append(";" + pMediaParameter.Name);
			}
		}
		foreach (SIP_Parameter pParameter in m_pParameters)
		{
			if (pParameter.Value != null)
			{
				stringBuilder.Append(";" + pParameter.Name + "=" + pParameter.Value);
			}
			else
			{
				stringBuilder.Append(";" + pParameter.Name);
			}
		}
		return stringBuilder.ToString();
	}
}
