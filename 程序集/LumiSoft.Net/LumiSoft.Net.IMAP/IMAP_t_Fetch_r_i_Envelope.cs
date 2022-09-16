using System;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Envelope : IMAP_t_Fetch_r_i
{
	private DateTime m_Date = DateTime.MinValue;

	private string m_Subject;

	private Mail_t_Address[] m_pFrom;

	private Mail_t_Address[] m_pSender;

	private Mail_t_Address[] m_pReplyTo;

	private Mail_t_Address[] m_pTo;

	private Mail_t_Address[] m_pCc;

	private Mail_t_Address[] m_pBcc;

	private string m_InReplyTo;

	private string m_MessageID;

	public DateTime Date => m_Date;

	public string Subject => m_Subject;

	public Mail_t_Address[] From => m_pFrom;

	public Mail_t_Address[] Sender => m_pSender;

	public Mail_t_Address[] ReplyTo => m_pReplyTo;

	public Mail_t_Address[] To => m_pTo;

	public Mail_t_Address[] Cc => m_pCc;

	public Mail_t_Address[] Bcc => m_pBcc;

	public string InReplyTo => m_InReplyTo;

	public string MessageID => m_MessageID;

	public IMAP_t_Fetch_r_i_Envelope(DateTime date, string subject, Mail_t_Address[] from, Mail_t_Address[] sender, Mail_t_Address[] replyTo, Mail_t_Address[] to, Mail_t_Address[] cc, Mail_t_Address[] bcc, string inReplyTo, string messageID)
	{
		m_Date = date;
		m_Subject = subject;
		m_pFrom = from;
		m_pSender = sender;
		m_pReplyTo = replyTo;
		m_pTo = to;
		m_pCc = cc;
		m_pBcc = bcc;
		m_InReplyTo = inReplyTo;
		m_MessageID = messageID;
	}

	public static IMAP_t_Fetch_r_i_Envelope Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		DateTime date = DateTime.MinValue;
		string text = r.ReadWord();
		if (!string.IsNullOrEmpty(text) && !text.Equals("NIL", StringComparison.InvariantCultureIgnoreCase))
		{
			date = MIME_Utils.ParseRfc2822DateTime(text);
		}
		string subject = ReadAndDecodeWord(r);
		Mail_t_Address[] from = ReadAddresses(r);
		Mail_t_Address[] sender = ReadAddresses(r);
		Mail_t_Address[] replyTo = ReadAddresses(r);
		Mail_t_Address[] to = ReadAddresses(r);
		Mail_t_Address[] cc = ReadAddresses(r);
		Mail_t_Address[] bcc = ReadAddresses(r);
		string inReplyTo = r.ReadWord();
		string messageID = r.ReadWord();
		return new IMAP_t_Fetch_r_i_Envelope(date, subject, from, sender, replyTo, to, cc, bcc, inReplyTo, messageID);
	}

	public static string ConstructEnvelope(Mail_Message entity)
	{
		MIME_Encoding_EncodedWord mIME_Encoding_EncodedWord = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8);
		mIME_Encoding_EncodedWord.Split = false;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ENVELOPE (");
		try
		{
			if (entity.Date != DateTime.MinValue)
			{
				stringBuilder.Append(TextUtils.QuoteString(MIME_Utils.DateTimeToRfc2822(entity.Date)));
			}
			else
			{
				stringBuilder.Append("NIL");
			}
		}
		catch
		{
			stringBuilder.Append("NIL");
		}
		if (entity.Subject != null)
		{
			string text = mIME_Encoding_EncodedWord.Encode(entity.Subject);
			stringBuilder.Append(" {" + text.Length + "}\r\n" + text);
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.From != null && entity.From.Count > 0)
		{
			stringBuilder.Append(" " + ConstructAddresses(entity.From.ToArray(), mIME_Encoding_EncodedWord));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.Sender != null)
		{
			stringBuilder.Append(" (");
			stringBuilder.Append(ConstructAddress(entity.Sender, mIME_Encoding_EncodedWord));
			stringBuilder.Append(")");
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.ReplyTo != null)
		{
			stringBuilder.Append(" " + ConstructAddresses(entity.ReplyTo.Mailboxes, mIME_Encoding_EncodedWord));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.To != null && entity.To.Count > 0)
		{
			stringBuilder.Append(" " + ConstructAddresses(entity.To.Mailboxes, mIME_Encoding_EncodedWord));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.Cc != null && entity.Cc.Count > 0)
		{
			stringBuilder.Append(" " + ConstructAddresses(entity.Cc.Mailboxes, mIME_Encoding_EncodedWord));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.Bcc != null && entity.Bcc.Count > 0)
		{
			stringBuilder.Append(" " + ConstructAddresses(entity.Bcc.Mailboxes, mIME_Encoding_EncodedWord));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.InReplyTo != null)
		{
			stringBuilder.Append(" " + TextUtils.QuoteString(mIME_Encoding_EncodedWord.Encode(entity.InReplyTo)));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		if (entity.MessageID != null)
		{
			stringBuilder.Append(" " + TextUtils.QuoteString(mIME_Encoding_EncodedWord.Encode(entity.MessageID)));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private static Mail_t_Address[] ReadAddresses(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadToFirstChar();
		if (r.StartsWith("NIL", case_sensitive: false))
		{
			r.ReadWord();
			return null;
		}
		List<Mail_t_Address> list = new List<Mail_t_Address>();
		StringReader stringReader = new StringReader(r.ReadParenthesized());
		stringReader.ReadToFirstChar();
		while (stringReader.Available > 0)
		{
			if (stringReader.StartsWith("("))
			{
				stringReader.ReadSpecifiedLength(1);
			}
			string displayName = ReadAndDecodeWord(stringReader);
			stringReader.ReadWord();
			string text = stringReader.ReadWord();
			string text2 = stringReader.ReadWord();
			list.Add(new Mail_t_Mailbox(displayName, text + "@" + text2));
			if (stringReader.EndsWith(")"))
			{
				stringReader.ReadSpecifiedLength(1);
			}
			stringReader.ReadToFirstChar();
		}
		return list.ToArray();
	}

	private static string ReadAndDecodeWord(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadToFirstChar();
		if (r.SourceString.StartsWith("{"))
		{
			int length = Convert.ToInt32(r.ReadParenthesized());
			r.ReadSpecifiedLength(2);
			return MIME_Encoding_EncodedWord.DecodeTextS(r.ReadSpecifiedLength(length));
		}
		string text = r.ReadWord();
		if (text == null)
		{
			throw new ParseException("Excpetcted quoted-string or string-literal, but non available.");
		}
		if (string.Equals(text, "NIL", StringComparison.InvariantCultureIgnoreCase))
		{
			return "";
		}
		return MIME_Encoding_EncodedWord.DecodeTextS(text);
	}

	private static string ConstructAddresses(Mail_t_Mailbox[] mailboxes, MIME_Encoding_EncodedWord wordEncoder)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		foreach (Mail_t_Mailbox address in mailboxes)
		{
			stringBuilder.Append(ConstructAddress(address, wordEncoder));
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private static string ConstructAddress(Mail_t_Mailbox address, MIME_Encoding_EncodedWord wordEncoder)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		if (address.DisplayName != null)
		{
			stringBuilder.Append(TextUtils.QuoteString(wordEncoder.Encode(RemoveCrlf(address.DisplayName))));
		}
		else
		{
			stringBuilder.Append("NIL");
		}
		stringBuilder.Append(" NIL");
		stringBuilder.Append(" " + TextUtils.QuoteString(wordEncoder.Encode(RemoveCrlf(address.LocalPart))));
		if (address.Domain != null)
		{
			stringBuilder.Append(" " + TextUtils.QuoteString(wordEncoder.Encode(RemoveCrlf(address.Domain))));
		}
		else
		{
			stringBuilder.Append(" NIL");
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private static string RemoveCrlf(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value.Replace("\r", "").Replace("\n", "");
	}
}
