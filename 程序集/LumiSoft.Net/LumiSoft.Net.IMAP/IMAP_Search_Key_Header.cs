using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Header : IMAP_Search_Key
{
	private string m_FieldName = "";

	private string m_Value = "";

	public string FieldName => m_FieldName;

	public string Value => m_Value;

	public IMAP_Search_Key_Header(string fieldName, string value)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_FieldName = fieldName;
		m_Value = value;
	}

	internal static IMAP_Search_Key_Header Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "HEADER", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'HEADER' key.");
		}
		string fieldName = IMAP_Utils.ReadString(r) ?? throw new ParseException("Parse error: Invalid 'HEADER' field-name value.");
		string text = IMAP_Utils.ReadString(r);
		if (text == null)
		{
			throw new ParseException("Parse error: Invalid 'HEADER' string value.");
		}
		return new IMAP_Search_Key_Header(fieldName, text);
	}

	public override string ToString()
	{
		return "HEADER " + TextUtils.QuoteString(m_FieldName) + " " + TextUtils.QuoteString(m_Value);
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, "HEADER "));
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.String, m_FieldName));
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, " "));
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.String, m_Value));
	}
}
