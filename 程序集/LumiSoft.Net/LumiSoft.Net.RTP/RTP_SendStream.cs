using System;

namespace LumiSoft.Net.RTP;

public class RTP_SendStream
{
	private bool m_IsDisposed;

	private RTP_Source_Local m_pSource;

	private int m_SeqNoWrapCount;

	private int m_SeqNo;

	private DateTime m_LastPacketTime;

	private uint m_LastPacketRtpTimestamp;

	private long m_RtpPacketsSent;

	private long m_RtpBytesSent;

	private long m_RtpDataBytesSent;

	private int m_RtcpCyclesSinceWeSent = 9999;

	public bool IsDisposed => m_IsDisposed;

	public RTP_Session Session
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSource.Session;
		}
	}

	public RTP_Source Source
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSource;
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

	public int SeqNo
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SeqNo;
		}
	}

	public DateTime LastPacketTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastPacketTime;
		}
	}

	public uint LastPacketRtpTimestamp
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastPacketRtpTimestamp;
		}
	}

	public long RtpPacketsSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpPacketsSent;
		}
	}

	public long RtpBytesSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpBytesSent;
		}
	}

	public long RtpDataBytesSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpDataBytesSent;
		}
	}

	internal int RtcpCyclesSinceWeSent => m_RtcpCyclesSinceWeSent;

	public event EventHandler Disposed;

	public event EventHandler Closed;

	internal RTP_SendStream(RTP_Source_Local source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_pSource = source;
		m_SeqNo = new Random().Next(1, 32000);
	}

	private void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pSource = null;
			OnDisposed();
			this.Disposed = null;
			this.Closed = null;
		}
	}

	public void Close()
	{
		Close(null);
	}

	public void Close(string closeReason)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		m_pSource.Close(closeReason);
		OnClosed();
		Dispose();
	}

	public void Send(RTP_Packet packet)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (Session.StreamMode != RTP_StreamMode.Inactive && Session.StreamMode != RTP_StreamMode.Receive)
		{
			packet.SSRC = Source.SSRC;
			packet.SeqNo = NextSeqNo();
			packet.PayloadType = Session.Payload;
			m_RtpBytesSent += m_pSource.SendRtpPacket(packet);
			m_RtpPacketsSent++;
			m_RtpDataBytesSent += packet.Data.Length;
			m_LastPacketTime = DateTime.Now;
			m_LastPacketRtpTimestamp = packet.Timestamp;
			m_RtcpCyclesSinceWeSent = 0;
		}
	}

	internal void RtcpCycle()
	{
		m_RtcpCyclesSinceWeSent++;
	}

	private ushort NextSeqNo()
	{
		if (m_SeqNo >= 65535)
		{
			m_SeqNo = 0;
			m_SeqNoWrapCount++;
		}
		return (ushort)m_SeqNo++;
	}

	private void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, new EventArgs());
		}
	}
}
