using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Audio : MIME_b_SinglepartBase
{
	public MIME_b_Audio(string mediaType)
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
		MIME_b_Audio mIME_b_Audio = null;
		mIME_b_Audio = ((owner.ContentType == null) ? new MIME_b_Audio(defaultContentType.TypeWithSubtype) : new MIME_b_Audio(owner.ContentType.TypeWithSubtype));
		Net_Utils.StreamCopy(stream, mIME_b_Audio.EncodedStream, stream.LineBufferSize);
		return mIME_b_Audio;
	}
}
