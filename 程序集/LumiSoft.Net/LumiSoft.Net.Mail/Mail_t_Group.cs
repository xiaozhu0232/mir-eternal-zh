using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_t_Group : Mail_t_Address
{
	private string m_DisplayName;

	private List<Mail_t_Mailbox> m_pList;

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			m_DisplayName = value;
		}
	}

	public List<Mail_t_Mailbox> Members => m_pList;

	public Mail_t_Group(string displayName)
	{
		m_DisplayName = displayName;
		m_pList = new List<Mail_t_Mailbox>();
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (string.IsNullOrEmpty(m_DisplayName))
		{
			stringBuilder.Append(":");
		}
		else if (MIME_Encoding_EncodedWord.MustEncode(m_DisplayName))
		{
			stringBuilder.Append(wordEncoder.Encode(m_DisplayName) + ":");
		}
		else
		{
			stringBuilder.Append(TextUtils.QuoteString(m_DisplayName) + ":");
		}
		for (int i = 0; i < m_pList.Count; i++)
		{
			stringBuilder.Append(m_pList[i].ToString(wordEncoder));
			if (i < m_pList.Count - 1)
			{
				stringBuilder.Append(",");
			}
		}
		stringBuilder.Append(";");
		return stringBuilder.ToString();
	}
}
