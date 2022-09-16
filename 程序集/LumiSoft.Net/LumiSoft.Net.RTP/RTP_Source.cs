using System;
using System.Net;

namespace LumiSoft.Net.RTP;

public abstract class RTP_Source
{
	private RTP_SourceState m_State = RTP_SourceState.Passive;

	private RTP_Session m_pSession;

	private uint m_SSRC;

	private IPEndPoint m_pRtcpEP;

	private IPEndPoint m_pRtpEP;

	private DateTime m_LastRtcpPacket = DateTime.MinValue;

	private DateTime m_LastRtpPacket = DateTime.MinValue;

	private DateTime m_LastActivity = DateTime.Now;

	private DateTime m_LastRRTime = DateTime.MinValue;

	private string m_CloseReason;

	private object m_pTag;

	public RTP_SourceState State => m_State;

	public RTP_Session Session
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSession;
		}
	}

	public uint SSRC
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SSRC;
		}
	}

	public IPEndPoint RtcpEP
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRtcpEP;
		}
	}

	public IPEndPoint RtpEP
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRtpEP;
		}
	}

	public abstract bool IsLocal { get; }

	public DateTime LastActivity
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastActivity;
		}
	}

	public DateTime LastRtcpPacket
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastRtcpPacket;
		}
	}

	public DateTime LastRtpPacket
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastRtpPacket;
		}
	}

	public DateTime LastRRTime
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LastRRTime;
		}
	}

	public string CloseReason
	{
		get
		{
			if (m_State == RTP_SourceState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CloseReason;
		}
	}

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	internal abstract string CName { get; }

	public event EventHandler Closed;

	public event EventHandler Disposing;

	public event EventHandler StateChanged;

	internal RTP_Source(RTP_Session session, uint ssrc)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		m_pSession = session;
		m_SSRC = ssrc;
	}

	internal virtual void Dispose()
	{
		if (m_State != RTP_SourceState.Disposed)
		{
			OnDisposing();
			SetState(RTP_SourceState.Disposed);
			m_pSession = null;
			m_pRtcpEP = null;
			m_pRtpEP = null;
			this.Closed = null;
			this.Disposing = null;
			this.StateChanged = null;
		}
	}

	internal virtual void Close(string closeReason)
	{
		m_CloseReason = closeReason;
		OnClosed();
		Dispose();
	}

	internal void SetRtcpEP(IPEndPoint ep)
	{
		m_pRtcpEP = ep;
	}

	internal void SetRtpEP(IPEndPoint ep)
	{
		m_pRtpEP = ep;
	}

	internal void SetActivePassive(bool active)
	{
	}

	internal void SetLastRtcpPacket(DateTime time)
	{
		m_LastRtcpPacket = time;
		m_LastActivity = time;
	}

	internal void SetLastRtpPacket(DateTime time)
	{
		m_LastRtpPacket = time;
		m_LastActivity = time;
	}

	internal void SetRR(RTCP_Packet_ReportBlock rr)
	{
		if (rr == null)
		{
			throw new ArgumentNullException("rr");
		}
	}

	internal void GenerateNewSSRC()
	{
		m_SSRC = RTP_Utils.GenerateSSRC();
	}

	protected void SetState(RTP_SourceState state)
	{
		if (m_State != RTP_SourceState.Disposed && m_State != state)
		{
			m_State = state;
			OnStateChaged();
		}
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, new EventArgs());
		}
	}

	private void OnDisposing()
	{
		if (this.Disposing != null)
		{
			this.Disposing(this, new EventArgs());
		}
	}

	private void OnStateChaged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs());
		}
	}
}
