using System;
using System.Text;

namespace LumiSoft.Net.RTP;

public class RTP_Participant_Remote : RTP_Participant
{
	private string m_Name;

	private string m_Email;

	private string m_Phone;

	private string m_Location;

	private string m_Tool;

	private string m_Note;

	public string Name => m_Name;

	public string Email => m_Email;

	public string Phone => m_Phone;

	public string Location => m_Location;

	public string Tool => m_Tool;

	public string Note => m_Note;

	public event EventHandler<RTP_ParticipantEventArgs> Changed;

	internal RTP_Participant_Remote(string cname)
		: base(cname)
	{
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CNAME: " + base.CNAME);
		if (!string.IsNullOrEmpty(m_Name))
		{
			stringBuilder.AppendLine("Name: " + m_Name);
		}
		if (!string.IsNullOrEmpty(m_Email))
		{
			stringBuilder.AppendLine("Email: " + m_Email);
		}
		if (!string.IsNullOrEmpty(m_Phone))
		{
			stringBuilder.AppendLine("Phone: " + m_Phone);
		}
		if (!string.IsNullOrEmpty(m_Location))
		{
			stringBuilder.AppendLine("Location: " + m_Location);
		}
		if (!string.IsNullOrEmpty(m_Tool))
		{
			stringBuilder.AppendLine("Tool: " + m_Tool);
		}
		if (!string.IsNullOrEmpty(m_Note))
		{
			stringBuilder.AppendLine("Note: " + m_Note);
		}
		return stringBuilder.ToString().TrimEnd();
	}

	internal void Update(RTCP_Packet_SDES_Chunk sdes)
	{
		if (sdes == null)
		{
			throw new ArgumentNullException("sdes");
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(sdes.Name) && !string.Equals(m_Name, sdes.Name))
		{
			m_Name = sdes.Name;
			flag = true;
		}
		if (!string.IsNullOrEmpty(sdes.Email) && !string.Equals(m_Email, sdes.Email))
		{
			m_Email = sdes.Email;
			flag = true;
		}
		if (!string.IsNullOrEmpty(sdes.Phone) && !string.Equals(Phone, sdes.Phone))
		{
			m_Phone = sdes.Phone;
			flag = true;
		}
		if (!string.IsNullOrEmpty(sdes.Location) && !string.Equals(m_Location, sdes.Location))
		{
			m_Location = sdes.Location;
			flag = true;
		}
		if (!string.IsNullOrEmpty(sdes.Tool) && !string.Equals(m_Tool, sdes.Tool))
		{
			m_Tool = sdes.Tool;
			flag = true;
		}
		if (!string.IsNullOrEmpty(sdes.Note) && !string.Equals(m_Note, sdes.Note))
		{
			m_Note = sdes.Note;
			flag = true;
		}
		if (flag)
		{
			OnChanged();
		}
	}

	private void OnChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, new RTP_ParticipantEventArgs(this));
		}
	}
}
