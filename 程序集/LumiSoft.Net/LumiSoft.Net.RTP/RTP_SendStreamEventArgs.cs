using System;

namespace LumiSoft.Net.RTP;

public class RTP_SendStreamEventArgs : EventArgs
{
	private RTP_SendStream m_pStream;

	public RTP_SendStream Stream => m_pStream;

	public RTP_SendStreamEventArgs(RTP_SendStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
	}
}
