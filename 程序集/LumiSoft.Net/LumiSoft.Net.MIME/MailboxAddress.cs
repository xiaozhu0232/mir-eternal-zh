using System;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class MailboxAddress : Address
{
	private string m_DisplayName = "";

	private string m_EmailAddress = "";

	[Obsolete("Use ToMailboxAddressString instead !")]
	public string MailboxString
	{
		get
		{
			string text = "";
			if (DisplayName != "")
			{
				text = text + TextUtils.QuoteString(DisplayName) + " ";
			}
			return text + "<" + EmailAddress + ">";
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			m_DisplayName = value;
			OnChanged();
		}
	}

	public string EmailAddress
	{
		get
		{
			return m_EmailAddress;
		}
		set
		{
			if (!Core.IsAscii(value))
			{
				throw new Exception("Email address can contain ASCII chars only !");
			}
			m_EmailAddress = value;
			OnChanged();
		}
	}

	public string LocalPart
	{
		get
		{
			if (EmailAddress.IndexOf("@") > -1)
			{
				return EmailAddress.Substring(0, EmailAddress.IndexOf("@"));
			}
			return EmailAddress;
		}
	}

	public string Domain
	{
		get
		{
			if (EmailAddress.IndexOf("@") != -1)
			{
				return EmailAddress.Substring(EmailAddress.IndexOf("@") + 1);
			}
			return "";
		}
	}

	public MailboxAddress()
		: base(groupAddress: false)
	{
	}

	public MailboxAddress(string emailAddress)
		: base(groupAddress: false)
	{
		m_EmailAddress = emailAddress;
	}

	public MailboxAddress(string displayName, string emailAddress)
		: base(groupAddress: false)
	{
		m_DisplayName = displayName;
		m_EmailAddress = emailAddress;
	}

	public static MailboxAddress Parse(string mailbox)
	{
		mailbox = mailbox.Trim();
		string displayName = "";
		string text = mailbox;
		if (mailbox.IndexOf("<") > -1 && mailbox.IndexOf(">") > -1)
		{
			displayName = MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(mailbox.Substring(0, mailbox.LastIndexOf("<"))));
			text = mailbox.Substring(mailbox.LastIndexOf("<") + 1, mailbox.Length - mailbox.LastIndexOf("<") - 2).Trim();
		}
		else
		{
			if (mailbox.StartsWith("\""))
			{
				int num = mailbox.IndexOf("\"");
				if (num > -1 && mailbox.LastIndexOf("\"") > num)
				{
					displayName = MIME_Encoding_EncodedWord.DecodeS(mailbox.Substring(num + 1, mailbox.LastIndexOf("\"") - num - 1).Trim());
				}
				text = mailbox.Substring(mailbox.LastIndexOf("\"") + 1).Trim();
			}
			text = text.Replace("<", "").Replace(">", "").Trim();
		}
		return new MailboxAddress(displayName, text);
	}

	public string ToMailboxAddressString()
	{
		string text = "";
		if (m_DisplayName.Length > 0)
		{
			text = ((!Core.IsAscii(m_DisplayName)) ? (MimeUtils.EncodeWord(m_DisplayName) + " ") : (TextUtils.QuoteString(m_DisplayName) + " "));
		}
		return text + "<" + EmailAddress + ">";
	}

	internal void OnChanged()
	{
		if (base.Owner != null)
		{
			if (base.Owner is AddressList)
			{
				((AddressList)base.Owner).OnCollectionChanged();
			}
			else if (base.Owner is MailboxAddressCollection)
			{
				((MailboxAddressCollection)base.Owner).OnCollectionChanged();
			}
		}
	}
}
