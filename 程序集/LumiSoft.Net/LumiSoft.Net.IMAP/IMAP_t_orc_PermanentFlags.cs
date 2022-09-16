using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_PermanentFlags : IMAP_t_orc
{
	private string[] m_pFlags;

	public string[] Flags => m_pFlags;

	public IMAP_t_orc_PermanentFlags(string[] flags)
	{
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		m_pFlags = flags;
	}

	public new static IMAP_t_orc_PermanentFlags Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (!r.StartsWith("[PERMANENTFLAGS", case_sensitive: false))
		{
			throw new ArgumentException("Invalid PERMANENTFLAGS response value.", "r");
		}
		r.ReadSpecifiedLength(1);
		r.ReadWord();
		r.ReadToFirstChar();
		string[] flags = r.ReadParenthesized().Split(' ');
		r.ReadSpecifiedLength(1);
		return new IMAP_t_orc_PermanentFlags(flags);
	}

	public override string ToString()
	{
		return "PERMANENTFLAGS (" + Net_Utils.ArrayToString(m_pFlags, " ") + ")";
	}
}
