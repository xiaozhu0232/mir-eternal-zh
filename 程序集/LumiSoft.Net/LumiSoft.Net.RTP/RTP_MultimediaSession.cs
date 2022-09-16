using System;
using System.Collections.Generic;

namespace LumiSoft.Net.RTP;

public class RTP_MultimediaSession : IDisposable
{
	private bool m_IsDisposed;

	private RTP_Participant_Local m_pLocalParticipant;

	private List<RTP_Session> m_pSessions;

	private Dictionary<string, RTP_Participant_Remote> m_pParticipants;

	public bool IsDisposed => m_IsDisposed;

	public RTP_Session[] Sessions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSessions.ToArray();
		}
	}

	public RTP_Participant_Local LocalParticipant
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLocalParticipant;
		}
	}

	public RTP_Participant_Remote[] RemoteParticipants
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pParticipants)
			{
				RTP_Participant_Remote[] array = new RTP_Participant_Remote[m_pParticipants.Count];
				m_pParticipants.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public event EventHandler<EventArgs<RTP_Session>> SessionCreated;

	public event EventHandler<RTP_ParticipantEventArgs> NewParticipant;

	public event EventHandler<ExceptionEventArgs> Error;

	public RTP_MultimediaSession(string cname)
	{
		if (cname == null)
		{
			throw new ArgumentNullException("cname");
		}
		if (cname == string.Empty)
		{
			throw new ArgumentException("Argument 'cname' value must be specified.");
		}
		m_pLocalParticipant = new RTP_Participant_Local(cname);
		m_pSessions = new List<RTP_Session>();
		m_pParticipants = new Dictionary<string, RTP_Participant_Remote>();
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			RTP_Session[] array = m_pSessions.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
			m_IsDisposed = true;
			m_pLocalParticipant = null;
			m_pSessions = null;
			m_pParticipants = null;
			this.NewParticipant = null;
			this.Error = null;
		}
	}

	public void Close(string closeReason)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		RTP_Session[] array = m_pSessions.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Close(closeReason);
		}
		Dispose();
	}

	public void Start()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	public void Stop()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	public RTP_Session CreateSession(RTP_Address localEP, RTP_Clock clock)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		if (clock == null)
		{
			throw new ArgumentNullException("clock");
		}
		RTP_Session rTP_Session = new RTP_Session(this, localEP, clock);
		rTP_Session.Disposed += delegate(object s, EventArgs e)
		{
			m_pSessions.Remove((RTP_Session)s);
		};
		m_pSessions.Add(rTP_Session);
		OnSessionCreated(rTP_Session);
		return rTP_Session;
	}

	internal RTP_Participant_Remote GetOrCreateParticipant(string cname)
	{
		if (cname == null)
		{
			throw new ArgumentNullException("cname");
		}
		if (cname == string.Empty)
		{
			throw new ArgumentException("Argument 'cname' value must be specified.");
		}
		lock (m_pParticipants)
		{
			RTP_Participant_Remote participant = null;
			if (!m_pParticipants.TryGetValue(cname, out participant))
			{
				participant = new RTP_Participant_Remote(cname);
				participant.Removed += delegate
				{
					m_pParticipants.Remove(participant.CNAME);
				};
				m_pParticipants.Add(cname, participant);
				OnNewParticipant(participant);
			}
			return participant;
		}
	}

	private void OnSessionCreated(RTP_Session session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (this.SessionCreated != null)
		{
			this.SessionCreated(this, new EventArgs<RTP_Session>(session));
		}
	}

	private void OnNewParticipant(RTP_Participant_Remote participant)
	{
		if (this.NewParticipant != null)
		{
			this.NewParticipant(this, new RTP_ParticipantEventArgs(participant));
		}
	}

	internal void OnError(Exception exception)
	{
		if (this.Error != null)
		{
			this.Error(this, new ExceptionEventArgs(exception));
		}
	}
}
