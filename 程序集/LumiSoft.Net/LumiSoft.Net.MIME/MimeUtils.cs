using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class MimeUtils
{
	public static DateTime ParseDate(string date)
	{
		date = date.ToLower();
		date = date.Replace("ut", "-0000");
		date = date.Replace("gmt", "-0000");
		date = date.Replace("edt", "-0400");
		date = date.Replace("est", "-0500");
		date = date.Replace("cdt", "-0500");
		date = date.Replace("cst", "-0600");
		date = date.Replace("mdt", "-0600");
		date = date.Replace("mst", "-0700");
		date = date.Replace("pdt", "-0700");
		date = date.Replace("pst", "-0800");
		date = date.Replace("bst", "+0100");
		date = date.Replace("jan", "01");
		date = date.Replace("feb", "02");
		date = date.Replace("mar", "03");
		date = date.Replace("apr", "04");
		date = date.Replace("may", "05");
		date = date.Replace("jun", "06");
		date = date.Replace("jul", "07");
		date = date.Replace("aug", "08");
		date = date.Replace("sep", "09");
		date = date.Replace("oct", "10");
		date = date.Replace("nov", "11");
		date = date.Replace("dec", "12");
		if (date.IndexOf(',') > -1)
		{
			date = date.Substring(date.IndexOf(',') + 1);
		}
		if (date.IndexOf(" (") > -1)
		{
			date = date.Substring(0, date.IndexOf(" ("));
		}
		int num = 1900;
		int num2 = 1;
		int num3 = 1;
		int num4 = -1;
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		StringReader stringReader = new StringReader(date);
		try
		{
			num3 = Convert.ToInt32(stringReader.ReadWord(unQuote: true, new char[3] { '.', '-', ' ' }, removeWordTerminator: true));
		}
		catch
		{
			throw new Exception("Invalid date value '" + date + "', invalid day value !");
		}
		try
		{
			num2 = Convert.ToInt32(stringReader.ReadWord(unQuote: true, new char[3] { '.', '-', ' ' }, removeWordTerminator: true));
		}
		catch
		{
			throw new Exception("Invalid date value '" + date + "', invalid month value !");
		}
		try
		{
			num = Convert.ToInt32(stringReader.ReadWord(unQuote: true, new char[3] { '.', '-', ' ' }, removeWordTerminator: true));
		}
		catch
		{
			throw new Exception("Invalid date value '" + date + "', invalid year value !");
		}
		if (stringReader.Available > 0)
		{
			try
			{
				num4 = Convert.ToInt32(stringReader.ReadWord(unQuote: true, new char[1] { ':' }, removeWordTerminator: true));
			}
			catch
			{
				throw new Exception("Invalid date value '" + date + "', invalid hour value !");
			}
			try
			{
				num5 = Convert.ToInt32(stringReader.ReadWord(unQuote: true, new char[1] { ':' }, removeWordTerminator: false));
			}
			catch
			{
				throw new Exception("Invalid date value '" + date + "', invalid minute value !");
			}
			stringReader.ReadToFirstChar();
			if (stringReader.StartsWith(":"))
			{
				stringReader.ReadSpecifiedLength(1);
				try
				{
					string text = stringReader.ReadWord(unQuote: true, new char[1] { ' ' }, removeWordTerminator: true);
					if (text.IndexOf('.') > -1)
					{
						text = text.Substring(0, text.IndexOf('.'));
					}
					num6 = Convert.ToInt32(text);
				}
				catch
				{
					throw new Exception("Invalid date value '" + date + "', invalid second value !");
				}
			}
			stringReader.ReadToFirstChar();
			if (stringReader.Available > 3)
			{
				string text2 = stringReader.SourceString.Replace(":", "");
				if (text2.StartsWith("+") || text2.StartsWith("-"))
				{
					bool flag = text2.StartsWith("+");
					text2 = text2.Substring(1);
					while (text2.Length < 4)
					{
						text2 = "0" + text2;
					}
					try
					{
						int num8 = Convert.ToInt32(text2.Substring(0, 2));
						int num9 = Convert.ToInt32(text2.Substring(2));
						num7 = ((!flag) ? (num8 * 60 + num9) : (-(num8 * 60 + num9)));
					}
					catch
					{
					}
				}
			}
		}
		if (num4 != -1 && num5 != -1 && num6 != -1)
		{
			DateTime dateTime = new DateTime(num, num2, num3, num4, num5, num6).AddMinutes(num7);
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc).ToLocalTime();
		}
		return new DateTime(num, num2, num3);
	}

	public static string DateTimeToRfc2822(DateTime dateTime)
	{
		return dateTime.ToUniversalTime().ToString("r", DateTimeFormatInfo.InvariantInfo);
	}

	public static string ParseHeaders(Stream entryStrm)
	{
		byte[] array = new byte[2] { 13, 10 };
		MemoryStream memoryStream = new MemoryStream();
		StreamLineReader streamLineReader = new StreamLineReader(entryStrm);
		byte[] array2 = streamLineReader.ReadLine();
		while (array2 != null && array2.Length != 0)
		{
			memoryStream.Write(array2, 0, array2.Length);
			memoryStream.Write(array, 0, array.Length);
			array2 = streamLineReader.ReadLine();
		}
		return Encoding.Default.GetString(memoryStream.ToArray());
	}

	public static string ParseHeaderField(string fieldName, Stream entryStrm)
	{
		return ParseHeaderField(fieldName, ParseHeaders(entryStrm));
	}

	public static string ParseHeaderField(string fieldName, string headers)
	{
		using (TextReader textReader = new StreamReader(new MemoryStream(Encoding.Default.GetBytes(headers))))
		{
			for (string text = textReader.ReadLine(); text != null; text = textReader.ReadLine())
			{
				if (text.ToUpper().StartsWith(fieldName.ToUpper()))
				{
					string text2 = text.Substring(fieldName.Length).Trim();
					text = textReader.ReadLine();
					while (text != null && (text.StartsWith("\t") || text.StartsWith(" ")))
					{
						text2 += text;
						text = textReader.ReadLine();
					}
					return text2;
				}
			}
		}
		return "";
	}

	public static string ParseHeaderFiledParameter(string fieldName, string parameterName, string headers)
	{
		string text = ParseHeaderField(fieldName, headers);
		if (text.Length > 0)
		{
			int num = text.ToUpper().IndexOf(parameterName.ToUpper());
			if (num > -1)
			{
				text = text.Substring(num + parameterName.Length + 1);
				if (text.StartsWith("\""))
				{
					return text.Substring(1, text.IndexOf("\"", 1) - 1);
				}
				int length = text.Length;
				if (text.IndexOf(" ") > -1)
				{
					length = text.IndexOf(" ");
				}
				return text.Substring(0, length);
			}
		}
		return "";
	}

	public static MediaType_enum ParseMediaType(string headerFieldValue)
	{
		if (headerFieldValue == null)
		{
			return MediaType_enum.NotSpecified;
		}
		string text = TextUtils.SplitString(headerFieldValue, ';')[0].ToLower();
		if (text.IndexOf("text/plain") > -1)
		{
			return MediaType_enum.Text_plain;
		}
		if (text.IndexOf("text/html") > -1)
		{
			return MediaType_enum.Text_html;
		}
		if (text.IndexOf("text/xml") > -1)
		{
			return MediaType_enum.Text_xml;
		}
		if (text.IndexOf("text/rtf") > -1)
		{
			return MediaType_enum.Text_rtf;
		}
		if (text.IndexOf("text") > -1)
		{
			return MediaType_enum.Text;
		}
		if (text.IndexOf("image/gif") > -1)
		{
			return MediaType_enum.Image_gif;
		}
		if (text.IndexOf("image/tiff") > -1)
		{
			return MediaType_enum.Image_tiff;
		}
		if (text.IndexOf("image/jpeg") > -1)
		{
			return MediaType_enum.Image_jpeg;
		}
		if (text.IndexOf("image") > -1)
		{
			return MediaType_enum.Image;
		}
		if (text.IndexOf("audio") > -1)
		{
			return MediaType_enum.Audio;
		}
		if (text.IndexOf("video") > -1)
		{
			return MediaType_enum.Video;
		}
		if (text.IndexOf("application/octet-stream") > -1)
		{
			return MediaType_enum.Application_octet_stream;
		}
		if (text.IndexOf("application") > -1)
		{
			return MediaType_enum.Application;
		}
		if (text.IndexOf("multipart/mixed") > -1)
		{
			return MediaType_enum.Multipart_mixed;
		}
		if (text.IndexOf("multipart/alternative") > -1)
		{
			return MediaType_enum.Multipart_alternative;
		}
		if (text.IndexOf("multipart/parallel") > -1)
		{
			return MediaType_enum.Multipart_parallel;
		}
		if (text.IndexOf("multipart/related") > -1)
		{
			return MediaType_enum.Multipart_related;
		}
		if (text.IndexOf("multipart/signed") > -1)
		{
			return MediaType_enum.Multipart_signed;
		}
		if (text.IndexOf("multipart") > -1)
		{
			return MediaType_enum.Multipart;
		}
		if (text.IndexOf("message/rfc822") > -1)
		{
			return MediaType_enum.Message_rfc822;
		}
		if (text.IndexOf("message") > -1)
		{
			return MediaType_enum.Message;
		}
		return MediaType_enum.Unknown;
	}

	public static string MediaTypeToString(MediaType_enum mediaType)
	{
		return mediaType switch
		{
			MediaType_enum.Text_plain => "text/plain", 
			MediaType_enum.Text_html => "text/html", 
			MediaType_enum.Text_xml => "text/xml", 
			MediaType_enum.Text_rtf => "text/rtf", 
			MediaType_enum.Text => "text", 
			MediaType_enum.Image_gif => "image/gif", 
			MediaType_enum.Image_tiff => "image/tiff", 
			MediaType_enum.Image_jpeg => "image/jpeg", 
			MediaType_enum.Image => "image", 
			MediaType_enum.Audio => "audio", 
			MediaType_enum.Video => "video", 
			MediaType_enum.Application_octet_stream => "application/octet-stream", 
			MediaType_enum.Application => "application", 
			MediaType_enum.Multipart_mixed => "multipart/mixed", 
			MediaType_enum.Multipart_alternative => "multipart/alternative", 
			MediaType_enum.Multipart_parallel => "multipart/parallel", 
			MediaType_enum.Multipart_related => "multipart/related", 
			MediaType_enum.Multipart_signed => "multipart/signed", 
			MediaType_enum.Multipart => "multipart", 
			MediaType_enum.Message_rfc822 => "message/rfc822", 
			MediaType_enum.Message => "message", 
			MediaType_enum.Unknown => "unknown", 
			_ => null, 
		};
	}

	public static ContentTransferEncoding_enum ParseContentTransferEncoding(string headerFieldValue)
	{
		if (headerFieldValue == null)
		{
			return ContentTransferEncoding_enum.NotSpecified;
		}
		return headerFieldValue.ToLower() switch
		{
			"7bit" => ContentTransferEncoding_enum._7bit, 
			"quoted-printable" => ContentTransferEncoding_enum.QuotedPrintable, 
			"base64" => ContentTransferEncoding_enum.Base64, 
			"8bit" => ContentTransferEncoding_enum._8bit, 
			"binary" => ContentTransferEncoding_enum.Binary, 
			_ => ContentTransferEncoding_enum.Unknown, 
		};
	}

	public static string ContentTransferEncodingToString(ContentTransferEncoding_enum encoding)
	{
		return encoding switch
		{
			ContentTransferEncoding_enum._7bit => "7bit", 
			ContentTransferEncoding_enum.QuotedPrintable => "quoted-printable", 
			ContentTransferEncoding_enum.Base64 => "base64", 
			ContentTransferEncoding_enum._8bit => "8bit", 
			ContentTransferEncoding_enum.Binary => "binary", 
			ContentTransferEncoding_enum.Unknown => "unknown", 
			_ => null, 
		};
	}

	public static ContentDisposition_enum ParseContentDisposition(string headerFieldValue)
	{
		if (headerFieldValue == null)
		{
			return ContentDisposition_enum.NotSpecified;
		}
		string text = headerFieldValue.ToLower();
		if (text.IndexOf("attachment") > -1)
		{
			return ContentDisposition_enum.Attachment;
		}
		if (text.IndexOf("inline") > -1)
		{
			return ContentDisposition_enum.Inline;
		}
		return ContentDisposition_enum.Unknown;
	}

	public static string ContentDispositionToString(ContentDisposition_enum disposition)
	{
		return disposition switch
		{
			ContentDisposition_enum.Attachment => "attachment", 
			ContentDisposition_enum.Inline => "inline", 
			ContentDisposition_enum.Unknown => "unknown", 
			_ => null, 
		};
	}

	public static string EncodeWord(string text)
	{
		if (text == null)
		{
			return null;
		}
		if (Core.IsAscii(text))
		{
			return text;
		}
		return Core.CanonicalEncode(text, "utf-8");
	}

	public static string DecodeWords(string text)
	{
		if (text == null)
		{
			return null;
		}
		StringReader stringReader = new StringReader(text);
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		while (stringReader.Available > 0)
		{
			string text2 = stringReader.ReadToFirstChar();
			if (stringReader.StartsWith("=?") && stringReader.SourceString.IndexOf("?=") > -1)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				string text3 = null;
				try
				{
					stringBuilder2.Append(stringReader.ReadSpecifiedLength(2));
					string text4 = stringReader.QuotedReadToDelimiter('?');
					stringBuilder2.Append(text4 + "?");
					string text5 = stringReader.QuotedReadToDelimiter('?');
					stringBuilder2.Append(text5 + "?");
					string text6 = stringReader.QuotedReadToDelimiter('?');
					stringBuilder2.Append(text6 + "?");
					if (stringReader.StartsWith("="))
					{
						stringBuilder2.Append(stringReader.ReadSpecifiedLength(1));
						Encoding encoding = Encoding.GetEncoding(text4);
						if (text5.ToLower() == "q")
						{
							text3 = Core.QDecode(encoding, text6);
						}
						else if (text5.ToLower() == "b")
						{
							text3 = encoding.GetString(Core.Base64Decode(Encoding.Default.GetBytes(text6)));
						}
					}
				}
				catch
				{
				}
				if (!flag)
				{
					stringBuilder.Append(text2);
				}
				if (text3 == null)
				{
					stringBuilder.Append(stringBuilder2.ToString());
				}
				else
				{
					stringBuilder.Append(text3);
				}
				flag = true;
			}
			else if (stringReader.StartsWithWord())
			{
				stringBuilder.Append(text2 + stringReader.ReadWord(unQuote: false));
				flag = false;
			}
			else
			{
				stringBuilder.Append(text2 + stringReader.ReadSpecifiedLength(1));
			}
		}
		return stringBuilder.ToString();
	}

	public static string EncodeHeaderField(string text)
	{
		if (Core.IsAscii(text))
		{
			return text;
		}
		if (text.IndexOf("\"") > -1)
		{
			string text2 = text;
			int num = 0;
			while (num < text2.Length - 1)
			{
				int num2 = text2.IndexOf("\"", num);
				if (num2 == -1)
				{
					break;
				}
				int num3 = text2.IndexOf("\"", num2 + 1);
				if (num3 == -1)
				{
					break;
				}
				string text3 = text2.Substring(0, num2);
				string text4 = text2.Substring(num3 + 1);
				string text5 = text2.Substring(num2 + 1, num3 - num2 - 1);
				if (!Core.IsAscii(text5))
				{
					string text6 = Core.CanonicalEncode(text5, "utf-8");
					text2 = text3 + "\"" + text6 + "\"" + text4;
					num += num3 + 1 + text6.Length - text5.Length;
				}
				else
				{
					num += num3 + 1;
				}
			}
			if (Core.IsAscii(text2))
			{
				return text2;
			}
			return Core.CanonicalEncode(text, "utf-8");
		}
		return Core.CanonicalEncode(text, "utf-8");
	}

	public static string CreateMessageID()
	{
		return "<" + Guid.NewGuid().ToString().Replace("-", "") + "@" + Guid.NewGuid().ToString().Replace("-", "") + ">";
	}

	public static string FoldData(string data)
	{
		if (data.Length > 76)
		{
			int num = 0;
			int num2 = -1;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				char c = data[i];
				if (c == ' ' || c == '\t')
				{
					num2 = i;
				}
				if (i == data.Length - 1)
				{
					stringBuilder.Append(data.Substring(num));
				}
				else if (i - num >= 76)
				{
					if (num2 == -1)
					{
						num2 = i;
					}
					stringBuilder.Append(data.Substring(num, num2 - num) + "\r\n\t");
					i = num2;
					num2 = -1;
					num = i;
				}
			}
			return stringBuilder.ToString();
		}
		return data;
	}
}
