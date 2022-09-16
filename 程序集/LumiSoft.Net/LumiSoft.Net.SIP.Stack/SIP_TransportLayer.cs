using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using LumiSoft.Net.DNS;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.TCP;
using LumiSoft.Net.UDP;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_TransportLayer
{
	private class SIP_FlowManager : IDisposable
	{
		private bool m_IsDisposed;

		private SIP_TransportLayer m_pOwner;

		private Dictionary<string, SIP_Flow> m_pFlows;

		private TimerEx m_pTimeoutTimer;

		private int m_IdelTimeout = 300;

		private object m_pLock = new object();

		public bool IsDisposed => m_IsDisposed;

		public int Count
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_pFlows.Count;
			}
		}

		public SIP_Flow this[string flowID]
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (flowID == null)
				{
					throw new ArgumentNullException("flowID");
				}
				if (m_pFlows.ContainsKey(flowID))
				{
					return m_pFlows[flowID];
				}
				return null;
			}
		}

		public SIP_Flow[] Flows
		{
			get
			{
				lock (m_pLock)
				{
					SIP_Flow[] array = new SIP_Flow[m_pFlows.Count];
					m_pFlows.Values.CopyTo(array, 0);
					return array;
				}
			}
		}

		internal SIP_TransportLayer TransportLayer
		{
			get
			{
				if (m_IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_pOwner;
			}
		}

		internal SIP_FlowManager(SIP_TransportLayer owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			m_pFlows = new Dictionary<string, SIP_Flow>();
			m_pTimeoutTimer = new TimerEx(15000.0);
			m_pTimeoutTimer.AutoReset = true;
			m_pTimeoutTimer.Elapsed += m_pTimeoutTimer_Elapsed;
			m_pTimeoutTimer.Enabled = true;
		}

		public void Dispose()
		{
			lock (m_pLock)
			{
				if (!m_IsDisposed)
				{
					m_IsDisposed = true;
					SIP_Flow[] flows = Flows;
					for (int i = 0; i < flows.Length; i++)
					{
						flows[i].Dispose();
					}
					m_pOwner = null;
					m_pFlows = null;
					m_pTimeoutTimer.Dispose();
					m_pTimeoutTimer = null;
				}
			}
		}

		private void m_pTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (m_pLock)
			{
				if (m_IsDisposed)
				{
					return;
				}
				SIP_Flow[] flows = Flows;
				foreach (SIP_Flow sIP_Flow in flows)
				{
					try
					{
						if (sIP_Flow.LastActivity.AddSeconds(m_IdelTimeout) < DateTime.Now)
						{
							sIP_Flow.Dispose();
						}
					}
					catch (ObjectDisposedException ex)
					{
						_ = ex.Message;
					}
				}
			}
		}

		internal SIP_Flow GetOrCreateFlow(bool isServer, IPEndPoint localEP, IPEndPoint remoteEP, string transport)
		{
			if (localEP == null)
			{
				throw new ArgumentNullException("localEP");
			}
			if (remoteEP == null)
			{
				throw new ArgumentNullException("remoteEP");
			}
			if (transport == null)
			{
				throw new ArgumentNullException("transport");
			}
			string flowID = localEP.ToString() + "-" + remoteEP.ToString() + "-" + transport.ToString();
			lock (m_pLock)
			{
				SIP_Flow value = null;
				if (m_pFlows.TryGetValue(flowID, out value))
				{
					return value;
				}
				value = new SIP_Flow(m_pOwner.Stack, isServer, localEP, remoteEP, transport);
				m_pFlows.Add(value.ID, value);
				value.IsDisposing += delegate
				{
					lock (m_pLock)
					{
						m_pFlows.Remove(flowID);
					}
				};
				value.Start();
				return value;
			}
		}

		public SIP_Flow GetFlow(string flowID)
		{
			if (flowID == null)
			{
				throw new ArgumentNullException("flowID");
			}
			lock (m_pFlows)
			{
				SIP_Flow value = null;
				m_pFlows.TryGetValue(flowID, out value);
				return value;
			}
		}

		internal SIP_Flow CreateFromSession(TCP_ServerSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			string flowID = session.LocalEndPoint.ToString() + "-" + session.RemoteEndPoint.ToString() + "-" + (session.IsSecureConnection ? "TLS" : "TCP");
			lock (m_pLock)
			{
				SIP_Flow sIP_Flow = new SIP_Flow(m_pOwner.Stack, session);
				m_pFlows.Add(flowID, sIP_Flow);
				sIP_Flow.IsDisposing += delegate
				{
					lock (m_pLock)
					{
						m_pFlows.Remove(flowID);
					}
				};
				sIP_Flow.Start();
				return sIP_Flow;
			}
		}
	}

	private bool m_IsDisposed;

	private bool m_IsRunning;

	private SIP_Stack m_pStack;

	private IPBindInfo[] m_pBinds;

	private UDP_Server m_pUdpServer;

	private TCP_Server<TCP_ServerSession> m_pTcpServer;

	private SIP_FlowManager m_pFlowManager;

	private string m_StunServer;

	private CircleCollection<IPAddress> m_pLocalIPv4;

	private CircleCollection<IPAddress> m_pLocalIPv6;

	private Random m_pRandom;

	public bool IsRunning => m_IsRunning;

	public SIP_Stack Stack => m_pStack;

	public IPBindInfo[] BindInfo
	{
		get
		{
			return m_pBinds;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("BindInfo");
			}
			bool flag = false;
			if (m_pBinds.Length != value.Length)
			{
				flag = true;
			}
			else
			{
				for (int i = 0; i < m_pBinds.Length; i++)
				{
					if (!m_pBinds[i].Equals(value[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			m_pBinds = value;
			List<IPEndPoint> list = new List<IPEndPoint>();
			List<IPBindInfo> list2 = new List<IPBindInfo>();
			IPBindInfo[] pBinds = m_pBinds;
			foreach (IPBindInfo iPBindInfo in pBinds)
			{
				if (iPBindInfo.Protocol == BindInfoProtocol.UDP)
				{
					list.Add(new IPEndPoint(iPBindInfo.IP, iPBindInfo.Port));
				}
				else
				{
					list2.Add(iPBindInfo);
				}
			}
			m_pUdpServer.Bindings = list.ToArray();
			m_pTcpServer.Bindings = list2.ToArray();
			IPEndPoint[] localEndPoints = m_pTcpServer.LocalEndPoints;
			foreach (IPEndPoint iPEndPoint in localEndPoints)
			{
				if (iPEndPoint.AddressFamily == AddressFamily.InterNetwork)
				{
					m_pLocalIPv4.Add(iPEndPoint.Address);
				}
				else if (iPEndPoint.AddressFamily == AddressFamily.InterNetwork)
				{
					m_pLocalIPv6.Add(iPEndPoint.Address);
				}
			}
		}
	}

	public SIP_Flow[] Flows => m_pFlowManager.Flows;

	internal UDP_Server UdpServer => m_pUdpServer;

	internal string StunServer
	{
		get
		{
			return m_StunServer;
		}
		set
		{
			m_StunServer = value;
		}
	}

	internal SIP_TransportLayer(SIP_Stack stack)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		m_pStack = stack;
		m_pUdpServer = new UDP_Server();
		m_pUdpServer.PacketReceived += m_pUdpServer_PacketReceived;
		m_pUdpServer.Error += m_pUdpServer_Error;
		m_pTcpServer = new TCP_Server<TCP_ServerSession>();
		m_pTcpServer.SessionCreated += m_pTcpServer_SessionCreated;
		m_pFlowManager = new SIP_FlowManager(this);
		m_pBinds = new IPBindInfo[0];
		m_pRandom = new Random();
		m_pLocalIPv4 = new CircleCollection<IPAddress>();
		m_pLocalIPv6 = new CircleCollection<IPAddress>();
	}

	internal void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			Stop();
			m_IsRunning = false;
			m_pBinds = null;
			m_pRandom = null;
			m_pTcpServer.Dispose();
			m_pTcpServer = null;
			m_pUdpServer.Dispose();
			m_pUdpServer = null;
		}
	}

	private void m_pUdpServer_PacketReceived(object sender, UDP_e_PacketReceived e)
	{
		try
		{
			m_pFlowManager.GetOrCreateFlow(isServer: true, (IPEndPoint)e.Socket.LocalEndPoint, e.RemoteEP, "UDP").OnUdpPacketReceived(e);
		}
		catch (Exception x)
		{
			m_pStack.OnError(x);
		}
	}

	private void m_pUdpServer_Error(object sender, Error_EventArgs e)
	{
		m_pStack.OnError(e.Exception);
	}

	private void m_pTcpServer_SessionCreated(object sender, TCP_ServerSessionEventArgs<TCP_ServerSession> e)
	{
		m_pFlowManager.CreateFromSession(e.Session);
	}

	internal void Start()
	{
		if (!m_IsRunning)
		{
			m_IsRunning = true;
			m_pUdpServer.Start();
			m_pTcpServer.Start();
		}
	}

	internal void Stop()
	{
		if (m_IsRunning)
		{
			m_IsRunning = false;
			m_pUdpServer.Stop();
			m_pTcpServer.Stop();
		}
	}

	internal void OnMessageReceived(SIP_Flow flow, byte[] message)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		try
		{
			if (message.Length == 4)
			{
				if (Stack.Logger != null)
				{
					Stack.Logger.AddRead("", null, 2L, "Flow [id='" + flow.ID + "'] received \"ping\"", flow.LocalEP, flow.RemoteEP);
				}
				flow.SendInternal(new byte[2] { 13, 10 });
				if (Stack.Logger != null)
				{
					Stack.Logger.AddWrite("", null, 2L, "Flow [id='" + flow.ID + "'] sent \"pong\"", flow.LocalEP, flow.RemoteEP);
				}
				return;
			}
			if (message.Length == 2)
			{
				if (Stack.Logger != null)
				{
					Stack.Logger.AddRead("", null, 2L, "Flow [id='" + flow.ID + "'] received \"pong\"", flow.LocalEP, flow.RemoteEP);
				}
				return;
			}
			if (Encoding.UTF8.GetString(message, 0, 3).ToUpper().StartsWith("SIP"))
			{
				SIP_Response sIP_Response = null;
				try
				{
					sIP_Response = SIP_Response.Parse(message);
				}
				catch (Exception ex)
				{
					if (m_pStack.Logger != null)
					{
						m_pStack.Logger.AddText("Skipping message, parse error: " + ex.ToString());
					}
					return;
				}
				try
				{
					sIP_Response.Validate();
				}
				catch (Exception ex2)
				{
					if (m_pStack.Logger != null)
					{
						m_pStack.Logger.AddText("Response validation failed: " + ex2.ToString());
					}
					return;
				}
				SIP_ClientTransaction sIP_ClientTransaction = m_pStack.TransactionLayer.MatchClientTransaction(sIP_Response);
				if (sIP_ClientTransaction != null)
				{
					sIP_ClientTransaction.ProcessResponse(flow, sIP_Response);
					return;
				}
				SIP_Dialog sIP_Dialog = m_pStack.TransactionLayer.MatchDialog(sIP_Response);
				if (sIP_Dialog != null)
				{
					sIP_Dialog.ProcessResponse(sIP_Response);
				}
				else
				{
					m_pStack.OnResponseReceived(new SIP_ResponseReceivedEventArgs(m_pStack, null, sIP_Response));
				}
				return;
			}
			SIP_Request sIP_Request = null;
			try
			{
				sIP_Request = SIP_Request.Parse(message);
			}
			catch (Exception ex3)
			{
				if (m_pStack.Logger != null)
				{
					m_pStack.Logger.AddText("Skipping message, parse error: " + ex3.Message);
				}
				return;
			}
			try
			{
				sIP_Request.Validate();
			}
			catch (Exception ex4)
			{
				if (m_pStack.Logger != null)
				{
					m_pStack.Logger.AddText("Request validation failed: " + ex4.ToString());
				}
				SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ". " + ex4.Message, sIP_Request));
				return;
			}
			SIP_ValidateRequestEventArgs sIP_ValidateRequestEventArgs = m_pStack.OnValidateRequest(sIP_Request, flow.RemoteEP);
			if (sIP_ValidateRequestEventArgs.ResponseCode != null)
			{
				SendResponse(m_pStack.CreateResponse(sIP_ValidateRequestEventArgs.ResponseCode, sIP_Request));
				return;
			}
			sIP_Request.Flow = flow;
			sIP_Request.LocalEndPoint = flow.LocalEP;
			sIP_Request.RemoteEndPoint = flow.RemoteEP;
			SIP_t_ViaParm topMostValue = sIP_Request.Via.GetTopMostValue();
			topMostValue.Received = flow.RemoteEP.Address;
			if (topMostValue.RPort == 0)
			{
				topMostValue.RPort = flow.RemoteEP.Port;
			}
			bool flag = false;
			SIP_ServerTransaction sIP_ServerTransaction = m_pStack.TransactionLayer.MatchServerTransaction(sIP_Request);
			if (sIP_ServerTransaction != null)
			{
				sIP_ServerTransaction.ProcessRequest(flow, sIP_Request);
				flag = true;
			}
			else
			{
				SIP_Dialog sIP_Dialog2 = m_pStack.TransactionLayer.MatchDialog(sIP_Request);
				if (sIP_Dialog2 != null)
				{
					flag = sIP_Dialog2.ProcessRequest(new SIP_RequestReceivedEventArgs(m_pStack, flow, sIP_Request));
				}
			}
			if (!flag)
			{
				if (m_pStack.Logger != null)
				{
					byte[] array = sIP_Request.ToByteData();
					m_pStack.Logger.AddRead(Guid.NewGuid().ToString(), null, 0L, "Request [method='" + sIP_Request.RequestLine.Method + "'; cseq='" + sIP_Request.CSeq.SequenceNumber + "'; transport='" + flow.Transport + "'; size='" + array.Length + "'; received '" + flow.RemoteEP?.ToString() + "' -> '" + flow.LocalEP?.ToString() + "'.", flow.LocalEP, flow.RemoteEP, array);
				}
				m_pStack.OnRequestReceived(new SIP_RequestReceivedEventArgs(m_pStack, flow, sIP_Request));
			}
		}
		catch (SocketException ex5)
		{
			_ = ex5.Message;
		}
		catch (Exception x)
		{
			m_pStack.OnError(x);
		}
	}

	public SIP_Flow GetOrCreateFlow(string transport, IPEndPoint localEP, IPEndPoint remoteEP)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		if (localEP == null)
		{
			if (string.Equals(transport, "UDP", StringComparison.InvariantCultureIgnoreCase))
			{
				localEP = m_pUdpServer.GetLocalEndPoint(remoteEP);
			}
			else if (string.Equals(transport, "TCP", StringComparison.InvariantCultureIgnoreCase))
			{
				localEP = ((remoteEP.AddressFamily != AddressFamily.InterNetwork) ? new IPEndPoint(m_pLocalIPv4.Next(), m_pRandom.Next(10000, 65000)) : new IPEndPoint(m_pLocalIPv4.Next(), m_pRandom.Next(10000, 65000)));
			}
			else
			{
				if (!string.Equals(transport, "TLS", StringComparison.InvariantCultureIgnoreCase))
				{
					throw new ArgumentException("Not supported transoprt '" + transport + "'.");
				}
				localEP = ((remoteEP.AddressFamily != AddressFamily.InterNetwork) ? new IPEndPoint(m_pLocalIPv4.Next(), m_pRandom.Next(10000, 65000)) : new IPEndPoint(m_pLocalIPv4.Next(), m_pRandom.Next(10000, 65000)));
			}
		}
		return m_pFlowManager.GetOrCreateFlow(isServer: false, localEP, remoteEP, transport);
	}

	public SIP_Flow GetFlow(string flowID)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (flowID == null)
		{
			throw new ArgumentNullException("flowID");
		}
		return m_pFlowManager.GetFlow(flowID);
	}

	public void SendRequest(SIP_Request request)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		SIP_Hop[] hops = m_pStack.GetHops((SIP_Uri)request.RequestLine.Uri, request.ToByteData().Length, forceTLS: false);
		if (hops.Length == 0)
		{
			throw new SIP_TransportException("No target hops for URI '" + request.RequestLine.Uri.ToString() + "'.");
		}
		SIP_TransportException ex = null;
		SIP_Hop[] array = hops;
		foreach (SIP_Hop hop in array)
		{
			try
			{
				SendRequest(request, null, hop);
				return;
			}
			catch (SIP_TransportException ex2)
			{
				ex = ex2;
			}
		}
		throw ex;
	}

	public void SendRequest(SIP_Request request, IPEndPoint localEP, SIP_Hop hop)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (hop == null)
		{
			throw new ArgumentNullException("hop");
		}
		SendRequest(GetOrCreateFlow(hop.Transport, localEP, hop.EndPoint), request);
	}

	public void SendRequest(SIP_Flow flow, SIP_Request request)
	{
		SendRequest(flow, request, null);
	}

	internal void SendRequest(SIP_Flow flow, SIP_Request request, SIP_ClientTransaction transaction)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (request.Via.GetTopMostValue() == null)
		{
			throw new ArgumentException("Argument 'request' doesn't contain required Via: header field.");
		}
		SIP_t_ViaParm topMostValue = request.Via.GetTopMostValue();
		topMostValue.ProtocolTransport = flow.Transport;
		HostEndPoint hostEndPoint = null;
		IPBindInfo[] bindInfo = BindInfo;
		foreach (IPBindInfo iPBindInfo in bindInfo)
		{
			if (flow.Transport == "UDP" && iPBindInfo.Protocol == BindInfoProtocol.UDP)
			{
				hostEndPoint = (string.IsNullOrEmpty(iPBindInfo.HostName) ? new HostEndPoint(flow.LocalEP.Address.ToString(), iPBindInfo.Port) : new HostEndPoint(iPBindInfo.HostName, iPBindInfo.Port));
				break;
			}
			if (flow.Transport == "TLS" && iPBindInfo.Protocol == BindInfoProtocol.TCP && iPBindInfo.SslMode == SslMode.SSL)
			{
				hostEndPoint = (string.IsNullOrEmpty(iPBindInfo.HostName) ? new HostEndPoint(flow.LocalEP.Address.ToString(), iPBindInfo.Port) : new HostEndPoint(iPBindInfo.HostName, iPBindInfo.Port));
				break;
			}
			if (flow.Transport == "TCP" && iPBindInfo.Protocol == BindInfoProtocol.TCP)
			{
				hostEndPoint = (string.IsNullOrEmpty(iPBindInfo.HostName) ? new HostEndPoint(flow.LocalEP.Address.ToString(), iPBindInfo.Port) : new HostEndPoint(iPBindInfo.HostName, iPBindInfo.Port));
				break;
			}
		}
		if (hostEndPoint == null)
		{
			topMostValue.SentBy = new HostEndPoint(flow.LocalEP);
		}
		else
		{
			topMostValue.SentBy = hostEndPoint;
		}
		flow.Send(request);
		if (m_pStack.Logger != null)
		{
			byte[] array = request.ToByteData();
			m_pStack.Logger.AddWrite(Guid.NewGuid().ToString(), null, 0L, "Request [" + ((transaction == null) ? "" : ("transactionID='" + transaction.ID + "';")) + "method='" + request.RequestLine.Method + "'; cseq='" + request.CSeq.SequenceNumber + "'; transport='" + flow.Transport + "'; size='" + array.Length + "'; sent '" + flow.LocalEP?.ToString() + "' -> '" + flow.RemoteEP?.ToString() + "'.", flow.LocalEP, flow.RemoteEP, array);
		}
	}

	public void SendResponse(SIP_Response response)
	{
		SendResponse(response, null);
	}

	public void SendResponse(SIP_Response response, IPEndPoint localEP)
	{
		SendResponseInternal(null, response, localEP);
	}

	internal void SendResponse(SIP_ServerTransaction transaction, SIP_Response response)
	{
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		SendResponseInternal(transaction, response, null);
	}

	private void SendResponseInternal(SIP_ServerTransaction transaction, SIP_Response response, IPEndPoint localEP)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsRunning)
		{
			throw new InvalidOperationException("Stack has not been started.");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		SIP_t_ViaParm topMostValue = response.Via.GetTopMostValue();
		if (topMostValue == null)
		{
			throw new ArgumentException("Argument 'response' does not contain required Via: header field.");
		}
		string text = Guid.NewGuid().ToString();
		string text2 = ((transaction == null) ? "" : transaction.ID);
		if (transaction != null && transaction.Request.LocalEndPoint != null)
		{
			localEP = transaction.Request.LocalEndPoint;
		}
		else if (topMostValue.Parameters["localEP"] != null)
		{
			localEP = Net_Utils.ParseIPEndPoint(topMostValue.Parameters["localEP"].Value);
		}
		byte[] array = response.ToByteData();
		if (transaction != null)
		{
			try
			{
				SIP_Flow flow = transaction.Flow;
				flow.Send(response);
				if (m_pStack.Logger != null)
				{
					m_pStack.Logger.AddWrite(text, null, 0L, "Response [flowReuse=true; transactionID='" + text2 + "'; method='" + response.CSeq.RequestMethod + "'; cseq='" + response.CSeq.SequenceNumber + "'; transport='" + flow.Transport + "'; size='" + array.Length + "'; statusCode='" + response.StatusCode + "'; reason='" + response.ReasonPhrase + "'; sent '" + flow.LocalEP?.ToString() + "' -> '" + flow.RemoteEP?.ToString() + "'.", localEP, flow.RemoteEP, array);
				}
				return;
			}
			catch
			{
			}
		}
		if (SIP_Utils.IsReliableTransport(topMostValue.ProtocolTransport))
		{
			IPEndPoint iPEndPoint = null;
			if (transaction != null && transaction.Request.RemoteEndPoint != null)
			{
				iPEndPoint = transaction.Request.RemoteEndPoint;
			}
			else if (topMostValue.Received != null)
			{
				iPEndPoint = new IPEndPoint(topMostValue.Received, (topMostValue.SentBy.Port == -1) ? 5060 : topMostValue.SentBy.Port);
			}
			try
			{
				SIP_Flow sIP_Flow = null;
				if (transaction != null)
				{
					if (transaction.Request.Flow != null && !transaction.Request.Flow.IsDisposed)
					{
						sIP_Flow = transaction.Request.Flow;
					}
				}
				else
				{
					string value = topMostValue.Parameters["connectionID"].Value;
					if (value != null)
					{
						sIP_Flow = m_pFlowManager[value];
					}
				}
				if (sIP_Flow != null)
				{
					sIP_Flow.Send(response);
					if (m_pStack.Logger != null)
					{
						m_pStack.Logger.AddWrite(text, null, 0L, "Response [flowReuse=true; transactionID='" + text2 + "'; method='" + response.CSeq.RequestMethod + "'; cseq='" + response.CSeq.SequenceNumber + "'; transport='" + sIP_Flow.Transport + "'; size='" + array.Length + "'; statusCode='" + response.StatusCode + "'; reason='" + response.ReasonPhrase + "'; sent '" + sIP_Flow.RemoteEP?.ToString() + "' -> '" + sIP_Flow.LocalEP?.ToString() + "'.", localEP, iPEndPoint, array);
					}
					return;
				}
			}
			catch
			{
			}
			if (iPEndPoint != null)
			{
				try
				{
					SendResponseToHost(text, text2, null, iPEndPoint.Address.ToString(), iPEndPoint.Port, topMostValue.ProtocolTransport, response);
				}
				catch
				{
				}
			}
			SendResponse_RFC_3263_5(text, text2, localEP, response);
		}
		else
		{
			if (topMostValue.Maddr != null)
			{
				throw new SIP_TransportException("Sending responses to multicast address(Via: 'maddr') is not supported.");
			}
			if (topMostValue.Maddr == null && topMostValue.Received != null && topMostValue.RPort > 0)
			{
				SendResponseToHost(text, text2, localEP, topMostValue.Received.ToString(), topMostValue.RPort, topMostValue.ProtocolTransport, response);
			}
			else if (topMostValue.Received != null)
			{
				SendResponseToHost(text, text2, localEP, topMostValue.Received.ToString(), topMostValue.SentByPortWithDefault, topMostValue.ProtocolTransport, response);
			}
			else
			{
				SendResponse_RFC_3263_5(text, text2, localEP, response);
			}
		}
	}

	private void SendResponse_RFC_3263_5(string logID, string transactionID, IPEndPoint localEP, SIP_Response response)
	{
		SIP_t_ViaParm topMostValue = response.Via.GetTopMostValue();
		if (topMostValue.SentBy.IsIPAddress)
		{
			SendResponseToHost(logID, transactionID, localEP, topMostValue.SentBy.Host, topMostValue.SentByPortWithDefault, topMostValue.ProtocolTransport, response);
			return;
		}
		if (topMostValue.SentBy.Port != -1)
		{
			SendResponseToHost(logID, transactionID, localEP, topMostValue.SentBy.Host, topMostValue.SentByPortWithDefault, topMostValue.ProtocolTransport, response);
			return;
		}
		try
		{
			string queryText = "";
			if (topMostValue.ProtocolTransport == "UDP")
			{
				queryText = "_sip._udp." + topMostValue.SentBy.Host;
			}
			else if (topMostValue.ProtocolTransport == "TCP")
			{
				queryText = "_sip._tcp." + topMostValue.SentBy.Host;
			}
			else if (topMostValue.ProtocolTransport == "UDP")
			{
				queryText = "_sips._tcp." + topMostValue.SentBy.Host;
			}
			DnsServerResponse dnsServerResponse = m_pStack.Dns.Query(queryText, DNS_QType.SRV);
			if (dnsServerResponse.ResponseCode != 0)
			{
				throw new SIP_TransportException("Dns error: " + dnsServerResponse.ResponseCode);
			}
			DNS_rr_SRV[] sRVRecords = dnsServerResponse.GetSRVRecords();
			if (sRVRecords.Length != 0)
			{
				for (int i = 0; i < sRVRecords.Length; i++)
				{
					DNS_rr_SRV dNS_rr_SRV = sRVRecords[i];
					try
					{
						if (m_pStack.Logger != null)
						{
							m_pStack.Logger.AddText(logID, "Starts sending response to DNS SRV record '" + dNS_rr_SRV.Target + "'.");
						}
						SendResponseToHost(logID, transactionID, localEP, dNS_rr_SRV.Target, dNS_rr_SRV.Port, topMostValue.ProtocolTransport, response);
					}
					catch
					{
						if (i == sRVRecords.Length - 1)
						{
							if (m_pStack.Logger != null)
							{
								m_pStack.Logger.AddText(logID, "Failed to send response to DNS SRV record '" + dNS_rr_SRV.Target + "'.");
							}
							throw new SIP_TransportException("Host '" + topMostValue.SentBy.Host + "' is not accessible.");
						}
						if (m_pStack.Logger != null)
						{
							m_pStack.Logger.AddText(logID, "Failed to send response to DNS SRV record '" + dNS_rr_SRV.Target + "', will try next.");
						}
					}
				}
			}
			else
			{
				if (m_pStack.Logger != null)
				{
					m_pStack.Logger.AddText(logID, "No DNS SRV records found, starts sending to Via: sent-by host '" + topMostValue.SentBy.Host + "'.");
				}
				SendResponseToHost(logID, transactionID, localEP, topMostValue.SentBy.Host, topMostValue.SentByPortWithDefault, topMostValue.ProtocolTransport, response);
			}
		}
		catch (DNS_ClientException ex)
		{
			throw new SIP_TransportException("Dns error: " + ex.ErrorCode);
		}
	}

	private void SendResponseToHost(string logID, string transactionID, IPEndPoint localEP, string host, int port, string transport, SIP_Response response)
	{
		try
		{
			IPAddress[] array = null;
			if (Net_Utils.IsIPAddress(host))
			{
				array = new IPAddress[1] { IPAddress.Parse(host) };
			}
			else
			{
				array = m_pStack.Dns.GetHostAddresses(host);
				if (array.Length == 0)
				{
					throw new SIP_TransportException("Invalid Via: Sent-By host name '" + host + "' could not be resolved.");
				}
			}
			byte[] array2 = response.ToByteData();
			for (int i = 0; i < array.Length; i++)
			{
				IPEndPoint iPEndPoint = new IPEndPoint(array[i], port);
				try
				{
					GetOrCreateFlow(transport, localEP, iPEndPoint).Send(response);
					if (m_pStack.Logger != null)
					{
						m_pStack.Logger.AddWrite(logID, null, 0L, "Response [transactionID='" + transactionID + "'; method='" + response.CSeq.RequestMethod + "'; cseq='" + response.CSeq.SequenceNumber + "'; transport='" + transport + "'; size='" + array2.Length + "'; statusCode='" + response.StatusCode + "'; reason='" + response.ReasonPhrase + "'; sent '" + localEP?.ToString() + "' -> '" + iPEndPoint?.ToString() + "'.", localEP, iPEndPoint, array2);
					}
					break;
				}
				catch
				{
					if (i == array.Length - 1)
					{
						if (m_pStack.Logger != null)
						{
							m_pStack.Logger.AddText(logID, "Failed to send response to host '" + host + "' IP end point '" + iPEndPoint?.ToString() + "'.");
						}
						throw new SIP_TransportException("Host '" + host + ":" + port + "' is not accessible.");
					}
					if (m_pStack.Logger != null)
					{
						m_pStack.Logger.AddText(logID, "Failed to send response to host '" + host + "' IP end point '" + iPEndPoint?.ToString() + "', will try next A record.");
					}
				}
			}
		}
		catch (DNS_ClientException ex)
		{
			throw new SIP_TransportException("Dns error: " + ex.ErrorCode);
		}
	}

	internal HostEndPoint GetContactHost(SIP_Flow flow)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		HostEndPoint hostEndPoint = null;
		IPBindInfo[] bindInfo = BindInfo;
		foreach (IPBindInfo iPBindInfo in bindInfo)
		{
			if (iPBindInfo.Protocol == BindInfoProtocol.UDP && flow.Transport == "UDP")
			{
				if (iPBindInfo.IP.AddressFamily == flow.LocalEP.AddressFamily && iPBindInfo.Port == flow.LocalEP.Port)
				{
					hostEndPoint = new HostEndPoint(string.IsNullOrEmpty(iPBindInfo.HostName) ? flow.LocalEP.Address.ToString() : iPBindInfo.HostName, iPBindInfo.Port);
					break;
				}
			}
			else if (iPBindInfo.Protocol == BindInfoProtocol.TCP && iPBindInfo.SslMode == SslMode.SSL && flow.Transport == "TLS")
			{
				if (iPBindInfo.IP.AddressFamily == flow.LocalEP.AddressFamily)
				{
					hostEndPoint = ((iPBindInfo.IP != IPAddress.Any && iPBindInfo.IP != IPAddress.IPv6Any) ? new HostEndPoint(string.IsNullOrEmpty(iPBindInfo.HostName) ? iPBindInfo.IP.ToString() : iPBindInfo.HostName, iPBindInfo.Port) : new HostEndPoint(string.IsNullOrEmpty(iPBindInfo.HostName) ? flow.LocalEP.Address.ToString() : iPBindInfo.HostName, iPBindInfo.Port));
					break;
				}
			}
			else if (iPBindInfo.Protocol == BindInfoProtocol.TCP && flow.Transport == "TCP" && iPBindInfo.IP.AddressFamily == flow.LocalEP.AddressFamily)
			{
				hostEndPoint = ((!iPBindInfo.IP.Equals(IPAddress.Any) && !iPBindInfo.IP.Equals(IPAddress.IPv6Any)) ? new HostEndPoint(string.IsNullOrEmpty(iPBindInfo.HostName) ? iPBindInfo.IP.ToString() : iPBindInfo.HostName, iPBindInfo.Port) : new HostEndPoint(string.IsNullOrEmpty(iPBindInfo.HostName) ? flow.LocalEP.Address.ToString() : iPBindInfo.HostName, iPBindInfo.Port));
				break;
			}
		}
		if (hostEndPoint == null)
		{
			hostEndPoint = new HostEndPoint(flow.LocalEP);
		}
		if (hostEndPoint.IsIPAddress && Net_Utils.IsPrivateIP(IPAddress.Parse(hostEndPoint.Host)) && !Net_Utils.IsPrivateIP(flow.RemoteEP.Address))
		{
			hostEndPoint = new HostEndPoint(flow.LocalPublicEP);
		}
		return hostEndPoint;
	}

	internal string GetRecordRoute(string transport)
	{
		IPBindInfo[] pBinds = m_pBinds;
		foreach (IPBindInfo iPBindInfo in pBinds)
		{
			if (!string.IsNullOrEmpty(iPBindInfo.HostName))
			{
				if (iPBindInfo.Protocol == BindInfoProtocol.TCP && iPBindInfo.SslMode != 0 && transport == "TLS")
				{
					return "<sips:" + iPBindInfo.HostName + ":" + iPBindInfo.Port + ";lr>";
				}
				if (iPBindInfo.Protocol == BindInfoProtocol.TCP && transport == "TCP")
				{
					return "<sip:" + iPBindInfo.HostName + ":" + iPBindInfo.Port + ";lr>";
				}
				if (iPBindInfo.Protocol == BindInfoProtocol.UDP && transport == "UDP")
				{
					return "<sip:" + iPBindInfo.HostName + ":" + iPBindInfo.Port + ";lr>";
				}
			}
		}
		return null;
	}
}
