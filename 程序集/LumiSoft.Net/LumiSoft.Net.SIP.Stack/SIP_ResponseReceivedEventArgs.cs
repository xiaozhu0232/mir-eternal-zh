using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_ResponseReceivedEventArgs : EventArgs
{
	private SIP_Stack m_pStack;

	private SIP_Response m_pResponse;

	private SIP_ClientTransaction m_pTransaction;

	public SIP_Response Response => m_pResponse;

	public SIP_ClientTransaction ClientTransaction => m_pTransaction;

	public SIP_Dialog Dialog => m_pStack.TransactionLayer.MatchDialog(m_pResponse);

	public SIP_Dialog GetOrCreateDialog
	{
		get
		{
			if (!SIP_Utils.MethodCanEstablishDialog(m_pTransaction.Method))
			{
				throw new InvalidOperationException("Request method '" + m_pTransaction.Method + "' can't establish dialog.");
			}
			if (m_pResponse.To.Tag == null)
			{
				throw new InvalidOperationException("Request To-Tag is missing.");
			}
			return m_pStack.TransactionLayer.GetOrCreateDialog(m_pTransaction, m_pResponse);
		}
	}

	internal SIP_ResponseReceivedEventArgs(SIP_Stack stack, SIP_ClientTransaction transaction, SIP_Response response)
	{
		m_pStack = stack;
		m_pResponse = response;
		m_pTransaction = transaction;
	}
}
