using System;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_RcptTo
{
	private string m_Mailbox = "";

	private SMTP_DSN_Notify m_Notify;

	private string m_ORCPT = "";

	public string Mailbox => m_Mailbox;

	public SMTP_DSN_Notify Notify => m_Notify;

	public string ORCPT => m_ORCPT;

	public SMTP_RcptTo(string mailbox, SMTP_DSN_Notify notify, string orcpt)
	{
		if (mailbox == null)
		{
			throw new ArgumentNullException("mailbox");
		}
		m_Mailbox = mailbox;
		m_Notify = notify;
		m_ORCPT = orcpt;
	}
}
