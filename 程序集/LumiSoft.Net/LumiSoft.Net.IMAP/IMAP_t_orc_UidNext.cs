using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_UidNext : IMAP_t_orc
{
	private int m_UidNext;

	public int UidNext => m_UidNext;

	public IMAP_t_orc_UidNext(int uidNext)
	{
		m_UidNext = uidNext;
	}

	public new static IMAP_t_orc_UidNext Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("UIDNEXT", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid UIDNEXT response value.", "r");
		}
		if (array.Length != 2)
		{
			throw new ArgumentException("Invalid UIDNEXT response value.", "r");
		}
		return new IMAP_t_orc_UidNext(Convert.ToInt32(array[1]));
	}

	public override string ToString()
	{
		return "UIDNEXT " + m_UidNext;
	}
}
