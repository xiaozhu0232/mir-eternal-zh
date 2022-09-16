using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.IMAP;

public class IMAP_Utils
{
	public static string[] MessageFlagsAdd(string[] flags, string[] flagsToAdd)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		if (flagsToAdd == null)
		{
			throw new ArgumentNullException("flagsToAdd");
		}
		List<string> list = new List<string>();
		list.AddRange(flags);
		foreach (string text in flagsToAdd)
		{
			bool flag = false;
			for (int j = 0; j < flags.Length; j++)
			{
				if (string.Equals(flags[j], text, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public static string[] MessageFlagsRemove(string[] flags, string[] flagsToRemove)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		if (flagsToRemove == null)
		{
			throw new ArgumentNullException("flagsToRemove");
		}
		List<string> list = new List<string>();
		foreach (string text in flags)
		{
			bool flag = false;
			foreach (string b in flagsToRemove)
			{
				if (string.Equals(text, b, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public static string ACL_to_String(IMAP_ACL_Flags flags)
	{
		string text = "";
		if ((flags & IMAP_ACL_Flags.l) != 0)
		{
			text += "l";
		}
		if ((flags & IMAP_ACL_Flags.r) != 0)
		{
			text += "r";
		}
		if ((flags & IMAP_ACL_Flags.s) != 0)
		{
			text += "s";
		}
		if ((flags & IMAP_ACL_Flags.w) != 0)
		{
			text += "w";
		}
		if ((flags & IMAP_ACL_Flags.i) != 0)
		{
			text += "i";
		}
		if ((flags & IMAP_ACL_Flags.p) != 0)
		{
			text += "p";
		}
		if ((flags & IMAP_ACL_Flags.c) != 0)
		{
			text += "c";
		}
		if ((flags & IMAP_ACL_Flags.d) != 0)
		{
			text += "d";
		}
		if ((flags & IMAP_ACL_Flags.a) != 0)
		{
			text += "a";
		}
		return text;
	}

	public static IMAP_ACL_Flags ACL_From_String(string aclString)
	{
		IMAP_ACL_Flags iMAP_ACL_Flags = IMAP_ACL_Flags.None;
		aclString = aclString.ToLower();
		if (aclString.IndexOf('l') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.l;
		}
		if (aclString.IndexOf('r') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.r;
		}
		if (aclString.IndexOf('s') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.s;
		}
		if (aclString.IndexOf('w') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.w;
		}
		if (aclString.IndexOf('i') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.i;
		}
		if (aclString.IndexOf('p') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.p;
		}
		if (aclString.IndexOf('c') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.c;
		}
		if (aclString.IndexOf('d') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.d;
		}
		if (aclString.IndexOf('a') > -1)
		{
			iMAP_ACL_Flags |= IMAP_ACL_Flags.a;
		}
		return iMAP_ACL_Flags;
	}

	public static DateTime ParseDate(string date)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		if (date.IndexOf('-') > -1)
		{
			try
			{
				return DateTime.ParseExact(date.Trim(), new string[2] { "d-MMM-yyyy", "d-MMM-yyyy HH:mm:ss zzz" }, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
			}
			catch
			{
				throw new ArgumentException("Argument 'date' value '" + date + "' is not valid IMAP date.");
			}
		}
		return MIME_Utils.ParseRfc2822DateTime(date);
	}

	public static string DateTimeToString(DateTime date)
	{
		return string.Concat("" + date.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture), " ", date.ToString("zzz", CultureInfo.InvariantCulture).Replace(":", ""));
	}

	public static string Encode_IMAP_UTF7_String(string text)
	{
		char[] base64Chars = new char[64]
		{
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
			'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
			'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
			'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
			'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
			'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
			'8', '9', '+', ','
		};
		MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '&':
				memoryStream.Write(new byte[2] { 38, 45 }, 0, 2);
				continue;
			default:
				if (c < '\'' || c > '~')
				{
					break;
				}
				goto case ' ';
			case ' ':
			case '!':
			case '"':
			case '#':
			case '$':
			case '%':
				memoryStream.WriteByte((byte)c);
				continue;
			}
			MemoryStream memoryStream2 = new MemoryStream();
			for (int j = i; j < text.Length; j++)
			{
				char c2 = text[j];
				if ((c2 >= ' ' && c2 <= '%') || (c2 >= '\'' && c2 <= '~'))
				{
					break;
				}
				memoryStream2.WriteByte((byte)((c2 & 0xFF00) >> 8));
				memoryStream2.WriteByte((byte)(c2 & 0xFFu));
				i = j;
			}
			byte[] array = Net_Utils.Base64EncodeEx(memoryStream2.ToArray(), base64Chars, padd: false);
			memoryStream.WriteByte(38);
			memoryStream.Write(array, 0, array.Length);
			memoryStream.WriteByte(45);
		}
		return Encoding.Default.GetString(memoryStream.ToArray());
	}

	public static string Decode_IMAP_UTF7_String(string text)
	{
		char[] base64Chars = new char[64]
		{
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
			'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
			'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
			'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
			'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
			'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
			'8', '9', '+', ','
		};
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '&')
			{
				int num = -1;
				for (int j = i + 1; j < text.Length; j++)
				{
					if (text[j] == '-')
					{
						num = j;
						break;
					}
					if (text[j] == '&')
					{
						break;
					}
				}
				if (num == -1)
				{
					stringBuilder.Append(c);
					continue;
				}
				if (num - i == 1)
				{
					stringBuilder.Append(c);
					i++;
					continue;
				}
				byte[] bytes = Encoding.Default.GetBytes(text.Substring(i + 1, num - i - 1));
				byte[] array = Net_Utils.Base64DecodeEx(bytes, base64Chars);
				char[] array2 = new char[array.Length / 2];
				for (int k = 0; k < array2.Length; k++)
				{
					array2[k] = (char)((array[k * 2] << 8) | array[k * 2 + 1]);
				}
				stringBuilder.Append(array2);
				i += bytes.Length + 1;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string EncodeMailbox(string mailbox, IMAP_Mailbox_Encoding encoding)
	{
		if (mailbox == null)
		{
			throw new ArgumentNullException("mailbox");
		}
		if (encoding == IMAP_Mailbox_Encoding.ImapUtf7)
		{
			return "\"" + Encode_IMAP_UTF7_String(mailbox) + "\"";
		}
		_ = 2;
		return "\"" + mailbox + "\"";
	}

	public static string DecodeMailbox(string mailbox)
	{
		if (mailbox == null)
		{
			throw new ArgumentNullException("mailbox");
		}
		if (mailbox.StartsWith("*\""))
		{
			return mailbox.Substring(2, mailbox.Length - 3);
		}
		return Decode_IMAP_UTF7_String(TextUtils.UnQuoteString(mailbox));
	}

	public static string NormalizeFolder(string folder)
	{
		folder = folder.Replace("\\", "/");
		if (folder.StartsWith("/"))
		{
			folder = folder.Substring(1);
		}
		if (folder.EndsWith("/"))
		{
			folder = folder.Substring(0, folder.Length - 1);
		}
		return folder;
	}

	public static bool IsValidFolderName(string folder)
	{
		return true;
	}

	public static bool MustUseLiteralString(string value, bool utf8StringSupported)
	{
		if (value != null)
		{
			foreach (char c in value)
			{
				if (!utf8StringSupported && c > '~')
				{
					return true;
				}
				if (char.IsControl(c))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static byte[] ImapStringToByte(Encoding charset, bool utf8StringSupported, string value)
	{
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		if (value == null)
		{
			return Encoding.ASCII.GetBytes("NIL");
		}
		if (value == "")
		{
			return Encoding.ASCII.GetBytes("\"\"");
		}
		bool flag = false;
		bool flag2 = false;
		foreach (char c in value)
		{
			if (c > '\u007f')
			{
				flag = true;
			}
			else if (char.IsControl(c))
			{
				flag2 = true;
			}
		}
		if (flag2 || (!utf8StringSupported && flag))
		{
			byte[] bytes = charset.GetBytes(value);
			byte[] bytes2 = Encoding.ASCII.GetBytes("{" + bytes.Length + "}\r\n");
			byte[] array = new byte[bytes2.Length + bytes.Length];
			Array.Copy(bytes2, array, bytes2.Length);
			Array.Copy(bytes, 0, array, bytes2.Length, bytes.Length);
			return array;
		}
		if (utf8StringSupported)
		{
			return Encoding.UTF8.GetBytes("*" + TextUtils.QuoteString(value));
		}
		return charset.GetBytes(TextUtils.QuoteString(value));
	}

	public static string ReadString(StringReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.ReadToFirstChar();
		if (reader.SourceString.StartsWith("{"))
		{
			int length = Convert.ToInt32(reader.ReadParenthesized());
			reader.ReadSpecifiedLength(2);
			return reader.ReadSpecifiedLength(length);
		}
		if (reader.StartsWith("*\""))
		{
			reader.ReadSpecifiedLength(1);
			return reader.ReadWord();
		}
		string text = reader.ReadWord(unQuote: true, new char[11]
		{
			' ', ',', ';', '{', '}', '(', ')', '[', ']', '\r',
			'\n'
		}, removeWordTerminator: false);
		if (string.Equals(text, "NIL", StringComparison.InvariantCultureIgnoreCase))
		{
			return null;
		}
		return text;
	}

	[Obsolete("Use class IMAP_t_MsgFlags instead.")]
	public static IMAP_MessageFlags ParseMessageFlags(string flagsString)
	{
		IMAP_MessageFlags iMAP_MessageFlags = IMAP_MessageFlags.None;
		flagsString = flagsString.ToUpper();
		if (flagsString.IndexOf("ANSWERED") > -1)
		{
			iMAP_MessageFlags |= IMAP_MessageFlags.Answered;
		}
		if (flagsString.IndexOf("FLAGGED") > -1)
		{
			iMAP_MessageFlags |= IMAP_MessageFlags.Flagged;
		}
		if (flagsString.IndexOf("DELETED") > -1)
		{
			iMAP_MessageFlags |= IMAP_MessageFlags.Deleted;
		}
		if (flagsString.IndexOf("SEEN") > -1)
		{
			iMAP_MessageFlags |= IMAP_MessageFlags.Seen;
		}
		if (flagsString.IndexOf("DRAFT") > -1)
		{
			iMAP_MessageFlags |= IMAP_MessageFlags.Draft;
		}
		return iMAP_MessageFlags;
	}

	[Obsolete("Use class IMAP_t_MsgFlags instead.")]
	public static string[] MessageFlagsToStringArray(IMAP_MessageFlags msgFlags)
	{
		List<string> list = new List<string>();
		if ((IMAP_MessageFlags.Answered & msgFlags) != 0)
		{
			list.Add("\\ANSWERED");
		}
		if ((IMAP_MessageFlags.Flagged & msgFlags) != 0)
		{
			list.Add("\\FLAGGED");
		}
		if ((IMAP_MessageFlags.Deleted & msgFlags) != 0)
		{
			list.Add("\\DELETED");
		}
		if ((IMAP_MessageFlags.Seen & msgFlags) != 0)
		{
			list.Add("\\SEEN");
		}
		if ((IMAP_MessageFlags.Draft & msgFlags) != 0)
		{
			list.Add("\\DRAFT");
		}
		return list.ToArray();
	}

	[Obsolete("Use method 'MessageFlagsToStringArray' instead.")]
	public static string MessageFlagsToString(IMAP_MessageFlags msgFlags)
	{
		string text = "";
		if ((IMAP_MessageFlags.Answered & msgFlags) != 0)
		{
			text += " \\ANSWERED";
		}
		if ((IMAP_MessageFlags.Flagged & msgFlags) != 0)
		{
			text += " \\FLAGGED";
		}
		if ((IMAP_MessageFlags.Deleted & msgFlags) != 0)
		{
			text += " \\DELETED";
		}
		if ((IMAP_MessageFlags.Seen & msgFlags) != 0)
		{
			text += " \\SEEN";
		}
		if ((IMAP_MessageFlags.Draft & msgFlags) != 0)
		{
			text += " \\DRAFT";
		}
		return text.Trim();
	}
}
