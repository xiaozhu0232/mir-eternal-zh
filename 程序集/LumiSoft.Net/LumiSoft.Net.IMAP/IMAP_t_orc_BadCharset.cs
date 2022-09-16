using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_BadCharset : IMAP_t_orc
{
	private string[] m_pCharsets;

	public string[] Charsets => m_pCharsets;

	public IMAP_t_orc_BadCharset(string[] charsets)
	{
		if (charsets == null)
		{
			throw new ArgumentNullException("charsets");
		}
		m_pCharsets = charsets;
	}

	public new static IMAP_t_orc_BadCharset Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("BADCHARSET", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid BADCHARSET response value.", "r");
		}
		return new IMAP_t_orc_BadCharset(array[1].Trim().Split(' '));
	}

	public override string ToString()
	{
		return "BADCHARSET " + Net_Utils.ArrayToString(m_pCharsets, " ");
	}
}
