using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;
using LumiSoft.Net.Mail;

namespace LumiSoft.Net.MIME;

public class MIME_b_MessageRfc822 : MIME_b
{
	private Mail_Message m_pMessage;

	public override bool IsModified => m_pMessage.IsModified;

	public Mail_Message Message
	{
		get
		{
			return m_pMessage;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (base.Entity == null)
			{
				throw new InvalidOperationException("Body must be bounded to some entity first.");
			}
			if (base.Entity.ContentType == null || !string.Equals(base.Entity.ContentType.TypeWithSubtype, base.MediaType, StringComparison.InvariantCultureIgnoreCase))
			{
				base.Entity.ContentType = new MIME_h_ContentType(base.MediaType);
			}
			m_pMessage = value;
		}
	}

	public MIME_b_MessageRfc822()
		: base(new MIME_h_ContentType("message/rfc822"))
	{
		m_pMessage = new Mail_Message();
	}

	protected new static MIME_b Parse(MIME_Entity owner, MIME_h_ContentType defaultContentType, SmartStream stream)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (defaultContentType == null)
		{
			throw new ArgumentNullException("defaultContentType");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new MIME_b_MessageRfc822
		{
			m_pMessage = Mail_Message.ParseFromStream(stream)
		};
	}

	protected internal override void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pMessage.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
	}
}
