using System;
using System.IO;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_Fetch_r_i_Body : IMAP_t_Fetch_r_i
{
	private string m_Section;

	private int m_Offset = -1;

	private Stream m_pStream;

	public string BodySection => m_Section;

	public int Offset => m_Offset;

	public Stream Stream => m_pStream;

	public IMAP_t_Fetch_r_i_Body(string section, int offset, Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_Section = section;
		m_Offset = offset;
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
