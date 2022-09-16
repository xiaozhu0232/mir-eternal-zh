using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_Alert : IMAP_t_orc
{
	private string m_AlertText;

	public string AlertText => m_AlertText;

	public IMAP_t_orc_Alert(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_AlertText = text;
	}

	public new static IMAP_t_orc_Alert Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("ALERT", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid ALERT response value.", "r");
		}
		return new IMAP_t_orc_Alert((array.Length == 2) ? array[1] : "");
	}

	public override string ToString()
	{
		return "ALERT " + m_AlertText;
	}
}
