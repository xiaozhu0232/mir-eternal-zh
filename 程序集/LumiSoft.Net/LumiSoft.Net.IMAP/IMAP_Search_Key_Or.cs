using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Or : IMAP_Search_Key
{
	private IMAP_Search_Key m_pSearchKey1;

	private IMAP_Search_Key m_pSearchKey2;

	public IMAP_Search_Key SearchKey1 => m_pSearchKey1;

	public IMAP_Search_Key SearchKey2 => m_pSearchKey2;

	public IMAP_Search_Key_Or(IMAP_Search_Key key1, IMAP_Search_Key key2)
	{
		if (key1 == null)
		{
			throw new ArgumentNullException("key1");
		}
		if (key2 == null)
		{
			throw new ArgumentNullException("key2");
		}
		m_pSearchKey1 = key1;
		m_pSearchKey2 = key2;
	}

	internal static IMAP_Search_Key_Or Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "OR", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'OR' key.");
		}
		return new IMAP_Search_Key_Or(IMAP_Search_Key.ParseKey(r), IMAP_Search_Key.ParseKey(r));
	}

	public override string ToString()
	{
		return "OR " + m_pSearchKey1.ToString() + " " + m_pSearchKey2.ToString();
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, "OR "));
		m_pSearchKey1.ToCmdParts(list);
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, " "));
		m_pSearchKey2.ToCmdParts(list);
	}
}
