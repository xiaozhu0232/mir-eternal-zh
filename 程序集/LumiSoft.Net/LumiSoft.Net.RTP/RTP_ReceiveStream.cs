using System;

namespace LumiSoft.Net.RTP;

public class RTP_ReceiveStream
{
	private bool m_IsDisposed;

	private RTP_Session m_pSession;

	private RTP_Source m_pSSRC;

	private RTP_Participant_Remote m_pParticipant;

	private int m_SeqNoWrapCount;

	private ushort m_MaxSeqNo;

	private long m_PacketsReceived;

	private long m_PacketsMisorder;

	private long m_BytesReceived;

	private double m_Jitter;

	private RTCP_Report_Sender m_pLastSR;

	private uint m_BaseSeq;

	private long m_ReceivedPrior;

	private long m_ExpectedPrior;

	private int m_Transit;

	private uint m_LastBadSeqPlus1;

	private int m_Probation;

	private DateTime m_LastSRTime = DateTime.MinValue;

	private int MAX_DROPOUT = 3000;

	private int MAX_MISORDER = 100;

	private int MIN_SEQUENTIAL = 2;

	private uint RTP_SEQ_MOD = 65536u;

	public bool IsDisposed => m_IsDisposed;

	public RTP_Session Session
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSession;
		}
	}

	public RTP_Source SSRC
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSSRC;
		}
	}

	public RTP_Participant_Remote Participant
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pParticipant;
		}
	}

	public int SeqNoWrapCount
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SeqNoWrapCount;
		}
	}

	public int FirstSeqNo
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (int)m_BaseSeq;
		}
	}

	public int MaxSeqNo
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxSeqNo;
		}
	}

	public long PacketsReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_PacketsReceived;
		}
	}

	public long PacketsMisorder
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_PacketsMisorder;
		}
	}

	public long PacketsLost
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (uint)(65536 * m_SeqNoWrapCount + m_MaxSeqNo - (int)m_BaseSeq + 1) - m_PacketsReceived;
		}
	}

	public long BytesReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_BytesReceived;
		}
	}

	public double Jitter
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Jitter;
		}
	}

	public int DelaySinceLastSR
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (int)((m_LastSRTime == DateTime.MinValue) ? (-1.0) : (DateTime.Now - m_LastSRTime).TotalMilliseconds);
		}
	}

	public DateTime LastSRTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastSRTime;
		}
	}

	public RTCP_Report_Sender LastSR
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLastSR;
		}
	}

	public event EventHandler Closed;

	public event EventHandler Timeout;

	public event EventHandler SenderReport;

	public event EventHandler<RTP_PacketEventArgs> PacketReceived;

	internal RTP_ReceiveStream(RTP_Session session, RTP_Source ssrc, ushort packetSeqNo)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (ssrc == null)
		{
			throw new ArgumentNullException("ssrc");
		}
		m_pSession = session;
		m_pSSRC = ssrc;
		InitSeq(packetSeqNo);
		m_MaxSeqNo = (ushort)(packetSeqNo - 1);
		m_Probation = MIN_SEQUENTIAL;
	}

	internal void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pSession = null;
			m_pParticipant = null;
			this.Closed = null;
			this.Timeout = null;
			this.SenderReport = null;
			this.PacketReceived = null;
		}
	}

	internal void Process(RTP_Packet packet, int size)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		m_BytesReceived += size;
		if (UpdateSeq(packet.SeqNo))
		{
			OnPacketReceived(packet);
			int num = (int)(RTP_Utils.DateTimeToNTP32(DateTime.Now) - packet.Timestamp);
			int num2 = num - m_Transit;
			m_Transit = num;
			if (num2 < 0)
			{
				num2 = -num2;
			}
			m_Jitter += 0.0625 * ((double)num2 - m_Jitter);
		}
	}

	private void InitSeq(ushort seqNo)
	{
		m_BaseSeq = seqNo;
		m_MaxSeqNo = seqNo;
		m_LastBadSeqPlus1 = RTP_SEQ_MOD + 1;
		m_SeqNoWrapCount = 0;
		m_PacketsReceived = 0L;
		m_ReceivedPrior = 0L;
		m_ExpectedPrior = 0L;
	}

	private bool UpdateSeq(ushort seqNo)
	{
		ushort num = (ushort)(seqNo - m_MaxSeqNo);
		if (m_Probation > 0)
		{
			if (seqNo == m_MaxSeqNo + 1)
			{
				m_Probation--;
				m_MaxSeqNo = seqNo;
				if (m_Probation == 0)
				{
					InitSeq(seqNo);
					m_PacketsReceived++;
					m_pSession.OnNewReceiveStream(this);
					return true;
				}
			}
			else
			{
				m_Probation = MIN_SEQUENTIAL - 1;
				m_MaxSeqNo = seqNo;
			}
			return false;
		}
		if (num < MAX_DROPOUT)
		{
			if (seqNo < m_MaxSeqNo)
			{
				m_SeqNoWrapCount++;
			}
			m_MaxSeqNo = seqNo;
		}
		else if (num <= RTP_SEQ_MOD - MAX_MISORDER)
		{
			if (seqNo != m_LastBadSeqPlus1)
			{
				m_LastBadSeqPlus1 = (uint)((seqNo + 1) & (RTP_SEQ_MOD - 1));
				return false;
			}
			InitSeq(seqNo);
		}
		else
		{
			m_PacketsMisorder++;
		}
		m_PacketsReceived++;
		return true;
	}

	internal void SetSR(RTCP_Report_Sender report)
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		m_LastSRTime = DateTime.Now;
		m_pLastSR = report;
		OnSenderReport();
	}

	internal RTCP_Packet_ReportBlock CreateReceiverReport()
	{
		uint num = (uint)(m_SeqNoWrapCount << 16 + m_MaxSeqNo);
		uint num2 = num - m_BaseSeq + 1;
		int num3 = (int)(num2 - m_ExpectedPrior);
		m_ExpectedPrior = num2;
		int num4 = (int)(m_PacketsReceived - m_ReceivedPrior);
		m_ReceivedPrior = m_PacketsReceived;
		int num5 = num3 - num4;
		int num6 = 0;
		num6 = ((num3 != 0 && num5 > 0) ? ((num5 << 8) / num3) : 0);
		return new RTCP_Packet_ReportBlock(SSRC.SSRC)
		{
			FractionLost = (uint)num6,
			CumulativePacketsLost = (uint)PacketsLost,
			ExtendedHighestSeqNo = num,
			Jitter = (uint)m_Jitter,
			LastSR = ((m_pLastSR != null) ? ((uint)(int)((long)m_pLastSR.NtpTimestamp >> 8) & 0xFFFFu) : 0u),
			DelaySinceLastSR = (uint)Math.Max(0.0, (double)DelaySinceLastSR / 65.536)
		};
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, new EventArgs());
		}
	}

	internal void OnTimeout()
	{
		if (this.Timeout != null)
		{
			this.Timeout(this, new EventArgs());
		}
	}

	private void OnSenderReport()
	{
		if (this.SenderReport != null)
		{
			this.SenderReport(this, new EventArgs());
		}
	}

	private void OnPacketReceived(RTP_Packet packet)
	{
		if (this.PacketReceived != null)
		{
			this.PacketReceived(this, new RTP_PacketEventArgs(packet));
		}
	}
}
