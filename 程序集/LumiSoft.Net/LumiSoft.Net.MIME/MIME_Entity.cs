using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_Entity : IDisposable
{
	private bool m_IsDisposed;

	private MIME_Entity m_pParent;

	private MIME_h_Collection m_pHeader;

	private MIME_b m_pBody;

	private MIME_b_Provider m_pBodyProvider;

	public bool IsDisposed => m_IsDisposed;

	public bool IsModified
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_pHeader.IsModified)
			{
				return m_pBody.IsModified;
			}
			return true;
		}
	}

	public MIME_Entity Parent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pParent;
		}
	}

	public MIME_h_Collection Header
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pHeader;
		}
	}

	public string MimeVersion
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("MIME-Version");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("MIME-Version");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("MIME-Version");
			if (first == null)
			{
				first = new MIME_h_Unstructured("MIME-Version", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-ID");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-ID");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-ID");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-ID", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentDescription
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Description");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Description");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Description");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Description", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentTransferEncoding
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Transfer-Encoding");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value.Trim();
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Transfer-Encoding");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Transfer-Encoding");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Transfer-Encoding", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public MIME_h_ContentType ContentType
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Type");
			if (first != null)
			{
				if (!(first is MIME_h_ContentType))
				{
					throw new ParseException("Header field 'ContentType' parsing failed.");
				}
				return (MIME_h_ContentType)first;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Type");
			}
			else if (m_pHeader.GetFirst("Content-Type") == null)
			{
				m_pHeader.Add(value);
			}
			else
			{
				m_pHeader.ReplaceFirst(value);
			}
		}
	}

	public string ContentBase
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Base");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Base");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Base");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Base", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentLocation
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Location");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Location");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Location");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Location", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string Contentfeatures
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-features");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-features");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-features");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-features", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public MIME_h_ContentDisposition ContentDisposition
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Disposition");
			if (first != null)
			{
				if (!(first is MIME_h_ContentDisposition))
				{
					throw new ParseException("Header field 'ContentDisposition' parsing failed.");
				}
				return (MIME_h_ContentDisposition)first;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Disposition");
			}
			else if (m_pHeader.GetFirst("Content-Disposition") == null)
			{
				m_pHeader.Add(value);
			}
			else
			{
				m_pHeader.ReplaceFirst(value);
			}
		}
	}

	public string ContentLanguage
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Language");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Language");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Language");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Language", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentAlternative
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Alternative");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Alternative");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Alternative");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Alternative", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentMD5
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-MD5");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-MD5");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-MD5");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-MD5", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public string ContentDuration
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = m_pHeader.GetFirst("Content-Duration");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				m_pHeader.RemoveAll("Content-Duration");
				return;
			}
			MIME_h first = m_pHeader.GetFirst("Content-Duration");
			if (first == null)
			{
				first = new MIME_h_Unstructured("Content-Duration", value);
				m_pHeader.Add(first);
			}
			else
			{
				((MIME_h_Unstructured)first).Value = value;
			}
		}
	}

	public MIME_b Body
	{
		get
		{
			return m_pBody;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Body");
			}
			m_pBody = value;
			m_pBody.SetParent(this, setContentType: true);
		}
	}

	public MIME_Entity()
	{
		m_pHeader = new MIME_h_Collection(new MIME_h_Provider());
		m_pBodyProvider = new MIME_b_Provider();
	}

	public void Dispose()
	{
		lock (this)
		{
			if (!m_IsDisposed)
			{
				m_IsDisposed = true;
				m_pHeader = null;
				m_pParent = null;
			}
		}
	}

	public static MIME_Entity CreateEntity_Text_Plain(string transferEncoding, Encoding charset, string text)
	{
		if (transferEncoding == null)
		{
			throw new ArgumentNullException("transferEncoding");
		}
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		MIME_Entity mIME_Entity = new MIME_Entity();
		MIME_b_Text mIME_b_Text = (MIME_b_Text)(mIME_Entity.Body = new MIME_b_Text(MIME_MediaTypes.Text.plain));
		mIME_b_Text.SetText(transferEncoding, charset, text);
		return mIME_Entity;
	}

	public static MIME_Entity CreateEntity_Text_Html(string transferEncoding, Encoding charset, string text)
	{
		if (transferEncoding == null)
		{
			throw new ArgumentNullException("transferEncoding");
		}
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		MIME_Entity mIME_Entity = new MIME_Entity();
		MIME_b_Text mIME_b_Text = (MIME_b_Text)(mIME_Entity.Body = new MIME_b_Text(MIME_MediaTypes.Text.html));
		mIME_b_Text.SetText(transferEncoding, charset, text);
		return mIME_Entity;
	}

	public static MIME_Entity CreateEntity_Attachment(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("stream");
		}
		using Stream stream = File.OpenRead(fileName);
		return CreateEntity_Attachment(Path.GetFileName(fileName), stream);
	}

	public static MIME_Entity CreateEntity_Attachment(string attachmentName, string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("stream");
		}
		using Stream stream = File.OpenRead(fileName);
		return CreateEntity_Attachment(string.IsNullOrEmpty(attachmentName) ? Path.GetFileName(fileName) : attachmentName, stream);
	}

	public static MIME_Entity CreateEntity_Attachment(string attachmentName, Stream stream)
	{
		if (attachmentName == null)
		{
			throw new ArgumentNullException("attachmentName");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long param_Size = (stream.CanSeek ? (stream.Length - stream.Position) : (-1));
		MIME_Entity mIME_Entity = new MIME_Entity();
		MIME_b_Application mIME_b_Application = (MIME_b_Application)(mIME_Entity.Body = new MIME_b_Application(MIME_MediaTypes.Application.octet_stream));
		mIME_b_Application.SetData(stream, MIME_TransferEncodings.Base64);
		mIME_Entity.ContentType.Param_Name = Path.GetFileName(attachmentName);
		mIME_Entity.ContentDisposition = new MIME_h_ContentDisposition(MIME_DispositionTypes.Attachment)
		{
			Param_FileName = Path.GetFileName(attachmentName),
			Param_Size = param_Size
		};
		return mIME_Entity;
	}

	public void ToFile(string file, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset)
	{
		ToFile(file, headerWordEncoder, headerParmetersCharset, headerReencode: false);
	}

	public void ToFile(string file, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == "")
		{
			throw new ArgumentException("Argument 'file' value must be specified.");
		}
		using FileStream stream = File.Create(file);
		ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
	}

	public void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset)
	{
		ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode: false);
	}

	public void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pHeader.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
		stream.Write(new byte[2] { 13, 10 }, 0, 2);
		m_pBody.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset)
	{
		return ToString(headerWordEncoder, headerParmetersCharset, headerReencode: false);
	}

	public string ToString(MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		using MemoryStream memoryStream = new MemoryStream();
		ToStream(memoryStream, headerWordEncoder, headerParmetersCharset, headerReencode);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	public byte[] ToByte(MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset)
	{
		return ToByte(headerWordEncoder, headerParmetersCharset, headerReencode: false);
	}

	public byte[] ToByte(MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		using MemoryStream memoryStream = new MemoryStream();
		ToStream(memoryStream, headerWordEncoder, headerParmetersCharset, headerReencode);
		return memoryStream.ToArray();
	}

	public void DataToStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (Body == null)
		{
			throw new InvalidOperationException("Mime entity body has been not set yet.");
		}
		if (!(Body is MIME_b_SinglepartBase))
		{
			throw new InvalidOperationException("This method is available only for single part entities, not for multipart.");
		}
		using Stream source = ((MIME_b_SinglepartBase)Body).GetDataStream();
		Net_Utils.StreamCopy(source, stream, 64000);
	}

	public void DataToFile(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (fileName == string.Empty)
		{
			throw new ArgumentException("Argument 'fileName' value must be specified.");
		}
		if (Body == null)
		{
			throw new InvalidOperationException("Mime entity body has been not set yet.");
		}
		if (!(Body is MIME_b_SinglepartBase))
		{
			throw new InvalidOperationException("This method is available only for single part entities, not for multipart.");
		}
		MIME_b_SinglepartBase mIME_b_SinglepartBase = (MIME_b_SinglepartBase)Body;
		using Stream target = File.Create(fileName);
		using Stream source = mIME_b_SinglepartBase.GetDataStream();
		Net_Utils.StreamCopy(source, target, 64000);
	}

	public byte[] DataToByte()
	{
		if (Body == null)
		{
			throw new InvalidOperationException("Mime entity body has been not set yet.");
		}
		if (!(Body is MIME_b_SinglepartBase))
		{
			throw new InvalidOperationException("This method is available only for single part entities, not for multipart.");
		}
		MemoryStream memoryStream = new MemoryStream();
		using (Stream source = ((MIME_b_SinglepartBase)Body).GetDataStream())
		{
			Net_Utils.StreamCopy(source, memoryStream, 64000);
		}
		return memoryStream.ToArray();
	}

	protected internal void Parse(SmartStream stream, Encoding headerEncoding, MIME_h_ContentType defaultContentType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		if (defaultContentType == null)
		{
			throw new ArgumentNullException("defaultContentType");
		}
		m_pHeader.Parse(stream, headerEncoding);
		m_pBody = m_pBodyProvider.Parse(this, stream, defaultContentType);
		m_pBody.SetParent(this, setContentType: false);
	}

	internal void SetParent(MIME_Entity parent)
	{
		m_pParent = parent;
	}
}
