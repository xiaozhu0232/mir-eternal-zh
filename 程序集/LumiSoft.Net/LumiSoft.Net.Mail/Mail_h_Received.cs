using System;
using System.Net;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_h_Received : MIME_h
{
	private bool m_IsModified;

	private string m_ParseValue;

	private string m_From = "";

	private Mail_t_TcpInfo m_pFrom_TcpInfo;

	private string m_By = "";

	private Mail_t_TcpInfo m_pBy_TcpInfo;

	private string m_Via;

	private string m_With;

	private string m_ID;

	private string m_For;

	private DateTime m_Time;

	public override bool IsModified => m_IsModified;

	public override string Name => "Received";

	public string From
	{
		get
		{
			return m_From;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("From");
			}
			if (value == string.Empty)
			{
				throw new ArgumentException("Property 'From' value must be specified", "From");
			}
			m_From = value;
			m_IsModified = true;
		}
	}

	public Mail_t_TcpInfo From_TcpInfo
	{
		get
		{
			return m_pFrom_TcpInfo;
		}
		set
		{
			m_pFrom_TcpInfo = value;
			m_IsModified = true;
		}
	}

	public string By
	{
		get
		{
			return m_By;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("By");
			}
			if (value == string.Empty)
			{
				throw new ArgumentException("Property 'By' value must be specified", "By");
			}
			m_By = value;
			m_IsModified = true;
		}
	}

	public Mail_t_TcpInfo By_TcpInfo
	{
		get
		{
			return m_pBy_TcpInfo;
		}
		set
		{
			m_pBy_TcpInfo = value;
			m_IsModified = true;
		}
	}

	public string Via
	{
		get
		{
			return m_Via;
		}
		set
		{
			m_Via = value;
			m_IsModified = true;
		}
	}

	public string With
	{
		get
		{
			return m_With;
		}
		set
		{
			m_With = value;
			m_IsModified = true;
		}
	}

	public string ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			m_ID = value;
			m_IsModified = true;
		}
	}

	public string For
	{
		get
		{
			return m_For;
		}
		set
		{
			m_For = value;
			m_IsModified = true;
		}
	}

	public DateTime Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			m_Time = value;
			m_IsModified = true;
		}
	}

	public Mail_h_Received(string from, string by, DateTime time)
	{
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		if (from == string.Empty)
		{
			throw new ArgumentException("Argument 'from' value must be specified.", "from");
		}
		if (by == null)
		{
			throw new ArgumentNullException("by");
		}
		if (by == string.Empty)
		{
			throw new ArgumentException("Argument 'by' value must be specified.", "by");
		}
		m_From = from;
		m_By = by;
		m_Time = time;
	}

	public static Mail_h_Received Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array.Length != 2)
		{
			throw new ParseException("Invalid header field value '" + value + "'.");
		}
		Mail_h_Received mail_h_Received = new Mail_h_Received("a", "b", DateTime.MinValue);
		MIME_Reader mIME_Reader = new MIME_Reader(array[1]);
		while (true)
		{
			string text = mIME_Reader.Word();
			if (text == null && mIME_Reader.Available == 0)
			{
				break;
			}
			if (mIME_Reader.StartsWith("("))
			{
				mIME_Reader.ReadParenthesized();
				continue;
			}
			if (mIME_Reader.StartsWith(";"))
			{
				mIME_Reader.Char(readToFirstChar: false);
				try
				{
					mail_h_Received.m_Time = MIME_Utils.ParseRfc2822DateTime(mIME_Reader.QuotedReadToDelimiter(new char[1] { ';' }));
				}
				catch
				{
				}
				continue;
			}
			if (text == null)
			{
				mIME_Reader.Char(readToFirstChar: true);
				continue;
			}
			switch (text.ToUpperInvariant())
			{
			case "FROM":
			{
				mail_h_Received.m_From = mIME_Reader.DotAtom();
				mIME_Reader.ToFirstChar();
				if (!mIME_Reader.StartsWith("("))
				{
					break;
				}
				string[] array3 = mIME_Reader.ReadParenthesized().Split(' ');
				if (array3.Length == 1)
				{
					if (Net_Utils.IsIPAddress(array3[0]))
					{
						mail_h_Received.m_pFrom_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(array3[0]), null);
					}
				}
				else if (array3.Length == 2 && Net_Utils.IsIPAddress(array3[1]))
				{
					mail_h_Received.m_pFrom_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(array3[1]), array3[0]);
				}
				break;
			}
			case "BY":
			{
				mail_h_Received.m_By = mIME_Reader.DotAtom();
				mIME_Reader.ToFirstChar();
				if (!mIME_Reader.StartsWith("("))
				{
					break;
				}
				string[] array2 = mIME_Reader.ReadParenthesized().Split(' ');
				if (array2.Length == 1)
				{
					if (Net_Utils.IsIPAddress(array2[0]))
					{
						mail_h_Received.m_pBy_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(array2[0]), null);
					}
				}
				else if (array2.Length == 2 && Net_Utils.IsIPAddress(array2[1]))
				{
					mail_h_Received.m_pBy_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(array2[1]), array2[0]);
				}
				break;
			}
			case "VIA":
				mail_h_Received.m_Via = mIME_Reader.Word();
				break;
			case "WITH":
				mail_h_Received.m_With = mIME_Reader.Word();
				break;
			case "ID":
				if (mIME_Reader.StartsWith("<"))
				{
					mail_h_Received.m_ID = mIME_Reader.ReadParenthesized();
				}
				else
				{
					mail_h_Received.m_ID = mIME_Reader.Atom();
				}
				break;
			case "FOR":
			{
				mIME_Reader.ToFirstChar();
				if (mIME_Reader.StartsWith("<"))
				{
					mail_h_Received.m_For = mIME_Reader.ReadParenthesized();
					break;
				}
				string text2 = Mail_Utils.SMTP_Mailbox(mIME_Reader);
				if (text2 == null)
				{
					throw new ParseException("Invalid Received: For parameter value '" + mIME_Reader.ToEnd() + "'.");
				}
				mail_h_Received.m_For = text2;
				break;
			}
			default:
				mIME_Reader.Word();
				break;
			}
		}
		mail_h_Received.m_ParseValue = value;
		return mail_h_Received;
	}

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		if (reEncode || IsModified)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Received: ");
			stringBuilder.Append("from " + m_From);
			if (m_pFrom_TcpInfo != null)
			{
				stringBuilder.Append(" (" + m_pFrom_TcpInfo.ToString() + ")");
			}
			stringBuilder.Append(" by " + m_By);
			if (m_pBy_TcpInfo != null)
			{
				stringBuilder.Append(" (" + m_pBy_TcpInfo.ToString() + ")");
			}
			if (!string.IsNullOrEmpty(m_Via))
			{
				stringBuilder.Append(" via " + m_Via);
			}
			if (!string.IsNullOrEmpty(m_With))
			{
				stringBuilder.Append(" with " + m_With);
			}
			if (!string.IsNullOrEmpty(m_ID))
			{
				stringBuilder.Append(" id " + m_ID);
			}
			if (!string.IsNullOrEmpty(m_For))
			{
				stringBuilder.Append(" for " + m_For);
			}
			stringBuilder.Append("; " + MIME_Utils.DateTimeToRfc2822(m_Time));
			stringBuilder.Append("\r\n");
			return stringBuilder.ToString();
		}
		return m_ParseValue;
	}
}
