using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Quota : IMAP_r_u
{
	private string m_QuotaRootName = "";

	private IMAP_Quota_Entry[] m_pEntries;

	public string QuotaRootName => m_QuotaRootName;

	public IMAP_Quota_Entry[] Entries => m_pEntries;

	public IMAP_r_u_Quota(string quotaRootName, IMAP_Quota_Entry[] entries)
	{
		if (quotaRootName == null)
		{
			throw new ArgumentNullException("quotaRootName");
		}
		if (entries == null)
		{
			throw new ArgumentNullException("entries");
		}
		m_QuotaRootName = quotaRootName;
		m_pEntries = entries;
	}

	public static IMAP_r_u_Quota Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		StringReader stringReader = new StringReader(response);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string quotaRootName = stringReader.ReadWord();
		string[] array = stringReader.ReadParenthesized().Split(' ');
		List<IMAP_Quota_Entry> list = new List<IMAP_Quota_Entry>();
		for (int i = 0; i < array.Length; i += 3)
		{
			list.Add(new IMAP_Quota_Entry(array[i], Convert.ToInt64(array[i + 1]), Convert.ToInt64(array[i + 2])));
		}
		return new IMAP_r_u_Quota(quotaRootName, list.ToArray());
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* QUOTA \"" + m_QuotaRootName + "\" (");
		for (int i = 0; i < m_pEntries.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(m_pEntries[i].ResourceName + " " + m_pEntries[i].CurrentUsage + " " + m_pEntries[i].MaxUsage);
		}
		stringBuilder.Append(")\r\n");
		return stringBuilder.ToString();
	}
}
