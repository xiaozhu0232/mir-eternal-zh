using System;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Dialog_Invite : SIP_Dialog
{
	private SIP_Transaction m_pActiveInvite;

	private bool m_IsTerminatedByRemoteParty;

	private string m_TerminateReason;

	public bool HasPendingInvite
	{
		get
		{
			if (base.State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			SIP_Transaction[] transactions = base.Transactions;
			foreach (SIP_Transaction sIP_Transaction in transactions)
			{
				if (sIP_Transaction.State == SIP_TransactionState.Calling || sIP_Transaction.State == SIP_TransactionState.Proceeding)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsTerminatedByRemoteParty
	{
		get
		{
			if (base.State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsTerminatedByRemoteParty;
		}
	}

	public event EventHandler<SIP_RequestReceivedEventArgs> TerminatedByRemoteParty;

	internal SIP_Dialog_Invite()
	{
	}

	protected internal override void Init(SIP_Stack stack, SIP_Transaction transaction, SIP_Response response)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		base.Init(stack, transaction, response);
		if (transaction is SIP_ServerTransaction)
		{
			if (response.StatusCodeType == SIP_StatusCodeType.Success)
			{
				SetState(SIP_DialogState.Early, raiseEvent: false);
				return;
			}
			if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
			{
				SetState(SIP_DialogState.Early, raiseEvent: false);
				m_pActiveInvite = transaction;
				m_pActiveInvite.StateChanged += delegate
				{
					if (m_pActiveInvite != null && m_pActiveInvite.State == SIP_TransactionState.Terminated)
					{
						m_pActiveInvite = null;
						if (base.State == SIP_DialogState.Early)
						{
							SetState(SIP_DialogState.Confirmed, raiseEvent: true);
							Terminate("ACK was not received for initial INVITE 2xx response.", sendBye: true);
						}
						else if (base.State == SIP_DialogState.Terminating)
						{
							SetState(SIP_DialogState.Confirmed, raiseEvent: false);
							Terminate(m_TerminateReason, sendBye: true);
						}
					}
				};
				return;
			}
			throw new ArgumentException("Argument 'response' has invalid status code, 1xx - 2xx is only allowed.");
		}
		if (response.StatusCodeType == SIP_StatusCodeType.Success)
		{
			SetState(SIP_DialogState.Confirmed, raiseEvent: false);
			return;
		}
		if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
		{
			SetState(SIP_DialogState.Early, raiseEvent: false);
			m_pActiveInvite = transaction;
			m_pActiveInvite.StateChanged += delegate
			{
				if (m_pActiveInvite != null && m_pActiveInvite.State == SIP_TransactionState.Terminated)
				{
					m_pActiveInvite = null;
				}
			};
			((SIP_ClientTransaction)transaction).ResponseReceived += delegate(object s, SIP_ResponseReceivedEventArgs a)
			{
				if (a.Response.StatusCodeType == SIP_StatusCodeType.Success)
				{
					SetState(SIP_DialogState.Confirmed, raiseEvent: true);
				}
			};
			return;
		}
		throw new ArgumentException("Argument 'response' has invalid status code, 1xx - 2xx is only allowed.");
	}

	public override void Dispose()
	{
		lock (base.SyncRoot)
		{
			if (base.State != SIP_DialogState.Disposed)
			{
				m_pActiveInvite = null;
				base.Dispose();
			}
		}
	}

	public void Terminate(string reason, bool sendBye)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (base.State == SIP_DialogState.Terminating || base.State == SIP_DialogState.Terminated)
			{
				return;
			}
			m_TerminateReason = reason;
			if (sendBye)
			{
				if ((base.State == SIP_DialogState.Early && m_pActiveInvite is SIP_ClientTransaction) || base.State == SIP_DialogState.Confirmed)
				{
					SetState(SIP_DialogState.Terminating, raiseEvent: true);
					SIP_Request sIP_Request = CreateRequest("BYE");
					if (!string.IsNullOrEmpty(reason))
					{
						SIP_t_ReasonValue sIP_t_ReasonValue = new SIP_t_ReasonValue();
						sIP_t_ReasonValue.Protocol = "SIP";
						sIP_t_ReasonValue.Text = reason;
						sIP_Request.Reason.Add(sIP_t_ReasonValue.ToStringValue());
					}
					SIP_RequestSender sIP_RequestSender = CreateRequestSender(sIP_Request);
					sIP_RequestSender.Completed += delegate
					{
						SetState(SIP_DialogState.Terminated, raiseEvent: true);
					};
					sIP_RequestSender.Start();
				}
				else if (m_pActiveInvite != null && m_pActiveInvite.FinalResponse == null)
				{
					base.Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pActiveInvite.Request);
					SetState(SIP_DialogState.Terminated, raiseEvent: true);
				}
				else
				{
					SetState(SIP_DialogState.Terminating, raiseEvent: true);
				}
			}
			else
			{
				SetState(SIP_DialogState.Terminated, raiseEvent: true);
			}
		}
	}

	protected internal override bool ProcessRequest(SIP_RequestReceivedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (base.ProcessRequest(e))
		{
			return true;
		}
		if (e.Request.RequestLine.Method == "ACK")
		{
			if (base.State == SIP_DialogState.Early)
			{
				SetState(SIP_DialogState.Confirmed, raiseEvent: true);
			}
			else if (base.State == SIP_DialogState.Terminating)
			{
				SetState(SIP_DialogState.Confirmed, raiseEvent: false);
				Terminate(m_TerminateReason, sendBye: true);
			}
		}
		else
		{
			if (e.Request.RequestLine.Method == "BYE")
			{
				e.ServerTransaction.SendResponse(base.Stack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));
				m_IsTerminatedByRemoteParty = true;
				OnTerminatedByRemoteParty(e);
				SetState(SIP_DialogState.Terminated, raiseEvent: true);
				return true;
			}
			if (e.Request.RequestLine.Method == "INVITE")
			{
				if (HasPendingInvite)
				{
					e.ServerTransaction.SendResponse(base.Stack.CreateResponse(SIP_ResponseCodes.x491_Request_Pending, e.Request));
					return true;
				}
			}
			else if (SIP_Utils.MethodCanEstablishDialog(e.Request.RequestLine.Method))
			{
				e.ServerTransaction.SendResponse(base.Stack.CreateResponse(SIP_ResponseCodes.x603_Decline + " : New dialog usages in dialog not allowed (RFC 5057).", e.Request));
				return true;
			}
		}
		return false;
	}

	private void OnTerminatedByRemoteParty(SIP_RequestReceivedEventArgs bye)
	{
		if (this.TerminatedByRemoteParty != null)
		{
			this.TerminatedByRemoteParty(this, bye);
		}
	}
}
