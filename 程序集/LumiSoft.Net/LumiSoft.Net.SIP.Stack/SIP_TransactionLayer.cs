using System;
using System.Collections.Generic;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_TransactionLayer
{
	private bool m_IsDisposed;

	private SIP_Stack m_pStack;

	private Dictionary<string, SIP_ClientTransaction> m_pClientTransactions;

	private Dictionary<string, SIP_ServerTransaction> m_pServerTransactions;

	private Dictionary<string, SIP_Dialog> m_pDialogs;

	public SIP_Transaction[] Transactions
	{
		get
		{
			List<SIP_Transaction> list = new List<SIP_Transaction>();
			list.AddRange(ClientTransactions);
			list.AddRange(ServerTransactions);
			return list.ToArray();
		}
	}

	public SIP_ClientTransaction[] ClientTransactions
	{
		get
		{
			lock (m_pClientTransactions)
			{
				SIP_ClientTransaction[] array = new SIP_ClientTransaction[m_pClientTransactions.Values.Count];
				m_pClientTransactions.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public SIP_ServerTransaction[] ServerTransactions
	{
		get
		{
			lock (m_pServerTransactions)
			{
				SIP_ServerTransaction[] array = new SIP_ServerTransaction[m_pServerTransactions.Values.Count];
				m_pServerTransactions.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public SIP_Dialog[] Dialogs
	{
		get
		{
			lock (m_pDialogs)
			{
				SIP_Dialog[] array = new SIP_Dialog[m_pDialogs.Values.Count];
				m_pDialogs.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	internal SIP_TransactionLayer(SIP_Stack stack)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		m_pStack = stack;
		m_pClientTransactions = new Dictionary<string, SIP_ClientTransaction>();
		m_pServerTransactions = new Dictionary<string, SIP_ServerTransaction>();
		m_pDialogs = new Dictionary<string, SIP_Dialog>();
	}

	internal void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		SIP_ClientTransaction[] clientTransactions = ClientTransactions;
		foreach (SIP_ClientTransaction sIP_ClientTransaction in clientTransactions)
		{
			try
			{
				sIP_ClientTransaction.Dispose();
			}
			catch
			{
			}
		}
		SIP_ServerTransaction[] serverTransactions = ServerTransactions;
		foreach (SIP_ServerTransaction sIP_ServerTransaction in serverTransactions)
		{
			try
			{
				sIP_ServerTransaction.Dispose();
			}
			catch
			{
			}
		}
		SIP_Dialog[] dialogs = Dialogs;
		foreach (SIP_Dialog sIP_Dialog in dialogs)
		{
			try
			{
				sIP_Dialog.Dispose();
			}
			catch
			{
			}
		}
		m_IsDisposed = true;
	}

	public SIP_ClientTransaction CreateClientTransaction(SIP_Flow flow, SIP_Request request, bool addVia)
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
		if (addVia)
		{
			SIP_t_ViaParm sIP_t_ViaParm = new SIP_t_ViaParm();
			sIP_t_ViaParm.ProtocolName = "SIP";
			sIP_t_ViaParm.ProtocolVersion = "2.0";
			sIP_t_ViaParm.ProtocolTransport = flow.Transport;
			sIP_t_ViaParm.SentBy = new HostEndPoint("transport_layer_will_replace_it", -1);
			sIP_t_ViaParm.Branch = SIP_t_ViaParm.CreateBranch();
			sIP_t_ViaParm.RPort = 0;
			request.Via.AddToTop(sIP_t_ViaParm.ToStringValue());
		}
		lock (m_pClientTransactions)
		{
			SIP_ClientTransaction transaction = new SIP_ClientTransaction(m_pStack, flow, request);
			m_pClientTransactions.Add(transaction.Key, transaction);
			transaction.StateChanged += delegate
			{
				if (transaction.State == SIP_TransactionState.Terminated)
				{
					lock (m_pClientTransactions)
					{
						m_pClientTransactions.Remove(transaction.Key);
					}
				}
			};
			MatchDialog(request)?.AddTransaction(transaction);
			return transaction;
		}
	}

	public SIP_ServerTransaction CreateServerTransaction(SIP_Flow flow, SIP_Request request)
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
		lock (m_pServerTransactions)
		{
			SIP_ServerTransaction transaction = new SIP_ServerTransaction(m_pStack, flow, request);
			m_pServerTransactions.Add(transaction.Key, transaction);
			transaction.StateChanged += delegate
			{
				if (transaction.State == SIP_TransactionState.Terminated)
				{
					lock (m_pClientTransactions)
					{
						m_pServerTransactions.Remove(transaction.Key);
					}
				}
			};
			MatchDialog(request)?.AddTransaction(transaction);
			return transaction;
		}
	}

	public SIP_ServerTransaction EnsureServerTransaction(SIP_Flow flow, SIP_Request request)
	{
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (request.RequestLine.Method == "ACK")
		{
			throw new InvalidOperationException("ACK request is transaction less request, can't create transaction for it.");
		}
		string text = request.Via.GetTopMostValue().Branch + "-" + request.Via.GetTopMostValue().SentBy;
		if (request.RequestLine.Method == "CANCEL")
		{
			text += "-CANCEL";
		}
		lock (m_pServerTransactions)
		{
			SIP_ServerTransaction value = null;
			m_pServerTransactions.TryGetValue(text, out value);
			if (value == null)
			{
				value = CreateServerTransaction(flow, request);
			}
			return value;
		}
	}

	internal SIP_ClientTransaction MatchClientTransaction(SIP_Response response)
	{
		SIP_ClientTransaction value = null;
		string key = response.Via.GetTopMostValue().Branch + "-" + response.CSeq.RequestMethod;
		lock (m_pClientTransactions)
		{
			m_pClientTransactions.TryGetValue(key, out value);
			return value;
		}
	}

	internal SIP_ServerTransaction MatchServerTransaction(SIP_Request request)
	{
		SIP_ServerTransaction value = null;
		string text = request.Via.GetTopMostValue().Branch + "-" + request.Via.GetTopMostValue().SentBy;
		if (request.RequestLine.Method == "CANCEL")
		{
			text += "-CANCEL";
		}
		lock (m_pServerTransactions)
		{
			m_pServerTransactions.TryGetValue(text, out value);
		}
		if (value != null && request.RequestLine.Method == "ACK" && value.State == SIP_TransactionState.Terminated)
		{
			value = null;
		}
		return value;
	}

	public SIP_ServerTransaction MatchCancelToTransaction(SIP_Request cancelRequest)
	{
		if (cancelRequest == null)
		{
			throw new ArgumentNullException("cancelRequest");
		}
		if (cancelRequest.RequestLine.Method != "CANCEL")
		{
			throw new ArgumentException("Argument 'cancelRequest' is not SIP CANCEL request.");
		}
		SIP_ServerTransaction value = null;
		string key = cancelRequest.Via.GetTopMostValue().Branch + "-" + cancelRequest.Via.GetTopMostValue().SentBy;
		lock (m_pServerTransactions)
		{
			m_pServerTransactions.TryGetValue(key, out value);
			return value;
		}
	}

	public SIP_Dialog GetOrCreateDialog(SIP_Transaction transaction, SIP_Response response)
	{
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		string text = "";
		text = ((!(transaction is SIP_ServerTransaction)) ? (response.CallID + "-" + response.From.Tag + "-" + response.To.Tag) : (response.CallID + "-" + response.To.Tag + "-" + response.From.Tag));
		lock (m_pDialogs)
		{
			SIP_Dialog dialog = null;
			m_pDialogs.TryGetValue(text, out dialog);
			if (dialog == null)
			{
				if (response.CSeq.RequestMethod.ToUpper() == "INVITE")
				{
					dialog = new SIP_Dialog_Invite();
				}
				else
				{
					if (!(response.CSeq.RequestMethod.ToUpper() == "REFER"))
					{
						throw new ArgumentException("Method '" + response.CSeq.RequestMethod + "' has no dialog handler.");
					}
					dialog = new SIP_Dialog_Refer();
				}
				dialog.Init(m_pStack, transaction, response);
				dialog.StateChanged += delegate
				{
					if (dialog.State == SIP_DialogState.Terminated)
					{
						m_pDialogs.Remove(dialog.ID);
					}
				};
				m_pDialogs.Add(dialog.ID, dialog);
			}
			return dialog;
		}
	}

	internal void RemoveDialog(SIP_Dialog dialog)
	{
		lock (m_pDialogs)
		{
			m_pDialogs.Remove(dialog.ID);
		}
	}

	internal SIP_Dialog MatchDialog(SIP_Request request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		SIP_Dialog value = null;
		try
		{
			string callID = request.CallID;
			string tag = request.To.Tag;
			string tag2 = request.From.Tag;
			if (callID != null)
			{
				if (tag != null)
				{
					if (tag2 != null)
					{
						string key = callID + "-" + tag + "-" + tag2;
						lock (m_pDialogs)
						{
							m_pDialogs.TryGetValue(key, out value);
							return value;
						}
					}
					return value;
				}
				return value;
			}
			return value;
		}
		catch
		{
			return value;
		}
	}

	internal SIP_Dialog MatchDialog(SIP_Response response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		SIP_Dialog value = null;
		try
		{
			string callID = response.CallID;
			string tag = response.From.Tag;
			string tag2 = response.To.Tag;
			if (callID != null)
			{
				if (tag != null)
				{
					if (tag2 != null)
					{
						string key = callID + "-" + tag + "-" + tag2;
						lock (m_pDialogs)
						{
							m_pDialogs.TryGetValue(key, out value);
							return value;
						}
					}
					return value;
				}
				return value;
			}
			return value;
		}
		catch
		{
			return value;
		}
	}
}
