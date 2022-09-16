using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Unknown : MIME_b_SinglepartBase
{
	public MIME_b_Unknown(string mediaType)
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
		string text = null;
		try
		{
			text = owner.ContentType.TypeWithSubtype;
		}
		catch
		{
			text = "unparsable/unparsable";
		}
		MIME_b_Unknown mIME_b_Unknown = new MIME_b_Unknown(text);
		Net_Utils.StreamCopy(stream, mIME_b_Unknown.EncodedStream, stream.LineBufferSize);
		return mIME_b_Unknown;
	}
}
