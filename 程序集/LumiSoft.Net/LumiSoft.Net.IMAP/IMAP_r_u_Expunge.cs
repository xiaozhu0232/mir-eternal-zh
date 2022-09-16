using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Expunge : IMAP_r_u
{
	private int m_SeqNo = 1;

	public int SeqNo => m_SeqNo;

	public IMAP_r_u_Expunge(int seqNo)
	{
		if (seqNo < 1)
		{
			throw new ArgumentException("Arguments 'seqNo' value must be >= 1.", "seqNo");
		}
		m_SeqNo = seqNo;
	}

	public static IMAP_r_u_Expunge Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		return new IMAP_r_u_Expunge(Convert.ToInt32(response.Split(' ')[1]));
	}

	public override string ToString()
	{
		return "* " + m_SeqNo + " EXPUNGE\r\n";
	}
}
