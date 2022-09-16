using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public abstract class MIME_b
{
	private MIME_Entity m_pEntity;

	private MIME_h_ContentType m_pContentType;

	public abstract bool IsModified { get; }

	public MIME_Entity Entity => m_pEntity;

	public string MediaType => m_pContentType.TypeWithSubtype;

	internal MIME_h_ContentType ContentType
	{
		get
		{
			return m_pContentType;
		}
		set
		{
			m_pContentType = value;
		}
	}

	public MIME_b(MIME_h_ContentType contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		m_pContentType = contentType;
	}

	internal MIME_b()
	{
	}

	protected static MIME_b Parse(MIME_Entity owner, MIME_h_ContentType defaultContentType, SmartStream stream)
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
		throw new NotImplementedException("Body provider class does not implement required Parse method.");
	}

	internal virtual void SetParent(MIME_Entity entity, bool setContentType)
	{
		m_pEntity = entity;
		if (setContentType && (entity.ContentType == null || !string.Equals(entity.ContentType.TypeWithSubtype, MediaType, StringComparison.InvariantCultureIgnoreCase)))
		{
			entity.ContentType = m_pContentType;
		}
	}

	protected internal abstract void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode);
}
