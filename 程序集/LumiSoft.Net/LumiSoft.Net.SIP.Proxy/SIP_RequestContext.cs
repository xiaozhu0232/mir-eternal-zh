using System;
using System.Collections.Generic;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_RequestContext
{
	private SIP_Proxy m_pProxy;

	private SIP_Request m_pRequest;

	private SIP_Flow m_pFlow;

	private SIP_ServerTransaction m_pTransaction;

	private List<SIP_ProxyTarget> m_pTargets;

	private string m_User;

	private SIP_ProxyContext m_pProxyContext;

	public SIP_Request Request => m_pRequest;

	public SIP_ServerTransaction Transaction
	{
		get
		{
			if (Request.RequestLine.Method == "ACK")
			{
				throw new InvalidOperationException("ACK request is transactionless SIP method.");
			}
			if (m_pTransaction == null)
			{
				m_pTransaction = m_pProxy.Stack.TransactionLayer.EnsureServerTransaction(m_pFlow, m_pRequest);
			}
			return m_pTransaction;
		}
	}

	public List<SIP_ProxyTarget> Targets => m_pTargets;

	public string User => m_User;

	public SIP_ProxyContext ProxyContext
	{
		get
		{
			if (m_pProxyContext == null)
			{
				m_pProxy.CreateProxyContext(this, Transaction, Request, addRecordRoute: true);
			}
			return m_pProxyContext;
		}
	}

	internal SIP_RequestContext(SIP_Proxy proxy, SIP_Request request, SIP_Flow flow)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		m_pProxy = proxy;
		m_pRequest = request;
		m_pFlow = flow;
		m_pTargets = new List<SIP_ProxyTarget>();
	}

	public void ForwardStatelessly()
	{
		throw new NotImplementedException();
	}

	public void ChallengeRequest()
	{
		throw new NotImplementedException();
	}

	internal void SetUser(string user)
	{
		m_User = user;
	}
}
