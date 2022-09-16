using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_MessageDeliveryStatus : MIME_b
{
	private MIME_h_Collection m_pMessageFields;

	private List<MIME_h_Collection> m_pRecipientBlocks;

	public override bool IsModified
	{
		get
		{
			if (m_pMessageFields.IsModified)
			{
				return true;
			}
			foreach (MIME_h_Collection pRecipientBlock in m_pRecipientBlocks)
			{
				if (pRecipientBlock.IsModified)
				{
					return true;
				}
			}
			return false;
		}
	}

	public MIME_h_Collection MessageFields => m_pMessageFields;

	public List<MIME_h_Collection> RecipientBlocks => m_pRecipientBlocks;

	public MIME_b_MessageDeliveryStatus()
		: base(new MIME_h_ContentType("message/delivery-status"))
	{
		m_pMessageFields = new MIME_h_Collection(new MIME_h_Provider());
		m_pRecipientBlocks = new List<MIME_h_Collection>();
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
		MemoryStream memoryStream = new MemoryStream();
		Net_Utils.StreamCopy(stream, memoryStream, stream.LineBufferSize);
		memoryStream.Position = 0L;
		SmartStream smartStream = new SmartStream(memoryStream, owner: true);
		MIME_b_MessageDeliveryStatus mIME_b_MessageDeliveryStatus = new MIME_b_MessageDeliveryStatus();
		mIME_b_MessageDeliveryStatus.m_pMessageFields.Parse(smartStream);
		while (smartStream.Position - smartStream.BytesInReadBuffer < smartStream.Length)
		{
			MIME_h_Collection mIME_h_Collection = new MIME_h_Collection(new MIME_h_Provider());
			mIME_h_Collection.Parse(smartStream);
			mIME_b_MessageDeliveryStatus.m_pRecipientBlocks.Add(mIME_h_Collection);
		}
		return mIME_b_MessageDeliveryStatus;
	}

	protected internal override void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pMessageFields.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
		stream.Write(new byte[2] { 13, 10 }, 0, 2);
		foreach (MIME_h_Collection pRecipientBlock in m_pRecipientBlocks)
		{
			pRecipientBlock.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
			stream.Write(new byte[2] { 13, 10 }, 0, 2);
		}
	}
}
