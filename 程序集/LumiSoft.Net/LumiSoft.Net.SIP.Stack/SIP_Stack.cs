using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.DNS;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.Log;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Stack
{
	private SIP_StackState m_State = SIP_StackState.Stopped;

	private SIP_TransportLayer m_pTransportLayer;

	private SIP_TransactionLayer m_pTransactionLayer;

	private string m_UserAgent;

	private Auth_HttpDigest_NonceManager m_pNonceManager;

	private List<SIP_Uri> m_pProxyServers;

	private string m_Realm = "";

	private int m_CSeq = 1;

	private int m_MaxForwards = 70;

	private int m_MinExpireTime = 1800;

	private List<string> m_pAllow;

	private List<string> m_pSupported;

	private int m_MaximumConnections;

	private int m_MaximumMessageSize = 1000000;

	private int m_MinSessionExpires = 90;

	private int m_SessionExpires = 1800;

	private List<NetworkCredential> m_pCredentials;

	private List<SIP_UA_Registration> m_pRegistrations;

	private SIP_t_CallID m_RegisterCallID;

	private Logger m_pLogger;

	private Dns_Client m_pDnsClient;

	private int MTU = 1400;

	public SIP_StackState State => m_State;

	public SIP_TransportLayer TransportLayer
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pTransportLayer;
		}
	}

	public SIP_TransactionLayer TransactionLayer
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pTransactionLayer;
		}
	}

	public string UserAgent
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_UserAgent;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_UserAgent = value;
		}
	}

	public Auth_HttpDigest_NonceManager DigestNonceManager
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pNonceManager;
		}
	}

	public string StunServer
	{
		get
		{
			return m_pTransportLayer.StunServer;
		}
		set
		{
			m_pTransportLayer.StunServer = value;
		}
	}

	public List<SIP_Uri> ProxyServers
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pProxyServers;
		}
	}

	public string Realm
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Realm;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_Realm = value;
		}
	}

	public int MaxForwards
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaxForwards;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 1)
			{
				throw new ArgumentException("Value must be > 0.");
			}
			m_MaxForwards = value;
		}
	}

	public int MinimumExpireTime
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MinExpireTime;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 10)
			{
				throw new ArgumentException("Property MinimumExpireTime value must be >= 10 !");
			}
			m_MinExpireTime = value;
		}
	}

	public List<string> Allow
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAllow;
		}
	}

	public List<string> Supported
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSupported;
		}
	}

	public int MaximumConnections
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaximumConnections;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 1)
			{
				m_MaximumConnections = 0;
			}
			else
			{
				m_MaximumConnections = value;
			}
		}
	}

	public int MaximumMessageSize
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MaximumMessageSize;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 1)
			{
				m_MaximumMessageSize = 0;
			}
			else
			{
				m_MaximumMessageSize = value;
			}
		}
	}

	public int MinimumSessionExpries
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MinSessionExpires;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 90)
			{
				throw new ArgumentException("Minimum session expires value must be >= 90 !");
			}
			m_MinSessionExpires = value;
		}
	}

	public int SessionExpries
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SessionExpires;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 90)
			{
				throw new ArgumentException("Session expires value can't be < MinimumSessionExpries value !");
			}
			m_SessionExpires = value;
		}
	}

	public List<NetworkCredential> Credentials
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pCredentials;
		}
	}

	public IPBindInfo[] BindInfo
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pTransportLayer.BindInfo;
		}
		set
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_pTransportLayer.BindInfo = value;
		}
	}

	public Dns_Client Dns
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pDnsClient;
		}
	}

	public Logger Logger
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLogger;
		}
	}

	public SIP_UA_Registration[] Registrations
	{
		get
		{
			if (m_State == SIP_StackState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRegistrations.ToArray();
		}
	}

	public event EventHandler<SIP_ValidateRequestEventArgs> ValidateRequest;

	public event EventHandler<SIP_RequestReceivedEventArgs> RequestReceived;

	public event EventHandler<SIP_ResponseReceivedEventArgs> ResponseReceived;

	public event EventHandler<ExceptionEventArgs> Error;

	public SIP_Stack()
	{
		m_pTransportLayer = new SIP_TransportLayer(this);
		m_pTransactionLayer = new SIP_TransactionLayer(this);
		m_pNonceManager = new Auth_HttpDigest_NonceManager();
		m_pProxyServers = new List<SIP_Uri>();
		m_pRegistrations = new List<SIP_UA_Registration>();
		m_pCredentials = new List<NetworkCredential>();
		m_RegisterCallID = SIP_t_CallID.CreateCallID();
		m_pAllow = new List<string>();
		m_pAllow.AddRange(new string[5] { "INVITE", "ACK", "CANCEL", "BYE", "MESSAGE" });
		m_pSupported = new List<string>();
		m_pLogger = new Logger();
		m_pDnsClient = new Dns_Client();
	}

	public void Dispose()
	{
		if (m_State != SIP_StackState.Disposed)
		{
			Stop();
			m_State = SIP_StackState.Disposed;
			this.RequestReceived = null;
			this.ResponseReceived = null;
			this.Error = null;
			if (m_pTransactionLayer != null)
			{
				m_pTransactionLayer.Dispose();
			}
			if (m_pTransportLayer != null)
			{
				m_pTransportLayer.Dispose();
			}
			if (m_pNonceManager != null)
			{
				m_pNonceManager.Dispose();
			}
			if (m_pLogger != null)
			{
				m_pLogger.Dispose();
			}
		}
	}

	public void Start()
	{
		if (m_State != 0)
		{
			m_State = SIP_StackState.Started;
			m_pTransportLayer.Start();
		}
	}

	public void Stop()
	{
		if (m_State != 0)
		{
			return;
		}
		m_State = SIP_StackState.Stopping;
		SIP_UA_Registration[] array = m_pRegistrations.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].BeginUnregister(dispose: true);
		}
		SIP_Dialog[] dialogs = m_pTransactionLayer.Dialogs;
		for (int i = 0; i < dialogs.Length; i++)
		{
			dialogs[i].Terminate();
		}
		DateTime now = DateTime.Now;
		SIP_Transaction[] transactions;
		do
		{
			bool flag = false;
			transactions = m_pTransactionLayer.Transactions;
			foreach (SIP_Transaction sIP_Transaction in transactions)
			{
				if (sIP_Transaction.State == SIP_TransactionState.WaitingToStart || sIP_Transaction.State == SIP_TransactionState.Calling || sIP_Transaction.State == SIP_TransactionState.Proceeding || sIP_Transaction.State == SIP_TransactionState.Trying)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			Thread.Sleep(500);
		}
		while ((DateTime.Now - now).Seconds <= 10);
		transactions = m_pTransactionLayer.Transactions;
		foreach (SIP_Transaction sIP_Transaction2 in transactions)
		{
			try
			{
				sIP_Transaction2.Dispose();
			}
			catch
			{
			}
		}
		m_pTransportLayer.Stop();
		m_State = SIP_StackState.Stopped;
	}

	public SIP_Request CreateRequest(string method, SIP_t_NameAddress to, SIP_t_NameAddress from)
	{
		if (m_State == SIP_StackState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (method == "")
		{
			throw new ArgumentException("Argument 'method' value must be specified.");
		}
		if (to == null)
		{
			throw new ArgumentNullException("to");
		}
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		method = method.ToUpper();
		SIP_Request sIP_Request = new SIP_Request(method);
		sIP_Request.RequestLine.Uri = to.Uri;
		SIP_t_To sIP_t_To2 = (sIP_Request.To = new SIP_t_To(to));
		SIP_t_From sIP_t_From = new SIP_t_From(from);
		sIP_t_From.Tag = SIP_Utils.CreateTag();
		sIP_Request.From = sIP_t_From;
		if (method == "REGISTER")
		{
			sIP_Request.CallID = m_RegisterCallID.ToStringValue();
		}
		else
		{
			sIP_Request.CallID = SIP_t_CallID.CreateCallID().ToStringValue();
		}
		sIP_Request.CSeq = new SIP_t_CSeq(ConsumeCSeq(), method);
		sIP_Request.MaxForwards = m_MaxForwards;
		sIP_Request.Allow.Add(SIP_Utils.ListToString(m_pAllow));
		if (m_pSupported.Count > 0)
		{
			sIP_Request.Supported.Add(SIP_Utils.ListToString(m_pAllow));
		}
		foreach (SIP_Uri pProxyServer in m_pProxyServers)
		{
			sIP_Request.Route.Add(pProxyServer.ToString());
		}
		if (!string.IsNullOrEmpty(m_UserAgent))
		{
			sIP_Request.UserAgent = m_UserAgent;
		}
		return sIP_Request;
	}

	public SIP_RequestSender CreateRequestSender(SIP_Request request)
	{
		return CreateRequestSender(request, null);
	}

	internal SIP_RequestSender CreateRequestSender(SIP_Request request, SIP_Flow flow)
	{
		if (m_State == SIP_StackState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		SIP_RequestSender sIP_RequestSender = new SIP_RequestSender(this, request, flow);
		sIP_RequestSender.Credentials.AddRange(m_pCredentials);
		return sIP_RequestSender;
	}

	public int ConsumeCSeq()
	{
		return m_CSeq++;
	}

	public SIP_Response CreateResponse(string statusCode_reasonText, SIP_Request request)
	{
		return CreateResponse(statusCode_reasonText, request, null);
	}

	public SIP_Response CreateResponse(string statusCode_reasonText, SIP_Request request, SIP_Flow flow)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (request.RequestLine.Method == "ACK")
		{
			throw new InvalidOperationException("ACK is responseless request !");
		}
		SIP_Response sIP_Response = new SIP_Response(request);
		sIP_Response.StatusCode_ReasonPhrase = statusCode_reasonText;
		SIP_t_ViaParm[] allValues = request.Via.GetAllValues();
		foreach (SIP_t_ViaParm sIP_t_ViaParm in allValues)
		{
			sIP_Response.Via.Add(sIP_t_ViaParm.ToStringValue());
		}
		sIP_Response.From = request.From;
		sIP_Response.To = request.To;
		if (request.To.Tag == null)
		{
			sIP_Response.To.Tag = SIP_Utils.CreateTag();
		}
		sIP_Response.CallID = request.CallID;
		sIP_Response.CSeq = request.CSeq;
		sIP_Response.Allow.Add(SIP_Utils.ListToString(m_pAllow));
		if (m_pSupported.Count > 0)
		{
			sIP_Response.Supported.Add(SIP_Utils.ListToString(m_pAllow));
		}
		if (!string.IsNullOrEmpty(m_UserAgent))
		{
			request.UserAgent = m_UserAgent;
		}
		if (SIP_Utils.MethodCanEstablishDialog(request.RequestLine.Method))
		{
			SIP_t_AddressParam[] allValues2 = request.RecordRoute.GetAllValues();
			foreach (SIP_t_AddressParam sIP_t_AddressParam in allValues2)
			{
				sIP_Response.RecordRoute.Add(sIP_t_AddressParam.ToStringValue());
			}
			if (sIP_Response.Contact.GetTopMostValue() == null && flow != null)
			{
				string user = ((SIP_Uri)sIP_Response.To.Address.Uri).User;
				sIP_Response.Contact.Add((flow.IsSecure ? "sips:" : "sip:") + user + "@" + flow.LocalPublicEP.ToString());
			}
		}
		return sIP_Response;
	}

	public SIP_Hop[] GetHops(SIP_Uri uri, int messageSize, bool forceTLS)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		List<SIP_Hop> list = new List<SIP_Hop>();
		string text = "";
		bool flag = false;
		List<DNS_rr_SRV> list2 = new List<DNS_rr_SRV>();
		if (forceTLS)
		{
			flag = true;
			text = "TLS";
		}
		else if (uri.Param_Transport != null)
		{
			flag = true;
			text = uri.Param_Transport;
		}
		else if (Net_Utils.IsIPAddress(uri.Host) || uri.Port != -1)
		{
			text = (uri.IsSecure ? "TLS" : ((messageSize <= MTU) ? "UDP" : "TCP"));
		}
		else
		{
			DnsServerResponse dnsServerResponse = null;
			Dictionary<string, DNS_rr_SRV[]> dictionary = new Dictionary<string, DNS_rr_SRV[]>();
			bool flag2 = false;
			dnsServerResponse = m_pDnsClient.Query("_sips._tcp." + uri.Host, DNS_QType.SRV);
			if (dnsServerResponse.GetSRVRecords().Length != 0)
			{
				flag2 = true;
				dictionary.Add("TLS", dnsServerResponse.GetSRVRecords());
			}
			dnsServerResponse = m_pDnsClient.Query("_sip._tcp." + uri.Host, DNS_QType.SRV);
			if (dnsServerResponse.GetSRVRecords().Length != 0)
			{
				flag2 = true;
				dictionary.Add("TCP", dnsServerResponse.GetSRVRecords());
			}
			dnsServerResponse = m_pDnsClient.Query("_sip._udp." + uri.Host, DNS_QType.SRV);
			if (dnsServerResponse.GetSRVRecords().Length != 0)
			{
				flag2 = true;
				dictionary.Add("UDP", dnsServerResponse.GetSRVRecords());
			}
			if (!flag2)
			{
				text = (uri.IsSecure ? "TLS" : ((messageSize <= MTU) ? "UDP" : "TCP"));
			}
			else if (uri.IsSecure)
			{
				if (dictionary.ContainsKey("TLS"))
				{
					text = "TLS";
					list2.AddRange(dictionary["TLS"]);
				}
			}
			else if (messageSize > MTU)
			{
				if (dictionary.ContainsKey("TCP"))
				{
					text = "TCP";
					list2.AddRange(dictionary["TCP"]);
				}
				else if (dictionary.ContainsKey("TLS"))
				{
					text = "TLS";
					list2.AddRange(dictionary["TLS"]);
				}
			}
			else if (dictionary.ContainsKey("UDP"))
			{
				text = "UDP";
				list2.AddRange(dictionary["UDP"]);
			}
			else if (dictionary.ContainsKey("TCP"))
			{
				text = "TCP";
				list2.AddRange(dictionary["TCP"]);
			}
			else
			{
				text = "TLS";
				list2.AddRange(dictionary["TLS"]);
			}
		}
		if (Net_Utils.IsIPAddress(uri.Host))
		{
			if (uri.Port != -1)
			{
				list.Add(new SIP_Hop(IPAddress.Parse(uri.Host), uri.Port, text));
			}
			else if (forceTLS || uri.IsSecure)
			{
				list.Add(new SIP_Hop(IPAddress.Parse(uri.Host), 5061, text));
			}
			else
			{
				list.Add(new SIP_Hop(IPAddress.Parse(uri.Host), 5060, text));
			}
		}
		else if (uri.Port != -1)
		{
			IPAddress[] hostAddresses = m_pDnsClient.GetHostAddresses(uri.Host);
			foreach (IPAddress ip in hostAddresses)
			{
				list.Add(new SIP_Hop(ip, uri.Port, text));
			}
		}
		else
		{
			if (flag)
			{
				DnsServerResponse dnsServerResponse2 = null;
				dnsServerResponse2 = ((text == "TLS") ? m_pDnsClient.Query("_sips._tcp." + uri.Host, DNS_QType.SRV) : ((!(text == "TCP")) ? m_pDnsClient.Query("_sip._udp." + uri.Host, DNS_QType.SRV) : m_pDnsClient.Query("_sip._tcp." + uri.Host, DNS_QType.SRV)));
				list2.AddRange(dnsServerResponse2.GetSRVRecords());
			}
			if (list2.Count > 0)
			{
				foreach (DNS_rr_SRV item in list2)
				{
					if (Net_Utils.IsIPAddress(item.Target))
					{
						list.Add(new SIP_Hop(IPAddress.Parse(item.Target), item.Port, text));
						continue;
					}
					IPAddress[] hostAddresses = m_pDnsClient.GetHostAddresses(item.Target);
					foreach (IPAddress ip2 in hostAddresses)
					{
						list.Add(new SIP_Hop(ip2, item.Port, text));
					}
				}
			}
			else
			{
				int port = 5060;
				if (text == "TLS")
				{
					port = 5061;
				}
				IPAddress[] hostAddresses = m_pDnsClient.GetHostAddresses(uri.Host);
				foreach (IPAddress ip3 in hostAddresses)
				{
					list.Add(new SIP_Hop(ip3, port, text));
				}
			}
		}
		return list.ToArray();
	}

	public SIP_UA_Registration CreateRegistration(SIP_Uri server, string aor, AbsoluteUri contact, int expires)
	{
		if (m_State == SIP_StackState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (aor == null)
		{
			throw new ArgumentNullException("aor");
		}
		if (aor == string.Empty)
		{
			throw new ArgumentException("Argument 'aor' value must be specified.");
		}
		if (contact == null)
		{
			throw new ArgumentNullException("contact");
		}
		lock (m_pRegistrations)
		{
			SIP_UA_Registration registration = new SIP_UA_Registration(this, server, aor, contact, expires);
			registration.Disposed += delegate
			{
				if (m_State != SIP_StackState.Disposed)
				{
					m_pRegistrations.Remove(registration);
				}
			};
			m_pRegistrations.Add(registration);
			return registration;
		}
	}

	internal SIP_ValidateRequestEventArgs OnValidateRequest(SIP_Request request, IPEndPoint remoteEndPoint)
	{
		SIP_ValidateRequestEventArgs sIP_ValidateRequestEventArgs = new SIP_ValidateRequestEventArgs(request, remoteEndPoint);
		if (this.ValidateRequest != null)
		{
			this.ValidateRequest(this, sIP_ValidateRequestEventArgs);
		}
		return sIP_ValidateRequestEventArgs;
	}

	internal void OnRequestReceived(SIP_RequestReceivedEventArgs e)
	{
		if (this.RequestReceived != null)
		{
			this.RequestReceived(this, e);
		}
	}

	internal void OnResponseReceived(SIP_ResponseReceivedEventArgs e)
	{
		if (this.ResponseReceived != null)
		{
			this.ResponseReceived(this, e);
		}
	}

	internal void OnError(Exception x)
	{
		if (this.Error != null)
		{
			this.Error(this, new ExceptionEventArgs(x));
		}
	}
}
