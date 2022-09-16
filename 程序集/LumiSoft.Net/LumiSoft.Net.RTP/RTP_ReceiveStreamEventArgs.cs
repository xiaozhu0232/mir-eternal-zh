using System;

namespace LumiSoft.Net.RTP;

public class RTP_ReceiveStreamEventArgs : EventArgs
{
	private RTP_ReceiveStream m_pStream;

	public RTP_ReceiveStream Stream => m_pStream;

	public RTP_ReceiveStreamEventArgs(RTP_ReceiveStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
	}
}
