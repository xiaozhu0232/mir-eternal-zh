using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_RequestReceivedEventArgs : EventArgs
{
	private SIP_Stack m_pStack;

	private SIP_Flow m_pFlow;

	private SIP_Request m_pRequest;

	private SIP_ServerTransaction m_pTransaction;

	private bool m_IsHandled;

	public SIP_Flow Flow => m_pFlow;

	public SIP_Request Request => m_pRequest;

	public SIP_ServerTransaction ServerTransaction
	{
		get
		{
			if (m_pRequest.RequestLine.Method == "ACK")
			{
				return null;
			}
			if (m_pTransaction == null)
			{
				m_pTransaction = m_pStack.TransactionLayer.EnsureServerTransaction(m_pFlow, m_pRequest);
			}
			return m_pTransaction;
		}
	}

	public SIP_Dialog Dialog => m_pStack.TransactionLayer.MatchDialog(m_pRequest);

	public bool IsHandled
	{
		get
		{
			return m_IsHandled;
		}
		set
		{
			m_IsHandled = true;
		}
	}

	internal SIP_RequestReceivedEventArgs(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
		: this(stack, flow, request, null)
	{
	}

	internal SIP_RequestReceivedEventArgs(SIP_Stack stack, SIP_Flow flow, SIP_Request request, SIP_ServerTransaction transaction)
	{
		m_pStack = stack;
		m_pFlow = flow;
		m_pRequest = request;
		m_pTransaction = transaction;
	}
}
