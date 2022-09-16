using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_MultipartEncrypted : MIME_b_Multipart
{
	public MIME_b_MultipartEncrypted(MIME_h_ContentType contentType)
		: base(contentType)
	{
		if (!string.Equals(contentType.TypeWithSubtype, "multipart/encrypted", StringComparison.CurrentCultureIgnoreCase))
		{
			throw new ArgumentException("Argument 'contentType.TypeWithSubype' value must be 'multipart/encrypted'.");
		}
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
		if (owner.ContentType == null || owner.ContentType.Param_Boundary == null)
		{
			throw new ParseException("Multipart entity has not required 'boundary' paramter.");
		}
		MIME_b_MultipartEncrypted mIME_b_MultipartEncrypted = new MIME_b_MultipartEncrypted(owner.ContentType);
		MIME_b_Multipart.ParseInternal(owner, owner.ContentType.TypeWithSubtype, stream, mIME_b_MultipartEncrypted);
		return mIME_b_MultipartEncrypted;
	}
}
