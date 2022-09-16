using System;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_ResponseSentEventArgs : EventArgs
{
	private SIP_ServerTransaction m_pTransaction;

	private SIP_Response m_pResponse;

	public SIP_ServerTransaction ServerTransaction => m_pTransaction;

	public SIP_Response Response => m_pResponse;

	public SIP_ResponseSentEventArgs(SIP_ServerTransaction transaction, SIP_Response response)
	{
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pTransaction = transaction;
		m_pResponse = response;
	}
}
