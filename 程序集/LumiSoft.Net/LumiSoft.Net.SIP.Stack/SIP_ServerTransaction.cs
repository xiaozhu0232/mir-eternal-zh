using System;
using System.Timers;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_ServerTransaction : SIP_Transaction
{
	private TimerEx m_pTimer100;

	private TimerEx m_pTimerG;

	private TimerEx m_pTimerH;

	private TimerEx m_pTimerI;

	private TimerEx m_pTimerJ;

	private TimerEx m_pTimerL;

	public event EventHandler<SIP_ResponseSentEventArgs> ResponseSent;

	public event EventHandler Canceled;

	public SIP_ServerTransaction(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
		: base(stack, flow, request)
	{
		if (base.Stack.Logger != null)
		{
			base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] created.");
		}
		Start();
	}

	public override void Dispose()
	{
		lock (base.SyncRoot)
		{
			if (m_pTimer100 != null)
			{
				m_pTimer100.Dispose();
				m_pTimer100 = null;
			}
			if (m_pTimerG != null)
			{
				m_pTimerG.Dispose();
				m_pTimerG = null;
			}
			if (m_pTimerH != null)
			{
				m_pTimerH.Dispose();
				m_pTimerH = null;
			}
			if (m_pTimerI != null)
			{
				m_pTimerI.Dispose();
				m_pTimerI = null;
			}
			if (m_pTimerJ != null)
			{
				m_pTimerJ.Dispose();
				m_pTimerJ = null;
			}
			if (m_pTimerL != null)
			{
				m_pTimerL.Dispose();
				m_pTimerL = null;
			}
		}
	}

	private void m_pTimer100_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Proceeding && base.Responses.Length == 0)
			{
				SIP_Response sIP_Response = base.Stack.CreateResponse(SIP_ResponseCodes.x100_Trying, base.Request);
				if (base.Request.Timestamp != null)
				{
					sIP_Response.Timestamp = new SIP_t_Timestamp(base.Request.Timestamp.Time, (DateTime.Now - base.CreateTime).Seconds);
				}
				try
				{
					base.Stack.TransportLayer.SendResponse(this, sIP_Response);
				}
				catch (Exception exception)
				{
					OnTransportError(exception);
					SetState(SIP_TransactionState.Terminated);
					return;
				}
			}
			if (m_pTimer100 != null)
			{
				m_pTimer100.Dispose();
				m_pTimer100 = null;
			}
		}
	}

	private void m_pTimerG_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State != SIP_TransactionState.Completed)
			{
				return;
			}
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer G(INVITE response(3xx - 6xx) retransmission) triggered.");
			}
			try
			{
				base.Stack.TransportLayer.SendResponse(this, base.FinalResponse);
				m_pTimerG.Interval *= Math.Min(m_pTimerG.Interval * 2.0, 4000.0);
				m_pTimerG.Enabled = true;
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer G(INVITE response(3xx - 6xx) retransmission) updated, will trigger after " + m_pTimerG.Interval + ".");
				}
			}
			catch (Exception exception)
			{
				OnTransportError(exception);
				SetState(SIP_TransactionState.Terminated);
			}
		}
	}

	private void m_pTimerH_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Completed)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer H(INVITE ACK wait) triggered.");
				}
				OnTransactionError("ACK was never received.");
				SetState(SIP_TransactionState.Terminated);
			}
		}
	}

	private void m_pTimerI_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer I(INVITE ACK retransmission wait) triggered.");
			}
			SetState(SIP_TransactionState.Terminated);
		}
	}

	private void m_pTimerJ_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer I(Non-INVITE request retransmission wait) triggered.");
			}
			SetState(SIP_TransactionState.Terminated);
		}
	}

	private void m_pTimerL_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer L(ACK wait) triggered.");
			}
			SetState(SIP_TransactionState.Terminated);
		}
	}

	private void Start()
	{
		if (base.Method == "INVITE")
		{
			SetState(SIP_TransactionState.Proceeding);
			m_pTimer100 = new TimerEx(200.0, autoReset: false);
			m_pTimer100.Elapsed += m_pTimer100_Elapsed;
			m_pTimer100.Enabled = true;
		}
		else
		{
			SetState(SIP_TransactionState.Trying);
		}
	}

	public void SendResponse(SIP_Response response)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			try
			{
				if (base.Method == "INVITE")
				{
					if (base.State == SIP_TransactionState.Proceeding)
					{
						AddResponse(response);
						if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
						{
							base.Stack.TransportLayer.SendResponse(this, response);
							OnResponseSent(response);
							return;
						}
						if (response.StatusCodeType == SIP_StatusCodeType.Success)
						{
							base.Stack.TransportLayer.SendResponse(this, response);
							OnResponseSent(response);
							SetState(SIP_TransactionState.Accpeted);
							m_pTimerL = new TimerEx(32000.0);
							m_pTimerL.Elapsed += m_pTimerL_Elapsed;
							m_pTimerL.Enabled = true;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer L(ACK wait) started, will trigger after " + m_pTimerL.Interval + ".");
							}
							return;
						}
						base.Stack.TransportLayer.SendResponse(this, response);
						OnResponseSent(response);
						SetState(SIP_TransactionState.Completed);
						if (!base.Flow.IsReliable)
						{
							m_pTimerG = new TimerEx(500.0, autoReset: false);
							m_pTimerG.Elapsed += m_pTimerG_Elapsed;
							m_pTimerG.Enabled = true;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer G(INVITE response(3xx - 6xx) retransmission) started, will trigger after " + m_pTimerG.Interval + ".");
							}
						}
						m_pTimerH = new TimerEx(32000.0);
						m_pTimerH.Elapsed += m_pTimerH_Elapsed;
						m_pTimerH.Enabled = true;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer H(INVITE ACK wait) started, will trigger after " + m_pTimerH.Interval + ".");
						}
					}
					else if (base.State == SIP_TransactionState.Accpeted)
					{
						base.Stack.TransportLayer.SendResponse(this, response);
						OnResponseSent(response);
					}
					else if (base.State != SIP_TransactionState.Completed && base.State != SIP_TransactionState.Confirmed && base.State != SIP_TransactionState.Terminated)
					{
					}
				}
				else if (base.State == SIP_TransactionState.Trying)
				{
					AddResponse(response);
					if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
					{
						base.Stack.TransportLayer.SendResponse(this, response);
						OnResponseSent(response);
						SetState(SIP_TransactionState.Proceeding);
						return;
					}
					base.Stack.TransportLayer.SendResponse(this, response);
					OnResponseSent(response);
					SetState(SIP_TransactionState.Completed);
					m_pTimerJ = new TimerEx(32000.0, autoReset: false);
					m_pTimerJ.Elapsed += m_pTimerJ_Elapsed;
					m_pTimerJ.Enabled = true;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer J(Non-INVITE request retransmission wait) started, will trigger after " + m_pTimerJ.Interval + ".");
					}
				}
				else if (base.State == SIP_TransactionState.Proceeding)
				{
					AddResponse(response);
					if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
					{
						base.Stack.TransportLayer.SendResponse(this, response);
						OnResponseSent(response);
						return;
					}
					base.Stack.TransportLayer.SendResponse(this, response);
					OnResponseSent(response);
					SetState(SIP_TransactionState.Completed);
					m_pTimerJ = new TimerEx(32000.0, autoReset: false);
					m_pTimerJ.Elapsed += m_pTimerJ_Elapsed;
					m_pTimerJ.Enabled = true;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer J(Non-INVITE request retransmission wait) started, will trigger after " + m_pTimerJ.Interval + ".");
					}
				}
				else if (base.State != SIP_TransactionState.Completed)
				{
					_ = base.State;
					_ = 7;
				}
			}
			catch (SIP_TransportException ex)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] transport exception: " + ex.Message);
				}
				OnTransportError(ex);
			}
		}
	}

	public override void Cancel()
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (base.FinalResponse != null)
			{
				throw new InvalidOperationException("Final response is already sent, CANCEL not allowed.");
			}
			try
			{
				SIP_Response response = base.Stack.CreateResponse(SIP_ResponseCodes.x487_Request_Terminated, base.Request);
				base.Stack.TransportLayer.SendResponse(this, response);
				OnCanceled();
			}
			catch (SIP_TransportException ex)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] transport exception: " + ex.Message);
				}
				OnTransportError(ex);
				SetState(SIP_TransactionState.Terminated);
			}
		}
	}

	internal void ProcessRequest(SIP_Flow flow, SIP_Request request)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Disposed)
			{
				return;
			}
			try
			{
				if (base.Stack.Logger != null)
				{
					byte[] array = request.ToByteData();
					base.Stack.Logger.AddRead(Guid.NewGuid().ToString(), null, 0L, "Request [transactionID='" + base.ID + "'; method='" + request.RequestLine.Method + "'; cseq='" + request.CSeq.SequenceNumber + "'; transport='" + flow.Transport + "'; size='" + array.Length + "'; received '" + flow.LocalEP?.ToString() + "' <- '" + flow.RemoteEP?.ToString() + "'.", flow.LocalEP, flow.RemoteEP, array);
				}
				if (base.Method == "INVITE")
				{
					if (request.RequestLine.Method == "INVITE")
					{
						if (base.State == SIP_TransactionState.Proceeding)
						{
							SIP_Response lastProvisionalResponse = base.LastProvisionalResponse;
							if (lastProvisionalResponse != null)
							{
								base.Stack.TransportLayer.SendResponse(this, lastProvisionalResponse);
							}
						}
						else if (base.State == SIP_TransactionState.Completed)
						{
							base.Stack.TransportLayer.SendResponse(this, base.FinalResponse);
						}
					}
					else
					{
						if (!(request.RequestLine.Method == "ACK") || base.State == SIP_TransactionState.Accpeted || base.State != SIP_TransactionState.Completed)
						{
							return;
						}
						SetState(SIP_TransactionState.Confirmed);
						if (m_pTimerG != null)
						{
							m_pTimerG.Dispose();
							m_pTimerG = null;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer G(INVITE response(3xx - 6xx) retransmission) stopped.");
							}
						}
						if (m_pTimerH != null)
						{
							m_pTimerH.Dispose();
							m_pTimerH = null;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer H(INVITE ACK wait) stopped.");
							}
						}
						m_pTimerI = new TimerEx((!flow.IsReliable) ? 5000 : 0, autoReset: false);
						m_pTimerI.Elapsed += m_pTimerI_Elapsed;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer I(INVITE ACK retransission wait) started, will trigger after " + m_pTimerI.Interval + ".");
						}
						m_pTimerI.Enabled = true;
					}
				}
				else if (base.Method == request.RequestLine.Method)
				{
					if (base.State == SIP_TransactionState.Proceeding)
					{
						base.Stack.TransportLayer.SendResponse(this, base.LastProvisionalResponse);
					}
					else if (base.State == SIP_TransactionState.Completed)
					{
						base.Stack.TransportLayer.SendResponse(this, base.FinalResponse);
					}
				}
			}
			catch (SIP_TransportException ex)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] transport exception: " + ex.Message);
				}
				OnTransportError(ex);
			}
		}
	}

	private void OnResponseSent(SIP_Response response)
	{
		if (this.ResponseSent != null)
		{
			this.ResponseSent(this, new SIP_ResponseSentEventArgs(this, response));
		}
	}

	private void OnCanceled()
	{
		if (this.Canceled != null)
		{
			this.Canceled(this, new EventArgs());
		}
	}
}
