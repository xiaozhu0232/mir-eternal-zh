using System;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_h_Unstructured : MIME_h
{
	private string m_ParseValue;

	private string m_Name = "";

	private string m_Value = "";

	public override bool IsModified => m_ParseValue == null;

	public override string Name => m_Name;

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
				throw new ArgumentNullException("value");
			}
			m_Value = value;
			m_ParseValue = null;
		}
	}

	public MIME_h_Unstructured(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("Argument 'name' value must be specified.", "name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_Name = name;
		m_Value = value;
	}

	private MIME_h_Unstructured()
	{
	}

	public static MIME_h_Unstructured Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		MIME_h_Unstructured mIME_h_Unstructured = new MIME_h_Unstructured();
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array[0].Trim() == string.Empty)
		{
			throw new ParseException("Invalid header field '" + value + "' syntax.");
		}
		mIME_h_Unstructured.m_Name = array[0];
		mIME_h_Unstructured.m_Value = MIME_Encoding_EncodedWord.DecodeTextS(MIME_Utils.UnfoldHeader((array.Length == 2) ? array[1].TrimStart() : ""));
		mIME_h_Unstructured.m_ParseValue = value;
		return mIME_h_Unstructured;
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (!reEncode && m_ParseValue != null)
		{
			return m_ParseValue;
		}
		if (wordEncoder != null)
		{
			return m_Name + ": " + wordEncoder.Encode(m_Value) + "\r\n";
		}
		return m_Name + ": " + m_Value + "\r\n";
	}
}
