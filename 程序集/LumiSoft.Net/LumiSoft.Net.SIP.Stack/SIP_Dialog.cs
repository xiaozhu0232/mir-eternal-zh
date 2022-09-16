using System;
using System.Collections.Generic;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_Dialog
{
	private object m_pLock = new object();

	private SIP_DialogState m_State;

	private SIP_Stack m_pStack;

	private DateTime m_CreateTime;

	private string m_CallID = "";

	private string m_LocalTag = "";

	private string m_RemoteTag = "";

	private int m_LocalSeqNo;

	private int m_RemoteSeqNo;

	private AbsoluteUri m_pLocalUri;

	private AbsoluteUri m_pRemoteUri;

	private SIP_Uri m_pLocalContact;

	private SIP_Uri m_pRemoteTarget;

	private bool m_IsSecure;

	private SIP_t_AddressParam[] m_pRouteSet;

	private string[] m_pRemoteAllow;

	private string[] m_pRemoteSupported;

	private SIP_Flow m_pFlow;

	private List<SIP_Transaction> m_pTransactions;

	public object SyncRoot => m_pLock;

	public SIP_DialogState State => m_State;

	public SIP_Stack Stack
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pStack;
		}
	}

	public DateTime CreateTime
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CreateTime;
		}
	}

	public string ID
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return CallID + "-" + LocalTag + "-" + RemoteTag;
		}
	}

	public string CallID
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CallID;
		}
	}

	public string LocalTag
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LocalTag;
		}
	}

	public string RemoteTag
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RemoteTag;
		}
	}

	public int LocalSeqNo
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LocalSeqNo;
		}
	}

	public int RemoteSeqNo
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RemoteSeqNo;
		}
	}

	public AbsoluteUri LocalUri
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLocalUri;
		}
	}

	public AbsoluteUri RemoteUri
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRemoteUri;
		}
	}

	public SIP_Uri LocalContact
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLocalContact;
		}
	}

	public SIP_Uri RemoteTarget
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRemoteTarget;
		}
	}

	public bool IsSecure
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsSecure;
		}
	}

	public SIP_t_AddressParam[] RouteSet
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRouteSet;
		}
	}

	public string[] RemoteAllow
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRemoteAllow;
		}
	}

	public string[] RemoteSupported
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRemoteSupported;
		}
	}

	public SIP_Flow Flow
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pFlow;
		}
	}

	public SIP_Transaction[] Transactions
	{
		get
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pTransactions.ToArray();
		}
	}

	public event EventHandler StateChanged;

	public event EventHandler<SIP_RequestReceivedEventArgs> RequestReceived;

	public SIP_Dialog()
	{
		m_CreateTime = DateTime.Now;
		m_pRouteSet = new SIP_t_AddressParam[0];
		m_pTransactions = new List<SIP_Transaction>();
	}

	protected internal virtual void Init(SIP_Stack stack, SIP_Transaction transaction, SIP_Response response)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pStack = stack;
		if (transaction is SIP_ServerTransaction)
		{
			m_IsSecure = ((SIP_Uri)transaction.Request.RequestLine.Uri).IsSecure;
			m_pRouteSet = (SIP_t_AddressParam[])Net_Utils.ReverseArray(transaction.Request.RecordRoute.GetAllValues());
			m_pRemoteTarget = (SIP_Uri)transaction.Request.Contact.GetTopMostValue().Address.Uri;
			m_RemoteSeqNo = transaction.Request.CSeq.SequenceNumber;
			m_LocalSeqNo = 0;
			m_CallID = transaction.Request.CallID;
			m_LocalTag = response.To.Tag;
			m_RemoteTag = transaction.Request.From.Tag;
			m_pRemoteUri = transaction.Request.From.Address.Uri;
			m_pLocalUri = transaction.Request.To.Address.Uri;
			m_pLocalContact = (SIP_Uri)response.Contact.GetTopMostValue().Address.Uri;
			List<string> list = new List<string>();
			SIP_t_Method[] allValues = response.Allow.GetAllValues();
			foreach (SIP_t_Method sIP_t_Method in allValues)
			{
				list.Add(sIP_t_Method.Method);
			}
			m_pRemoteAllow = list.ToArray();
			List<string> list2 = new List<string>();
			SIP_t_OptionTag[] allValues2 = response.Supported.GetAllValues();
			foreach (SIP_t_OptionTag sIP_t_OptionTag in allValues2)
			{
				list2.Add(sIP_t_OptionTag.OptionTag);
			}
			m_pRemoteSupported = list2.ToArray();
		}
		else
		{
			m_IsSecure = ((SIP_Uri)transaction.Request.RequestLine.Uri).IsSecure;
			m_pRouteSet = (SIP_t_AddressParam[])Net_Utils.ReverseArray(response.RecordRoute.GetAllValues());
			m_pRemoteTarget = (SIP_Uri)response.Contact.GetTopMostValue().Address.Uri;
			m_LocalSeqNo = transaction.Request.CSeq.SequenceNumber;
			m_RemoteSeqNo = 0;
			m_CallID = transaction.Request.CallID;
			m_LocalTag = transaction.Request.From.Tag;
			m_RemoteTag = response.To.Tag;
			m_pRemoteUri = transaction.Request.To.Address.Uri;
			m_pLocalUri = transaction.Request.From.Address.Uri;
			m_pLocalContact = (SIP_Uri)transaction.Request.Contact.GetTopMostValue().Address.Uri;
			List<string> list3 = new List<string>();
			SIP_t_Method[] allValues = response.Allow.GetAllValues();
			foreach (SIP_t_Method sIP_t_Method2 in allValues)
			{
				list3.Add(sIP_t_Method2.Method);
			}
			m_pRemoteAllow = list3.ToArray();
			List<string> list4 = new List<string>();
			SIP_t_OptionTag[] allValues2 = response.Supported.GetAllValues();
			foreach (SIP_t_OptionTag sIP_t_OptionTag2 in allValues2)
			{
				list4.Add(sIP_t_OptionTag2.OptionTag);
			}
			m_pRemoteSupported = list4.ToArray();
		}
		m_pFlow = transaction.Flow;
		AddTransaction(transaction);
	}

	public virtual void Dispose()
	{
		lock (m_pLock)
		{
			if (State != SIP_DialogState.Disposed)
			{
				SetState(SIP_DialogState.Disposed, raiseEvent: true);
				this.RequestReceived = null;
				m_pStack = null;
				m_CallID = null;
				m_LocalTag = null;
				m_RemoteTag = null;
				m_pLocalUri = null;
				m_pRemoteUri = null;
				m_pLocalContact = null;
				m_pRemoteTarget = null;
				m_pRouteSet = null;
				m_pFlow = null;
			}
		}
	}

	public void Terminate()
	{
		lock (m_pLock)
		{
			if (State == SIP_DialogState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			SetState(SIP_DialogState.Terminated, raiseEvent: true);
		}
	}

	public SIP_Request CreateRequest(string method)
	{
		if (State == SIP_DialogState.Disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (method == string.Empty)
		{
			throw new ArgumentException("Argument 'method' value must be specified.");
		}
		lock (m_pLock)
		{
			SIP_Request sIP_Request = m_pStack.CreateRequest(method, new SIP_t_NameAddress("", m_pRemoteUri), new SIP_t_NameAddress("", m_pLocalUri));
			sIP_Request.Route.RemoveAll();
			if (m_pRouteSet.Length == 0)
			{
				sIP_Request.RequestLine.Uri = m_pRemoteTarget;
			}
			else
			{
				SIP_Uri sIP_Uri = (SIP_Uri)m_pRouteSet[0].Address.Uri;
				if (sIP_Uri.Param_Lr)
				{
					sIP_Request.RequestLine.Uri = m_pRemoteTarget;
					for (int i = 0; i < m_pRouteSet.Length; i++)
					{
						sIP_Request.Route.Add(m_pRouteSet[i].ToStringValue());
					}
				}
				else
				{
					sIP_Request.RequestLine.Uri = SIP_Utils.UriToRequestUri(sIP_Uri);
					for (int j = 1; j < m_pRouteSet.Length; j++)
					{
						sIP_Request.Route.Add(m_pRouteSet[j].ToStringValue());
					}
				}
			}
			sIP_Request.To.Tag = m_RemoteTag;
			sIP_Request.From.Tag = m_LocalTag;
			sIP_Request.CallID = m_CallID;
			if (method != "ACK")
			{
				sIP_Request.CSeq.SequenceNumber = ++m_LocalSeqNo;
			}
			sIP_Request.Contact.Add(m_pLocalContact.ToString());
			return sIP_Request;
		}
	}

	public SIP_RequestSender CreateRequestSender(SIP_Request request)
	{
		lock (m_pLock)
		{
			if (State == SIP_DialogState.Terminated)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			return m_pStack.CreateRequestSender(request, Flow);
		}
	}

	protected bool IsTargetRefresh(string method)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		method = method.ToUpper();
		return method switch
		{
			"INVITE" => true, 
			"UPDATE" => true, 
			"SUBSCRIBE" => true, 
			"NOTIFY" => true, 
			"REFER" => true, 
			_ => false, 
		};
	}

	protected void SetState(SIP_DialogState state, bool raiseEvent)
	{
		m_State = state;
		if (raiseEvent)
		{
			OnStateChanged();
		}
		if (m_State == SIP_DialogState.Terminated)
		{
			Dispose();
		}
	}

	protected internal virtual bool ProcessRequest(SIP_RequestReceivedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (m_RemoteSeqNo == 0)
		{
			m_RemoteSeqNo = e.Request.CSeq.SequenceNumber;
		}
		else
		{
			if (e.Request.CSeq.SequenceNumber < m_RemoteSeqNo)
			{
				e.ServerTransaction.SendResponse(Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": The mid-dialog request is out of order(late arriving request).", e.Request));
				return true;
			}
			m_RemoteSeqNo = e.Request.CSeq.SequenceNumber;
		}
		if (IsTargetRefresh(e.Request.RequestLine.Method) && e.Request.Contact.Count != 0)
		{
			m_pRemoteTarget = (SIP_Uri)e.Request.Contact.GetTopMostValue().Address.Uri;
		}
		OnRequestReceived(e);
		return e.IsHandled;
	}

	protected internal virtual bool ProcessResponse(SIP_Response response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		return false;
	}

	internal void AddTransaction(SIP_Transaction transaction)
	{
		if (transaction == null)
		{
			throw new ArgumentNullException("transaction");
		}
		m_pTransactions.Add(transaction);
		transaction.Disposed += delegate
		{
			m_pTransactions.Remove(transaction);
		};
	}

	private void OnStateChanged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs());
		}
	}

	private void OnRequestReceived(SIP_RequestReceivedEventArgs e)
	{
		if (this.RequestReceived != null)
		{
			this.RequestReceived(this, e);
		}
	}
}
