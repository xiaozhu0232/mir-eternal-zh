using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_Unseen : IMAP_t_orc
{
	private int m_FirstUnseen;

	public int SeqNo => m_FirstUnseen;

	public IMAP_t_orc_Unseen(int firstUnseen)
	{
		m_FirstUnseen = firstUnseen;
	}

	public new static IMAP_t_orc_Unseen Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("UNSEEN", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid UNSEEN response value.", "r");
		}
		if (array.Length != 2)
		{
			throw new ArgumentException("Invalid UNSEEN response value.", "r");
		}
		return new IMAP_t_orc_Unseen(Convert.ToInt32(array[1]));
	}

	public override string ToString()
	{
		return "UNSEEN " + m_FirstUnseen;
	}
}
