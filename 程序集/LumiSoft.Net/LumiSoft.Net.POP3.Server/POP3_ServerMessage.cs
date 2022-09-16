using System;

namespace LumiSoft.Net.POP3.Server;

public class POP3_ServerMessage
{
	private int m_SequenceNumber = -1;

	private string m_UID = "";

	private int m_Size;

	private bool m_IsMarkedForDeletion;

	private object m_pTag;

	public string UID => m_UID;

	public int Size => m_Size;

	public bool IsMarkedForDeletion => m_IsMarkedForDeletion;

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	internal int SequenceNumber
	{
		get
		{
			return m_SequenceNumber;
		}
		set
		{
			m_SequenceNumber = value;
		}
	}

	public POP3_ServerMessage(string uid, int size)
		: this(uid, size, null)
	{
	}

	public POP3_ServerMessage(string uid, int size, object tag)
	{
		if (uid == null)
		{
			throw new ArgumentNullException("uid");
		}
		if (uid == string.Empty)
		{
			throw new ArgumentException("Argument 'uid' value must be specified.");
		}
		if (size < 0)
		{
			throw new ArgumentException("Argument 'size' value must be >= 0.");
		}
		m_UID = uid;
		m_Size = size;
		m_pTag = tag;
	}

	internal void SetIsMarkedForDeletion(bool value)
	{
		m_IsMarkedForDeletion = value;
	}
}
