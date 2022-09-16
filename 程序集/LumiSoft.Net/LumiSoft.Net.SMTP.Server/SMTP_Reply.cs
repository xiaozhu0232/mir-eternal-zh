using System;
using System.Text;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_Reply
{
	private int m_ReplyCode;

	private string[] m_pReplyLines;

	public int ReplyCode => m_ReplyCode;

	public string[] ReplyLines => m_pReplyLines;

	public SMTP_Reply(int replyCode, string replyLine)
		: this(replyCode, new string[1] { replyLine })
	{
		if (replyLine == null)
		{
			throw new ArgumentNullException("replyLine");
		}
	}

	public SMTP_Reply(int replyCode, string[] replyLines)
	{
		if (replyCode < 200 || replyCode > 599)
		{
			throw new ArgumentException("Argument 'replyCode' value must be >= 200 and <= 599.", "replyCode");
		}
		if (replyLines == null)
		{
			throw new ArgumentNullException("replyLines");
		}
		if (replyLines.Length == 0)
		{
			throw new ArgumentException("Argument 'replyLines' must conatin at least one line.", "replyLines");
		}
		m_ReplyCode = replyCode;
		m_pReplyLines = replyLines;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_pReplyLines.Length; i++)
		{
			if (i == m_pReplyLines.Length - 1)
			{
				stringBuilder.Append(m_ReplyCode + " " + m_pReplyLines[i] + "\r\n");
			}
			else
			{
				stringBuilder.Append(m_ReplyCode + "-" + m_pReplyLines[i] + "\r\n");
			}
		}
		return stringBuilder.ToString();
	}
}
