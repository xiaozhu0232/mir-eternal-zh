using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public abstract class MIME_b_SinglepartBase : MIME_b
{
	private bool m_IsModified;

	private Stream m_pEncodedDataStream;

	public override bool IsModified => m_IsModified;

	public int EncodedDataSize => (int)m_pEncodedDataStream.Length;

	public byte[] EncodedData
	{
		get
		{
			MemoryStream memoryStream = new MemoryStream();
			Net_Utils.StreamCopy(GetEncodedDataStream(), memoryStream, 84000);
			return memoryStream.ToArray();
		}
	}

	public byte[] Data
	{
		get
		{
			MemoryStream memoryStream = new MemoryStream();
			Net_Utils.StreamCopy(GetDataStream(), memoryStream, 84000);
			return memoryStream.ToArray();
		}
	}

	protected Stream EncodedStream => m_pEncodedDataStream;

	public MIME_b_SinglepartBase(MIME_h_ContentType contentType)
		: base(contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		m_pEncodedDataStream = new MemoryStreamEx();
	}

	~MIME_b_SinglepartBase()
	{
		if (m_pEncodedDataStream != null)
		{
			m_pEncodedDataStream.Close();
		}
	}

	internal override void SetParent(MIME_Entity entity, bool setContentType)
	{
		base.SetParent(entity, setContentType);
		if (setContentType && (base.Entity.ContentType == null || !string.Equals(base.Entity.ContentType.TypeWithSubtype, base.MediaType, StringComparison.InvariantCultureIgnoreCase)))
		{
			base.Entity.ContentType = new MIME_h_ContentType(base.MediaType);
		}
	}

	protected internal override void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Net_Utils.StreamCopy(GetEncodedDataStream(), stream, 84000);
	}

	protected void SetModified(bool isModified)
	{
		m_IsModified = isModified;
	}

	public Stream GetEncodedDataStream()
	{
		if (base.Entity == null)
		{
			throw new InvalidOperationException("Body must be bounded to some entity first.");
		}
		m_pEncodedDataStream.Position = 0L;
		return m_pEncodedDataStream;
	}

	public void SetEncodedData(string contentTransferEncoding, Stream stream)
	{
		if (contentTransferEncoding == null)
		{
			throw new ArgumentNullException("contentTransferEncoding");
		}
		if (contentTransferEncoding == string.Empty)
		{
			throw new ArgumentException("Argument 'contentTransferEncoding' value must be specified.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (base.Entity == null)
		{
			throw new InvalidOperationException("Body must be bounded to some entity first.");
		}
		if (base.Entity.ContentType == null || !string.Equals(base.Entity.ContentType.TypeWithSubtype, base.MediaType, StringComparison.InvariantCultureIgnoreCase))
		{
			base.Entity.ContentType = new MIME_h_ContentType(base.MediaType);
		}
		base.Entity.ContentTransferEncoding = contentTransferEncoding;
		m_pEncodedDataStream.SetLength(0L);
		Net_Utils.StreamCopy(stream, m_pEncodedDataStream, 84000);
		m_IsModified = true;
	}

	public Stream GetDataStream()
	{
		if (base.Entity == null)
		{
			throw new InvalidOperationException("Body must be bounded to some entity first.");
		}
		string text = MIME_TransferEncodings.SevenBit;
		if (base.Entity.ContentTransferEncoding != null)
		{
			text = base.Entity.ContentTransferEncoding.ToLowerInvariant();
		}
		m_pEncodedDataStream.Position = 0L;
		if (text == MIME_TransferEncodings.QuotedPrintable)
		{
			return new QuotedPrintableStream(new SmartStream(m_pEncodedDataStream, owner: false), FileAccess.Read);
		}
		if (text == MIME_TransferEncodings.Base64)
		{
			return new Base64Stream(m_pEncodedDataStream, owner: false, addLineBreaks: true, FileAccess.Read)
			{
				IgnoreInvalidPadding = true
			};
		}
		if (text == MIME_TransferEncodings.Binary)
		{
			return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
		}
		if (text == MIME_TransferEncodings.EightBit)
		{
			return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
		}
		if (text == MIME_TransferEncodings.SevenBit)
		{
			return new ReadWriteControlledStream(m_pEncodedDataStream, FileAccess.Read);
		}
		throw new NotSupportedException("Not supported Content-Transfer-Encoding '" + base.Entity.ContentTransferEncoding + "'.");
	}

	public void SetData(Stream stream, string transferEncoding)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (transferEncoding == null)
		{
			throw new ArgumentNullException("transferEncoding");
		}
		if (string.Equals(transferEncoding, MIME_TransferEncodings.QuotedPrintable, StringComparison.InvariantCultureIgnoreCase))
		{
			using (MemoryStreamEx memoryStreamEx = new MemoryStreamEx())
			{
				QuotedPrintableStream quotedPrintableStream = new QuotedPrintableStream(new SmartStream(memoryStreamEx, owner: false), FileAccess.ReadWrite);
				Net_Utils.StreamCopy(stream, quotedPrintableStream, 84000);
				quotedPrintableStream.Flush();
				memoryStreamEx.Position = 0L;
				SetEncodedData(transferEncoding, memoryStreamEx);
				return;
			}
		}
		if (string.Equals(transferEncoding, MIME_TransferEncodings.Base64, StringComparison.InvariantCultureIgnoreCase))
		{
			using (MemoryStreamEx memoryStreamEx2 = new MemoryStreamEx())
			{
				Base64Stream base64Stream = new Base64Stream(memoryStreamEx2, owner: false, addLineBreaks: true, FileAccess.ReadWrite);
				Net_Utils.StreamCopy(stream, base64Stream, 84000);
				base64Stream.Finish();
				memoryStreamEx2.Position = 0L;
				SetEncodedData(transferEncoding, memoryStreamEx2);
				return;
			}
		}
		if (string.Equals(transferEncoding, MIME_TransferEncodings.Binary, StringComparison.InvariantCultureIgnoreCase))
		{
			SetEncodedData(transferEncoding, stream);
			return;
		}
		if (string.Equals(transferEncoding, MIME_TransferEncodings.EightBit, StringComparison.InvariantCultureIgnoreCase))
		{
			SetEncodedData(transferEncoding, stream);
			return;
		}
		if (string.Equals(transferEncoding, MIME_TransferEncodings.SevenBit, StringComparison.InvariantCultureIgnoreCase))
		{
			SetEncodedData(transferEncoding, stream);
			return;
		}
		throw new NotSupportedException("Not supported Content-Transfer-Encoding '" + transferEncoding + "'.");
	}

	public void SetDataFromFile(string file, string transferEncoding)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		using FileStream stream = File.OpenRead(file);
		SetData(stream, transferEncoding);
	}

	public void DataToStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using Stream source = GetDataStream();
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
		using Stream target = File.Create(fileName);
		using Stream source = GetDataStream();
		Net_Utils.StreamCopy(source, target, 64000);
	}
}
