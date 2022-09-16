using System;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.SDP;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.UA;

[Obsolete("Use SIP stack instead.")]
public class SIP_UA_Call : IDisposable
{
	private SIP_UA_CallState m_State;

	private SIP_UA m_pUA;

	private SIP_Request m_pInvite;

	private SDP_Message m_pLocalSDP;

	private SDP_Message m_pRemoteSDP;

	private DateTime m_StartTime;

	private List<SIP_Dialog_Invite> m_pEarlyDialogs;

	private SIP_Dialog m_pDialog;

	private bool m_IsRedirected;

	private SIP_RequestSender m_pInitialInviteSender;

	private SIP_ServerTransaction m_pInitialInviteTransaction;

	private AbsoluteUri m_pLocalUri;

	private AbsoluteUri m_pRemoteUri;

	private Dictionary<string, object> m_pTags;

	private object m_pLock = "";

	public bool IsDisposed => m_State == SIP_UA_CallState.Disposed;

	public SIP_UA_CallState State => m_State;

	public DateTime StartTime => m_StartTime;

	public AbsoluteUri LocalUri => m_pLocalUri;

	public AbsoluteUri RemoteUri => m_pRemoteUri;

	public SDP_Message LocalSDP => m_pLocalSDP;

	public SDP_Message RemoteSDP => m_pRemoteSDP;

	public int Duration => (DateTime.Now - m_StartTime).Seconds;

	public bool IsRedirected => m_IsRedirected;

	public bool IsOnhold => false;

	public Dictionary<string, object> Tag => m_pTags;

	public event EventHandler StateChanged;

	internal SIP_UA_Call(SIP_UA ua, SIP_Request invite)
	{
		if (ua == null)
		{
			throw new ArgumentNullException("ua");
		}
		if (invite == null)
		{
			throw new ArgumentNullException("invite");
		}
		if (invite.RequestLine.Method != "INVITE")
		{
			throw new ArgumentException("Argument 'invite' is not INVITE request.");
		}
		m_pUA = ua;
		m_pInvite = invite;
		m_pLocalUri = invite.From.Address.Uri;
		m_pRemoteUri = invite.To.Address.Uri;
		m_State = SIP_UA_CallState.WaitingForStart;
		m_pEarlyDialogs = new List<SIP_Dialog_Invite>();
		m_pTags = new Dictionary<string, object>();
	}

	internal SIP_UA_Call(SIP_UA ua, SIP_ServerTransaction invite)
	{
		if (ua == null)
		{
			throw new ArgumentNullException("ua");
		}
		if (invite == null)
		{
			throw new ArgumentNullException("invite");
		}
		m_pUA = ua;
		m_pInitialInviteTransaction = invite;
		m_pLocalUri = invite.Request.To.Address.Uri;
		m_pRemoteUri = invite.Request.From.Address.Uri;
		m_pInitialInviteTransaction.Canceled += delegate
		{
			SetState(SIP_UA_CallState.Terminated);
		};
		if (invite.Request.ContentType != null && invite.Request.ContentType.ToLower().IndexOf("application/sdp") > -1)
		{
			m_pRemoteSDP = SDP_Message.Parse(Encoding.UTF8.GetString(invite.Request.Data));
		}
		m_pTags = new Dictionary<string, object>();
		m_State = SIP_UA_CallState.WaitingToAccept;
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (m_State != SIP_UA_CallState.Disposed)
			{
				SetState(SIP_UA_CallState.Disposed);
				this.StateChanged = null;
			}
		}
	}

	private void m_pDialog_StateChanged(object sender, EventArgs e)
	{
		if (State != SIP_UA_CallState.Terminated && m_pDialog.State == SIP_DialogState.Terminated)
		{
			SetState(SIP_UA_CallState.Terminated);
			m_pDialog.Dispose();
		}
	}

	private void m_pInitialInviteSender_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		try
		{
			lock (m_pLock)
			{
				if (e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1)
				{
					m_pRemoteSDP = SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data));
				}
				if (e.Response.StatusCodeType == SIP_StatusCodeType.Provisional)
				{
					if (e.Response.StatusCode == 180)
					{
						SetState(SIP_UA_CallState.Ringing);
					}
					else if (e.Response.StatusCode == 182)
					{
						SetState(SIP_UA_CallState.Queued);
					}
					if (e.Response.StatusCode > 100 && e.Response.To.Tag != null)
					{
						m_pEarlyDialogs.Add((SIP_Dialog_Invite)m_pUA.Stack.TransactionLayer.GetOrCreateDialog(e.ClientTransaction, e.Response));
					}
				}
				else if (e.Response.StatusCodeType == SIP_StatusCodeType.Success)
				{
					m_StartTime = DateTime.Now;
					SetState(SIP_UA_CallState.Active);
					m_pDialog = m_pUA.Stack.TransactionLayer.GetOrCreateDialog(e.ClientTransaction, e.Response);
					m_pDialog.StateChanged += m_pDialog_StateChanged;
					SIP_Dialog_Invite[] array = m_pEarlyDialogs.ToArray();
					foreach (SIP_Dialog_Invite sIP_Dialog_Invite in array)
					{
						if (!m_pDialog.Equals(sIP_Dialog_Invite))
						{
							sIP_Dialog_Invite.Terminate("Another forking leg accepted.", sendBye: true);
						}
					}
				}
				else
				{
					SIP_Dialog_Invite[] array = m_pEarlyDialogs.ToArray();
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Terminate("All early dialogs are considered terminated upon reception of the non-2xx final response. (RFC 3261 13.2.2.3)", sendBye: false);
					}
					m_pEarlyDialogs.Clear();
					Error();
					SetState(SIP_UA_CallState.Terminated);
				}
			}
		}
		catch (Exception x)
		{
			m_pUA.Stack.OnError(x);
		}
	}

	public void Start()
	{
		lock (m_pLock)
		{
			if (m_State == SIP_UA_CallState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_State != 0)
			{
				throw new InvalidOperationException("Start method can be called only in 'SIP_UA_CallState.WaitingForStart' state.");
			}
			SetState(SIP_UA_CallState.Calling);
			m_pInitialInviteSender = m_pUA.Stack.CreateRequestSender(m_pInvite);
			m_pInitialInviteSender.ResponseReceived += m_pInitialInviteSender_ResponseReceived;
			m_pInitialInviteSender.Start();
		}
	}

	public void SendRinging(SDP_Message sdp)
	{
		if (m_State == SIP_UA_CallState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_State != SIP_UA_CallState.WaitingToAccept)
		{
			throw new InvalidOperationException("Accept method can be called only in 'SIP_UA_CallState.WaitingToAccept' state.");
		}
		SIP_Response sIP_Response = m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x180_Ringing, m_pInitialInviteTransaction.Request, m_pInitialInviteTransaction.Flow);
		if (sdp != null)
		{
			sIP_Response.ContentType = "application/sdp";
			sIP_Response.Data = sdp.ToByte();
			m_pLocalSDP = sdp;
		}
		m_pInitialInviteTransaction.SendResponse(sIP_Response);
	}

	public void Accept(SDP_Message sdp)
	{
		if (m_State == SIP_UA_CallState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_State != SIP_UA_CallState.WaitingToAccept)
		{
			throw new InvalidOperationException("Accept method can be called only in 'SIP_UA_CallState.WaitingToAccept' state.");
		}
		if (sdp == null)
		{
			throw new ArgumentNullException("sdp");
		}
		m_pLocalSDP = sdp;
		SIP_Response sIP_Response = m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x200_Ok, m_pInitialInviteTransaction.Request, m_pInitialInviteTransaction.Flow);
		sIP_Response.ContentType = "application/sdp";
		sIP_Response.Data = sdp.ToByte();
		m_pInitialInviteTransaction.SendResponse(sIP_Response);
		SetState(SIP_UA_CallState.Active);
		m_pDialog = m_pUA.Stack.TransactionLayer.GetOrCreateDialog(m_pInitialInviteTransaction, sIP_Response);
		m_pDialog.StateChanged += m_pDialog_StateChanged;
	}

	public void Reject(string statusCode_reason)
	{
		lock (m_pLock)
		{
			if (State == SIP_UA_CallState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (State != SIP_UA_CallState.WaitingToAccept)
			{
				throw new InvalidOperationException("Call is not in valid state.");
			}
			if (statusCode_reason == null)
			{
				throw new ArgumentNullException("statusCode_reason");
			}
			m_pInitialInviteTransaction.SendResponse(m_pUA.Stack.CreateResponse(statusCode_reason, m_pInitialInviteTransaction.Request));
			SetState(SIP_UA_CallState.Terminated);
		}
	}

	public void Redirect(SIP_t_ContactParam[] contacts)
	{
		lock (m_pLock)
		{
			if (State == SIP_UA_CallState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (State != SIP_UA_CallState.WaitingToAccept)
			{
				throw new InvalidOperationException("Call is not in valid state.");
			}
			if (contacts == null)
			{
				throw new ArgumentNullException("contacts");
			}
			if (contacts.Length == 0)
			{
				throw new ArgumentException("Arguments 'contacts' must contain at least 1 value.");
			}
			throw new NotImplementedException();
		}
	}

	public void Terminate()
	{
		lock (m_pLock)
		{
			if (m_State == SIP_UA_CallState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_State != SIP_UA_CallState.Terminating && m_State != SIP_UA_CallState.Terminated)
			{
				if (m_State == SIP_UA_CallState.WaitingForStart)
				{
					SetState(SIP_UA_CallState.Terminated);
				}
				else if (m_State == SIP_UA_CallState.WaitingToAccept)
				{
					m_pInitialInviteTransaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x487_Request_Terminated, m_pInitialInviteTransaction.Request));
					SetState(SIP_UA_CallState.Terminated);
				}
				else if (m_State == SIP_UA_CallState.Active)
				{
					m_pDialog.Terminate();
					SetState(SIP_UA_CallState.Terminated);
				}
				else if (m_pInitialInviteSender != null)
				{
					SetState(SIP_UA_CallState.Terminating);
					m_pInitialInviteSender.Cancel();
				}
			}
		}
	}

	public void ToggleOnHold()
	{
		throw new NotImplementedException();
	}

	public void Transfer()
	{
		throw new NotImplementedException();
	}

	private void SetState(SIP_UA_CallState state)
	{
		m_State = state;
		OnStateChanged(state);
	}

	private void OnStateChanged(SIP_UA_CallState state)
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs());
		}
	}

	private void Error()
	{
	}
}
