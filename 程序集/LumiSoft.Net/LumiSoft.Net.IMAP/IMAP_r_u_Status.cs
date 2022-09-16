using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Status : IMAP_r_u
{
	private string m_FolderName = "";

	private int m_MessageCount;

	private int m_RecentCount;

	private long m_UidNext;

	private long m_FolderUid;

	private int m_UnseenCount;

	public string FolderName => m_FolderName;

	public int MessagesCount => m_MessageCount;

	public int RecentCount => m_RecentCount;

	public long UidNext => m_UidNext;

	public long FolderUid => m_FolderUid;

	public int UnseenCount => m_UnseenCount;

	public IMAP_r_u_Status(string folder, int messagesCount, int recentCount, long uidNext, long folderUid, int unseenCount)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		m_FolderName = folder;
		m_MessageCount = messagesCount;
		m_RecentCount = recentCount;
		m_UidNext = uidNext;
		m_FolderUid = folderUid;
		m_UnseenCount = unseenCount;
	}

	public static IMAP_r_u_Status Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		StringReader stringReader = new StringReader(response);
		stringReader.ReadWord();
		stringReader.ReadWord();
		int messagesCount = 0;
		int recentCount = 0;
		long uidNext = 0L;
		long folderUid = 0L;
		int unseenCount = 0;
		string folder = TextUtils.UnQuoteString(IMAP_Utils.Decode_IMAP_UTF7_String(stringReader.ReadWord()));
		string[] array = stringReader.ReadParenthesized().Split(' ');
		for (int i = 0; i < array.Length; i += 2)
		{
			if (array[i].Equals("MESSAGES", StringComparison.InvariantCultureIgnoreCase))
			{
				messagesCount = Convert.ToInt32(array[i + 1]);
			}
			else if (array[i].Equals("RECENT", StringComparison.InvariantCultureIgnoreCase))
			{
				recentCount = Convert.ToInt32(array[i + 1]);
			}
			else if (array[i].Equals("UIDNEXT", StringComparison.InvariantCultureIgnoreCase))
			{
				uidNext = Convert.ToInt64(array[i + 1]);
			}
			else if (array[i].Equals("UIDVALIDITY", StringComparison.InvariantCultureIgnoreCase))
			{
				folderUid = Convert.ToInt64(array[i + 1]);
			}
			else if (array[i].Equals("UNSEEN", StringComparison.InvariantCultureIgnoreCase))
			{
				unseenCount = Convert.ToInt32(array[i + 1]);
			}
		}
		return new IMAP_r_u_Status(folder, messagesCount, recentCount, uidNext, folderUid, unseenCount);
	}

	public override string ToString()
	{
		return ToString(IMAP_Mailbox_Encoding.None);
	}

	public override string ToString(IMAP_Mailbox_Encoding encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* STATUS");
		stringBuilder.Append(" " + IMAP_Utils.EncodeMailbox(m_FolderName, encoding));
		stringBuilder.Append(" (");
		bool flag = true;
		if (m_MessageCount >= 0)
		{
			stringBuilder.Append("MESSAGES " + m_MessageCount);
			flag = false;
		}
		if (m_RecentCount >= 0)
		{
			if (!flag)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("RECENT " + m_RecentCount);
			flag = false;
		}
		if (m_UidNext >= 0)
		{
			if (!flag)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("UIDNEXT " + m_UidNext);
			flag = false;
		}
		if (m_FolderUid >= 0)
		{
			if (!flag)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("UIDVALIDITY " + m_FolderUid);
			flag = false;
		}
		if (m_UnseenCount >= 0)
		{
			if (!flag)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("UNSEEN " + m_UnseenCount);
			flag = false;
		}
		stringBuilder.Append(")\r\n");
		return stringBuilder.ToString();
	}
}
