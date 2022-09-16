using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_AppendUid : IMAP_t_orc
{
	private long m_MailboxUid;

	private int m_MessageUid;

	public long MailboxUid => m_MailboxUid;

	public int MessageUid => m_MessageUid;

	public IMAP_t_orc_AppendUid(long mailboxUid, int msgUid)
	{
		m_MailboxUid = mailboxUid;
		m_MessageUid = msgUid;
	}

	public new static IMAP_t_orc_AppendUid Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 3);
		if (!string.Equals("APPENDUID", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid APPENDUID response value.", "r");
		}
		if (array.Length != 3)
		{
			throw new ArgumentException("Invalid APPENDUID response value.", "r");
		}
		return new IMAP_t_orc_AppendUid(Convert.ToInt64(array[1]), Convert.ToInt32(array[2]));
	}

	public override string ToString()
	{
		return "APPENDUID " + m_MailboxUid + " " + m_MessageUid;
	}
}
