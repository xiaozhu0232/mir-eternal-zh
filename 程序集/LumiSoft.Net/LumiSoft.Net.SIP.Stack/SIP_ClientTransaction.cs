using System;
using System.Threading;
using System.Timers;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_ClientTransaction : SIP_Transaction
{
	private TimerEx m_pTimerA;

	private TimerEx m_pTimerB;

	private TimerEx m_pTimerD;

	private TimerEx m_pTimerE;

	private TimerEx m_pTimerF;

	private TimerEx m_pTimerK;

	private TimerEx m_pTimerM;

	private bool m_IsCanceling;

	private int m_RSeq = -1;

	internal int RSeq
	{
		get
		{
			return m_RSeq;
		}
		set
		{
			m_RSeq = value;
		}
	}

	public event EventHandler<SIP_ResponseReceivedEventArgs> ResponseReceived;

	internal SIP_ClientTransaction(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
		: base(stack, flow, request)
	{
		if (base.Stack.Logger != null)
		{
			base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] created.");
		}
		SetState(SIP_TransactionState.WaitingToStart);
	}

	public override void Dispose()
	{
		lock (base.SyncRoot)
		{
			if (!base.IsDisposed)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] disposed.");
				}
				if (m_pTimerA != null)
				{
					m_pTimerA.Dispose();
					m_pTimerA = null;
				}
				if (m_pTimerB != null)
				{
					m_pTimerB.Dispose();
					m_pTimerB = null;
				}
				if (m_pTimerD != null)
				{
					m_pTimerD.Dispose();
					m_pTimerD = null;
				}
				if (m_pTimerE != null)
				{
					m_pTimerE.Dispose();
					m_pTimerE = null;
				}
				if (m_pTimerF != null)
				{
					m_pTimerF.Dispose();
					m_pTimerF = null;
				}
				if (m_pTimerK != null)
				{
					m_pTimerK.Dispose();
					m_pTimerK = null;
				}
				if (m_pTimerM != null)
				{
					m_pTimerM.Dispose();
					m_pTimerM = null;
				}
				this.ResponseReceived = null;
				base.Dispose();
			}
		}
	}

	private void m_pTimerA_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Calling)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer A(INVITE request retransmission) triggered.");
				}
				try
				{
					base.Stack.TransportLayer.SendRequest(base.Flow, base.Request, this);
				}
				catch (Exception exception)
				{
					OnTransportError(exception);
					SetState(SIP_TransactionState.Terminated);
					return;
				}
				m_pTimerA.Interval *= 2.0;
				m_pTimerA.Enabled = true;
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer A(INVITE request retransmission) updated, will trigger after " + m_pTimerA.Interval + ".");
				}
			}
		}
	}

	private void m_pTimerB_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Calling)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer B(INVITE calling state timeout) triggered.");
				}
				OnTimedOut();
				SetState(SIP_TransactionState.Terminated);
				if (m_pTimerA != null)
				{
					m_pTimerA.Dispose();
					m_pTimerA = null;
				}
				if (m_pTimerB != null)
				{
					m_pTimerB.Dispose();
					m_pTimerB = null;
				}
			}
		}
	}

	private void m_pTimerD_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Completed)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) triggered.");
				}
				SetState(SIP_TransactionState.Terminated);
			}
		}
	}

	private void m_pTimerE_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Trying)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer E(-NonINVITE request retransmission) triggered.");
				}
				try
				{
					base.Stack.TransportLayer.SendRequest(base.Flow, base.Request, this);
				}
				catch (Exception exception)
				{
					OnTransportError(exception);
					SetState(SIP_TransactionState.Terminated);
					return;
				}
				m_pTimerE.Interval = Math.Min(m_pTimerE.Interval * 2.0, 4000.0);
				m_pTimerE.Enabled = true;
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer E(Non-INVITE request retransmission) updated, will trigger after " + m_pTimerE.Interval + ".");
				}
			}
		}
	}

	private void m_pTimerF_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State != SIP_TransactionState.Trying && base.State != SIP_TransactionState.Proceeding)
			{
				return;
			}
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) triggered.");
			}
			OnTimedOut();
			if (base.State != SIP_TransactionState.Disposed)
			{
				SetState(SIP_TransactionState.Terminated);
				if (m_pTimerE != null)
				{
					m_pTimerE.Dispose();
					m_pTimerE = null;
				}
				if (m_pTimerF != null)
				{
					m_pTimerF.Dispose();
					m_pTimerF = null;
				}
			}
		}
	}

	private void m_pTimerK_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Completed)
			{
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) triggered.");
				}
				SetState(SIP_TransactionState.Terminated);
			}
		}
	}

	private void m_pTimerM_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			if (base.Stack.Logger != null)
			{
				base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer M(2xx response retransmission wait) triggered.");
			}
			SetState(SIP_TransactionState.Terminated);
		}
	}

	public void Start()
	{
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (base.State != 0)
			{
				throw new InvalidOperationException("Start method is valid only in 'WaitingToStart' state.");
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				lock (base.SyncRoot)
				{
					if (base.Method == "INVITE")
					{
						SetState(SIP_TransactionState.Calling);
						try
						{
							base.Stack.TransportLayer.SendRequest(base.Flow, base.Request, this);
						}
						catch (Exception exception)
						{
							OnTransportError(exception);
							if (base.State != SIP_TransactionState.Disposed)
							{
								SetState(SIP_TransactionState.Terminated);
							}
							return;
						}
						if (!base.Flow.IsReliable)
						{
							m_pTimerA = new TimerEx(500.0, autoReset: false);
							m_pTimerA.Elapsed += m_pTimerA_Elapsed;
							m_pTimerA.Enabled = true;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer A(INVITE request retransmission) started, will trigger after " + m_pTimerA.Interval + ".");
							}
						}
						m_pTimerB = new TimerEx(32000.0, autoReset: false);
						m_pTimerB.Elapsed += m_pTimerB_Elapsed;
						m_pTimerB.Enabled = true;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer B(INVITE calling state timeout) started, will trigger after " + m_pTimerB.Interval + ".");
						}
					}
					else
					{
						SetState(SIP_TransactionState.Trying);
						m_pTimerF = new TimerEx(32000.0, autoReset: false);
						m_pTimerF.Elapsed += m_pTimerF_Elapsed;
						m_pTimerF.Enabled = true;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) started, will trigger after " + m_pTimerF.Interval + ".");
						}
						try
						{
							base.Stack.TransportLayer.SendRequest(base.Flow, base.Request, this);
						}
						catch (Exception exception2)
						{
							OnTransportError(exception2);
							if (base.State != SIP_TransactionState.Disposed)
							{
								SetState(SIP_TransactionState.Terminated);
							}
							return;
						}
						if (!base.Flow.IsReliable)
						{
							m_pTimerE = new TimerEx(500.0, autoReset: false);
							m_pTimerE.Elapsed += m_pTimerE_Elapsed;
							m_pTimerE.Enabled = true;
							if (base.Stack.Logger != null)
							{
								base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer E(Non-INVITE request retransmission) started, will trigger after " + m_pTimerE.Interval + ".");
							}
						}
					}
				}
			});
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
			if (base.State == SIP_TransactionState.WaitingToStart)
			{
				SetState(SIP_TransactionState.Terminated);
			}
			else if (!m_IsCanceling && base.State != SIP_TransactionState.Terminated && base.FinalResponse == null)
			{
				m_IsCanceling = true;
				if (base.Responses.Length != 0)
				{
					SendCancel();
				}
			}
		}
	}

	internal void ProcessResponse(SIP_Flow flow, SIP_Response response)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		lock (base.SyncRoot)
		{
			if (base.State == SIP_TransactionState.Disposed)
			{
				return;
			}
			if (m_IsCanceling && response.StatusCodeType == SIP_StatusCodeType.Provisional)
			{
				SendCancel();
				return;
			}
			if (base.Stack.Logger != null)
			{
				byte[] array = response.ToByteData();
				base.Stack.Logger.AddRead(Guid.NewGuid().ToString(), null, 0L, "Response [transactionID='" + base.ID + "'; method='" + response.CSeq.RequestMethod + "'; cseq='" + response.CSeq.SequenceNumber + "'; transport='" + flow.Transport + "'; size='" + array.Length + "'; statusCode='" + response.StatusCode + "'; reason='" + response.ReasonPhrase + "'; received '" + flow.LocalEP?.ToString() + "' <- '" + flow.RemoteEP?.ToString() + "'.", flow.LocalEP, flow.RemoteEP, array);
			}
			if (base.Method == "INVITE")
			{
				if (base.State == SIP_TransactionState.Calling)
				{
					AddResponse(response);
					if (m_pTimerA != null)
					{
						m_pTimerA.Dispose();
						m_pTimerA = null;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer A(INVITE request retransmission) stopped.");
						}
					}
					if (m_pTimerB != null)
					{
						m_pTimerB.Dispose();
						m_pTimerB = null;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer B(INVITE calling state timeout) stopped.");
						}
					}
					if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
					{
						OnResponseReceived(response);
						SetState(SIP_TransactionState.Proceeding);
						return;
					}
					if (response.StatusCodeType == SIP_StatusCodeType.Success)
					{
						OnResponseReceived(response);
						SetState(SIP_TransactionState.Accpeted);
						m_pTimerM = new TimerEx(32000.0);
						m_pTimerM.Elapsed += m_pTimerM_Elapsed;
						m_pTimerM.Enabled = true;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer M(2xx retransmission wait) started, will trigger after " + m_pTimerM.Interval + ".");
						}
						return;
					}
					SendAck(response);
					OnResponseReceived(response);
					SetState(SIP_TransactionState.Completed);
					m_pTimerD = new TimerEx((!base.Flow.IsReliable) ? 32000 : 0, autoReset: false);
					m_pTimerD.Elapsed += m_pTimerD_Elapsed;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) started, will trigger after " + m_pTimerD.Interval + ".");
					}
					m_pTimerD.Enabled = true;
				}
				else if (base.State == SIP_TransactionState.Proceeding)
				{
					AddResponse(response);
					if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
					{
						OnResponseReceived(response);
						return;
					}
					if (response.StatusCodeType == SIP_StatusCodeType.Success)
					{
						OnResponseReceived(response);
						SetState(SIP_TransactionState.Accpeted);
						m_pTimerM = new TimerEx(32000.0);
						m_pTimerM.Elapsed += m_pTimerM_Elapsed;
						m_pTimerM.Enabled = true;
						if (base.Stack.Logger != null)
						{
							base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=true] timer M(2xx retransmission wait) started, will trigger after " + m_pTimerM.Interval + ".");
						}
						return;
					}
					SendAck(response);
					OnResponseReceived(response);
					SetState(SIP_TransactionState.Completed);
					m_pTimerD = new TimerEx((!base.Flow.IsReliable) ? 32000 : 0, autoReset: false);
					m_pTimerD.Elapsed += m_pTimerD_Elapsed;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) started, will trigger after " + m_pTimerD.Interval + ".");
					}
					m_pTimerD.Enabled = true;
				}
				else if (base.State == SIP_TransactionState.Accpeted)
				{
					if (response.StatusCodeType == SIP_StatusCodeType.Success)
					{
						OnResponseReceived(response);
					}
				}
				else if (base.State == SIP_TransactionState.Completed)
				{
					if (response.StatusCode >= 300)
					{
						SendAck(response);
					}
				}
				else if (base.State != SIP_TransactionState.Terminated)
				{
				}
			}
			else if (base.State == SIP_TransactionState.Trying)
			{
				AddResponse(response);
				if (m_pTimerE != null)
				{
					m_pTimerE.Dispose();
					m_pTimerE = null;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer E(Non-INVITE request retransmission) stopped.");
					}
				}
				if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
				{
					OnResponseReceived(response);
					SetState(SIP_TransactionState.Proceeding);
					return;
				}
				if (m_pTimerF != null)
				{
					m_pTimerF.Dispose();
					m_pTimerF = null;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) stopped.");
					}
				}
				OnResponseReceived(response);
				SetState(SIP_TransactionState.Completed);
				m_pTimerK = new TimerEx(base.Flow.IsReliable ? 1 : 5000, autoReset: false);
				m_pTimerK.Elapsed += m_pTimerK_Elapsed;
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) started, will trigger after " + m_pTimerK.Interval + ".");
				}
				m_pTimerK.Enabled = true;
			}
			else if (base.State == SIP_TransactionState.Proceeding)
			{
				AddResponse(response);
				if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
				{
					OnResponseReceived(response);
					return;
				}
				if (m_pTimerF != null)
				{
					m_pTimerF.Dispose();
					m_pTimerF = null;
					if (base.Stack.Logger != null)
					{
						base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) stopped.");
					}
				}
				OnResponseReceived(response);
				SetState(SIP_TransactionState.Completed);
				m_pTimerK = new TimerEx((!base.Flow.IsReliable) ? 5000 : 0, autoReset: false);
				m_pTimerK.Elapsed += m_pTimerK_Elapsed;
				if (base.Stack.Logger != null)
				{
					base.Stack.Logger.AddText(base.ID, "Transaction [branch='" + base.ID + "';method='" + base.Method + "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) started, will trigger after " + m_pTimerK.Interval + ".");
				}
				m_pTimerK.Enabled = true;
			}
			else if (base.State != SIP_TransactionState.Completed)
			{
				_ = base.State;
				_ = 7;
			}
		}
	}

	private void SendCancel()
	{
		SIP_Request sIP_Request = new SIP_Request("CANCEL");
		sIP_Request.RequestLine.Uri = base.Request.RequestLine.Uri;
		sIP_Request.Via.Add(base.Request.Via.GetTopMostValue().ToStringValue());
		sIP_Request.CallID = base.Request.CallID;
		sIP_Request.From = base.Request.From;
		sIP_Request.To = base.Request.To;
		sIP_Request.CSeq = new SIP_t_CSeq(base.Request.CSeq.SequenceNumber, "CANCEL");
		SIP_t_AddressParam[] allValues = base.Request.Route.GetAllValues();
		foreach (SIP_t_AddressParam sIP_t_AddressParam in allValues)
		{
			sIP_Request.Route.Add(sIP_t_AddressParam.ToStringValue());
		}
		sIP_Request.MaxForwards = 70;
		base.Stack.TransactionLayer.CreateClientTransaction(base.Flow, sIP_Request, addVia: false).Start();
	}

	private void SendAck(SIP_Response response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("resposne");
		}
		SIP_Request sIP_Request = new SIP_Request("ACK");
		sIP_Request.RequestLine.Uri = base.Request.RequestLine.Uri;
		sIP_Request.Via.AddToTop(base.Request.Via.GetTopMostValue().ToStringValue());
		sIP_Request.CallID = base.Request.CallID;
		sIP_Request.From = base.Request.From;
		sIP_Request.To = response.To;
		sIP_Request.CSeq = new SIP_t_CSeq(base.Request.CSeq.SequenceNumber, "ACK");
		SIP_HeaderField[] array = response.Header.Get("Route:");
		foreach (SIP_HeaderField sIP_HeaderField in array)
		{
			sIP_Request.Header.Add("Route:", sIP_HeaderField.Value);
		}
		sIP_Request.MaxForwards = 70;
		try
		{
			base.Stack.TransportLayer.SendRequest(base.Flow, sIP_Request, this);
		}
		catch (SIP_TransportException exception)
		{
			OnTransportError(exception);
			SetState(SIP_TransactionState.Terminated);
		}
	}

	private void OnResponseReceived(SIP_Response response)
	{
		if (this.ResponseReceived != null)
		{
			this.ResponseReceived(this, new SIP_ResponseReceivedEventArgs(base.Stack, this, response));
		}
	}
}
