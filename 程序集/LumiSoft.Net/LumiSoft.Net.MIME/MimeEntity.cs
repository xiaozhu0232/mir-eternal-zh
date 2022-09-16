using System;
using System.Collections;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class MimeEntity
{
	private HeaderFieldCollection m_pHeader;

	private MimeEntity m_pParentEntity;

	private MimeEntityCollection m_pChildEntities;

	private byte[] m_EncodedData;

	private Hashtable m_pHeaderFieldCache;

	public HeaderFieldCollection Header => m_pHeader;

	public string HeaderString => m_pHeader.ToHeaderString("utf-8");

	public MimeEntity ParentEntity => m_pParentEntity;

	public MimeEntityCollection ChildEntities => m_pChildEntities;

	public string MimeVersion
	{
		get
		{
			if (m_pHeader.Contains("Mime-Version:"))
			{
				return m_pHeader.GetFirst("Mime-Version:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Mime-Version:"))
			{
				m_pHeader.GetFirst("Mime-Version:").Value = value;
			}
			else
			{
				m_pHeader.Add("Mime-Version:", value);
			}
		}
	}

	public string ContentClass
	{
		get
		{
			if (m_pHeader.Contains("Content-Class:"))
			{
				return m_pHeader.GetFirst("Content-Class:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Content-Class:"))
			{
				m_pHeader.GetFirst("Content-Class:").Value = value;
			}
			else
			{
				m_pHeader.Add("Content-Class:", value);
			}
		}
	}

	public MediaType_enum ContentType
	{
		get
		{
			if (m_pHeader.Contains("Content-Type:"))
			{
				return MimeUtils.ParseMediaType(new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:")).Value);
			}
			return MediaType_enum.NotSpecified;
		}
		set
		{
			if (DataEncoded != null)
			{
				throw new Exception("ContentType can't be changed while there is data specified, set data to null before !");
			}
			switch (value)
			{
			case MediaType_enum.Unknown:
				throw new Exception("MediaType_enum.Unkown isn't allowed to set !");
			case MediaType_enum.NotSpecified:
				throw new Exception("MediaType_enum.NotSpecified isn't allowed to set !");
			}
			string text = "";
			text = value switch
			{
				MediaType_enum.Text_plain => "text/plain; charset=\"utf-8\"", 
				MediaType_enum.Text_html => "text/html; charset=\"utf-8\"", 
				MediaType_enum.Text_xml => "text/xml; charset=\"utf-8\"", 
				MediaType_enum.Text_rtf => "text/rtf; charset=\"utf-8\"", 
				MediaType_enum.Text => "text; charset=\"utf-8\"", 
				MediaType_enum.Image_gif => "image/gif", 
				MediaType_enum.Image_tiff => "image/tiff", 
				MediaType_enum.Image_jpeg => "image/jpeg", 
				MediaType_enum.Image => "image", 
				MediaType_enum.Audio => "audio", 
				MediaType_enum.Video => "video", 
				MediaType_enum.Application_octet_stream => "application/octet-stream", 
				MediaType_enum.Application => "application", 
				MediaType_enum.Multipart_mixed => "multipart/mixed;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Multipart_alternative => "multipart/alternative;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Multipart_parallel => "multipart/parallel;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Multipart_related => "multipart/related;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Multipart_signed => "multipart/signed;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Multipart => "multipart;\tboundary=\"part_" + Guid.NewGuid().ToString().Replace("-", "_") + "\"", 
				MediaType_enum.Message_rfc822 => "message/rfc822", 
				MediaType_enum.Message => "message", 
				_ => throw new Exception("Invalid flags combination of MediaType_enum was specified !"), 
			};
			if (m_pHeader.Contains("Content-Type:"))
			{
				m_pHeader.GetFirst("Content-Type:").Value = text;
			}
			else
			{
				m_pHeader.Add("Content-Type:", text);
			}
		}
	}

	public string ContentTypeString
	{
		get
		{
			if (m_pHeader.Contains("Content-Type:"))
			{
				return m_pHeader.GetFirst("Content-Type:").Value;
			}
			return null;
		}
		set
		{
			if (DataEncoded != null)
			{
				throw new Exception("ContentType can't be changed while there is data specified, set data to null before !");
			}
			if (m_pHeader.Contains("Content-Type:"))
			{
				m_pHeader.GetFirst("Content-Type:").Value = value;
			}
			else
			{
				m_pHeader.Add("Content-Type:", value);
			}
		}
	}

	public ContentTransferEncoding_enum ContentTransferEncoding
	{
		get
		{
			if (m_pHeader.Contains("Content-Transfer-Encoding:"))
			{
				return MimeUtils.ParseContentTransferEncoding(m_pHeader.GetFirst("Content-Transfer-Encoding:").Value);
			}
			return ContentTransferEncoding_enum.NotSpecified;
		}
		set
		{
			switch (value)
			{
			case ContentTransferEncoding_enum.Unknown:
				throw new Exception("ContentTransferEncoding_enum.Unknown isn't allowed to set !");
			case ContentTransferEncoding_enum.NotSpecified:
				throw new Exception("ContentTransferEncoding_enum.NotSpecified isn't allowed to set !");
			}
			string value2 = MimeUtils.ContentTransferEncodingToString(value);
			if (DataEncoded != null)
			{
				ContentTransferEncoding_enum contentTransferEncoding = ContentTransferEncoding;
				if (contentTransferEncoding == ContentTransferEncoding_enum.Unknown || contentTransferEncoding == ContentTransferEncoding_enum.NotSpecified)
				{
					throw new Exception("Data can't be converted because old encoding '" + MimeUtils.ContentTransferEncodingToString(contentTransferEncoding) + "' is unknown !");
				}
				DataEncoded = EncodeData(Data, value);
			}
			if (m_pHeader.Contains("Content-Transfer-Encoding:"))
			{
				m_pHeader.GetFirst("Content-Transfer-Encoding:").Value = value2;
			}
			else
			{
				m_pHeader.Add("Content-Transfer-Encoding:", value2);
			}
		}
	}

	public ContentDisposition_enum ContentDisposition
	{
		get
		{
			if (m_pHeader.Contains("Content-Disposition:"))
			{
				return MimeUtils.ParseContentDisposition(m_pHeader.GetFirst("Content-Disposition:").Value);
			}
			return ContentDisposition_enum.NotSpecified;
		}
		set
		{
			switch (value)
			{
			case ContentDisposition_enum.Unknown:
				throw new Exception("ContentDisposition_enum.Unknown isn't allowed to set !");
			case ContentDisposition_enum.NotSpecified:
			{
				HeaderField first = m_pHeader.GetFirst("Content-Disposition:");
				if (first != null)
				{
					m_pHeader.Remove(first);
				}
				break;
			}
			default:
			{
				string value2 = MimeUtils.ContentDispositionToString(value);
				if (m_pHeader.Contains("Content-Disposition:"))
				{
					m_pHeader.GetFirst("Content-Disposition:").Value = value2;
				}
				else
				{
					m_pHeader.Add("Content-Disposition:", value2);
				}
				break;
			}
			}
		}
	}

	public string ContentDescription
	{
		get
		{
			if (m_pHeader.Contains("Content-Description:"))
			{
				return m_pHeader.GetFirst("Content-Description:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Content-Description:"))
			{
				m_pHeader.GetFirst("Content-Description:").Value = value;
			}
			else
			{
				m_pHeader.Add("Content-Description:", value);
			}
		}
	}

	public string ContentID
	{
		get
		{
			if (m_pHeader.Contains("Content-ID:"))
			{
				return m_pHeader.GetFirst("Content-ID:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Content-ID:"))
			{
				m_pHeader.GetFirst("Content-ID:").Value = value;
			}
			else
			{
				m_pHeader.Add("Content-ID:", value);
			}
		}
	}

	public string ContentType_Name
	{
		get
		{
			if (m_pHeader.Contains("Content-Type:"))
			{
				ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
				if (parametizedHeaderField.Parameters.Contains("name"))
				{
					return parametizedHeaderField.Parameters["name"];
				}
				return null;
			}
			return null;
		}
		set
		{
			if (!m_pHeader.Contains("Content-Type:"))
			{
				throw new Exception("Please specify Content-Type first !");
			}
			if ((ContentType & MediaType_enum.Application) == 0)
			{
				throw new Exception("Parameter name is available only for ContentType application/xxx !");
			}
			ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
			if (parametizedHeaderField.Parameters.Contains("name"))
			{
				parametizedHeaderField.Parameters["name"] = value;
			}
			else
			{
				parametizedHeaderField.Parameters.Add("name", value);
			}
		}
	}

	public string ContentType_CharSet
	{
		get
		{
			if (m_pHeader.Contains("Content-Type:"))
			{
				ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
				if (parametizedHeaderField.Parameters.Contains("charset"))
				{
					return parametizedHeaderField.Parameters["charset"];
				}
				return null;
			}
			return null;
		}
		set
		{
			if (!m_pHeader.Contains("Content-Type:"))
			{
				throw new Exception("Please specify Content-Type first !");
			}
			if ((ContentType & MediaType_enum.Text) == 0)
			{
				throw new Exception("Parameter boundary is available only for ContentType text/xxx !");
			}
			if (DataEncoded != null)
			{
				string text = ContentType_CharSet;
				if (text == null)
				{
					text = "ascii";
				}
				try
				{
					Encoding.GetEncoding(text);
				}
				catch
				{
					throw new Exception("Data can't be converted because current charset '" + text + "' isn't supported !");
				}
				try
				{
					Encoding encoding = Encoding.GetEncoding(value);
					Data = encoding.GetBytes(DataText);
				}
				catch
				{
					throw new Exception("Data can't be converted because new charset '" + value + "' isn't supported !");
				}
			}
			ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
			if (parametizedHeaderField.Parameters.Contains("charset"))
			{
				parametizedHeaderField.Parameters["charset"] = value;
			}
			else
			{
				parametizedHeaderField.Parameters.Add("charset", value);
			}
		}
	}

	public string ContentType_Boundary
	{
		get
		{
			if (m_pHeader.Contains("Content-Type:"))
			{
				ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
				if (parametizedHeaderField.Parameters.Contains("boundary"))
				{
					return parametizedHeaderField.Parameters["boundary"];
				}
				return null;
			}
			return null;
		}
		set
		{
			if (!m_pHeader.Contains("Content-Type:"))
			{
				throw new Exception("Please specify Content-Type first !");
			}
			if ((ContentType & MediaType_enum.Multipart) == 0)
			{
				throw new Exception("Parameter boundary is available only for ContentType multipart/xxx !");
			}
			ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Type:"));
			if (parametizedHeaderField.Parameters.Contains("boundary"))
			{
				parametizedHeaderField.Parameters["boundary"] = value;
			}
			else
			{
				parametizedHeaderField.Parameters.Add("boundary", value);
			}
		}
	}

	public string ContentDisposition_FileName
	{
		get
		{
			if (m_pHeader.Contains("Content-Disposition:"))
			{
				ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Disposition:"));
				if (parametizedHeaderField.Parameters.Contains("filename"))
				{
					return MimeUtils.DecodeWords(parametizedHeaderField.Parameters["filename"]);
				}
				return null;
			}
			return null;
		}
		set
		{
			if (!m_pHeader.Contains("Content-Disposition:"))
			{
				throw new Exception("Please specify Content-Disposition first !");
			}
			ParametizedHeaderField parametizedHeaderField = new ParametizedHeaderField(m_pHeader.GetFirst("Content-Disposition:"));
			if (parametizedHeaderField.Parameters.Contains("filename"))
			{
				parametizedHeaderField.Parameters["filename"] = MimeUtils.EncodeWord(value);
			}
			else
			{
				parametizedHeaderField.Parameters.Add("filename", MimeUtils.EncodeWord(value));
			}
		}
	}

	public DateTime Date
	{
		get
		{
			if (m_pHeader.Contains("Date:"))
			{
				try
				{
					return MIME_Utils.ParseRfc2822DateTime(m_pHeader.GetFirst("Date:").Value);
				}
				catch
				{
					return DateTime.MinValue;
				}
			}
			return DateTime.MinValue;
		}
		set
		{
			if (m_pHeader.Contains("Date:"))
			{
				m_pHeader.GetFirst("Date:").Value = MIME_Utils.DateTimeToRfc2822(value);
			}
			else
			{
				m_pHeader.Add("Date:", MimeUtils.DateTimeToRfc2822(value));
			}
		}
	}

	public string MessageID
	{
		get
		{
			if (m_pHeader.Contains("Message-ID:"))
			{
				return m_pHeader.GetFirst("Message-ID:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Message-ID:"))
			{
				m_pHeader.GetFirst("Message-ID:").Value = value;
			}
			else
			{
				m_pHeader.Add("Message-ID:", value);
			}
		}
	}

	public AddressList To
	{
		get
		{
			if (m_pHeader.Contains("To:"))
			{
				if (m_pHeaderFieldCache.Contains("To:"))
				{
					return (AddressList)m_pHeaderFieldCache["To:"];
				}
				HeaderField first = m_pHeader.GetFirst("To:");
				AddressList addressList = new AddressList();
				addressList.Parse(first.EncodedValue);
				addressList.BoundedHeaderField = first;
				m_pHeaderFieldCache["To:"] = addressList;
				return addressList;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.Remove(m_pHeader.GetFirst("To:"));
				return;
			}
			if (m_pHeaderFieldCache["To:"] != null)
			{
				((AddressList)m_pHeaderFieldCache["To:"]).BoundedHeaderField = null;
			}
			HeaderField headerField = m_pHeader.GetFirst("To:");
			if (headerField == null)
			{
				headerField = new HeaderField("To:", value.ToAddressListString());
				m_pHeader.Add(headerField);
			}
			else
			{
				headerField.Value = value.ToAddressListString();
			}
			value.BoundedHeaderField = headerField;
			m_pHeaderFieldCache["To:"] = value;
		}
	}

	public AddressList Cc
	{
		get
		{
			if (m_pHeader.Contains("Cc:"))
			{
				if (m_pHeaderFieldCache.Contains("Cc:"))
				{
					return (AddressList)m_pHeaderFieldCache["Cc:"];
				}
				HeaderField first = m_pHeader.GetFirst("Cc:");
				AddressList addressList = new AddressList();
				addressList.Parse(first.EncodedValue);
				addressList.BoundedHeaderField = first;
				m_pHeaderFieldCache["Cc:"] = addressList;
				return addressList;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.Remove(m_pHeader.GetFirst("Cc:"));
				return;
			}
			if (m_pHeaderFieldCache["Cc:"] != null)
			{
				((AddressList)m_pHeaderFieldCache["Cc:"]).BoundedHeaderField = null;
			}
			HeaderField headerField = m_pHeader.GetFirst("Cc:");
			if (headerField == null)
			{
				headerField = new HeaderField("Cc:", value.ToAddressListString());
				m_pHeader.Add(headerField);
			}
			else
			{
				headerField.Value = value.ToAddressListString();
			}
			value.BoundedHeaderField = headerField;
			m_pHeaderFieldCache["Cc:"] = value;
		}
	}

	public AddressList Bcc
	{
		get
		{
			if (m_pHeader.Contains("Bcc:"))
			{
				if (m_pHeaderFieldCache.Contains("Bcc:"))
				{
					return (AddressList)m_pHeaderFieldCache["Bcc:"];
				}
				HeaderField first = m_pHeader.GetFirst("Bcc:");
				AddressList addressList = new AddressList();
				addressList.Parse(first.EncodedValue);
				addressList.BoundedHeaderField = first;
				m_pHeaderFieldCache["Bcc:"] = addressList;
				return addressList;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_pHeader.Remove(m_pHeader.GetFirst("Bcc:"));
				return;
			}
			if (m_pHeaderFieldCache["Bcc:"] != null)
			{
				((AddressList)m_pHeaderFieldCache["Bcc:"]).BoundedHeaderField = null;
			}
			HeaderField headerField = m_pHeader.GetFirst("Bcc:");
			if (headerField == null)
			{
				headerField = new HeaderField("Bcc:", value.ToAddressListString());
				m_pHeader.Add(headerField);
			}
			else
			{
				headerField.Value = value.ToAddressListString();
			}
			value.BoundedHeaderField = headerField;
			m_pHeaderFieldCache["Bcc:"] = value;
		}
	}

	public AddressList From
	{
		get
		{
			if (m_pHeader.Contains("From:"))
			{
				if (m_pHeaderFieldCache.Contains("From:"))
				{
					return (AddressList)m_pHeaderFieldCache["From:"];
				}
				HeaderField first = m_pHeader.GetFirst("From:");
				AddressList addressList = new AddressList();
				addressList.Parse(first.EncodedValue);
				addressList.BoundedHeaderField = first;
				m_pHeaderFieldCache["From:"] = addressList;
				return addressList;
			}
			return null;
		}
		set
		{
			if (value == null && m_pHeader.Contains("From:"))
			{
				m_pHeader.Remove(m_pHeader.GetFirst("From:"));
				return;
			}
			if (m_pHeaderFieldCache["From:"] != null)
			{
				((AddressList)m_pHeaderFieldCache["From:"]).BoundedHeaderField = null;
			}
			HeaderField headerField = m_pHeader.GetFirst("From:");
			if (headerField == null)
			{
				headerField = new HeaderField("From:", value.ToAddressListString());
				m_pHeader.Add(headerField);
			}
			else
			{
				headerField.Value = value.ToAddressListString();
			}
			value.BoundedHeaderField = headerField;
			m_pHeaderFieldCache["From:"] = value;
		}
	}

	public MailboxAddress Sender
	{
		get
		{
			if (m_pHeader.Contains("Sender:"))
			{
				return MailboxAddress.Parse(m_pHeader.GetFirst("Sender:").EncodedValue);
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Sender:"))
			{
				m_pHeader.GetFirst("Sender:").Value = value.ToMailboxAddressString();
			}
			else
			{
				m_pHeader.Add("Sender:", value.ToMailboxAddressString());
			}
		}
	}

	public AddressList ReplyTo
	{
		get
		{
			if (m_pHeader.Contains("Reply-To:"))
			{
				if (m_pHeaderFieldCache.Contains("Reply-To:"))
				{
					return (AddressList)m_pHeaderFieldCache["Reply-To:"];
				}
				HeaderField first = m_pHeader.GetFirst("Reply-To:");
				AddressList addressList = new AddressList();
				addressList.Parse(first.Value);
				addressList.BoundedHeaderField = first;
				m_pHeaderFieldCache["Reply-To:"] = addressList;
				return addressList;
			}
			return null;
		}
		set
		{
			if (value == null && m_pHeader.Contains("Reply-To:"))
			{
				m_pHeader.Remove(m_pHeader.GetFirst("Reply-To:"));
				return;
			}
			if (m_pHeaderFieldCache["Reply-To:"] != null)
			{
				((AddressList)m_pHeaderFieldCache["Reply-To:"]).BoundedHeaderField = null;
			}
			HeaderField headerField = m_pHeader.GetFirst("Reply-To:");
			if (headerField == null)
			{
				headerField = new HeaderField("Reply-To:", value.ToAddressListString());
				m_pHeader.Add(headerField);
			}
			else
			{
				headerField.Value = value.ToAddressListString();
			}
			value.BoundedHeaderField = headerField;
			m_pHeaderFieldCache["Reply-To:"] = value;
		}
	}

	public string InReplyTo
	{
		get
		{
			if (m_pHeader.Contains("In-Reply-To:"))
			{
				return m_pHeader.GetFirst("In-Reply-To:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("In-Reply-To:"))
			{
				m_pHeader.GetFirst("In-Reply-To:").Value = value;
			}
			else
			{
				m_pHeader.Add("In-Reply-To:", value);
			}
		}
	}

	public string DSN
	{
		get
		{
			if (m_pHeader.Contains("Disposition-Notification-To:"))
			{
				return m_pHeader.GetFirst("Disposition-Notification-To:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Disposition-Notification-To:"))
			{
				m_pHeader.GetFirst("Disposition-Notification-To:").Value = value;
			}
			else
			{
				m_pHeader.Add("Disposition-Notification-To:", value);
			}
		}
	}

	public string Subject
	{
		get
		{
			if (m_pHeader.Contains("Subject:"))
			{
				return m_pHeader.GetFirst("Subject:").Value;
			}
			return null;
		}
		set
		{
			if (m_pHeader.Contains("Subject:"))
			{
				m_pHeader.GetFirst("Subject:").Value = value;
			}
			else
			{
				m_pHeader.Add("Subject:", value);
			}
		}
	}

	public byte[] Data
	{
		get
		{
			return ContentTransferEncoding switch
			{
				ContentTransferEncoding_enum.Base64 => Core.Base64Decode(DataEncoded), 
				ContentTransferEncoding_enum.QuotedPrintable => Core.QuotedPrintableDecode(DataEncoded), 
				_ => DataEncoded, 
			};
		}
		set
		{
			if (value == null)
			{
				DataEncoded = null;
				return;
			}
			ContentTransferEncoding_enum contentTransferEncoding = ContentTransferEncoding;
			DataEncoded = EncodeData(value, contentTransferEncoding);
		}
	}

	public string DataText
	{
		get
		{
			if ((ContentType & MediaType_enum.Text) == 0 && (ContentType & MediaType_enum.NotSpecified) == 0)
			{
				throw new Exception("This property is available only if ContentType is Text/xxx... !");
			}
			try
			{
				string contentType_CharSet = ContentType_CharSet;
				if (contentType_CharSet == null)
				{
					return Encoding.Default.GetString(Data);
				}
				return Encoding.GetEncoding(contentType_CharSet).GetString(Data);
			}
			catch
			{
				return Encoding.Default.GetString(Data);
			}
		}
		set
		{
			if (value == null)
			{
				DataEncoded = null;
				return;
			}
			string contentType_CharSet = ContentType_CharSet;
			if (contentType_CharSet == null)
			{
				throw new Exception("Please specify CharSet property first !");
			}
			Encoding encoding = null;
			try
			{
				encoding = Encoding.GetEncoding(contentType_CharSet);
			}
			catch
			{
				throw new Exception("Not supported charset '" + contentType_CharSet + "' ! If you need to use this charset, then set data through Data or DataEncoded property.");
			}
			Data = encoding.GetBytes(value);
		}
	}

	public byte[] DataEncoded
	{
		get
		{
			return m_EncodedData;
		}
		set
		{
			m_EncodedData = value;
		}
	}

	public MimeEntity()
	{
		m_pHeader = new HeaderFieldCollection();
		m_pChildEntities = new MimeEntityCollection(this);
		m_pHeaderFieldCache = new Hashtable();
	}

	internal bool Parse(SmartStream stream, string toBoundary)
	{
		m_pHeader.Clear();
		m_pHeaderFieldCache.Clear();
		m_pHeader.Parse(stream);
		if ((ContentType & MediaType_enum.Multipart) != 0)
		{
			string contentType_Boundary = ContentType_Boundary;
			if (contentType_Boundary != null)
			{
				SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				stream.ReadLine(readLineAsyncOP, async: false);
				if (readLineAsyncOP.Error != null)
				{
					throw readLineAsyncOP.Error;
				}
				string lineUtf = readLineAsyncOP.LineUtf8;
				while (lineUtf != null && !lineUtf.StartsWith("--" + contentType_Boundary))
				{
					stream.ReadLine(readLineAsyncOP, async: false);
					if (readLineAsyncOP.Error != null)
					{
						throw readLineAsyncOP.Error;
					}
					lineUtf = readLineAsyncOP.LineUtf8;
				}
				if (string.IsNullOrEmpty(lineUtf))
				{
					return false;
				}
				MimeEntity mimeEntity;
				do
				{
					mimeEntity = new MimeEntity();
					ChildEntities.Add(mimeEntity);
				}
				while (mimeEntity.Parse(stream, contentType_Boundary));
				if (!string.IsNullOrEmpty(toBoundary))
				{
					stream.ReadLine(readLineAsyncOP, async: false);
					if (readLineAsyncOP.Error != null)
					{
						throw readLineAsyncOP.Error;
					}
					lineUtf = readLineAsyncOP.LineUtf8;
					while (lineUtf != null && !lineUtf.StartsWith("--" + toBoundary))
					{
						stream.ReadLine(readLineAsyncOP, async: false);
						if (readLineAsyncOP.Error != null)
						{
							throw readLineAsyncOP.Error;
						}
						lineUtf = readLineAsyncOP.LineUtf8;
					}
					if (string.IsNullOrEmpty(lineUtf))
					{
						return false;
					}
					if (lineUtf.EndsWith(toBoundary + "--"))
					{
						return false;
					}
					return true;
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(toBoundary))
			{
				MemoryStream memoryStream = new MemoryStream();
				SmartStream.ReadLineAsyncOP readLineAsyncOP2 = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
				while (true)
				{
					stream.ReadLine(readLineAsyncOP2, async: false);
					if (readLineAsyncOP2.Error != null)
					{
						throw readLineAsyncOP2.Error;
					}
					if (readLineAsyncOP2.BytesInBuffer == 0)
					{
						m_EncodedData = memoryStream.ToArray();
						return false;
					}
					if (readLineAsyncOP2.LineBytesInBuffer >= 2 && readLineAsyncOP2.Buffer[0] == 45 && readLineAsyncOP2.Buffer[1] == 45)
					{
						string lineUtf2 = readLineAsyncOP2.LineUtf8;
						if (lineUtf2 == "--" + toBoundary + "--")
						{
							m_EncodedData = memoryStream.ToArray();
							return false;
						}
						if (lineUtf2 == "--" + toBoundary)
						{
							break;
						}
					}
					memoryStream.Write(readLineAsyncOP2.Buffer, 0, readLineAsyncOP2.BytesInBuffer);
				}
				m_EncodedData = memoryStream.ToArray();
				return true;
			}
			MemoryStream memoryStream2 = new MemoryStream();
			stream.ReadAll(memoryStream2);
			m_EncodedData = memoryStream2.ToArray();
		}
		return false;
	}

	public void ToStream(Stream storeStream)
	{
		byte[] bytes = Encoding.Default.GetBytes(FoldHeader(HeaderString));
		storeStream.Write(bytes, 0, bytes.Length);
		if ((ContentType & MediaType_enum.Multipart) != 0)
		{
			string contentType_Boundary = ContentType_Boundary;
			foreach (MimeEntity childEntity in ChildEntities)
			{
				bytes = Encoding.Default.GetBytes("\r\n--" + contentType_Boundary + "\r\n");
				storeStream.Write(bytes, 0, bytes.Length);
				childEntity.ToStream(storeStream);
			}
			bytes = Encoding.Default.GetBytes("\r\n--" + contentType_Boundary + "--\r\n");
			storeStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			storeStream.Write(new byte[2] { 13, 10 }, 0, 2);
			if (DataEncoded != null)
			{
				storeStream.Write(DataEncoded, 0, DataEncoded.Length);
			}
		}
	}

	public void DataToFile(string fileName)
	{
		using FileStream stream = File.Create(fileName);
		DataToStream(stream);
	}

	public void DataToStream(Stream stream)
	{
		byte[] data = Data;
		stream.Write(data, 0, data.Length);
	}

	public void DataFromFile(string fileName)
	{
		using FileStream stream = File.OpenRead(fileName);
		DataFromStream(stream);
	}

	public void DataFromStream(Stream stream)
	{
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, (int)stream.Length);
		Data = array;
	}

	private byte[] EncodeData(byte[] data, ContentTransferEncoding_enum encoding)
	{
		return encoding switch
		{
			ContentTransferEncoding_enum.NotSpecified => throw new Exception("Please specify Content-Transfer-Encoding first !"), 
			ContentTransferEncoding_enum.Unknown => throw new Exception("Not supported Content-Transfer-Encoding. If it's your custom encoding, encode data yourself and set it with DataEncoded property !"), 
			ContentTransferEncoding_enum.Base64 => Core.Base64Encode(data), 
			ContentTransferEncoding_enum.QuotedPrintable => Core.QuotedPrintableEncode(data), 
			_ => data, 
		};
	}

	private string FoldHeader(string header)
	{
		StringBuilder stringBuilder = new StringBuilder();
		header = header.Replace("\r\n", "\n");
		string[] array = header.Split('\n');
		foreach (string text in array)
		{
			if (text.IndexOf('\t') > -1)
			{
				stringBuilder.Append(text.Replace("\t", "\r\n\t") + "\r\n");
			}
			else
			{
				stringBuilder.Append(text + "\r\n");
			}
		}
		if (stringBuilder.Length > 1)
		{
			return stringBuilder.ToString(0, stringBuilder.Length - 2);
		}
		return stringBuilder.ToString();
	}
}
