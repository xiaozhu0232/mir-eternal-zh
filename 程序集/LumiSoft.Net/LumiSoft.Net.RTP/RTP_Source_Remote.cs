using System;

namespace LumiSoft.Net.RTP;

public class RTP_Source_Remote : RTP_Source
{
	private RTP_Participant_Remote m_pParticipant;

	private RTP_ReceiveStream m_pStream;

	public override bool IsLocal
	{
		get
		{
			if (base.State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return false;
		}
	}

	public RTP_Participant_Remote Participant
	{
		get
		{
			if (base.State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pParticipant;
		}
	}

	public RTP_ReceiveStream Stream
	{
		get
		{
			if (base.State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pStream;
		}
	}

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

	public event EventHandler<EventArgs<RTCP_Packet_APP>> ApplicationPacket;

	internal RTP_Source_Remote(RTP_Session session, uint ssrc)
		: base(session, ssrc)
	{
	}

	internal override void Dispose()
	{
		m_pParticipant = null;
		if (m_pStream != null)
		{
			m_pStream.Dispose();
		}
		this.ApplicationPacket = null;
		base.Dispose();
	}

	internal void SetParticipant(RTP_Participant_Remote participant)
	{
		if (participant == null)
		{
			throw new ArgumentNullException("participant");
		}
		m_pParticipant = participant;
	}

	internal void OnRtpPacketReceived(RTP_Packet packet, int size)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		SetLastRtpPacket(DateTime.Now);
		if (m_pStream == null)
		{
			m_pStream = new RTP_ReceiveStream(base.Session, this, packet.SeqNo);
			SetState(RTP_SourceState.Active);
		}
		m_pStream.Process(packet, size);
	}

	internal void OnSenderReport(RTCP_Report_Sender report)
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		if (m_pStream != null)
		{
			m_pStream.SetSR(report);
		}
	}

	internal void OnAppPacket(RTCP_Packet_APP packet)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		OnApplicationPacket(packet);
	}

	private void OnApplicationPacket(RTCP_Packet_APP packet)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (this.ApplicationPacket != null)
		{
			this.ApplicationPacket(this, new EventArgs<RTCP_Packet_APP>(packet));
		}
	}
}
