using System;
using System.Collections.Generic;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_Proxy : IDisposable
{
	private bool m_IsDisposed;

	private SIP_Stack m_pStack;

	private SIP_ProxyMode m_ProxyMode = SIP_ProxyMode.Registrar | SIP_ProxyMode.Statefull;

	private SIP_ForkingMode m_ForkingMode = SIP_ForkingMode.Parallel;

	private SIP_Registrar m_pRegistrar;

	private SIP_B2BUA m_pB2BUA;

	private string m_Opaque = "";

	internal List<SIP_ProxyContext> m_pProxyContexts;

	private List<SIP_ProxyHandler> m_pHandlers;

	public bool IsDisposed => m_IsDisposed;

	public SIP_Stack Stack
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pStack;
		}
	}

	public SIP_ProxyMode ProxyMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_ProxyMode;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if ((value & SIP_ProxyMode.Statefull) != 0 && (value & SIP_ProxyMode.Stateless) != 0)
			{
				throw new ArgumentException("Proxy can't be at Statefull and Stateless at same time !");
			}
			m_ProxyMode = value;
		}
	}

	public SIP_ForkingMode ForkingMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_ForkingMode;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_ForkingMode = value;
		}
	}

	public SIP_Registrar Registrar
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRegistrar;
		}
	}

	public SIP_B2BUA B2BUA
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pB2BUA;
		}
	}

	public List<SIP_ProxyHandler> Handlers
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pHandlers;
		}
	}

	public event SIP_IsLocalUriEventHandler IsLocalUri;

	public event SIP_AuthenticateEventHandler Authenticate;

	public event SIP_AddressExistsEventHandler AddressExists;

	public SIP_Proxy(SIP_Stack stack)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		m_pStack = stack;
		m_pStack.RequestReceived += m_pStack_RequestReceived;
		m_pStack.ResponseReceived += m_pStack_ResponseReceived;
		m_pRegistrar = new SIP_Registrar(this);
		m_pB2BUA = new SIP_B2BUA(this);
		m_Opaque = Auth_HttpDigest.CreateOpaque();
		m_pProxyContexts = new List<SIP_ProxyContext>();
		m_pHandlers = new List<SIP_ProxyHandler>();
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			if (m_pStack != null)
			{
				m_pStack.Dispose();
				m_pStack = null;
			}
			m_pRegistrar = null;
			m_pB2BUA = null;
			m_pProxyContexts = null;
		}
	}

	private void m_pStack_RequestReceived(object sender, SIP_RequestReceivedEventArgs e)
	{
		OnRequestReceived(e);
	}

	private void m_pStack_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		OnResponseReceived(e);
	}

	private void OnRequestReceived(SIP_RequestReceivedEventArgs e)
	{
		SIP_Request request = e.Request;
		try
		{
			if ((m_ProxyMode & SIP_ProxyMode.Statefull) != 0)
			{
				if (e.Request.RequestLine.Method == "CANCEL")
				{
					SIP_ServerTransaction sIP_ServerTransaction = m_pStack.TransactionLayer.MatchCancelToTransaction(e.Request);
					if (sIP_ServerTransaction != null)
					{
						sIP_ServerTransaction.Cancel();
						e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, request));
					}
					else
					{
						ForwardRequest(statefull: false, e, addRecordRoute: true);
					}
				}
				else if (e.Request.RequestLine.Method == "ACK")
				{
					ForwardRequest(statefull: false, e, addRecordRoute: true);
				}
				else
				{
					ForwardRequest(statefull: true, e, addRecordRoute: true);
				}
			}
			else if ((m_ProxyMode & SIP_ProxyMode.B2BUA) != 0)
			{
				m_pB2BUA.OnRequestReceived(e);
			}
			else if ((m_ProxyMode & SIP_ProxyMode.Stateless) != 0)
			{
				ForwardRequest(statefull: false, e, addRecordRoute: true);
			}
			else
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x501_Not_Implemented, request));
			}
		}
		catch (Exception ex)
		{
			try
			{
				m_pStack.TransportLayer.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": " + ex.Message, e.Request));
			}
			catch
			{
			}
			if (!(ex is SIP_TransportException))
			{
				m_pStack.OnError(ex);
			}
		}
	}

	private void OnResponseReceived(SIP_ResponseReceivedEventArgs e)
	{
		if ((m_ProxyMode & SIP_ProxyMode.B2BUA) != 0)
		{
			m_pB2BUA.OnResponseReceived(e);
		}
		else if ((m_ProxyMode & SIP_ProxyMode.Statefull) == 0)
		{
			e.Response.Via.RemoveTopMostValue();
			if ((m_ProxyMode & SIP_ProxyMode.Statefull) != 0)
			{
				m_pStack.TransportLayer.SendResponse(e.Response);
			}
			else if ((m_ProxyMode & SIP_ProxyMode.Stateless) != 0)
			{
				m_pStack.TransportLayer.SendResponse(e.Response);
			}
		}
	}

	internal void ForwardRequest(bool statefull, SIP_RequestReceivedEventArgs e, bool addRecordRoute)
	{
		SIP_RequestContext sIP_RequestContext = new SIP_RequestContext(this, e.Request, e.Flow);
		SIP_Request request = e.Request;
		SIP_Uri sIP_Uri = null;
		if (request.MaxForwards <= 0)
		{
			e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x483_Too_Many_Hops, request));
			return;
		}
		if (!SIP_Utils.IsSipOrSipsUri(request.RequestLine.Uri.ToString()) || !OnIsLocalUri(((SIP_Uri)request.RequestLine.Uri).Host))
		{
			bool flag = false;
			if (request.To.Address.IsSipOrSipsUri)
			{
				SIP_Registration registration = m_pRegistrar.GetRegistration(((SIP_Uri)request.To.Address.Uri).Address);
				if (registration != null && registration.GetBinding(request.RequestLine.Uri) != null)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				string userName = null;
				if (!(request.RequestLine.Method == "ACK") && !AuthenticateRequest(e, out userName))
				{
					return;
				}
				sIP_RequestContext.SetUser(userName);
			}
		}
		if (request.RequestLine.Uri is SIP_Uri && IsRecordRoute((SIP_Uri)request.RequestLine.Uri) && request.Route.GetAllValues().Length != 0)
		{
			request.RequestLine.Uri = request.Route.GetAllValues()[request.Route.GetAllValues().Length - 1].Address.Uri;
			SIP_t_AddressParam[] allValues = request.Route.GetAllValues();
			sIP_Uri = (SIP_Uri)allValues[allValues.Length - 1].Address.Uri;
			request.Route.RemoveLastValue();
		}
		if (request.Route.GetAllValues().Length != 0)
		{
			sIP_Uri = (SIP_Uri)request.Route.GetTopMostValue().Address.Uri;
			if (sIP_Uri.Param_Lr)
			{
				request.Route.RemoveTopMostValue();
			}
			else if (IsLocalRoute(sIP_Uri))
			{
				request.Route.RemoveTopMostValue();
			}
		}
		if (e.Request.RequestLine.Method == "REGISTER")
		{
			SIP_Uri sIP_Uri2 = (SIP_Uri)e.Request.RequestLine.Uri;
			if (OnIsLocalUri(sIP_Uri2.Host))
			{
				if ((m_ProxyMode & SIP_ProxyMode.Registrar) != 0)
				{
					m_pRegistrar.Register(e);
				}
				else
				{
					e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x405_Method_Not_Allowed, e.Request));
				}
				return;
			}
		}
		if (e.Request.RequestLine.Uri is SIP_Uri)
		{
			SIP_Uri sIP_Uri3 = (SIP_Uri)e.Request.RequestLine.Uri;
			if (!OnIsLocalUri(sIP_Uri3.Host))
			{
				SIP_Flow flow = null;
				string text = ((sIP_Uri != null && sIP_Uri.Parameters["flowInfo"] != null) ? sIP_Uri.Parameters["flowInfo"].Value : null);
				if (text != null && request.To.Tag != null)
				{
					string text2 = text.Substring(0, text.IndexOf(':'));
					string flowID = text.Substring(text.IndexOf(':') + 1, text.IndexOf('/') - text.IndexOf(':') - 1);
					string flowID2 = text.Substring(text.IndexOf('/') + 1);
					flow = ((!(text2 == request.To.Tag)) ? m_pStack.TransportLayer.GetFlow(flowID2) : m_pStack.TransportLayer.GetFlow(flowID));
				}
				sIP_RequestContext.Targets.Add(new SIP_ProxyTarget(sIP_Uri3, flow));
			}
			else
			{
				SIP_Registration registration2 = m_pRegistrar.GetRegistration(sIP_Uri3.Address);
				if (registration2 != null)
				{
					SIP_RegistrationBinding[] bindings = registration2.Bindings;
					foreach (SIP_RegistrationBinding sIP_RegistrationBinding in bindings)
					{
						if (sIP_RegistrationBinding.ContactURI is SIP_Uri && sIP_RegistrationBinding.TTL > 0)
						{
							sIP_RequestContext.Targets.Add(new SIP_ProxyTarget((SIP_Uri)sIP_RegistrationBinding.ContactURI, sIP_RegistrationBinding.Flow));
						}
					}
				}
				else if (!OnAddressExists(sIP_Uri3.Address))
				{
					e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x404_Not_Found, e.Request));
					return;
				}
				if (sIP_RequestContext.Targets.Count == 0)
				{
					e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x480_Temporarily_Unavailable, e.Request));
					return;
				}
			}
		}
		foreach (SIP_ProxyHandler handler in Handlers)
		{
			try
			{
				SIP_ProxyHandler sIP_ProxyHandler = handler;
				if (!handler.IsReusable)
				{
					sIP_ProxyHandler = (SIP_ProxyHandler)Activator.CreateInstance(handler.GetType());
				}
				if (sIP_ProxyHandler.ProcessRequest(sIP_RequestContext))
				{
					return;
				}
			}
			catch (Exception x)
			{
				m_pStack.OnError(x);
			}
		}
		if (sIP_RequestContext.Targets.Count == 0 && !SIP_Utils.IsSipOrSipsUri(request.RequestLine.Uri.ToString()))
		{
			e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x416_Unsupported_URI_Scheme, e.Request));
			return;
		}
		if (statefull)
		{
			CreateProxyContext(sIP_RequestContext, e.ServerTransaction, request, addRecordRoute).Start();
			return;
		}
		bool flag2 = false;
		SIP_Hop[] array = null;
		SIP_Request sIP_Request = request.Copy();
		sIP_Request.RequestLine.Uri = sIP_RequestContext.Targets[0].TargetUri;
		sIP_Request.MaxForwards--;
		if (sIP_Request.Route.GetAllValues().Length != 0 && !sIP_Request.Route.GetTopMostValue().Parameters.Contains("lr"))
		{
			sIP_Request.Route.Add(sIP_Request.RequestLine.Uri.ToString());
			sIP_Request.RequestLine.Uri = SIP_Utils.UriToRequestUri(sIP_Request.Route.GetTopMostValue().Address.Uri);
			sIP_Request.Route.RemoveTopMostValue();
			flag2 = true;
		}
		SIP_Uri sIP_Uri4 = null;
		sIP_Uri4 = (flag2 ? ((SIP_Uri)sIP_Request.RequestLine.Uri) : ((sIP_Request.Route.GetTopMostValue() == null) ? ((SIP_Uri)sIP_Request.RequestLine.Uri) : ((SIP_Uri)sIP_Request.Route.GetTopMostValue().Address.Uri)));
		array = m_pStack.GetHops(sIP_Uri4, sIP_Request.ToByteData().Length, ((SIP_Uri)sIP_Request.RequestLine.Uri).IsSecure);
		if (array.Length == 0)
		{
			if (sIP_Request.RequestLine.Method != "ACK")
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x503_Service_Unavailable + ": No hop(s) for target.", sIP_Request));
			}
			return;
		}
		sIP_Request.Via.AddToTop("SIP/2.0/transport-tl-addign sentBy-tl-assign-it;branch=z9hG4bK-" + Net_Utils.ComputeMd5(request.Via.GetTopMostValue().Branch, hex: true));
		sIP_Request.Via.GetTopMostValue().Parameters.Add("flowID", request.Flow.ID);
		try
		{
			try
			{
				if (sIP_RequestContext.Targets[0].Flow != null)
				{
					m_pStack.TransportLayer.SendRequest(sIP_RequestContext.Targets[0].Flow, request);
				}
			}
			catch
			{
				m_pStack.TransportLayer.SendRequest(request, null, array[0]);
			}
		}
		catch (SIP_TransportException ex)
		{
			_ = ex.Message;
			if (sIP_Request.RequestLine.Method != "ACK")
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x503_Service_Unavailable + ": Transport error.", sIP_Request));
			}
		}
	}

	internal bool AuthenticateRequest(SIP_RequestReceivedEventArgs e)
	{
		string userName = null;
		return AuthenticateRequest(e, out userName);
	}

	internal bool AuthenticateRequest(SIP_RequestReceivedEventArgs e, out string userName)
	{
		userName = null;
		SIP_t_Credentials credentials = SIP_Utils.GetCredentials(e.Request, m_pStack.Realm);
		if (credentials == null)
		{
			SIP_Response sIP_Response = m_pStack.CreateResponse(SIP_ResponseCodes.x407_Proxy_Authentication_Required, e.Request);
			sIP_Response.ProxyAuthenticate.Add(new Auth_HttpDigest(m_pStack.Realm, m_pStack.DigestNonceManager.CreateNonce(), m_Opaque).ToChallenge());
			e.ServerTransaction.SendResponse(sIP_Response);
			return false;
		}
		Auth_HttpDigest auth_HttpDigest = new Auth_HttpDigest(credentials.AuthData, e.Request.RequestLine.Method);
		if (auth_HttpDigest.Opaque != m_Opaque)
		{
			SIP_Response sIP_Response2 = m_pStack.CreateResponse(SIP_ResponseCodes.x407_Proxy_Authentication_Required + ": Opaque value won't match !", e.Request);
			sIP_Response2.ProxyAuthenticate.Add(new Auth_HttpDigest(m_pStack.Realm, m_pStack.DigestNonceManager.CreateNonce(), m_Opaque).ToChallenge());
			e.ServerTransaction.SendResponse(sIP_Response2);
			return false;
		}
		if (!m_pStack.DigestNonceManager.NonceExists(auth_HttpDigest.Nonce))
		{
			SIP_Response sIP_Response3 = m_pStack.CreateResponse(SIP_ResponseCodes.x407_Proxy_Authentication_Required + ": Invalid nonce value !", e.Request);
			sIP_Response3.ProxyAuthenticate.Add(new Auth_HttpDigest(m_pStack.Realm, m_pStack.DigestNonceManager.CreateNonce(), m_Opaque).ToChallenge());
			e.ServerTransaction.SendResponse(sIP_Response3);
			return false;
		}
		m_pStack.DigestNonceManager.RemoveNonce(auth_HttpDigest.Nonce);
		if (!OnAuthenticate(auth_HttpDigest).Authenticated)
		{
			SIP_Response sIP_Response4 = m_pStack.CreateResponse(SIP_ResponseCodes.x407_Proxy_Authentication_Required + ": Authentication failed.", e.Request);
			sIP_Response4.ProxyAuthenticate.Add(new Auth_HttpDigest(m_pStack.Realm, m_pStack.DigestNonceManager.CreateNonce(), m_Opaque).ToChallenge());
			e.ServerTransaction.SendResponse(sIP_Response4);
			return false;
		}
		userName = auth_HttpDigest.UserName;
		return true;
	}

	internal bool IsLocalRoute(SIP_Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (uri.User != null)
		{
			return false;
		}
		IPBindInfo[] bindInfo = m_pStack.BindInfo;
		foreach (IPBindInfo iPBindInfo in bindInfo)
		{
			if (uri.Host.ToLower() == iPBindInfo.HostName.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRecordRoute(SIP_Uri route)
	{
		if (route == null)
		{
			throw new ArgumentNullException("route");
		}
		IPBindInfo[] bindInfo = m_pStack.BindInfo;
		foreach (IPBindInfo iPBindInfo in bindInfo)
		{
			if (route.Host.ToLower() == iPBindInfo.HostName.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	internal SIP_ProxyContext CreateProxyContext(SIP_RequestContext requestContext, SIP_ServerTransaction transaction, SIP_Request request, bool addRecordRoute)
	{
		SIP_ProxyContext sIP_ProxyContext = new SIP_ProxyContext(this, transaction, request, addRecordRoute, m_ForkingMode, (ProxyMode & SIP_ProxyMode.B2BUA) != 0, noCancel: false, noRecurse: false, requestContext.Targets.ToArray());
		m_pProxyContexts.Add(sIP_ProxyContext);
		return sIP_ProxyContext;
	}

	internal bool OnIsLocalUri(string uri)
	{
		if (this.IsLocalUri != null)
		{
			return this.IsLocalUri(uri);
		}
		return true;
	}

	internal SIP_AuthenticateEventArgs OnAuthenticate(Auth_HttpDigest auth)
	{
		SIP_AuthenticateEventArgs sIP_AuthenticateEventArgs = new SIP_AuthenticateEventArgs(auth);
		if (this.Authenticate != null)
		{
			this.Authenticate(sIP_AuthenticateEventArgs);
		}
		return sIP_AuthenticateEventArgs;
	}

	internal bool OnAddressExists(string address)
	{
		if (this.AddressExists != null)
		{
			return this.AddressExists(address);
		}
		return false;
	}
}
