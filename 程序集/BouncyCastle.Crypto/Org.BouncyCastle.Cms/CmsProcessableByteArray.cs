using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Cms;

public class CmsProcessableByteArray : CmsProcessable, CmsReadable
{
	private readonly DerObjectIdentifier type;

	private readonly byte[] bytes;

	public DerObjectIdentifier Type => type;

	public CmsProcessableByteArray(byte[] bytes)
	{
		type = CmsObjectIdentifiers.Data;
		this.bytes = bytes;
	}

	public CmsProcessableByteArray(DerObjectIdentifier type, byte[] bytes)
	{
		this.bytes = bytes;
		this.type = type;
	}

	public virtual Stream GetInputStream()
	{
		return new MemoryStream(bytes, writable: false);
	}

	public virtual void Write(Stream zOut)
	{
		zOut.Write(bytes, 0, bytes.Length);
	}

	[Obsolete]
	public virtual object GetContent()
	{
		return bytes.Clone();
	}
}
