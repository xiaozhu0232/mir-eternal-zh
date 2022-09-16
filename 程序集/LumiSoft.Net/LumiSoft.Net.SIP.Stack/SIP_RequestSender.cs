using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_RequestSender : IDisposable
{
	private enum SIP_RequestSenderState
	{
		Initial,
		Starting,
		Started,
		Completed,
		Disposed
	}

	private object m_pLock = new object();

	private SIP_RequestSenderState m_State;

	private bool m_IsStarted;

	private SIP_Stack m_pStack;

	private SIP_Request m_pRequest;

	private List<NetworkCredential> m_pCredentials;

	private Queue<SIP_Hop> m_pHops;

	private SIP_ClientTransaction m_pTransaction;

	private SIP_Flow m_pFlow;

	private object m_pTag;

	public bool IsDisposed => m_State == SIP_RequestSenderState.Disposed;

	public bool IsStarted
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsStarted;
		}
	}

	public bool IsCompleted
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_State == SIP_RequestSenderState.Completed;
		}
	}

	public SIP_Stack Stack
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pStack;
		}
	}

	public SIP_Request Request
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRequest;
		}
	}

	public SIP_Flow Flow
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pFlow;
		}
	}

	public List<NetworkCredential> Credentials
	{
		get
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pCredentials;
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

	public event EventHandler<SIP_ResponseReceivedEventArgs> ResponseReceived;

	public event EventHandler Completed;

	public event EventHandler Disposed;

	internal SIP_RequestSender(SIP_Stack stack, SIP_Request request, SIP_Flow flow)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		m_pStack = stack;
		m_pRequest = request;
		m_pFlow = flow;
		m_pCredentials = new List<NetworkCredential>();
		m_pHops = new Queue<SIP_Hop>();
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (m_State != SIP_RequestSenderState.Disposed)
			{
				m_State = SIP_RequestSenderState.Disposed;
				OnDisposed();
				this.ResponseReceived = null;
				this.Completed = null;
				this.Disposed = null;
				m_pStack = null;
				m_pRequest = null;
				m_pCredentials = null;
				m_pHops = null;
				m_pTransaction = null;
				m_pLock = null;
			}
		}
	}

	private void ClientTransaction_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		lock (m_pLock)
		{
			m_pFlow = e.ClientTransaction.Request.Flow;
			if (e.Response.StatusCode == 401 || e.Response.StatusCode == 407)
			{
				bool flag = false;
				SIP_t_Challenge[] allValues = e.Response.WWWAuthenticate.GetAllValues();
				foreach (SIP_t_Challenge sIP_t_Challenge in allValues)
				{
					SIP_t_Credentials[] allValues2 = m_pTransaction.Request.Authorization.GetAllValues();
					foreach (SIP_t_Credentials sIP_t_Credentials in allValues2)
					{
						if (new Auth_HttpDigest(sIP_t_Challenge.AuthData, "").Realm == new Auth_HttpDigest(sIP_t_Credentials.AuthData, "").Realm)
						{
							flag = true;
							break;
						}
					}
				}
				allValues = e.Response.ProxyAuthenticate.GetAllValues();
				foreach (SIP_t_Challenge sIP_t_Challenge2 in allValues)
				{
					SIP_t_Credentials[] allValues2 = m_pTransaction.Request.ProxyAuthorization.GetAllValues();
					foreach (SIP_t_Credentials sIP_t_Credentials2 in allValues2)
					{
						if (new Auth_HttpDigest(sIP_t_Challenge2.AuthData, "").Realm == new Auth_HttpDigest(sIP_t_Credentials2.AuthData, "").Realm)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					OnResponseReceived(e.Response);
					return;
				}
				SIP_Request sIP_Request = m_pRequest.Copy();
				sIP_Request.CSeq = new SIP_t_CSeq(m_pStack.ConsumeCSeq(), sIP_Request.CSeq.RequestMethod);
				if (Authorize(sIP_Request, e.Response, Credentials.ToArray()))
				{
					SIP_Flow flow = m_pTransaction.Flow;
					CleanUpActiveTransaction();
					SendToFlow(flow, sIP_Request);
				}
				else
				{
					OnResponseReceived(e.Response);
				}
			}
			else
			{
				OnResponseReceived(e.Response);
				if (e.Response.StatusCodeType != 0)
				{
					OnCompleted();
				}
			}
		}
	}

	private void ClientTransaction_TimedOut(object sender, EventArgs e)
	{
		lock (m_pLock)
		{
			if (m_pHops.Count > 0)
			{
				CleanUpActiveTransaction();
				SendToNextHop();
			}
			else
			{
				OnResponseReceived(m_pStack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pRequest));
				OnCompleted();
			}
		}
	}

	private void ClientTransaction_TransportError(object sender, EventArgs e)
	{
		lock (m_pLock)
		{
			if (m_pHops.Count > 0)
			{
				CleanUpActiveTransaction();
				SendToNextHop();
			}
			else
			{
				OnResponseReceived(m_pStack.CreateResponse(SIP_ResponseCodes.x503_Service_Unavailable + ": Transport error.", m_pRequest));
				OnCompleted();
			}
		}
	}

	public void Start()
	{
		lock (m_pLock)
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_IsStarted)
			{
				throw new InvalidOperationException("Start method has been already called.");
			}
			m_IsStarted = true;
			m_State = SIP_RequestSenderState.Starting;
			ThreadPool.QueueUserWorkItem(delegate
			{
				lock (m_pLock)
				{
					if (m_State != SIP_RequestSenderState.Disposed)
					{
						SIP_Uri sIP_Uri = null;
						sIP_Uri = (false ? ((SIP_Uri)m_pRequest.RequestLine.Uri) : ((m_pRequest.Route.GetTopMostValue() == null) ? ((SIP_Uri)m_pRequest.RequestLine.Uri) : ((SIP_Uri)m_pRequest.Route.GetTopMostValue().Address.Uri)));
						try
						{
							SIP_Hop[] hops = m_pStack.GetHops(sIP_Uri, m_pRequest.ToByteData().Length, ((SIP_Uri)m_pRequest.RequestLine.Uri).IsSecure);
							foreach (SIP_Hop item in hops)
							{
								m_pHops.Enqueue(item);
							}
						}
						catch (Exception ex)
						{
							OnTransportError(new SIP_TransportException("SIP hops resolving failed '" + ex.Message + "'."));
							OnCompleted();
							return;
						}
						if (m_pHops.Count == 0)
						{
							OnTransportError(new SIP_TransportException("No target hops resolved for '" + sIP_Uri?.ToString() + "'."));
							OnCompleted();
						}
						else
						{
							m_State = SIP_RequestSenderState.Started;
							try
							{
								if (m_pFlow != null)
								{
									SendToFlow(m_pFlow, m_pRequest.Copy());
									return;
								}
							}
							catch
							{
							}
							SendToNextHop();
						}
					}
				}
			});
		}
	}

	public void Cancel()
	{
		while (m_State == SIP_RequestSenderState.Starting)
		{
			Thread.Sleep(5);
		}
		lock (m_pLock)
		{
			if (m_State == SIP_RequestSenderState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_IsStarted)
			{
				throw new InvalidOperationException("Request sending has not started, nothing to cancel.");
			}
			if (m_State != SIP_RequestSenderState.Started)
			{
				return;
			}
			m_pHops.Clear();
		}
		m_pTransaction.Cancel();
	}

	private bool Authorize(SIP_Request request, SIP_Response response, NetworkCredential[] credentials)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (credentials == null)
		{
			throw new ArgumentNullException("credentials");
		}
		bool result = true;
		SIP_t_Challenge[] allValues = response.WWWAuthenticate.GetAllValues();
		for (int i = 0; i < allValues.Length; i++)
		{
			Auth_HttpDigest auth_HttpDigest = new Auth_HttpDigest(allValues[i].AuthData, request.RequestLine.Method);
			NetworkCredential networkCredential = null;
			NetworkCredential[] array = credentials;
			foreach (NetworkCredential networkCredential2 in array)
			{
				if (string.Equals(networkCredential2.Domain, auth_HttpDigest.Realm, StringComparison.InvariantCultureIgnoreCase))
				{
					networkCredential = networkCredential2;
					break;
				}
			}
			if (networkCredential == null)
			{
				result = false;
				continue;
			}
			auth_HttpDigest.UserName = networkCredential.UserName;
			auth_HttpDigest.Password = networkCredential.Password;
			auth_HttpDigest.CNonce = Auth_HttpDigest.CreateNonce();
			auth_HttpDigest.Uri = request.RequestLine.Uri.ToString();
			request.Authorization.Add(auth_HttpDigest.ToAuthorization());
		}
		allValues = response.ProxyAuthenticate.GetAllValues();
		for (int i = 0; i < allValues.Length; i++)
		{
			Auth_HttpDigest auth_HttpDigest2 = new Auth_HttpDigest(allValues[i].AuthData, request.RequestLine.Method);
			NetworkCredential networkCredential3 = null;
			NetworkCredential[] array = credentials;
			foreach (NetworkCredential networkCredential4 in array)
			{
				if (string.Equals(networkCredential4.Domain, auth_HttpDigest2.Realm, StringComparison.InvariantCultureIgnoreCase))
				{
					networkCredential3 = networkCredential4;
					break;
				}
			}
			if (networkCredential3 == null)
			{
				result = false;
				continue;
			}
			auth_HttpDigest2.UserName = networkCredential3.UserName;
			auth_HttpDigest2.Password = networkCredential3.Password;
			auth_HttpDigest2.CNonce = Auth_HttpDigest.CreateNonce();
			auth_HttpDigest2.Uri = request.RequestLine.Uri.ToString();
			request.ProxyAuthorization.Add(auth_HttpDigest2.ToAuthorization());
		}
		return result;
	}

	private void SendToNextHop()
	{
		if (m_pHops.Count == 0)
		{
			throw new InvalidOperationException("No more hop(s).");
		}
		try
		{
			SIP_Hop sIP_Hop = m_pHops.Dequeue();
			SendToFlow(m_pStack.TransportLayer.GetOrCreateFlow(sIP_Hop.Transport, null, sIP_Hop.EndPoint), m_pRequest.Copy());
		}
		catch (ObjectDisposedException ex)
		{
			if (m_pStack.State != SIP_StackState.Disposed)
			{
				throw ex;
			}
		}
	}

	private void SendToFlow(SIP_Flow flow, SIP_Request request)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		SIP_t_ContactParam topMostValue = request.Contact.GetTopMostValue();
		if (SIP_Utils.MethodCanEstablishDialog(request.RequestLine.Method) && topMostValue == null)
		{
			SIP_Uri sIP_Uri = (SIP_Uri)request.From.Address.Uri;
			request.Contact.Add((flow.IsSecure ? "sips:" : "sip:") + sIP_Uri.User + "@" + flow.LocalPublicEP.ToString());
		}
		else if (topMostValue != null && topMostValue.Address.Uri is SIP_Uri && ((SIP_Uri)topMostValue.Address.Uri).Host == "auto-allocate")
		{
			((SIP_Uri)topMostValue.Address.Uri).Host = flow.LocalPublicEP.ToString();
		}
		m_pTransaction = m_pStack.TransactionLayer.CreateClientTransaction(flow, request, addVia: true);
		m_pTransaction.ResponseReceived += ClientTransaction_ResponseReceived;
		m_pTransaction.TimedOut += ClientTransaction_TimedOut;
		m_pTransaction.TransportError += ClientTransaction_TransportError;
		m_pTransaction.Start();
	}

	private void CleanUpActiveTransaction()
	{
		if (m_pTransaction != null)
		{
			m_pTransaction.ResponseReceived -= ClientTransaction_ResponseReceived;
			m_pTransaction.TimedOut -= ClientTransaction_TimedOut;
			m_pTransaction.TransportError -= ClientTransaction_TransportError;
			m_pTransaction = null;
		}
	}

	private void OnResponseReceived(SIP_Response response)
	{
		if (this.ResponseReceived != null)
		{
			this.ResponseReceived(this, new SIP_ResponseReceivedEventArgs(m_pStack, m_pTransaction, response));
		}
	}

	private void OnTransportError(Exception exception)
	{
	}

	private void OnCompleted()
	{
		m_State = SIP_RequestSenderState.Completed;
		if (this.Completed != null)
		{
			this.Completed(this, new EventArgs());
		}
	}

	private void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}
}
