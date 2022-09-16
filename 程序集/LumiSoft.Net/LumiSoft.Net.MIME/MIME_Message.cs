using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_Message : MIME_Entity
{
	private bool m_IsDisposed;

	public bool IsSigned
	{
		get
		{
			MIME_Entity[] allEntities = AllEntities;
			foreach (MIME_Entity mIME_Entity in allEntities)
			{
				if (string.Equals(mIME_Entity.ContentType.TypeWithSubtype, MIME_MediaTypes.Application.pkcs7_mime, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
				if (string.Equals(mIME_Entity.ContentType.TypeWithSubtype, MIME_MediaTypes.Multipart.signed, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}

	public MIME_Entity[] AllEntities
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			List<MIME_Entity> list = new List<MIME_Entity>();
			List<MIME_Entity> list2 = new List<MIME_Entity>();
			list2.Add(this);
			while (list2.Count > 0)
			{
				MIME_Entity mIME_Entity = list2[0];
				list2.RemoveAt(0);
				list.Add(mIME_Entity);
				if (base.Body != null && mIME_Entity.Body.GetType().IsSubclassOf(typeof(MIME_b_Multipart)))
				{
					MIME_EntityCollection bodyParts = ((MIME_b_Multipart)mIME_Entity.Body).BodyParts;
					for (int i = 0; i < bodyParts.Count; i++)
					{
						list2.Insert(i, bodyParts[i]);
					}
				}
				else if (base.Body != null && mIME_Entity.Body is MIME_b_MessageRfc822)
				{
					list2.Add(((MIME_b_MessageRfc822)mIME_Entity.Body).Message);
				}
			}
			return list.ToArray();
		}
	}

	public static MIME_Message ParseFromFile(string file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == "")
		{
			throw new ArgumentException("Argument 'file' value must be specified.");
		}
		using FileStream stream = File.OpenRead(file);
		return ParseFromStream(stream, Encoding.UTF8);
	}

	public static MIME_Message ParseFromFile(string file, Encoding headerEncoding)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == "")
		{
			throw new ArgumentException("Argument 'file' value must be specified.");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		using FileStream stream = File.OpenRead(file);
		return ParseFromStream(stream, headerEncoding);
	}

	public static MIME_Message ParseFromStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return ParseFromStream(stream, Encoding.UTF8);
	}

	public static MIME_Message ParseFromStream(Stream stream, Encoding headerEncoding)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		MIME_Message mIME_Message = new MIME_Message();
		mIME_Message.Parse(new SmartStream(stream, owner: false), headerEncoding, new MIME_h_ContentType("text/plain"));
		return mIME_Message;
	}

	public void ToFile(string file)
	{
		ToFile(file, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
	}

	public void ToStream(Stream stream)
	{
		ToStream(stream, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
	}

	public override string ToString()
	{
		return ToString(new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
	}

	public byte[] ToByte()
	{
		return ToByte(new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
	}

	public MIME_Entity[] GetAllEntities(bool includeEmbbedMessage)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		List<MIME_Entity> list = new List<MIME_Entity>();
		List<MIME_Entity> list2 = new List<MIME_Entity>();
		list2.Add(this);
		while (list2.Count > 0)
		{
			MIME_Entity mIME_Entity = list2[0];
			list2.RemoveAt(0);
			list.Add(mIME_Entity);
			if (base.Body != null && mIME_Entity.Body.GetType().IsSubclassOf(typeof(MIME_b_Multipart)))
			{
				MIME_EntityCollection bodyParts = ((MIME_b_Multipart)mIME_Entity.Body).BodyParts;
				for (int i = 0; i < bodyParts.Count; i++)
				{
					list2.Insert(i, bodyParts[i]);
				}
			}
			else if (includeEmbbedMessage && base.Body != null && mIME_Entity.Body is MIME_b_MessageRfc822)
			{
				list2.Add(((MIME_b_MessageRfc822)mIME_Entity.Body).Message);
			}
		}
		return list.ToArray();
	}

	public MIME_Entity GetEntityByCID(string cid)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (cid == null)
		{
			throw new ArgumentNullException("cid");
		}
		if (cid == "")
		{
			throw new ArgumentException("Argument 'cid' value must be specified.", "cid");
		}
		MIME_Entity[] allEntities = AllEntities;
		foreach (MIME_Entity mIME_Entity in allEntities)
		{
			if (mIME_Entity.ContentID == cid)
			{
				return mIME_Entity;
			}
		}
		return null;
	}

	public void ConvertToMultipartSigned(X509Certificate2 signerCert)
	{
		if (signerCert == null)
		{
			throw new ArgumentNullException("signerCert");
		}
		if (IsSigned)
		{
			throw new InvalidOperationException("Message is already signed.");
		}
		MIME_Entity mIME_Entity = new MIME_Entity();
		mIME_Entity.Body = base.Body;
		mIME_Entity.ContentDisposition = base.ContentDisposition;
		mIME_Entity.ContentTransferEncoding = base.ContentTransferEncoding;
		base.ContentTransferEncoding = null;
		MIME_b_MultipartSigned mIME_b_MultipartSigned = (MIME_b_MultipartSigned)(base.Body = new MIME_b_MultipartSigned());
		mIME_b_MultipartSigned.SetCertificate(signerCert);
		mIME_b_MultipartSigned.BodyParts.Add(mIME_Entity);
	}

	public bool VerifySignatures()
	{
		MIME_Entity[] allEntities = AllEntities;
		foreach (MIME_Entity mIME_Entity in allEntities)
		{
			if (string.Equals(mIME_Entity.ContentType.TypeWithSubtype, MIME_MediaTypes.Application.pkcs7_mime, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!((MIME_b_ApplicationPkcs7Mime)mIME_Entity.Body).VerifySignature())
				{
					return false;
				}
			}
			else if (string.Equals(mIME_Entity.ContentType.TypeWithSubtype, MIME_MediaTypes.Multipart.signed, StringComparison.InvariantCultureIgnoreCase) && !((MIME_b_MultipartSigned)mIME_Entity.Body).VerifySignature())
			{
				return false;
			}
		}
		return true;
	}

	[Obsolete("Use MIME_Entity.CreateEntity_Attachment instead.")]
	public static MIME_Entity CreateAttachment(string file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		MIME_Entity mIME_Entity = new MIME_Entity();
		MIME_b_Application mIME_b_Application = (MIME_b_Application)(mIME_Entity.Body = new MIME_b_Application(MIME_MediaTypes.Application.octet_stream));
		mIME_b_Application.SetDataFromFile(file, MIME_TransferEncodings.Base64);
		mIME_Entity.ContentType.Param_Name = Path.GetFileName(file);
		FileInfo fileInfo = new FileInfo(file);
		mIME_Entity.ContentDisposition = new MIME_h_ContentDisposition(MIME_DispositionTypes.Attachment)
		{
			Param_FileName = Path.GetFileName(file),
			Param_Size = fileInfo.Length,
			Param_CreationDate = fileInfo.CreationTime,
			Param_ModificationDate = fileInfo.LastWriteTime,
			Param_ReadDate = fileInfo.LastAccessTime
		};
		return mIME_Entity;
	}

	[Obsolete("Use MIME_Entity.CreateEntity_Attachment instead.")]
	public static MIME_Entity CreateAttachment(Stream stream, string fileName)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		long param_Size = (stream.CanSeek ? (stream.Length - stream.Position) : (-1));
		MIME_Entity mIME_Entity = new MIME_Entity();
		MIME_b_Application mIME_b_Application = (MIME_b_Application)(mIME_Entity.Body = new MIME_b_Application(MIME_MediaTypes.Application.octet_stream));
		mIME_b_Application.SetData(stream, MIME_TransferEncodings.Base64);
		mIME_Entity.ContentType.Param_Name = Path.GetFileName(fileName);
		mIME_Entity.ContentDisposition = new MIME_h_ContentDisposition(MIME_DispositionTypes.Attachment)
		{
			Param_FileName = Path.GetFileName(fileName),
			Param_Size = param_Size
		};
		return mIME_Entity;
	}
}
