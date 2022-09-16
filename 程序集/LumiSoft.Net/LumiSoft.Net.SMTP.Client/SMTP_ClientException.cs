using System;

namespace LumiSoft.Net.SMTP.Client;

public class SMTP_ClientException : Exception
{
	private SMTP_t_ReplyLine[] m_pReplyLines;

	[Obsolete("Use property 'ReplyLines' insead.")]
	public int StatusCode => m_pReplyLines[0].ReplyCode;

	[Obsolete("Use property 'ReplyLines' insead.")]
	public string ResponseText => m_pReplyLines[0].Text;

	public SMTP_t_ReplyLine[] ReplyLines => m_pReplyLines;

	public bool IsPermanentError
	{
		get
		{
			if (m_pReplyLines[0].ReplyCode >= 500 && m_pReplyLines[0].ReplyCode <= 599)
			{
				return true;
			}
			return false;
		}
	}

	public SMTP_ClientException(string responseLine)
		: base(responseLine.TrimEnd())
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		m_pReplyLines = new SMTP_t_ReplyLine[1] { SMTP_t_ReplyLine.Parse(responseLine) };
	}

	public SMTP_ClientException(SMTP_t_ReplyLine[] replyLines)
		: base(replyLines[0].ToString().TrimEnd())
	{
		if (replyLines == null)
		{
			throw new ArgumentNullException("replyLines");
		}
		m_pReplyLines = replyLines;
	}
}
