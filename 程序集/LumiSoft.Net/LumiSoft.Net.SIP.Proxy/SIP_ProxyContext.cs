using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_ProxyContext : IDisposable
{
	private class TargetHandler : IDisposable
	{
		private object m_pLock = new object();

		private bool m_IsDisposed;

		private bool m_IsStarted;

		private SIP_ProxyContext m_pOwner;

		private SIP_Request m_pRequest;

		private SIP_Flow m_pFlow;

		private SIP_Uri m_pTargetUri;

		private bool m_AddRecordRoute = true;

		private bool m_IsRecursed;

		private Queue<SIP_Hop> m_pHops;

		private SIP_ClientTransaction m_pTransaction;

		private TimerEx m_pTimerC;

		private bool m_HasReceivedResponse;

		private bool m_IsCompleted;

		public bool IsDisposed => m_IsDisposed;

		public bool IsStarted
		{
			get
			{
				if (m_IsDisposed)
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
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_IsCompleted;
			}
		}

		public SIP_Request Request
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_pRequest;
			}
		}

		public SIP_Uri TargetUri
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_pTargetUri;
			}
		}

		public bool IsRecordingRoute => m_AddRecordRoute;

		public bool IsRecursed => m_IsRecursed;

		public bool HasReceivedResponse
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_HasReceivedResponse;
			}
		}

		public TargetHandler(SIP_ProxyContext owner, SIP_Flow flow, SIP_Uri targetUri, bool addRecordRoute, bool isRecursed)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			if (targetUri == null)
			{
				throw new ArgumentNullException("targetUri");
			}
			m_pOwner = owner;
			m_pFlow = flow;
			m_pTargetUri = targetUri;
			m_AddRecordRoute = addRecordRoute;
			m_IsRecursed = isRecursed;
			m_pHops = new Queue<SIP_Hop>();
		}

		public void Dispose()
		{
			lock (m_pLock)
			{
				if (!m_IsDisposed)
				{
					m_IsDisposed = true;
					m_pOwner.TargetHandler_Disposed(this);
					m_pOwner = null;
					m_pRequest = null;
					m_pTargetUri = null;
					m_pHops = null;
					if (m_pTransaction != null)
					{
						m_pTransaction.Dispose();
						m_pTransaction = null;
					}
					if (m_pTimerC != null)
					{
						m_pTimerC.Dispose();
						m_pTimerC = null;
					}
				}
			}
		}

		private void Init()
		{
			bool flag = false;
			m_pRequest = m_pOwner.Request.Copy();
			m_pRequest.RequestLine.Uri = m_pTargetUri;
			m_pRequest.MaxForwards--;
			if (m_pRequest.Route.GetAllValues().Length != 0 && !m_pRequest.Route.GetTopMostValue().Parameters.Contains("lr"))
			{
				m_pRequest.Route.Add(m_pRequest.RequestLine.Uri.ToString());
				m_pRequest.RequestLine.Uri = SIP_Utils.UriToRequestUri(m_pRequest.Route.GetTopMostValue().Address.Uri);
				m_pRequest.Route.RemoveTopMostValue();
				flag = true;
			}
			SIP_Uri sIP_Uri = null;
			sIP_Uri = (flag ? ((SIP_Uri)m_pRequest.RequestLine.Uri) : ((m_pRequest.Route.GetTopMostValue() == null) ? ((SIP_Uri)m_pRequest.RequestLine.Uri) : ((SIP_Uri)m_pRequest.Route.GetTopMostValue().Address.Uri)));
			SIP_Hop[] hops = m_pOwner.Proxy.Stack.GetHops(sIP_Uri, m_pRequest.ToByteData().Length, ((SIP_Uri)m_pRequest.RequestLine.Uri).IsSecure);
			foreach (SIP_Hop item in hops)
			{
				m_pHops.Enqueue(item);
			}
			if (m_pHops.Count > 0 && m_AddRecordRoute && m_pRequest.RequestLine.Method != "ACK")
			{
				string recordRoute = m_pOwner.Proxy.Stack.TransportLayer.GetRecordRoute(m_pHops.Peek().Transport);
				if (recordRoute != null)
				{
					m_pRequest.RecordRoute.Add(recordRoute);
				}
			}
		}

		private void ClientTransaction_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
		{
			lock (m_pLock)
			{
				m_HasReceivedResponse = true;
				if (m_pTimerC != null && e.Response.StatusCode >= 101 && e.Response.StatusCode <= 199)
				{
					m_pTimerC.Interval = 180000.0;
				}
				if (e.Response.StatusCodeType != 0)
				{
					m_IsCompleted = true;
				}
				m_pOwner.ProcessResponse(this, m_pTransaction, e.Response);
			}
		}

		private void ClientTransaction_TimedOut(object sender, EventArgs e)
		{
			lock (m_pLock)
			{
				if (m_pHops.Count > 0)
				{
					CleanUpActiveHop();
					SendToNextHop();
				}
				else
				{
					m_IsCompleted = true;
					m_pOwner.ProcessResponse(this, m_pTransaction, m_pOwner.Proxy.Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pTransaction.Request));
					Dispose();
				}
			}
		}

		private void ClientTransaction_TransportError(object sender, ExceptionEventArgs e)
		{
			lock (m_pLock)
			{
				if (m_pHops.Count > 0)
				{
					CleanUpActiveHop();
					SendToNextHop();
				}
				else
				{
					m_IsCompleted = true;
					m_pOwner.ProcessResponse(this, m_pTransaction, m_pOwner.Proxy.Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pTransaction.Request));
					Dispose();
				}
			}
		}

		private void m_pTransaction_Disposed(object sender, EventArgs e)
		{
			lock (m_pLock)
			{
				if (!m_IsDisposed && HasReceivedResponse)
				{
					Dispose();
				}
			}
		}

		private void m_pTimerC_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (m_pLock)
			{
				if (m_pTransaction.HasProvisionalResponse)
				{
					m_pTransaction.Cancel();
					return;
				}
				m_pOwner.ProcessResponse(this, m_pTransaction, m_pOwner.Proxy.Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pTransaction.Request));
				Dispose();
			}
		}

		public void Start()
		{
			lock (m_pLock)
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_IsStarted)
				{
					throw new InvalidOperationException("Start has already called.");
				}
				m_IsStarted = true;
				Init();
				if (m_pHops.Count == 0)
				{
					m_pOwner.ProcessResponse(this, m_pTransaction, m_pOwner.Proxy.Stack.CreateResponse(SIP_ResponseCodes.x503_Service_Unavailable + ": No hop(s) for target.", m_pTransaction.Request));
					Dispose();
				}
				else if (m_pFlow != null)
				{
					SendToFlow(m_pFlow, m_pRequest.Copy());
				}
				else
				{
					SendToNextHop();
				}
			}
		}

		public void Cancel()
		{
			lock (m_pLock)
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_IsStarted)
				{
					m_pTransaction.Cancel();
				}
				else
				{
					Dispose();
				}
			}
		}

		private void SendToNextHop()
		{
			if (m_pHops.Count == 0)
			{
				throw new InvalidOperationException("No more hop(s).");
			}
			SIP_Hop sIP_Hop = m_pHops.Dequeue();
			SendToFlow(m_pOwner.Proxy.Stack.TransportLayer.GetOrCreateFlow(sIP_Hop.Transport, null, sIP_Hop.EndPoint), m_pRequest.Copy());
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
			if (m_AddRecordRoute && request.From.Tag != null && request.RecordRoute.GetAllValues().Length != 0)
			{
				string value = request.From.Tag + ":" + m_pOwner.ServerTransaction.Flow.ID + "/" + flow.ID;
				((SIP_Uri)request.RecordRoute.GetTopMostValue().Address.Uri).Parameters.Add("flowInfo", value);
			}
			m_pTransaction = m_pOwner.Proxy.Stack.TransactionLayer.CreateClientTransaction(flow, request, addVia: true);
			m_pTransaction.ResponseReceived += ClientTransaction_ResponseReceived;
			m_pTransaction.TimedOut += ClientTransaction_TimedOut;
			m_pTransaction.TransportError += ClientTransaction_TransportError;
			m_pTransaction.Disposed += m_pTransaction_Disposed;
			m_pTransaction.Start();
			if (request.RequestLine.Method == "INVITE")
			{
				m_pTimerC = new TimerEx();
				m_pTimerC.AutoReset = false;
				m_pTimerC.Interval = 180000.0;
				m_pTimerC.Elapsed += m_pTimerC_Elapsed;
			}
		}

		private void CleanUpActiveHop()
		{
			if (m_pTimerC != null)
			{
				m_pTimerC.Dispose();
				m_pTimerC = null;
			}
			if (m_pTransaction != null)
			{
				m_pTransaction.Dispose();
				m_pTransaction = null;
			}
		}
	}

	private bool m_IsDisposed;

	private bool m_IsStarted;

	private SIP_Proxy m_pProxy;

	private SIP_ServerTransaction m_pServerTransaction;

	private SIP_Request m_pRequest;

	private bool m_AddRecordRoute;

	private SIP_ForkingMode m_ForkingMode = SIP_ForkingMode.Parallel;

	private bool m_IsB2BUA = true;

	private bool m_NoCancel;

	private bool m_NoRecurse = true;

	private string m_ID = "";

	private DateTime m_CreateTime;

	private List<TargetHandler> m_pTargetsHandlers;

	private List<SIP_Response> m_pResponses;

	private Queue<TargetHandler> m_pTargets;

	private List<NetworkCredential> m_pCredentials;

	private bool m_IsFinalResponseSent;

	private object m_pLock = new object();

	public bool IsDisposed => m_IsDisposed;

	public SIP_Proxy Proxy
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pProxy;
		}
	}

	public string ID
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_ID;
		}
	}

	public DateTime CreateTime
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_CreateTime;
		}
	}

	public SIP_ForkingMode ForkingMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_ForkingMode;
		}
	}

	public bool NoCancel
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_NoCancel;
		}
	}

	public bool Recurse
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return !m_NoRecurse;
		}
	}

	public SIP_ServerTransaction ServerTransaction
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_pServerTransaction;
		}
	}

	public SIP_Request Request
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SIP_ProxyContext");
			}
			return m_pRequest;
		}
	}

	public SIP_Response[] Responses
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pResponses.ToArray();
		}
	}

	public List<NetworkCredential> Credentials
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pCredentials;
		}
	}

	internal SIP_ProxyContext(SIP_Proxy proxy, SIP_ServerTransaction transaction, SIP_Request request, bool addRecordRoute, SIP_ForkingMode forkingMode, bool isB2BUA, bool noCancel, bool noRecurse, SIP_ProxyTarget[] targets)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (targets == null)
		{
			throw new ArgumentNullException("targets");
		}
		if (targets.Length == 0)
		{
			throw new ArgumentException("Argumnet 'targets' must contain at least 1 value.");
		}
		m_pProxy = proxy;
		m_pServerTransaction = transaction;
		m_pServerTransaction.Canceled += m_pServerTransaction_Canceled;
		m_pServerTransaction.Disposed += m_pServerTransaction_Disposed;
		m_pRequest = request;
		m_AddRecordRoute = addRecordRoute;
		m_ForkingMode = forkingMode;
		m_IsB2BUA = isB2BUA;
		m_NoCancel = noCancel;
		m_NoRecurse = noRecurse;
		m_pTargetsHandlers = new List<TargetHandler>();
		m_pResponses = new List<SIP_Response>();
		m_ID = Guid.NewGuid().ToString();
		m_CreateTime = DateTime.Now;
		m_pTargets = new Queue<TargetHandler>();
		foreach (SIP_ProxyTarget sIP_ProxyTarget in targets)
		{
			m_pTargets.Enqueue(new TargetHandler(this, sIP_ProxyTarget.Flow, sIP_ProxyTarget.TargetUri, m_AddRecordRoute, isRecursed: false));
		}
		m_pCredentials = new List<NetworkCredential>();
		SIP_t_Directive[] allValues = request.RequestDisposition.GetAllValues();
		foreach (SIP_t_Directive sIP_t_Directive in allValues)
		{
			if (sIP_t_Directive.Directive == SIP_t_Directive.DirectiveType.NoFork)
			{
				m_ForkingMode = SIP_ForkingMode.None;
			}
			else if (sIP_t_Directive.Directive == SIP_t_Directive.DirectiveType.Parallel)
			{
				m_ForkingMode = SIP_ForkingMode.Parallel;
			}
			else if (sIP_t_Directive.Directive == SIP_t_Directive.DirectiveType.Sequential)
			{
				m_ForkingMode = SIP_ForkingMode.Sequential;
			}
			else if (sIP_t_Directive.Directive == SIP_t_Directive.DirectiveType.NoCancel)
			{
				m_NoCancel = true;
			}
			else if (sIP_t_Directive.Directive == SIP_t_Directive.DirectiveType.NoRecurse)
			{
				m_NoRecurse = true;
			}
		}
		m_pProxy.Stack.Logger.AddText("ProxyContext(id='" + m_ID + "') created.");
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (!m_IsDisposed)
			{
				m_IsDisposed = true;
				m_pProxy.Stack.Logger.AddText("ProxyContext(id='" + m_ID + "') disposed.");
				m_pProxy.m_pProxyContexts.Remove(this);
				m_pProxy = null;
				m_pServerTransaction = null;
				m_pTargetsHandlers = null;
				m_pResponses = null;
				m_pTargets = null;
			}
		}
	}

	private void m_pServerTransaction_Canceled(object sender, EventArgs e)
	{
		lock (m_pLock)
		{
			CancelAllTargets();
		}
	}

	private void m_pServerTransaction_Disposed(object sender, EventArgs e)
	{
		Dispose();
	}

	private void TargetHandler_Disposed(TargetHandler handler)
	{
		lock (m_pLock)
		{
			m_pTargetsHandlers.Remove(handler);
			if (m_pTargets.Count == 0 && m_pTargetsHandlers.Count == 0)
			{
				Dispose();
			}
		}
	}

	public void Start()
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_IsStarted)
			{
				throw new InvalidOperationException("Start has already called.");
			}
			m_IsStarted = true;
			if (m_ForkingMode == SIP_ForkingMode.None)
			{
				TargetHandler targetHandler = m_pTargets.Dequeue();
				m_pTargetsHandlers.Add(targetHandler);
				targetHandler.Start();
			}
			else if (m_ForkingMode == SIP_ForkingMode.Parallel)
			{
				while (!m_IsDisposed && m_pTargets.Count > 0)
				{
					TargetHandler targetHandler2 = m_pTargets.Dequeue();
					m_pTargetsHandlers.Add(targetHandler2);
					targetHandler2.Start();
				}
			}
			else if (m_ForkingMode == SIP_ForkingMode.Sequential)
			{
				TargetHandler targetHandler3 = m_pTargets.Dequeue();
				m_pTargetsHandlers.Add(targetHandler3);
				targetHandler3.Start();
			}
		}
	}

	public void Cancel()
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_IsStarted)
			{
				throw new InvalidOperationException("Start method is not called, nothing to cancel.");
			}
			m_pServerTransaction.Cancel();
		}
	}

	private void ProcessResponse(TargetHandler handler, SIP_ClientTransaction transaction, SIP_Response response)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		bool flag = false;
		lock (m_pLock)
		{
			if (!m_IsB2BUA)
			{
				response.Via.RemoveTopMostValue();
				if (response.Via.GetAllValues().Length == 0)
				{
					return;
				}
			}
			if (response.StatusCodeType == SIP_StatusCodeType.Redirection && !m_NoRecurse && !handler.IsRecursed)
			{
				SIP_t_ContactParam[] allValues = response.Contact.GetAllValues();
				response.Contact.RemoveAll();
				SIP_t_ContactParam[] array = allValues;
				foreach (SIP_t_ContactParam sIP_t_ContactParam in array)
				{
					if (sIP_t_ContactParam.Address.IsSipOrSipsUri)
					{
						m_pTargets.Enqueue(new TargetHandler(this, null, (SIP_Uri)sIP_t_ContactParam.Address.Uri, m_AddRecordRoute, isRecursed: true));
					}
					else
					{
						response.Contact.Add(sIP_t_ContactParam.ToStringValue());
					}
				}
				if (response.Contact.GetAllValues().Length != 0)
				{
					m_pResponses.Add(response);
				}
				if (m_pTargets.Count > 0)
				{
					if (m_ForkingMode == SIP_ForkingMode.Parallel)
					{
						while (m_pTargets.Count > 0)
						{
							TargetHandler targetHandler = m_pTargets.Dequeue();
							m_pTargetsHandlers.Add(handler);
							targetHandler.Start();
						}
					}
					else
					{
						TargetHandler targetHandler2 = m_pTargets.Dequeue();
						m_pTargetsHandlers.Add(handler);
						targetHandler2.Start();
					}
					return;
				}
			}
			else
			{
				m_pResponses.Add(response);
			}
			if (!m_IsFinalResponseSent)
			{
				if (response.StatusCodeType == SIP_StatusCodeType.Provisional && response.StatusCode != 100)
				{
					flag = true;
				}
				else if (response.StatusCodeType == SIP_StatusCodeType.Success)
				{
					flag = true;
				}
				else if (response.StatusCodeType == SIP_StatusCodeType.GlobalFailure)
				{
					CancelAllTargets();
				}
			}
			else if (response.StatusCodeType == SIP_StatusCodeType.Success && m_pServerTransaction.Request.RequestLine.Method == "INVITE")
			{
				flag = true;
			}
			if (m_ForkingMode == SIP_ForkingMode.Sequential && response.StatusCodeType != 0 && response.StatusCodeType != SIP_StatusCodeType.Success && response.StatusCodeType != SIP_StatusCodeType.GlobalFailure && m_pTargets.Count > 0)
			{
				TargetHandler targetHandler3 = m_pTargets.Dequeue();
				m_pTargetsHandlers.Add(handler);
				targetHandler3.Start();
				return;
			}
			if (!m_IsFinalResponseSent && !flag && m_pTargets.Count == 0)
			{
				bool flag2 = true;
				foreach (TargetHandler pTargetsHandler in m_pTargetsHandlers)
				{
					if (!pTargetsHandler.IsCompleted)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					response = GetBestFinalResponse();
					if (response == null)
					{
						response = Proxy.Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout, m_pServerTransaction.Request);
					}
					flag = true;
				}
			}
			if (!flag)
			{
				return;
			}
			if (response.StatusCode == 401 || response.StatusCode == 407)
			{
				SIP_Response[] array2 = m_pResponses.ToArray();
				foreach (SIP_Response sIP_Response in array2)
				{
					if (response != sIP_Response && (sIP_Response.StatusCode == 401 || sIP_Response.StatusCode == 407))
					{
						SIP_SingleValueHF<SIP_t_Challenge>[] headerFields = sIP_Response.WWWAuthenticate.HeaderFields;
						foreach (SIP_HeaderField sIP_HeaderField in headerFields)
						{
							sIP_Response.WWWAuthenticate.Add(sIP_HeaderField.Value);
						}
						headerFields = sIP_Response.ProxyAuthenticate.HeaderFields;
						foreach (SIP_HeaderField sIP_HeaderField2 in headerFields)
						{
							sIP_Response.ProxyAuthenticate.Add(sIP_HeaderField2.Value);
						}
					}
				}
			}
			SendResponse(transaction, response);
			if (response.StatusCodeType != 0)
			{
				m_IsFinalResponseSent = true;
			}
			if (response.StatusCodeType != 0)
			{
				CancelAllTargets();
			}
		}
	}

	private void SendResponse(SIP_ClientTransaction transaction, SIP_Response response)
	{
		if (m_IsB2BUA)
		{
			SIP_Request request = m_pServerTransaction.Request;
			SIP_Response sIP_Response = response.Copy();
			sIP_Response.Via.RemoveAll();
			sIP_Response.Via.AddToTop(request.Via.GetTopMostValue().ToStringValue());
			sIP_Response.CallID = request.CallID;
			sIP_Response.CSeq = request.CSeq;
			sIP_Response.Contact.RemoveAll();
			sIP_Response.RecordRoute.RemoveAll();
			sIP_Response.Allow.RemoveAll();
			sIP_Response.Supported.RemoveAll();
			if (request.RequestLine.Method != "ACK" && request.RequestLine.Method != "BYE")
			{
				sIP_Response.Allow.Add("INVITE,ACK,OPTIONS,CANCEL,BYE,PRACK");
			}
			if (request.RequestLine.Method != "ACK")
			{
				sIP_Response.Supported.Add("100rel,timer");
			}
			sIP_Response.Require.RemoveAll();
			m_pServerTransaction.SendResponse(sIP_Response);
			if (response.CSeq.RequestMethod.ToUpper() == "INVITE" && response.StatusCodeType == SIP_StatusCodeType.Success)
			{
				m_pProxy.B2BUA.AddCall(m_pServerTransaction.Dialog, transaction.Dialog);
			}
		}
		else
		{
			m_pServerTransaction.SendResponse(response);
		}
	}

	private void CancelAllTargets()
	{
		if (!m_NoCancel)
		{
			m_pTargets.Clear();
			TargetHandler[] array = m_pTargetsHandlers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Cancel();
			}
		}
	}

	private SIP_Response GetBestFinalResponse()
	{
		SIP_Response[] array = m_pResponses.ToArray();
		foreach (SIP_Response sIP_Response in array)
		{
			if (sIP_Response.StatusCodeType == SIP_StatusCodeType.GlobalFailure)
			{
				return sIP_Response;
			}
		}
		array = m_pResponses.ToArray();
		foreach (SIP_Response sIP_Response2 in array)
		{
			if (sIP_Response2.StatusCodeType == SIP_StatusCodeType.Success)
			{
				return sIP_Response2;
			}
		}
		array = m_pResponses.ToArray();
		foreach (SIP_Response sIP_Response3 in array)
		{
			if (sIP_Response3.StatusCodeType == SIP_StatusCodeType.Redirection)
			{
				return sIP_Response3;
			}
		}
		array = m_pResponses.ToArray();
		foreach (SIP_Response sIP_Response4 in array)
		{
			if (sIP_Response4.StatusCodeType == SIP_StatusCodeType.RequestFailure)
			{
				return sIP_Response4;
			}
		}
		array = m_pResponses.ToArray();
		foreach (SIP_Response sIP_Response5 in array)
		{
			if (sIP_Response5.StatusCodeType == SIP_StatusCodeType.ServerFailure)
			{
				return sIP_Response5;
			}
		}
		return null;
	}

	private NetworkCredential GetCredential(string realm)
	{
		if (realm == null)
		{
			throw new ArgumentNullException("realm");
		}
		foreach (NetworkCredential pCredential in m_pCredentials)
		{
			if (pCredential.Domain.ToLower() == realm.ToLower())
			{
				return pCredential;
			}
		}
		return null;
	}
}
