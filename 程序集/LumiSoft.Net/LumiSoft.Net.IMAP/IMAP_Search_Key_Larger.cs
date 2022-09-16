using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Larger : IMAP_Search_Key
{
	private int m_Value;

	public int Value => m_Value;

	public IMAP_Search_Key_Larger(int value)
	{
		m_Value = value;
	}

	internal static IMAP_Search_Key_Larger Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "LARGER", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'LARGER' key.");
		}
		string s = r.ReadWord() ?? throw new ParseException("Parse error: Invalid 'LARGER' value.");
		int result = 0;
		if (!int.TryParse(s, out result))
		{
			throw new ParseException("Parse error: Invalid 'LARGER' value.");
		}
		return new IMAP_Search_Key_Larger(result);
	}

	public override string ToString()
	{
		return "LARGER " + m_Value;
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
