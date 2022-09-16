using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_From : IMAP_Search_Key
{
	private string m_Value = "";

	public string Value => m_Value;

	public IMAP_Search_Key_From(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_Value = value;
	}

	internal static IMAP_Search_Key_From Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "FROM", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'FROM' key.");
		}
		return new IMAP_Search_Key_From(IMAP_Utils.ReadString(r) ?? throw new ParseException("Parse error: Invalid 'FROM' value."));
	}

	public override string ToString()
	{
		return "FROM " + TextUtils.QuoteString(m_Value);
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, "FROM "));
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.String, m_Value));
	}
}
