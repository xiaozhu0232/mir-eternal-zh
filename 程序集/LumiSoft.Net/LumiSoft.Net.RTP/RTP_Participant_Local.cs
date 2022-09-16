using System;

namespace LumiSoft.Net.RTP;

public class RTP_Participant_Local : RTP_Participant
{
	private string m_Name;

	private string m_Email;

	private string m_Phone;

	private string m_Location;

	private string m_Tool;

	private string m_Note;

	private CircleCollection<string> m_pOtionalItemsRoundRobin;

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
			ConstructOptionalItems();
		}
	}

	public string Email
	{
		get
		{
			return m_Email;
		}
		set
		{
			m_Email = value;
			ConstructOptionalItems();
		}
	}

	public string Phone
	{
		get
		{
			return m_Phone;
		}
		set
		{
			m_Phone = value;
			ConstructOptionalItems();
		}
	}

	public string Location
	{
		get
		{
			return m_Location;
		}
		set
		{
			m_Location = value;
			ConstructOptionalItems();
		}
	}

	public string Tool
	{
		get
		{
			return m_Tool;
		}
		set
		{
			m_Tool = value;
			ConstructOptionalItems();
		}
	}

	public string Note
	{
		get
		{
			return m_Note;
		}
		set
		{
			m_Note = value;
			ConstructOptionalItems();
		}
	}

	public RTP_Participant_Local(string cname)
		: base(cname)
	{
		m_pOtionalItemsRoundRobin = new CircleCollection<string>();
	}

	internal void AddNextOptionalSdesItem(RTCP_Packet_SDES_Chunk sdes)
	{
		if (sdes == null)
		{
			throw new ArgumentNullException("sdes");
		}
		lock (m_pOtionalItemsRoundRobin)
		{
			if (m_pOtionalItemsRoundRobin.Count > 0)
			{
				switch (m_pOtionalItemsRoundRobin.Next())
				{
				case "name":
					sdes.Name = m_Name;
					break;
				case "email":
					sdes.Email = m_Email;
					break;
				case "phone":
					sdes.Phone = m_Phone;
					break;
				case "location":
					sdes.Location = m_Location;
					break;
				case "tool":
					sdes.Tool = m_Tool;
					break;
				case "note":
					sdes.Note = m_Note;
					break;
				}
			}
		}
	}

	private void ConstructOptionalItems()
	{
		lock (m_pOtionalItemsRoundRobin)
		{
			m_pOtionalItemsRoundRobin.Clear();
			if (!string.IsNullOrEmpty(m_Note))
			{
				m_pOtionalItemsRoundRobin.Add("note");
			}
			if (!string.IsNullOrEmpty(m_Name))
			{
				m_pOtionalItemsRoundRobin.Add("name");
			}
			if (!string.IsNullOrEmpty(m_Email))
			{
				m_pOtionalItemsRoundRobin.Add("email");
			}
			if (!string.IsNullOrEmpty(m_Phone))
			{
				m_pOtionalItemsRoundRobin.Add("phone");
			}
			if (!string.IsNullOrEmpty(m_Location))
			{
				m_pOtionalItemsRoundRobin.Add("location");
			}
			if (!string.IsNullOrEmpty(m_Tool))
			{
				m_pOtionalItemsRoundRobin.Add("tool");
			}
		}
	}
}
