using System;
using System.Collections.Generic;
using System.Globalization;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_SentSince : IMAP_Search_Key
{
	private DateTime m_Date;

	public DateTime Date => m_Date;

	public IMAP_Search_Key_SentSince(DateTime value)
	{
		m_Date = value;
	}

	internal static IMAP_Search_Key_SentSince Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "SENTSINCE", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'SENTSINCE' key.");
		}
		string text = r.ReadWord();
		if (text == null)
		{
			throw new ParseException("Parse error: Invalid 'SENTSINCE' value.");
		}
		DateTime value;
		try
		{
			value = IMAP_Utils.ParseDate(text);
		}
		catch
		{
			throw new ParseException("Parse error: Invalid 'SENTSINCE' value.");
		}
		return new IMAP_Search_Key_SentSince(value);
	}

	public override string ToString()
	{
		return "SENTSINCE " + m_Date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
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
