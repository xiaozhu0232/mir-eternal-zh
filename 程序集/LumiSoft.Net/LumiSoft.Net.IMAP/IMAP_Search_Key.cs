using System;
using System.Collections.Generic;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public abstract class IMAP_Search_Key
{
	internal static IMAP_Search_Key ParseKey(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadToFirstChar();
		if (r.StartsWith("(", case_sensitive: false))
		{
			return IMAP_Search_Key_Group.Parse(new StringReader(r.ReadParenthesized()));
		}
		if (r.StartsWith("ALL", case_sensitive: false))
		{
			return IMAP_Search_Key_All.Parse(r);
		}
		if (r.StartsWith("ANSWERED", case_sensitive: false))
		{
			return IMAP_Search_Key_Answered.Parse(r);
		}
		if (r.StartsWith("BCC", case_sensitive: false))
		{
			return IMAP_Search_Key_Bcc.Parse(r);
		}
		if (r.StartsWith("BEFORE", case_sensitive: false))
		{
			return IMAP_Search_Key_Before.Parse(r);
		}
		if (r.StartsWith("BODY", case_sensitive: false))
		{
			return IMAP_Search_Key_Body.Parse(r);
		}
		if (r.StartsWith("CC", case_sensitive: false))
		{
			return IMAP_Search_Key_Cc.Parse(r);
		}
		if (r.StartsWith("DELETED", case_sensitive: false))
		{
			return IMAP_Search_Key_Deleted.Parse(r);
		}
		if (r.StartsWith("DRAFT", case_sensitive: false))
		{
			return IMAP_Search_Key_Draft.Parse(r);
		}
		if (r.StartsWith("FLAGGED", case_sensitive: false))
		{
			return IMAP_Search_Key_Flagged.Parse(r);
		}
		if (r.StartsWith("FROM", case_sensitive: false))
		{
			return IMAP_Search_Key_From.Parse(r);
		}
		if (r.StartsWith("HEADER", case_sensitive: false))
		{
			return IMAP_Search_Key_Header.Parse(r);
		}
		if (r.StartsWith("KEYWORD", case_sensitive: false))
		{
			return IMAP_Search_Key_Keyword.Parse(r);
		}
		if (r.StartsWith("LARGER", case_sensitive: false))
		{
			return IMAP_Search_Key_Larger.Parse(r);
		}
		if (r.StartsWith("NEW", case_sensitive: false))
		{
			return IMAP_Search_Key_New.Parse(r);
		}
		if (r.StartsWith("NOT", case_sensitive: false))
		{
			return IMAP_Search_Key_Not.Parse(r);
		}
		if (r.StartsWith("OLD", case_sensitive: false))
		{
			return IMAP_Search_Key_Old.Parse(r);
		}
		if (r.StartsWith("ON", case_sensitive: false))
		{
			return IMAP_Search_Key_On.Parse(r);
		}
		if (r.StartsWith("OR", case_sensitive: false))
		{
			return IMAP_Search_Key_Or.Parse(r);
		}
		if (r.StartsWith("RECENT", case_sensitive: false))
		{
			return IMAP_Search_Key_Recent.Parse(r);
		}
		if (r.StartsWith("SEEN", case_sensitive: false))
		{
			return IMAP_Search_Key_Seen.Parse(r);
		}
		if (r.StartsWith("SENTBEFORE", case_sensitive: false))
		{
			return IMAP_Search_Key_SentBefore.Parse(r);
		}
		if (r.StartsWith("SENTON", case_sensitive: false))
		{
			return IMAP_Search_Key_SentOn.Parse(r);
		}
		if (r.StartsWith("SENTSINCE", case_sensitive: false))
		{
			return IMAP_Search_Key_SentSince.Parse(r);
		}
		if (r.StartsWith("SEQSET", case_sensitive: false))
		{
			return IMAP_Search_Key_SeqSet.Parse(r);
		}
		if (r.StartsWith("SINCE", case_sensitive: false))
		{
			return IMAP_Search_Key_Since.Parse(r);
		}
		if (r.StartsWith("SMALLER", case_sensitive: false))
		{
			return IMAP_Search_Key_Smaller.Parse(r);
		}
		if (r.StartsWith("SUBJECT", case_sensitive: false))
		{
			return IMAP_Search_Key_Subject.Parse(r);
		}
		if (r.StartsWith("TEXT", case_sensitive: false))
		{
			return IMAP_Search_Key_Text.Parse(r);
		}
		if (r.StartsWith("TO", case_sensitive: false))
		{
			return IMAP_Search_Key_To.Parse(r);
		}
		if (r.StartsWith("UID", case_sensitive: false))
		{
			return IMAP_Search_Key_Uid.Parse(r);
		}
		if (r.StartsWith("UNANSWERED", case_sensitive: false))
		{
			return IMAP_Search_Key_Unanswered.Parse(r);
		}
		if (r.StartsWith("UNDELETED", case_sensitive: false))
		{
			return IMAP_Search_Key_Undeleted.Parse(r);
		}
		if (r.StartsWith("UNDRAFT", case_sensitive: false))
		{
			return IMAP_Search_Key_Undraft.Parse(r);
		}
		if (r.StartsWith("UNFLAGGED", case_sensitive: false))
		{
			return IMAP_Search_Key_Unflagged.Parse(r);
		}
		if (r.StartsWith("UNKEYWORD", case_sensitive: false))
		{
			return IMAP_Search_Key_Unkeyword.Parse(r);
		}
		if (r.StartsWith("UNSEEN", case_sensitive: false))
		{
			return IMAP_Search_Key_Unseen.Parse(r);
		}
		try
		{
			return IMAP_Search_Key_SeqSet.Parse(r);
		}
		catch
		{
			throw new ParseException("Unknown search key '" + r.ReadToEnd() + "'.");
		}
	}

	internal abstract void ToCmdParts(List<IMAP_Client_CmdPart> list);
}
