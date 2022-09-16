using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_MultipartAlternative : MIME_b_Multipart
{
	public MIME_b_MultipartAlternative()
	{
		base.ContentType = new MIME_h_ContentType(MIME_MediaTypes.Multipart.alternative)
		{
			Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.')
		};
	}

	public MIME_b_MultipartAlternative(MIME_h_ContentType contentType)
		: base(contentType)
	{
		if (!string.Equals(contentType.TypeWithSubtype, "multipart/alternative", StringComparison.CurrentCultureIgnoreCase))
		{
			throw new ArgumentException("Argument 'contentType.TypeWithSubype' value must be 'multipart/alternative'.");
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
		MIME_b_MultipartAlternative mIME_b_MultipartAlternative = new MIME_b_MultipartAlternative(owner.ContentType);
		MIME_b_Multipart.ParseInternal(owner, owner.ContentType.TypeWithSubtype, stream, mIME_b_MultipartAlternative);
		return mIME_b_MultipartAlternative;
	}
}
