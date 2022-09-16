using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_Utils
{
	public static string DateTimeToRfc2822(DateTime dateTime)
	{
		return dateTime.ToString("ddd, dd MMM yyyy HH':'mm':'ss ", DateTimeFormatInfo.InvariantInfo) + dateTime.ToString("zzz").Replace(":", "");
	}

	public static DateTime ParseRfc2822DateTime(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(value);
		}
		try
		{
			MIME_Reader mIME_Reader = new MIME_Reader(value);
			string text = mIME_Reader.Atom();
			if (text.Length == 3)
			{
				mIME_Reader.Char(readToFirstChar: true);
				text = mIME_Reader.Atom();
			}
			int day = Convert.ToInt32(text);
			text = mIME_Reader.Atom().ToLower();
			int num = 1;
			num = text switch
			{
				"jan" => 1, 
				"feb" => 2, 
				"mar" => 3, 
				"apr" => 4, 
				"may" => 5, 
				"jun" => 6, 
				"jul" => 7, 
				"aug" => 8, 
				"sep" => 9, 
				"oct" => 10, 
				"nov" => 11, 
				"dec" => 12, 
				_ => throw new ArgumentException("Invalid month-name value '" + value + "'."), 
			};
			int year = Convert.ToInt32(mIME_Reader.Atom());
			int hour = Convert.ToInt32(mIME_Reader.Atom());
			mIME_Reader.Char(readToFirstChar: true);
			int minute = Convert.ToInt32(mIME_Reader.Atom());
			int second = 0;
			if (mIME_Reader.Peek(readToFirstChar: true) == 58)
			{
				mIME_Reader.Char(readToFirstChar: true);
				second = Convert.ToInt32(mIME_Reader.Atom());
			}
			int num2 = 0;
			text = mIME_Reader.Atom();
			if (text != null)
			{
				if (text[0] == '+' || text[0] == '-')
				{
					num2 = ((text[0] != '+') ? (-(Convert.ToInt32(text.Substring(1, 2)) * 60 + Convert.ToInt32(text.Substring(3, 2)))) : (Convert.ToInt32(text.Substring(1, 2)) * 60 + Convert.ToInt32(text.Substring(3, 2))));
				}
				else
				{
					switch (text.ToUpper())
					{
					case "A":
						num2 = 60;
						break;
					case "ACDT":
						num2 = 630;
						break;
					case "ACST":
						num2 = 570;
						break;
					case "ADT":
						num2 = -180;
						break;
					case "AEDT":
						num2 = 660;
						break;
					case "AEST":
						num2 = 600;
						break;
					case "AKDT":
						num2 = -480;
						break;
					case "AKST":
						num2 = -540;
						break;
					case "AST":
						num2 = -240;
						break;
					case "AWDT":
						num2 = 540;
						break;
					case "AWST":
						num2 = 480;
						break;
					case "B":
						num2 = 120;
						break;
					case "BST":
						num2 = 60;
						break;
					case "C":
						num2 = 180;
						break;
					case "CDT":
						num2 = -300;
						break;
					case "CEDT":
						num2 = 120;
						break;
					case "CEST":
						num2 = 120;
						break;
					case "CET":
						num2 = 60;
						break;
					case "CST":
						num2 = -360;
						break;
					case "CXT":
						num2 = 60;
						break;
					case "D":
						num2 = 240;
						break;
					case "E":
						num2 = 300;
						break;
					case "EDT":
						num2 = -240;
						break;
					case "EEDT":
						num2 = 180;
						break;
					case "EEST":
						num2 = 180;
						break;
					case "EET":
						num2 = 120;
						break;
					case "EST":
						num2 = -300;
						break;
					case "F":
						num2 = 360;
						break;
					case "G":
						num2 = 420;
						break;
					case "GMT":
						num2 = 0;
						break;
					case "H":
						num2 = 480;
						break;
					case "I":
						num2 = 540;
						break;
					case "IST":
						num2 = 60;
						break;
					case "K":
						num2 = 600;
						break;
					case "L":
						num2 = 660;
						break;
					case "M":
						num2 = 720;
						break;
					case "MDT":
						num2 = -360;
						break;
					case "MST":
						num2 = -420;
						break;
					case "N":
						num2 = -60;
						break;
					case "NDT":
						num2 = -150;
						break;
					case "NFT":
						num2 = 690;
						break;
					case "NST":
						num2 = -210;
						break;
					case "O":
						num2 = -120;
						break;
					case "P":
						num2 = -180;
						break;
					case "PDT":
						num2 = -420;
						break;
					case "PST":
						num2 = -480;
						break;
					case "Q":
						num2 = -240;
						break;
					case "R":
						num2 = -300;
						break;
					case "S":
						num2 = -360;
						break;
					case "T":
						num2 = -420;
						break;
					case "":
						num2 = -480;
						break;
					case "UTC":
						num2 = 0;
						break;
					case "V":
						num2 = -540;
						break;
					case "W":
						num2 = -600;
						break;
					case "WEDT":
						num2 = 60;
						break;
					case "WEST":
						num2 = 60;
						break;
					case "WET":
						num2 = 0;
						break;
					case "WST":
						num2 = 480;
						break;
					case "X":
						num2 = -660;
						break;
					case "Y":
						num2 = -720;
						break;
					case "Z":
						num2 = 0;
						break;
					}
				}
			}
			DateTime dateTime = new DateTime(year, num, day, hour, minute, second).AddMinutes(-num2);
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc).ToLocalTime();
		}
		catch (Exception ex)
		{
			_ = ex.Message;
			throw new ArgumentException("Argumnet 'value' value '" + value + "' is not valid RFC 822/2822 date-time string.");
		}
	}

	public static string UnfoldHeader(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value.Replace("\r\n", "");
	}

	public static string CreateMessageID()
	{
		return "<" + Guid.NewGuid().ToString().Replace("-", "")
			.Substring(16) + "@" + Guid.NewGuid().ToString().Replace("-", "")
			.Substring(16) + ">";
	}

	internal static string ParseHeaders(Stream entryStrm)
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

	public static string QDecode(Encoding encoding, string data)
	{
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return encoding.GetString(QuotedPrintableDecode(Encoding.ASCII.GetBytes(data.Replace("_", " "))));
	}

	public static byte[] QuotedPrintableDecode(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		MemoryStream memoryStream = new MemoryStream();
		MemoryStream memoryStream2 = new MemoryStream(data);
		for (int num = memoryStream2.ReadByte(); num > -1; num = memoryStream2.ReadByte())
		{
			if (num == 61)
			{
				byte[] array = new byte[2];
				int num2 = memoryStream2.Read(array, 0, 2);
				if (num2 == 2)
				{
					if (array[0] != 13 || array[1] != 10)
					{
						try
						{
							memoryStream.Write(Net_Utils.FromHex(array), 0, 1);
						}
						catch
						{
							memoryStream.WriteByte(61);
							memoryStream.Write(array, 0, 2);
						}
					}
				}
				else
				{
					memoryStream.Write(array, 0, num2);
				}
			}
			else
			{
				memoryStream.WriteByte((byte)num);
			}
		}
		return memoryStream.ToArray();
	}
}
