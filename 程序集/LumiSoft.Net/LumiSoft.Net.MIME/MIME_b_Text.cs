using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Text : MIME_b_SinglepartBase
{
	public string Text => GetCharset().GetString(base.Data);

	public MIME_b_Text(string mediaType)
		: base(new MIME_h_ContentType(mediaType))
	{
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
		MIME_b_Text mIME_b_Text = null;
		mIME_b_Text = ((owner.ContentType == null) ? new MIME_b_Text(defaultContentType.TypeWithSubtype) : new MIME_b_Text(owner.ContentType.TypeWithSubtype));
		Net_Utils.StreamCopy(stream, mIME_b_Text.EncodedStream, stream.LineBufferSize);
		mIME_b_Text.SetModified(isModified: false);
		return mIME_b_Text;
	}

	public void SetText(string transferEncoding, Encoding charset, string text)
	{
		if (transferEncoding == null)
		{
			throw new ArgumentNullException("transferEncoding");
		}
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (base.Entity == null)
		{
			throw new InvalidOperationException("Body must be bounded to some entity first.");
		}
		SetData(new MemoryStream(charset.GetBytes(text)), transferEncoding);
		base.Entity.ContentType.Param_Charset = charset.WebName;
	}

	public Encoding GetCharset()
	{
		if (base.Entity.ContentType == null || string.IsNullOrEmpty(base.Entity.ContentType.Param_Charset))
		{
			return Encoding.ASCII;
		}
		if (base.Entity.ContentType.Param_Charset.ToLower().StartsWith("x-"))
		{
			return Encoding.GetEncoding(base.Entity.ContentType.Param_Charset.Substring(2));
		}
		if (string.Equals(base.Entity.ContentType.Param_Charset, "cp1252", StringComparison.InvariantCultureIgnoreCase))
		{
			return Encoding.GetEncoding("windows-1252");
		}
		if (string.Equals(base.Entity.ContentType.Param_Charset, "utf8", StringComparison.InvariantCultureIgnoreCase))
		{
			return Encoding.GetEncoding("utf-8");
		}
		if (string.Equals(base.Entity.ContentType.Param_Charset, "iso8859_1", StringComparison.InvariantCultureIgnoreCase))
		{
			return Encoding.GetEncoding("iso-8859-1");
		}
		return Encoding.GetEncoding(base.Entity.ContentType.Param_Charset);
	}
}
