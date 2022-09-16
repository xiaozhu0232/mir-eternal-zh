using System;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_MailFrom
{
	private string m_Mailbox = "";

	private int m_Size = -1;

	private string m_Body;

	private SMTP_DSN_Ret m_RET;

	private string m_ENVID;

	public string Mailbox => m_Mailbox;

	public int Size => m_Size;

	public string Body => m_Body;

	public SMTP_DSN_Ret RET => m_RET;

	public string ENVID => m_ENVID;

	public SMTP_MailFrom(string mailbox, int size, string body, SMTP_DSN_Ret ret, string envid)
	{
		if (mailbox == null)
		{
			throw new ArgumentNullException("mailbox");
		}
		m_Mailbox = mailbox;
		m_Size = size;
		m_Body = body;
		m_RET = ret;
		m_ENVID = envid;
	}
}
