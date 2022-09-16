using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Not : IMAP_Search_Key
{
	private IMAP_Search_Key m_pSearchKey;

	public IMAP_Search_Key SearchKey => m_pSearchKey;

	public IMAP_Search_Key_Not(IMAP_Search_Key key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		m_pSearchKey = key;
	}

	internal static IMAP_Search_Key_Not Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "NOT", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'NOT' key.");
		}
		return new IMAP_Search_Key_Not(IMAP_Search_Key.ParseKey(r));
	}

	public override string ToString()
	{
		return "NOT " + m_pSearchKey.ToString();
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, "NOT "));
		m_pSearchKey.ToCmdParts(list);
	}
}
