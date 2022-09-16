using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Rfc822Size : IMAP_t_Fetch_r_i
{
	private int m_Size;

	public int Size => m_Size;

	public IMAP_t_Fetch_r_i_Rfc822Size(int size)
	{
		if (size < 0)
		{
			throw new ArgumentException("Argument 'size' value must be >= 0.", "size");
		}
		m_Size = size;
	}
}
