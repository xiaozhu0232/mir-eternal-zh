using System;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.SMTP;

public class SMTP_t_Mailbox
{
	private string m_LocalPart;

	private string m_Domain;

	public string LocalPart => m_LocalPart;

	public string Domain => m_Domain;

	public SMTP_t_Mailbox(string localPart, string domain)
	{
		if (localPart == null)
		{
			throw new ArgumentNullException("localPart");
		}
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		m_LocalPart = localPart;
		m_Domain = domain;
	}

	public override string ToString()
	{
		if (MIME_Reader.IsDotAtom(m_LocalPart))
		{
			return m_LocalPart + "@" + (Net_Utils.IsIPAddress(m_Domain) ? ("[" + m_Domain + "]") : m_Domain);
		}
		return TextUtils.QuoteString(m_LocalPart) + "@" + (Net_Utils.IsIPAddress(m_Domain) ? ("[" + m_Domain + "]") : m_Domain);
	}
}
