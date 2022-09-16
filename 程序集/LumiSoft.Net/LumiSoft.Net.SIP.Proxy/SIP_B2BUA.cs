using System;
using System.Collections.Generic;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_B2BUA : IDisposable
{
	private SIP_Proxy m_pProxy;

	private List<SIP_B2BUA_Call> m_pCalls;

	private bool m_IsDisposed;

	public SIP_Stack Stack => m_pProxy.Stack;

	public SIP_B2BUA_Call[] Calls => m_pCalls.ToArray();

	public event EventHandler CallCreated;

	public event EventHandler CallTerminated;

	internal SIP_B2BUA(SIP_Proxy owner)
	{
		m_pProxy = owner;
		m_pCalls = new List<SIP_B2BUA_Call>();
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		foreach (SIP_B2BUA_Call pCall in m_pCalls)
		{
			pCall.Terminate();
		}
	}

	internal void OnRequestReceived(SIP_RequestReceivedEventArgs e)
	{
		SIP_Request request = e.Request;
		if (request.RequestLine.Method == "CANCEL")
		{
			m_pProxy.Stack.TransactionLayer.MatchCancelToTransaction(e.Request)?.Cancel();
		}
		else
		{
			if (request.RequestLine.Method == "BYE" || request.RequestLine.Method == "ACK" || request.RequestLine.Method == "OPTIONS" || request.RequestLine.Method == "PRACK" || request.RequestLine.Method == "UPDATE")
			{
				return;
			}
			SIP_Request sIP_Request = e.Request.Copy();
			sIP_Request.Via.RemoveAll();
			sIP_Request.MaxForwards = 70;
			sIP_Request.CallID = SIP_t_CallID.CreateCallID().CallID;
			sIP_Request.CSeq.SequenceNumber = 1;
			sIP_Request.Contact.RemoveAll();
			if (sIP_Request.Route.Count > 0 && m_pProxy.IsLocalRoute(SIP_Uri.Parse(sIP_Request.Route.GetTopMostValue().Address.Uri.ToString())))
			{
				sIP_Request.Route.RemoveTopMostValue();
			}
			sIP_Request.RecordRoute.RemoveAll();
			SIP_SingleValueHF<SIP_t_Credentials>[] headerFields = sIP_Request.ProxyAuthorization.HeaderFields;
			foreach (SIP_SingleValueHF<SIP_t_Credentials> sIP_SingleValueHF in headerFields)
			{
				try
				{
					Auth_HttpDigest auth_HttpDigest = new Auth_HttpDigest(sIP_SingleValueHF.ValueX.AuthData, sIP_Request.RequestLine.Method);
					if (m_pProxy.Stack.Realm == auth_HttpDigest.Realm)
					{
						sIP_Request.ProxyAuthorization.Remove(sIP_SingleValueHF);
					}
				}
				catch
				{
				}
			}
			sIP_Request.Allow.RemoveAll();
			sIP_Request.Supported.RemoveAll();
			if (request.RequestLine.Method != "ACK" && request.RequestLine.Method != "BYE")
			{
				sIP_Request.Allow.Add("INVITE,ACK,OPTIONS,CANCEL,BYE,PRACK");
			}
			if (request.RequestLine.Method != "ACK")
			{
				sIP_Request.Supported.Add("100rel,timer");
			}
			sIP_Request.Require.RemoveAll();
			if (request.RequestLine.Method == "INVITE" || request.RequestLine.Method == "UPDATE")
			{
				sIP_Request.SessionExpires = new SIP_t_SessionExpires(m_pProxy.Stack.SessionExpries, "uac");
				sIP_Request.MinSE = new SIP_t_MinSE(m_pProxy.Stack.MinimumSessionExpries);
			}
		}
	}

	internal void OnResponseReceived(SIP_ResponseReceivedEventArgs e)
	{
	}

	internal void AddCall(SIP_Dialog caller, SIP_Dialog calee)
	{
		lock (m_pCalls)
		{
			SIP_B2BUA_Call sIP_B2BUA_Call = new SIP_B2BUA_Call(this, caller, calee);
			m_pCalls.Add(sIP_B2BUA_Call);
			OnCallCreated(sIP_B2BUA_Call);
		}
	}

	internal void RemoveCall(SIP_B2BUA_Call call)
	{
		m_pCalls.Remove(call);
		OnCallTerminated(call);
	}

	public SIP_B2BUA_Call GetCallByID(string callID)
	{
		SIP_B2BUA_Call[] array = m_pCalls.ToArray();
		foreach (SIP_B2BUA_Call sIP_B2BUA_Call in array)
		{
			if (sIP_B2BUA_Call.CallID == callID)
			{
				return sIP_B2BUA_Call;
			}
		}
		return null;
	}

	protected void OnCallCreated(SIP_B2BUA_Call call)
	{
		if (this.CallCreated != null)
		{
			this.CallCreated(call, new EventArgs());
		}
	}

	protected internal void OnCallTerminated(SIP_B2BUA_Call call)
	{
		if (this.CallTerminated != null)
		{
			this.CallTerminated(call, new EventArgs());
		}
	}
}
