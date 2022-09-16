using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_UidValidity : IMAP_t_orc
{
	private long m_Uid;

	public long Uid => m_Uid;

	public IMAP_t_orc_UidValidity(long uid)
	{
		m_Uid = uid;
	}

	public new static IMAP_t_orc_UidValidity Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("UIDVALIDITY", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid UIDVALIDITY response value.", "r");
		}
		if (array.Length != 2)
		{
			throw new ArgumentException("Invalid UIDVALIDITY response value.", "r");
		}
		return new IMAP_t_orc_UidValidity(Convert.ToInt64(array[1]));
	}

	public override string ToString()
	{
		return "UIDVALIDITY " + m_Uid;
	}
}
