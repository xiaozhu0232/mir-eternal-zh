using System;
using System.Collections.Generic;
using System.Globalization;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_SentBefore : IMAP_Search_Key
{
	private DateTime m_Date;

	public DateTime Date => m_Date;

	public IMAP_Search_Key_SentBefore(DateTime value)
	{
		m_Date = value;
	}

	internal static IMAP_Search_Key_SentBefore Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!string.Equals(r.ReadWord(), "SENTBEFORE", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Parse error: Not a SEARCH 'SENTBEFORE' key.");
		}
		string text = r.ReadWord();
		if (text == null)
		{
			throw new ParseException("Parse error: Invalid 'SENTBEFORE' value.");
		}
		DateTime value;
		try
		{
			value = IMAP_Utils.ParseDate(text);
		}
		catch
		{
			throw new ParseException("Parse error: Invalid 'SENTBEFORE' value.");
		}
		return new IMAP_Search_Key_SentBefore(value);
	}

	public override string ToString()
	{
		return "SENTBEFORE " + m_Date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
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
