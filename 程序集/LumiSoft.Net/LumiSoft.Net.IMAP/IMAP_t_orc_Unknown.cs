using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_Unknown : IMAP_t_orc
{
	private string m_Value;

	public string Value => m_Value;

	public IMAP_t_orc_Unknown(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_Value = value;
	}

	public new static IMAP_t_orc_Unknown Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		return new IMAP_t_orc_Unknown(r.ReadParenthesized());
	}

	public override string ToString()
	{
		return m_Value;
	}
}
