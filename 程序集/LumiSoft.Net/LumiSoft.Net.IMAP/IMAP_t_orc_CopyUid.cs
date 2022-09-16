using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_CopyUid : IMAP_t_orc
{
	private long m_TargetMailboxUid;

	private IMAP_t_SeqSet m_pSourceSeqSet;

	private IMAP_t_SeqSet m_pTargetSeqSet;

	public long TargetMailboxUid => m_TargetMailboxUid;

	public IMAP_t_SeqSet SourceSeqSet => m_pSourceSeqSet;

	public IMAP_t_SeqSet TargetSeqSet => m_pTargetSeqSet;

	public IMAP_t_orc_CopyUid(long targetMailboxUid, IMAP_t_SeqSet sourceSeqSet, IMAP_t_SeqSet targetSeqSet)
	{
		if (sourceSeqSet == null)
		{
			throw new ArgumentNullException("sourceSeqSet");
		}
		if (targetSeqSet == null)
		{
			throw new ArgumentNullException("targetSeqSet");
		}
		m_TargetMailboxUid = targetMailboxUid;
		m_pSourceSeqSet = sourceSeqSet;
		m_pTargetSeqSet = targetSeqSet;
	}

	public new static IMAP_t_orc_CopyUid Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 4);
		if (!string.Equals("COPYUID", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid COPYUID response value.", "r");
		}
		if (array.Length != 4)
		{
			throw new ArgumentException("Invalid COPYUID response value.", "r");
		}
		return new IMAP_t_orc_CopyUid(Convert.ToInt64(array[1]), IMAP_t_SeqSet.Parse(array[2]), IMAP_t_SeqSet.Parse(array[3]));
	}

	public override string ToString()
	{
		return "COPYUID m_MailboxUid m_MessageUid";
	}
}
