using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Keyword : IMAP_Search_Key
{
	private string m_Value = "";

	public string Value => m_Value;

	public IMAP_Search_Key_Keyword(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_Value = value;
	}

	internal static IMAP_Search_Key_Keyword Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "KEYWORD", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'KEYWORD' key.");
		}
		return new IMAP_Search_Key_Keyword(r.ReadWord() ?? throw new ParseException("Parse error: Invalid 'KEYWORD' value."));
	}

	public override string ToString()
	{
		return "KEYWORD " + m_Value;
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, ToString()));
	}
}
