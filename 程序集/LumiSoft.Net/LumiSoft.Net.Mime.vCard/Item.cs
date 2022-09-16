using System;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mime.vCard;

public class Item
{
	private vCard m_pCard;

	private string m_Name = "";

	private string m_Parameters = "";

	private string m_Value = "";

	private bool m_FoldData = true;

	public string Name => m_Name;

	public string ParametersString
	{
		get
		{
			return m_Parameters;
		}
		set
		{
			m_Parameters = value;
		}
	}

	public string Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public string DecodedValue
	{
		get
		{
			string text = m_Value;
			string text2 = null;
			string text3 = null;
			string[] array = m_Parameters.ToLower().Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2[0] == "encoding" && array2.Length > 1)
				{
					text2 = array2[1];
				}
				else if (array2[0] == "charset" && array2.Length > 1)
				{
					text3 = array2[1];
				}
			}
			switch (text2)
			{
			case "quoted-printable":
				text = Encoding.Default.GetString(MIME_Utils.QuotedPrintableDecode(Encoding.Default.GetBytes(text)));
				break;
			case "b":
			case "base64":
				text = Encoding.Default.GetString(Net_Utils.FromBase64(Encoding.Default.GetBytes(text)));
				break;
			default:
				throw new Exception("Unknown data encoding '" + text2 + "' !");
			case null:
				break;
			}
			if (text3 != null)
			{
				text = Encoding.GetEncoding(text3).GetString(Encoding.Default.GetBytes(text));
			}
			return text;
		}
	}

	public bool FoldLongLines
	{
		get
		{
			return m_FoldData;
		}
		set
		{
			m_FoldData = value;
		}
	}

	internal vCard Owner => m_pCard;

	internal Item(vCard card, string name, string parameters, string value)
	{
		m_pCard = card;
		m_Name = name;
		m_Parameters = parameters;
		m_Value = value;
	}

	public void SetDecodedValue(string value)
	{
		string text = "";
		string[] array = m_Parameters.ToLower().Split(';');
		foreach (string text2 in array)
		{
			string[] array2 = text2.Split('=');
			if (!(array2[0] == "encoding") && !(array2[0] == "charset") && text2.Length > 0)
			{
				text = text + text2 + ";";
			}
		}
		if (m_pCard.Version.StartsWith("3"))
		{
			if (!Net_Utils.IsAscii(value))
			{
				text += "CHARSET=utf-8";
			}
			ParametersString = text;
			Value = vCard_Utils.Encode(m_pCard.Version, m_pCard.Charset, value);
		}
		else if (NeedEncode(value))
		{
			text = (ParametersString = text + "ENCODING=QUOTED-PRINTABLE;CHARSET=" + m_pCard.Charset.WebName);
			Value = vCard_Utils.Encode(m_pCard.Version, m_pCard.Charset, value);
		}
		else
		{
			ParametersString = text;
			Value = value;
		}
	}

	internal string ToItemString()
	{
		string text = m_Value;
		if (m_FoldData)
		{
			text = FoldData(text);
		}
		if (m_Parameters.Length > 0)
		{
			return m_Name + ";" + m_Parameters + ":" + text;
		}
		return m_Name + ":" + text;
	}

	private bool NeedEncode(string value)
	{
		if (!Net_Utils.IsAscii(value))
		{
			return true;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (char.IsControl(value[i]))
			{
				return true;
			}
		}
		return false;
	}

	private string FoldData(string data)
	{
		if (data.Length > 76)
		{
			int num = 0;
			int num2 = -1;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				char c = data[i];
				if (c == ' ' || c == '\t')
				{
					num2 = i;
				}
				if (i == data.Length - 1)
				{
					stringBuilder.Append(data.Substring(num));
				}
				else if (i - num >= 76)
				{
					if (num2 == -1)
					{
						num2 = i;
					}
					stringBuilder.Append(data.Substring(num, num2 - num) + "\r\n\t");
					i = num2;
					num2 = -1;
					num = i;
				}
			}
			return stringBuilder.ToString();
		}
		return data;
	}
}
