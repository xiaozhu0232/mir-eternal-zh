using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Bye : IMAP_r_u
{
	private string m_Text;

	public string Text => m_Text;

	public IMAP_r_u_Bye(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_Text = text;
	}

	public static IMAP_r_u_Bye Parse(string byeResponse)
	{
		if (byeResponse == null)
		{
			throw new ArgumentNullException("byeResponse");
		}
		StringReader stringReader = new StringReader(byeResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		return new IMAP_r_u_Bye(stringReader.ReadToEnd());
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* BYE " + m_Text + "\r\n");
		return stringBuilder.ToString();
	}
}
