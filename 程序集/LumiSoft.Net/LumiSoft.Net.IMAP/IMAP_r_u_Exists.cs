using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Exists : IMAP_r_u
{
	private int m_MessageCount;

	public int MessageCount => m_MessageCount;

	public IMAP_r_u_Exists(int messageCount)
	{
		if (messageCount < 0)
		{
			throw new ArgumentException("Arguments 'messageCount' value must be >= 0.", "messageCount");
		}
		m_MessageCount = messageCount;
	}

	public static IMAP_r_u_Exists Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		return new IMAP_r_u_Exists(Convert.ToInt32(response.Split(' ')[1]));
	}

	public override string ToString()
	{
		return "* " + m_MessageCount + " EXISTS\r\n";
	}
}
