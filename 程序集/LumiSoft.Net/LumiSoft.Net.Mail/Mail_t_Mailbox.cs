using System;
using System.Text.RegularExpressions;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_t_Mailbox : Mail_t_Address
{
	private string m_DisplayName;

	private string m_Address;

	public string DisplayName => m_DisplayName;

	public string Address => m_Address;

	public string LocalPart => m_Address.Split('@')[0];

	public string Domain
	{
		get
		{
			string[] array = m_Address.Split('@');
			if (array.Length == 2)
			{
				return array[1];
			}
			return "";
		}
	}

	public Mail_t_Mailbox(string displayName, string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		m_DisplayName = displayName;
		m_Address = address;
	}

	public static Mail_t_Mailbox Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(value);
		new Mail_t_MailboxList();
		string text = mIME_Reader.QuotedReadToDelimiter(new char[2] { ',', '<' });
		if (string.IsNullOrEmpty(text) && mIME_Reader.Available == 0)
		{
			throw new ParseException("Not valid 'mailbox' value '" + value + "'.");
		}
		if (mIME_Reader.Peek(readToFirstChar: true) == 60)
		{
			return new Mail_t_Mailbox((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text.Trim())) : null, mIME_Reader.ReadParenthesized());
		}
		return new Mail_t_Mailbox(null, text);
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder)
	{
		if (string.IsNullOrEmpty(m_DisplayName))
		{
			return m_Address;
		}
		if (wordEncoder != null && MIME_Encoding_EncodedWord.MustEncode(m_DisplayName))
		{
			return wordEncoder.Encode(m_DisplayName) + " <" + m_Address + ">";
		}
		if (Regex.IsMatch(m_DisplayName, "[\"(),:;<>@\\[\\\\\\]]"))
		{
			return TextUtils.QuoteString(m_DisplayName) + " <" + m_Address + ">";
		}
		return m_DisplayName + " <" + m_Address + ">";
	}
}
