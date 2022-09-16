using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_ApplicationPkcs7Mime : MIME_b_Application
{
	public MIME_b_ApplicationPkcs7Mime()
		: base(MIME_MediaTypes.Application.pkcs7_mime)
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
		MIME_b_ApplicationPkcs7Mime mIME_b_ApplicationPkcs7Mime = new MIME_b_ApplicationPkcs7Mime();
		Net_Utils.StreamCopy(stream, mIME_b_ApplicationPkcs7Mime.EncodedStream, stream.LineBufferSize);
		return mIME_b_ApplicationPkcs7Mime;
	}

	public X509Certificate2Collection GetCertificates()
	{
		if (base.Data == null)
		{
			return null;
		}
		SignedCms signedCms = new SignedCms();
		signedCms.Decode(base.Data);
		return signedCms.Certificates;
	}

	public bool VerifySignature()
	{
		if (!string.Equals(base.Entity.ContentType.Parameters["smime-type"], "signed-data", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new InvalidOperationException("The VerifySignature method is only valid if Content-Type parameter smime-type=signed-data.");
		}
		if (base.Data == null)
		{
			return false;
		}
		try
		{
			SignedCms signedCms = new SignedCms();
			signedCms.Decode(base.Data);
			signedCms.CheckSignature(verifySignatureOnly: true);
			return true;
		}
		catch
		{
		}
		return false;
	}

	public MIME_Message GetSignedMime()
	{
		if (!string.Equals(base.Entity.ContentType.Parameters["smime-type"], "signed-data", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new InvalidOperationException("The VerifySignature method is only valid if Content-Type parameter smime-type=signed-data.");
		}
		if (base.Data != null)
		{
			SignedCms signedCms = new SignedCms();
			signedCms.Decode(base.Data);
			return MIME_Message.ParseFromStream(new MemoryStream(signedCms.ContentInfo.Content));
		}
		return null;
	}

	public MIME_Message GetEnvelopedMime(X509Certificate2 cert)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		if (!string.Equals(base.Entity.ContentType.Parameters["smime-type"], "enveloped-data", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new InvalidOperationException("The VerifySignature method is only valid if Content-Type parameter smime-type=enveloped-data.");
		}
		EnvelopedCms envelopedCms = new EnvelopedCms();
		envelopedCms.Decode(base.Data);
		X509Certificate2Collection extraStore = new X509Certificate2Collection(cert);
		envelopedCms.Decrypt(extraStore);
		return MIME_Message.ParseFromStream(new MemoryStream(envelopedCms.Encode()));
	}
}
