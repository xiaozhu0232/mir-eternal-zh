using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_TryCreate : IMAP_t_orc
{
	public new static IMAP_t_orc_TryCreate Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("TRYCREATE", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid TRYCREATE response value.", "r");
		}
		return new IMAP_t_orc_TryCreate();
	}

	public override string ToString()
	{
		return "TRYCREATE";
	}
}
