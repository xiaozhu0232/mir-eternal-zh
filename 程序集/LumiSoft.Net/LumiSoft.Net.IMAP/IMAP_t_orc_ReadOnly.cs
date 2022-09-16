using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_ReadOnly : IMAP_t_orc
{
	public new static IMAP_t_orc_ReadOnly Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("READ-ONLY", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid READ-ONLY response value.", "r");
		}
		return new IMAP_t_orc_ReadOnly();
	}

	public override string ToString()
	{
		return "READ-ONLY";
	}
}
