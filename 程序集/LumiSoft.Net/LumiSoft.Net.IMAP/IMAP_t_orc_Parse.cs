using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_Parse : IMAP_t_orc
{
	private string m_ErrorText;

	public string ErrorText => m_ErrorText;

	public IMAP_t_orc_Parse(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_ErrorText = text;
	}

	public new static IMAP_t_orc_Parse Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("PARSE", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid PARSE response value.", "r");
		}
		return new IMAP_t_orc_Parse((array.Length == 2) ? array[1] : "");
	}

	public override string ToString()
	{
		return "PARSE " + m_ErrorText;
	}
}
