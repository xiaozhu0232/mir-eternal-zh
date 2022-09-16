using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public class IMAP_Fetch_DataItem_Body : IMAP_Fetch_DataItem
{
	private string m_Section;

	private int m_Offset = -1;

	private int m_MaxCount = -1;

	public string Section => m_Section;

	public int Offset => m_Offset;

	public int MaxCount => m_MaxCount;

	public IMAP_Fetch_DataItem_Body()
	{
	}

	public IMAP_Fetch_DataItem_Body(string section, int offset, int maxCount)
	{
		m_Section = section;
		m_Offset = offset;
		m_MaxCount = maxCount;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("BODY[");
		if (m_Section != null)
		{
			stringBuilder.Append(m_Section);
		}
		stringBuilder.Append("]");
		if (m_Offset > -1)
		{
			stringBuilder.Append("<" + m_Offset);
			if (m_MaxCount > -1)
			{
				stringBuilder.Append("." + m_MaxCount);
			}
			stringBuilder.Append(">");
		}
		return stringBuilder.ToString();
	}
}
