using System;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public class IMAP_Envelope
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

	public IMAP_Envelope(DateTime date, string subject, Mail_t_Address[] from, Mail_t_Address[] sender, Mail_t_Address[] replyTo, Mail_t_Address[] to, Mail_t_Address[] cc, Mail_t_Address[] bcc, string inReplyTo, string messageID)
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

	public static IMAP_Envelope Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadWord();
		r.ReadToFirstChar();
		r.ReadSpecifiedLength(1);
		DateTime date = DateTime.MinValue;
		string text = r.ReadWord();
		if (text != null)
		{
			date = MIME_Utils.ParseRfc2822DateTime(text);
		}
		string subject = ReadAndDecodeWord(r.ReadWord());
		Mail_t_Address[] from = ReadAddresses(r);
		Mail_t_Address[] sender = ReadAddresses(r);
		Mail_t_Address[] replyTo = ReadAddresses(r);
		Mail_t_Address[] to = ReadAddresses(r);
		Mail_t_Address[] cc = ReadAddresses(r);
		Mail_t_Address[] bcc = ReadAddresses(r);
		string inReplyTo = r.ReadWord();
		string messageID = r.ReadWord();
		r.ReadToFirstChar();
		r.ReadSpecifiedLength(1);
		return new IMAP_Envelope(date, subject, from, sender, replyTo, to, cc, bcc, inReplyTo, messageID);
	}

	internal static IMAP_Envelope Parse(IMAP_Client._FetchResponseReader fetchReader)
	{
		if (fetchReader == null)
		{
			throw new ArgumentNullException("fetchReader");
		}
		fetchReader.GetReader().ReadWord();
		fetchReader.GetReader().ReadToFirstChar();
		fetchReader.GetReader().ReadSpecifiedLength(1);
		DateTime date = DateTime.MinValue;
		string text = fetchReader.ReadString();
		if (text != null)
		{
			date = MIME_Utils.ParseRfc2822DateTime(text);
		}
		string subject = ReadAndDecodeWord(fetchReader.ReadString());
		Mail_t_Address[] from = ReadAddresses(fetchReader);
		Mail_t_Address[] sender = ReadAddresses(fetchReader);
		Mail_t_Address[] replyTo = ReadAddresses(fetchReader);
		Mail_t_Address[] to = ReadAddresses(fetchReader);
		Mail_t_Address[] cc = ReadAddresses(fetchReader);
		Mail_t_Address[] bcc = ReadAddresses(fetchReader);
		string inReplyTo = fetchReader.ReadString();
		string messageID = fetchReader.ReadString();
		fetchReader.GetReader().ReadToFirstChar();
		fetchReader.GetReader().ReadSpecifiedLength(1);
		return new IMAP_Envelope(date, subject, from, sender, replyTo, to, cc, bcc, inReplyTo, messageID);
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
		r.ReadSpecifiedLength(1);
		while (r.Available > 0)
		{
			if (r.StartsWith(")"))
			{
				r.ReadSpecifiedLength(1);
				break;
			}
			r.ReadSpecifiedLength(1);
			string displayName = ReadAndDecodeWord(r.ReadWord());
			r.ReadWord();
			string text = r.ReadWord();
			string text2 = r.ReadWord();
			list.Add(new Mail_t_Mailbox(displayName, text + "@" + text2));
			r.ReadSpecifiedLength(1);
		}
		return list.ToArray();
	}

	private static Mail_t_Address[] ReadAddresses(IMAP_Client._FetchResponseReader fetchReader)
	{
		if (fetchReader == null)
		{
			throw new ArgumentNullException("fetchReader");
		}
		fetchReader.GetReader().ReadToFirstChar();
		if (fetchReader.GetReader().StartsWith("NIL", case_sensitive: false))
		{
			fetchReader.GetReader().ReadWord();
			return null;
		}
		List<Mail_t_Address> list = new List<Mail_t_Address>();
		fetchReader.GetReader().ReadSpecifiedLength(1);
		while (fetchReader.GetReader().Available > 0)
		{
			if (fetchReader.GetReader().StartsWith(")"))
			{
				fetchReader.GetReader().ReadSpecifiedLength(1);
				break;
			}
			fetchReader.GetReader().ReadSpecifiedLength(1);
			string displayName = ReadAndDecodeWord(fetchReader.ReadString());
			fetchReader.ReadString();
			string text = fetchReader.ReadString();
			string text2 = fetchReader.ReadString();
			list.Add(new Mail_t_Mailbox(displayName, text + "@" + text2));
			fetchReader.GetReader().ReadSpecifiedLength(1);
			fetchReader.GetReader().ReadToFirstChar();
		}
		return list.ToArray();
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

	private static string ReadAndDecodeWord(string text)
	{
		if (text == null)
		{
			return null;
		}
		if (string.Equals(text, "NIL", StringComparison.InvariantCultureIgnoreCase))
		{
			return "";
		}
		return MIME_Encoding_EncodedWord.DecodeTextS(text);
	}
}
