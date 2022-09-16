using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_SeqSet : IMAP_Search_Key
{
	private IMAP_t_SeqSet m_pSeqSet;

	public IMAP_t_SeqSet Value => m_pSeqSet;

	public IMAP_Search_Key_SeqSet(IMAP_t_SeqSet seqSet)
	{
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		m_pSeqSet = seqSet;
	}

	internal static IMAP_Search_Key_SeqSet Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadToFirstChar();
		string text = r.QuotedReadToDelimiter(' ');
		if (text == null)
		{
			throw new ParseException("Parse error: Invalid 'sequence-set' value.");
		}
		try
		{
			return new IMAP_Search_Key_SeqSet(IMAP_t_SeqSet.Parse(text));
		}
		catch
		{
			throw new ParseException("Parse error: Invalid 'sequence-set' value.");
		}
	}

	public override string ToString()
	{
		return m_pSeqSet.ToString();
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, ToString()));
	}

	[Obsolete("Use constructor 'IMAP_Search_Key_SeqSet(IMAP_t_SeqSet seqSet)' instead.")]
	public IMAP_Search_Key_SeqSet(IMAP_SequenceSet seqSet)
	{
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		m_pSeqSet = IMAP_t_SeqSet.Parse(seqSet.ToSequenceSetString());
	}
}
