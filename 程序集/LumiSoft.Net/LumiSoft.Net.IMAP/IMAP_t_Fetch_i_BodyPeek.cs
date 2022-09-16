using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_i_BodyPeek : IMAP_t_Fetch_i
{
	private string m_Section;

	private int m_Offset = -1;

	private int m_MaxCount = -1;

	public string Section => m_Section;

	public int Offset => m_Offset;

	public int MaxCount => m_MaxCount;

	public IMAP_t_Fetch_i_BodyPeek()
	{
	}

	public IMAP_t_Fetch_i_BodyPeek(string section, int offset, int maxCount)
	{
		m_Section = section;
		m_Offset = offset;
		m_MaxCount = maxCount;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("BODY.PEEK[");
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
