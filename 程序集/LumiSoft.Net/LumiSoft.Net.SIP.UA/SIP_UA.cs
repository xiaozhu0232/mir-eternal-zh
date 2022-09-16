using System;
using System.Collections.Generic;
using System.Threading;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.UA;

[Obsolete("Use SIP stack instead.")]
public class SIP_UA : IDisposable
{
	private bool m_IsDisposed;

	private SIP_Stack m_pStack;

	private List<SIP_UA_Call> m_pCalls;

	private object m_pLock = new object();

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

	public SIP_UA_Call[] Calls
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pCalls.ToArray();
		}
	}

	public event EventHandler<SIP_RequestReceivedEventArgs> RequestReceived;

	public event EventHandler<SIP_UA_Call_EventArgs> IncomingCall;

	public SIP_UA()
	{
		m_pStack = new SIP_Stack();
		m_pStack.RequestReceived += m_pStack_RequestReceived;
		m_pCalls = new List<SIP_UA_Call>();
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (m_IsDisposed)
			{
				return;
			}
			SIP_UA_Call[] array = m_pCalls.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Terminate();
			}
			DateTime now = DateTime.Now;
			while (m_pCalls.Count > 0)
			{
				Thread.Sleep(500);
				if ((DateTime.Now - now).Seconds > 15)
				{
					break;
				}
			}
			m_IsDisposed = true;
			this.RequestReceived = null;
			this.IncomingCall = null;
			m_pStack.Dispose();
			m_pStack = null;
		}
	}

	private void m_pStack_RequestReceived(object sender, SIP_RequestReceivedEventArgs e)
	{
		if (e.Request.RequestLine.Method == "CANCEL")
		{
			SIP_ServerTransaction sIP_ServerTransaction = m_pStack.TransactionLayer.MatchCancelToTransaction(e.Request);
			if (sIP_ServerTransaction != null)
			{
				sIP_ServerTransaction.Cancel();
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));
			}
			else
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist, e.Request));
			}
		}
		else if (e.Request.RequestLine.Method == "BYE")
		{
			SIP_Dialog sIP_Dialog = m_pStack.TransactionLayer.MatchDialog(e.Request);
			if (sIP_Dialog != null)
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));
				sIP_Dialog.Terminate();
			}
			else
			{
				e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist, e.Request));
			}
		}
		else if (e.Request.RequestLine.Method == "INVITE")
		{
			e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x100_Trying, e.Request));
			SIP_UA_Call sIP_UA_Call = new SIP_UA_Call(this, e.ServerTransaction);
			sIP_UA_Call.StateChanged += Call_StateChanged;
			m_pCalls.Add(sIP_UA_Call);
			OnIncomingCall(sIP_UA_Call);
		}
		else
		{
			OnRequestReceived(e);
		}
	}

	private void Call_StateChanged(object sender, EventArgs e)
	{
		SIP_UA_Call sIP_UA_Call = (SIP_UA_Call)sender;
		if (sIP_UA_Call.State == SIP_UA_CallState.Terminated)
		{
			m_pCalls.Remove(sIP_UA_Call);
		}
	}

	public SIP_UA_Call CreateCall(SIP_Request invite)
	{
		if (invite == null)
		{
			throw new ArgumentNullException("invite");
		}
		if (invite.RequestLine.Method != "INVITE")
		{
			throw new ArgumentException("Argument 'invite' is not INVITE request.");
		}
		lock (m_pLock)
		{
			SIP_UA_Call sIP_UA_Call = new SIP_UA_Call(this, invite);
			sIP_UA_Call.StateChanged += Call_StateChanged;
			m_pCalls.Add(sIP_UA_Call);
			return sIP_UA_Call;
		}
	}

	protected void OnRequestReceived(SIP_RequestReceivedEventArgs request)
	{
		if (this.RequestReceived != null)
		{
			this.RequestReceived(this, request);
		}
	}

	private void OnIncomingCall(SIP_UA_Call call)
	{
		if (this.IncomingCall != null)
		{
			this.IncomingCall(this, new SIP_UA_Call_EventArgs(call));
		}
	}
}
