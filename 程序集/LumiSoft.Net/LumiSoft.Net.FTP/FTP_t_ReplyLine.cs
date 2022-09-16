using System;

namespace LumiSoft.Net.FTP;

public class FTP_t_ReplyLine
{
	private int m_ReplyCode;

	private string m_Text;

	private bool m_IsLastLine = true;

	public int ReplyCode => m_ReplyCode;

	public string Text => m_Text;

	public bool IsLastLine => m_IsLastLine;

	public FTP_t_ReplyLine(int replyCode, string text, bool isLastLine)
	{
		if (text == null)
		{
			text = "";
		}
		m_ReplyCode = replyCode;
		m_Text = text;
		m_IsLastLine = isLastLine;
	}

	public static FTP_t_ReplyLine Parse(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (line.Length < 3)
		{
			throw new ParseException("Invalid FTP server reply-line '" + line + "'.");
		}
		int result = 0;
		if (!int.TryParse(line.Substring(0, 3), out result))
		{
			throw new ParseException("Invalid FTP server reply-line '" + line + "' reply-code.");
		}
		bool isLastLine = true;
		if (line.Length > 3)
		{
			isLastLine = line[3] == ' ';
		}
		string text = "";
		if (line.Length > 5)
		{
			text = line.Substring(4);
		}
		return new FTP_t_ReplyLine(result, text, isLastLine);
	}

	public override string ToString()
	{
		if (m_IsLastLine)
		{
			return m_ReplyCode + " " + m_Text + "\r\n";
		}
		return m_ReplyCode + "-" + m_Text + "\r\n";
	}
}
