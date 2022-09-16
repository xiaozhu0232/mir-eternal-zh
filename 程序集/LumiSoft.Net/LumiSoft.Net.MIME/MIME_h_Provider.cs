using System;
using System.Collections.Generic;

namespace LumiSoft.Net.MIME;

public class MIME_h_Provider
{
	private Type m_pDefaultHeaderField;

	private Dictionary<string, Type> m_pHeadrFields;

	public Type DefaultHeaderField
	{
		get
		{
			return m_pDefaultHeaderField;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DefaultHeaderField");
			}
			if (!value.GetType().IsSubclassOf(typeof(MIME_h)))
			{
				throw new ArgumentException("Property 'DefaultHeaderField' value must be based on MIME_h class.");
			}
			m_pDefaultHeaderField = value;
		}
	}

	public Dictionary<string, Type> HeaderFields => m_pHeadrFields;

	public MIME_h_Provider()
	{
		m_pDefaultHeaderField = typeof(MIME_h_Unstructured);
		m_pHeadrFields = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
		m_pHeadrFields.Add("Content-Type", typeof(MIME_h_ContentType));
		m_pHeadrFields.Add("Content-Disposition", typeof(MIME_h_ContentDisposition));
	}

	public MIME_h Parse(string field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		if (!field.EndsWith("\r\n"))
		{
			field += "\r\n";
		}
		MIME_h mIME_h = null;
		string text = field.Split(new char[1] { ':' }, 2)[0].Trim();
		if (text == string.Empty)
		{
			throw new ParseException("Invalid header field value '" + field + "'.");
		}
		try
		{
			if (m_pHeadrFields.ContainsKey(text))
			{
				return (MIME_h)m_pHeadrFields[text].GetMethod("Parse").Invoke(null, new object[1] { field });
			}
			return (MIME_h)m_pDefaultHeaderField.GetMethod("Parse").Invoke(null, new object[1] { field });
		}
		catch (Exception ex)
		{
			return new MIME_h_Unparsed(field, ex.InnerException);
		}
	}
}
