using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Video : MIME_b_SinglepartBase
{
	public MIME_b_Video(string mediaType)
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
		MIME_b_Video mIME_b_Video = null;
		mIME_b_Video = ((owner.ContentType == null) ? new MIME_b_Video(defaultContentType.TypeWithSubtype) : new MIME_b_Video(owner.ContentType.TypeWithSubtype));
		Net_Utils.StreamCopy(stream, mIME_b_Video.EncodedStream, stream.LineBufferSize);
		return mIME_b_Video;
	}
}
