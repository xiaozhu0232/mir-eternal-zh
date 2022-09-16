using System;
using System.Collections;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class ParametizedHeaderField
{
	private HeaderField m_pHeaderField;

	private HeaderFieldParameterCollection m_pParameters;

	public string Name => m_pHeaderField.Name;

	public string Value
	{
		get
		{
			return TextUtils.SplitQuotedString(m_pHeaderField.Value, ';')[0];
		}
		set
		{
			StoreParameters(value, ParseParameters());
		}
	}

	public HeaderFieldParameterCollection Parameters => m_pParameters;

	public ParametizedHeaderField(HeaderField headerField)
	{
		m_pHeaderField = headerField;
		m_pParameters = new HeaderFieldParameterCollection(this);
	}

	internal Hashtable ParseParameters()
	{
		string[] array = TextUtils.SplitQuotedString(m_pHeaderField.EncodedValue, ';');
		Hashtable hashtable = new Hashtable();
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Trim().Split(new char[1] { '=' }, 2);
			if (hashtable.ContainsKey(array2[0].ToLower()))
			{
				continue;
			}
			if (array2.Length == 2)
			{
				string text = array2[1];
				if (text.StartsWith("\""))
				{
					text = TextUtils.UnQuoteString(array2[1]);
				}
				hashtable.Add(array2[0].ToLower(), text);
			}
			else
			{
				hashtable.Add(array2[0].ToLower(), "");
			}
		}
		return hashtable;
	}

	internal void StoreParameters(string value, Hashtable parameters)
	{
		string text = value;
		foreach (DictionaryEntry parameter in parameters)
		{
			text = text + ";\t" + parameter.Key?.ToString() + "=\"" + parameter.Value?.ToString() + "\"";
		}
		m_pHeaderField.Value = text;
	}
}
