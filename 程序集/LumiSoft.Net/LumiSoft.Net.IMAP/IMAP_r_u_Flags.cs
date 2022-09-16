using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Flags : IMAP_r_u
{
	private string[] m_pFlags;

	public string[] Flags => m_pFlags;

	public IMAP_r_u_Flags(string[] flags)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		m_pFlags = flags;
	}

	public static IMAP_r_u_Flags Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		return new IMAP_r_u_Flags(new StringReader(response.Split(new char[1] { ' ' }, 3)[2]).ReadParenthesized().Split(' '));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* FLAGS (");
		for (int i = 0; i < m_pFlags.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(m_pFlags[i]);
		}
		stringBuilder.Append(")\r\n");
		return stringBuilder.ToString();
	}
}
