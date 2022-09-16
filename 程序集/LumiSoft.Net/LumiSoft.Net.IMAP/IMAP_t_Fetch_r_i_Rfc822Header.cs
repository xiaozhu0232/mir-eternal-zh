using System;
using System.IO;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Rfc822Header : IMAP_t_Fetch_r_i
{
	private Stream m_pStream;

	public Stream Stream => m_pStream;

	public IMAP_t_Fetch_r_i_Rfc822Header(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
	}

	internal void SetStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
	}
}
