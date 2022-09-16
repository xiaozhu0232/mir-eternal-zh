using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_MultipartSigned : MIME_b_Multipart
{
	private X509Certificate2 m_pSignerCert;

	public MIME_b_MultipartSigned()
	{
		MIME_h_ContentType mIME_h_ContentType = new MIME_h_ContentType(MIME_MediaTypes.Multipart.signed);
		mIME_h_ContentType.Parameters["protocol"] = "application/x-pkcs7-signature";
		mIME_h_ContentType.Parameters["micalg"] = "sha1";
		mIME_h_ContentType.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
		base.ContentType = mIME_h_ContentType;
	}

	public MIME_b_MultipartSigned(MIME_h_ContentType contentType)
		: base(contentType)
	{
		if (!string.Equals(contentType.TypeWithSubtype, "multipart/signed", StringComparison.CurrentCultureIgnoreCase))
		{
			throw new ArgumentException("Argument 'contentType.TypeWithSubype' value must be 'multipart/signed'.");
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
		MIME_b_MultipartSigned mIME_b_MultipartSigned = new MIME_b_MultipartSigned(owner.ContentType);
		MIME_b_Multipart.ParseInternal(owner, owner.ContentType.TypeWithSubtype, stream, mIME_b_MultipartSigned);
		return mIME_b_MultipartSigned;
	}

	public void SetCertificate(X509Certificate2 signerCert)
	{
		if (signerCert == null)
		{
			throw new ArgumentNullException("signerCert");
		}
		m_pSignerCert = signerCert;
	}

	public X509Certificate2Collection GetCertificates()
	{
		if (base.BodyParts.Count != 2)
		{
			return null;
		}
		MIME_Entity mIME_Entity = base.BodyParts[1];
		SignedCms signedCms = new SignedCms();
		signedCms.Decode(((MIME_b_SinglepartBase)mIME_Entity.Body).Data);
		return signedCms.Certificates;
	}

	public bool VerifySignature()
	{
		if (m_pSignerCert != null)
		{
			return true;
		}
		if (base.BodyParts.Count != 2)
		{
			return false;
		}
		MIME_Entity mIME_Entity = base.BodyParts[1];
		MemoryStream memoryStream = new MemoryStream();
		base.BodyParts[0].ToStream(memoryStream, null, null, headerReencode: false);
		try
		{
			SignedCms signedCms = new SignedCms(new ContentInfo(memoryStream.ToArray()), detached: true);
			signedCms.Decode(((MIME_b_SinglepartBase)mIME_Entity.Body).Data);
			signedCms.CheckSignature(verifySignatureOnly: true);
			return true;
		}
		catch
		{
			return false;
		}
	}

	protected internal override void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (base.BodyParts.Count > 0 && m_pSignerCert != null)
		{
			if (base.BodyParts.Count > 1)
			{
				base.BodyParts.Remove(1);
			}
			MemoryStream memoryStream = new MemoryStream();
			base.BodyParts[0].ToStream(memoryStream, null, null, headerReencode: false);
			SignedCms signedCms = new SignedCms(new ContentInfo(memoryStream.ToArray()), detached: true);
			signedCms.ComputeSignature(new CmsSigner(m_pSignerCert));
			byte[] buffer = signedCms.Encode();
			MIME_Entity mIME_Entity = new MIME_Entity();
			MIME_b_Application mIME_b_Application = (MIME_b_Application)(mIME_Entity.Body = new MIME_b_Application(MIME_MediaTypes.Application.x_pkcs7_signature));
			mIME_b_Application.SetData(new MemoryStream(buffer), MIME_TransferEncodings.Base64);
			mIME_Entity.ContentType.Param_Name = "smime.p7s";
			mIME_Entity.ContentDescription = "S/MIME Cryptographic Signature";
			base.BodyParts.Add(mIME_Entity);
			signedCms.Decode(mIME_b_Application.Data);
			signedCms.CheckSignature(verifySignatureOnly: true);
		}
		base.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
	}
}
