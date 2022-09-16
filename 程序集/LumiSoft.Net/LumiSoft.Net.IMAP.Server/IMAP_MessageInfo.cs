using System;
using System.Text;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_MessageInfo
{
	private string m_ID;

	private long m_UID;

	private string[] m_pFlags;

	private int m_Size;

	private DateTime m_InternalDate;

	private int m_SeqNo = 1;

	public string ID => m_ID;

	public long UID => m_UID;

	public string[] Flags => m_pFlags;

	public int Size => m_Size;

	public DateTime InternalDate => m_InternalDate;

	internal int SeqNo
	{
		get
		{
			return m_SeqNo;
		}
		set
		{
			m_SeqNo = value;
		}
	}

	public IMAP_MessageInfo(string id, long uid, string[] flags, int size, DateTime internalDate)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (id == string.Empty)
		{
			throw new ArgumentException("Argument 'id' value must be specified.", "id");
		}
		if (uid < 1)
		{
			throw new ArgumentException("Argument 'uid' value must be >= 1.", "uid");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		m_ID = id;
		m_UID = uid;
		m_pFlags = flags;
		m_Size = size;
		m_InternalDate = internalDate;
	}

	public bool ContainsFlag(string flag)
	{
		if (flag == null)
		{
			throw new ArgumentNullException("flag");
		}
		string[] pFlags = m_pFlags;
		for (int i = 0; i < pFlags.Length; i++)
		{
			if (string.Equals(pFlags[i], flag, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	internal string FlagsToImapString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		for (int i = 0; i < m_pFlags.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append("\\" + m_pFlags[i]);
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	internal void UpdateFlags(IMAP_Flags_SetType setType, string[] flags)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		switch (setType)
		{
		case IMAP_Flags_SetType.Add:
			m_pFlags = IMAP_Utils.MessageFlagsAdd(m_pFlags, flags);
			break;
		case IMAP_Flags_SetType.Remove:
			m_pFlags = IMAP_Utils.MessageFlagsRemove(m_pFlags, flags);
			break;
		default:
			m_pFlags = flags;
			break;
		}
	}
}
