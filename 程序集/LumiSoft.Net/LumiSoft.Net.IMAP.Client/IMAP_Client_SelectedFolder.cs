using System;
using System.Text;

namespace LumiSoft.Net.IMAP.Client;

public class IMAP_Client_SelectedFolder
{
	private string m_Name = "";

	private long m_UidValidity = -1L;

	private string[] m_pFlags = new string[0];

	private string[] m_pPermanentFlags = new string[0];

	private bool m_IsReadOnly;

	private long m_UidNext = -1L;

	private int m_FirstUnseen = -1;

	private int m_MessagesCount;

	private int m_RecentMessagesCount;

	public string Name => m_Name;

	public long UidValidity => m_UidValidity;

	public string[] Flags => m_pFlags;

	public string[] PermanentFlags => m_pPermanentFlags;

	public bool IsReadOnly => m_IsReadOnly;

	public long UidNext => m_UidNext;

	public int FirstUnseen => m_FirstUnseen;

	public int MessagesCount => m_MessagesCount;

	public int RecentMessagesCount => m_RecentMessagesCount;

	public IMAP_Client_SelectedFolder(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("The argument 'name' value must be specified.", "name");
		}
		m_Name = name;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Name: " + Name);
		stringBuilder.AppendLine("UidValidity: " + UidValidity);
		stringBuilder.AppendLine("Flags: " + StringArrayToString(Flags));
		stringBuilder.AppendLine("PermanentFlags: " + StringArrayToString(PermanentFlags));
		stringBuilder.AppendLine("IsReadOnly: " + IsReadOnly);
		stringBuilder.AppendLine("UidNext: " + UidNext);
		stringBuilder.AppendLine("FirstUnseen: " + FirstUnseen);
		stringBuilder.AppendLine("MessagesCount: " + MessagesCount);
		stringBuilder.AppendLine("RecentMessagesCount: " + RecentMessagesCount);
		return stringBuilder.ToString();
	}

	internal void SetUidValidity(long value)
	{
		m_UidValidity = value;
	}

	internal void SetFlags(string[] value)
	{
		m_pFlags = value;
	}

	internal void SetPermanentFlags(string[] value)
	{
		m_pPermanentFlags = value;
	}

	internal void SetReadOnly(bool value)
	{
		m_IsReadOnly = value;
	}

	internal void SetUidNext(long value)
	{
		m_UidNext = value;
	}

	internal void SetFirstUnseen(int value)
	{
		m_FirstUnseen = value;
	}

	internal void SetMessagesCount(int value)
	{
		m_MessagesCount = value;
	}

	internal void SetRecentMessagesCount(int value)
	{
		m_RecentMessagesCount = value;
	}

	private string StringArrayToString(string[] value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < value.Length; i++)
		{
			if (i == value.Length - 1)
			{
				stringBuilder.Append(value[i]);
			}
			else
			{
				stringBuilder.Append(value[i] + ",");
			}
		}
		return stringBuilder.ToString();
	}
}
