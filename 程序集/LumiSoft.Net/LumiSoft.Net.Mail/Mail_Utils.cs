using System;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_Utils
{
	internal static string SMTP_Mailbox(MIME_Reader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (reader.Peek(readToFirstChar: true) == 34)
		{
			stringBuilder.Append("\"" + reader.QuotedString() + "\"");
		}
		else
		{
			stringBuilder.Append(reader.DotAtom());
		}
		if (reader.Peek(readToFirstChar: true) != 64)
		{
			return null;
		}
		reader.Char(readToFirstChar: true);
		stringBuilder.Append('@');
		stringBuilder.Append(reader.DotAtom());
		return stringBuilder.ToString();
	}
}
