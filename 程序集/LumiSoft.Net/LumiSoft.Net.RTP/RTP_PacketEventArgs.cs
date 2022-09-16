using System;

namespace LumiSoft.Net.RTP;

public class RTP_PacketEventArgs : EventArgs
{
	private RTP_Packet m_pPacket;

	public RTP_Packet Packet => m_pPacket;

	public RTP_PacketEventArgs(RTP_Packet packet)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		m_pPacket = packet;
	}
}
