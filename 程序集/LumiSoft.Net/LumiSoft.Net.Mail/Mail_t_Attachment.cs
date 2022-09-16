using System;
using System.IO;
using System.IO.Compression;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.Mail;

public class Mail_t_Attachment
{
	private string m_Name;

	private string m_FileName;

	private bool m_ZipCompress;

	private bool m_CloseStream = true;

	private Stream m_pStream;

	public string Name
	{
		get
		{
			if (m_ZipCompress)
			{
				return Path.GetFileNameWithoutExtension(m_Name) + ".zip";
			}
			return m_Name;
		}
	}

	public Mail_t_Attachment(string fileName)
		: this(null, fileName)
	{
	}

	public Mail_t_Attachment(string fileName, bool zipCompress)
		: this(null, fileName, zipCompress)
	{
	}

	public Mail_t_Attachment(string attachmentName, string fileName)
		: this(null, fileName, zipCompress: false)
	{
	}

	public Mail_t_Attachment(string attachmentName, string fileName, bool zipCompress)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentException("Argument 'fileName' value must be specified.", "fileName");
		}
		if (string.IsNullOrEmpty(attachmentName))
		{
			m_Name = Path.GetFileName(fileName);
		}
		else
		{
			m_Name = attachmentName;
		}
		m_FileName = fileName;
		m_ZipCompress = zipCompress;
		m_CloseStream = true;
	}

	public Mail_t_Attachment(string attachmentName, byte[] data)
		: this(attachmentName, data, zipCompress: false)
	{
	}

	public Mail_t_Attachment(string attachmentName, byte[] data, bool zipCompress)
	{
		if (attachmentName == null)
		{
			throw new ArgumentNullException("attachmentName");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_Name = attachmentName;
		m_pStream = new MemoryStream(data);
	}

	public Mail_t_Attachment(string attachmentName, Stream stream)
		: this(attachmentName, stream, zipCompress: false)
	{
	}

	public Mail_t_Attachment(string attachmentName, Stream stream, bool zipCompress)
	{
		if (attachmentName == null)
		{
			throw new ArgumentNullException("attachmentName");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_Name = attachmentName;
		m_pStream = stream;
	}

	internal Stream GetStream()
	{
		if (m_pStream == null)
		{
			m_pStream = File.OpenRead(m_FileName);
		}
		if (m_ZipCompress)
		{
			MemoryStreamEx memoryStreamEx = new MemoryStreamEx();
			using (ZipArchive zipArchive = new ZipArchive(memoryStreamEx, ZipArchiveMode.Create))
			{
				using Stream target = zipArchive.CreateEntry(m_Name, CompressionLevel.Optimal).Open();
				Net_Utils.StreamCopy(m_pStream, target, 64000);
			}
			memoryStreamEx.Position = 0L;
			CloseStream();
			return memoryStreamEx;
		}
		return m_pStream;
	}

	internal void CloseStream()
	{
		if (m_CloseStream)
		{
			m_pStream.Dispose();
		}
	}
}
