using System;

namespace LumiSoft.Net.RTP;

public class RTP_SourceEventArgs : EventArgs
{
	private RTP_Source m_pSource;

	public RTP_Source Source => m_pSource;

	public RTP_SourceEventArgs(RTP_Source source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_pSource = source;
	}
}
