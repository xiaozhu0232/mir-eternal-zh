using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_ReadWrite : IMAP_t_orc
{
	public new static IMAP_t_orc_ReadWrite Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("READ-WRITE", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid READ-WRITE response value.", "r");
		}
		return new IMAP_t_orc_ReadWrite();
	}

	public override string ToString()
	{
		return "READ-WRITE";
	}
}
