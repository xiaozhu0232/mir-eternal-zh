using System;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_h_Mailbox : MIME_h
{
	private string m_ParseValue;

	private string m_Name;

	private Mail_t_Mailbox m_pAddress;

	public override bool IsModified => false;

	public override string Name => m_Name;

	public Mail_t_Mailbox Address => m_pAddress;

	public Mail_h_Mailbox(string fieldName, Mail_t_Mailbox mailbox)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		if (fieldName == string.Empty)
		{
			throw new ArgumentException("Argument 'fieldName' value must be specified.");
		}
		if (mailbox == null)
		{
			throw new ArgumentNullException("mailbox");
		}
		m_Name = fieldName;
		m_pAddress = mailbox;
	}

	public static Mail_h_Mailbox Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid header field value '" + value + "'.");
		}
		MIME_Reader mIME_Reader = new MIME_Reader(array[1].Trim());
		string text = mIME_Reader.QuotedReadToDelimiter(new char[3] { ',', '<', ':' });
		if (text == null)
		{
			throw new ParseException("Invalid header field value '" + value + "'.");
		}
		if (mIME_Reader.Peek(readToFirstChar: true) == 60)
		{
			return new Mail_h_Mailbox(array[0], new Mail_t_Mailbox((text != null) ? MIME_Encoding_EncodedWord.DecodeS(TextUtils.UnQuoteString(text)) : null, mIME_Reader.ReadParenthesized()))
			{
				m_ParseValue = value
			};
		}
		return new Mail_h_Mailbox(array[0], new Mail_t_Mailbox(null, text))
		{
			m_ParseValue = value
		};
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (!reEncode && m_ParseValue != null)
		{
			return m_ParseValue;
		}
		return m_Name + ": " + m_pAddress.ToString(wordEncoder) + "\r\n";
	}
}
