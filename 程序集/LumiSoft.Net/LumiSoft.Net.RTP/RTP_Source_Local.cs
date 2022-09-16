using System;
using System.Net;

namespace LumiSoft.Net.RTP;

public class RTP_Source_Local : RTP_Source
{
	private RTP_SendStream m_pStream;

	public override bool IsLocal
	{
		get
		{
			if (base.State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return true;
		}
	}

	public RTP_Participant_Local Participant => base.Session.Session.LocalParticipant;

	public RTP_SendStream Stream => m_pStream;

	internal override string CName
	{
		get
		{
			if (Participant != null)
			{
				return null;
			}
			return Participant.CNAME;
		}
	}

	internal RTP_Source_Local(RTP_Session session, uint ssrc, IPEndPoint rtcpEP, IPEndPoint rtpEP)
		: base(session, ssrc)
	{
		if (rtcpEP == null)
		{
			throw new ArgumentNullException("rtcpEP");
		}
		if (rtpEP == null)
		{
			throw new ArgumentNullException("rtpEP");
		}
		SetRtcpEP(rtcpEP);
		SetRtpEP(rtpEP);
	}

	public void SendApplicationPacket(RTCP_Packet_APP packet)
	{
		if (base.State == RTP_SourceState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		packet.Source = base.SSRC;
		RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
		new RTCP_Packet_RR().SSRC = base.SSRC;
		rTCP_CompoundPacket.Packets.Add(packet);
		base.Session.SendRtcpPacket(rTCP_CompoundPacket);
	}

	internal override void Close(string closeReason)
	{
		if (base.State == RTP_SourceState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
		RTCP_Packet_RR rTCP_Packet_RR = new RTCP_Packet_RR();
		rTCP_Packet_RR.SSRC = base.SSRC;
		rTCP_CompoundPacket.Packets.Add(rTCP_Packet_RR);
		RTCP_Packet_BYE rTCP_Packet_BYE = new RTCP_Packet_BYE();
		rTCP_Packet_BYE.Sources = new uint[1] { base.SSRC };
		if (!string.IsNullOrEmpty(closeReason))
		{
			rTCP_Packet_BYE.LeavingReason = closeReason;
		}
		rTCP_CompoundPacket.Packets.Add(rTCP_Packet_BYE);
		base.Session.SendRtcpPacket(rTCP_CompoundPacket);
		base.Close(closeReason);
	}

	internal void CreateStream()
	{
		if (m_pStream != null)
		{
			throw new InvalidOperationException("Stream is already created.");
		}
		m_pStream = new RTP_SendStream(this);
		m_pStream.Disposed += delegate
		{
			m_pStream = null;
			Dispose();
		};
		SetState(RTP_SourceState.Active);
	}

	internal int SendRtpPacket(RTP_Packet packet)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (m_pStream == null)
		{
			throw new InvalidOperationException("RTP stream is not created by CreateStream method.");
		}
		SetLastRtpPacket(DateTime.Now);
		SetState(RTP_SourceState.Active);
		return base.Session.SendRtpPacket(m_pStream, packet);
	}
}
