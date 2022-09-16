using System;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_h_Unparsed : MIME_h
{
	private string m_ParseValue;

	private string m_Name;

	private string m_Value;

	private Exception m_pException;

	public override bool IsModified => false;

	public override string Name => m_Name;

	public string Value => m_Value;

	public Exception Exception => m_pException;

	internal MIME_h_Unparsed(string value, Exception exception)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
		}
		m_Name = array[0];
		m_Value = array[1].Trim();
		m_ParseValue = value;
		m_pException = exception;
	}

	public static MIME_h_Unparsed Parse(string value)
	{
		throw new InvalidOperationException();
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		return m_ParseValue;
	}
}
