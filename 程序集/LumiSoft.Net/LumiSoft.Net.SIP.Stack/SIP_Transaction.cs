using System;
using System.Collections.Generic;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public abstract class SIP_Transaction : IDisposable
{
	private SIP_TransactionState m_State;

	private SIP_Stack m_pStack;

	private SIP_Flow m_pFlow;

	private SIP_Request m_pRequest;

	private string m_Method = "";

	private string m_ID = "";

	private string m_Key = "";

	private DateTime m_CreateTime;

	private List<SIP_Response> m_pResponses;

	private object m_pTag;

	private object m_pLock = new object();

	public object SyncRoot => m_pLock;

	public bool IsDisposed => m_State == SIP_TransactionState.Disposed;

	public SIP_TransactionState State => m_State;

	public SIP_Stack Stack
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pStack;
		}
	}

	public SIP_Flow Flow
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pFlow;
		}
	}

	public SIP_Request Request
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRequest;
		}
	}

	public string Method
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Method;
		}
	}

	public string ID
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_ID;
		}
	}

	public DateTime CreateTime
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CreateTime;
		}
	}

	public SIP_Response[] Responses
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pResponses.ToArray();
		}
	}

	public SIP_Response LastProvisionalResponse
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			for (int num = Responses.Length - 1; num > -1; num--)
			{
				if (Responses[num].StatusCodeType == SIP_StatusCodeType.Provisional)
				{
					return Responses[num];
				}
			}
			return null;
		}
	}

	public SIP_Response FinalResponse
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			SIP_Response[] responses = Responses;
			foreach (SIP_Response sIP_Response in responses)
			{
				if (sIP_Response.StatusCodeType != 0)
				{
					return sIP_Response;
				}
			}
			return null;
		}
	}

	public bool HasProvisionalResponse
	{
		get
		{
			if (State == SIP_TransactionState.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			foreach (SIP_Response pResponse in m_pResponses)
			{
				if (pResponse.StatusCodeType == SIP_StatusCodeType.Provisional)
				{
					return true;
				}
			}
			return false;
		}
	}

	public SIP_Dialog Dialog => null;

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	internal string Key => m_Key;

	public event EventHandler StateChanged;

	public event EventHandler Disposed;

	public event EventHandler TimedOut;

	public event EventHandler<ExceptionEventArgs> TransportError;

	public event EventHandler TransactionError;

	public SIP_Transaction(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (flow == null)
		{
			throw new ArgumentNullException("flow");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		m_pStack = stack;
		m_pFlow = flow;
		m_pRequest = request;
		m_Method = request.RequestLine.Method;
		m_CreateTime = DateTime.Now;
		m_pResponses = new List<SIP_Response>();
		SIP_t_ViaParm topMostValue = request.Via.GetTopMostValue();
		if (topMostValue == null)
		{
			throw new ArgumentException("Via: header is missing !");
		}
		if (topMostValue.Branch == null)
		{
			throw new ArgumentException("Via: header 'branch' parameter is missing !");
		}
		m_ID = topMostValue.Branch;
		if (this is SIP_ServerTransaction)
		{
			string text = request.Via.GetTopMostValue().Branch + "-" + request.Via.GetTopMostValue().SentBy;
			if (request.RequestLine.Method == "CANCEL")
			{
				text += "-CANCEL";
			}
			m_Key = text;
		}
		else
		{
			m_Key = m_ID + "-" + request.RequestLine.Method;
		}
	}

	public virtual void Dispose()
	{
		SetState(SIP_TransactionState.Disposed);
		OnDisposed();
		m_pStack = null;
		m_pFlow = null;
		m_pRequest = null;
		this.StateChanged = null;
		this.Disposed = null;
		this.TimedOut = null;
		this.TransportError = null;
	}

	public abstract void Cancel();

	protected void SetState(SIP_TransactionState state)
	{
		if (Stack.Logger != null)
		{
			Stack.Logger.AddText(ID, "Transaction [branch='" + ID + "';method='" + Method + "';IsServer=" + (this is SIP_ServerTransaction) + "] switched to '" + state.ToString() + "' state.");
		}
		m_State = state;
		OnStateChanged();
		if (m_State == SIP_TransactionState.Terminated)
		{
			Dispose();
		}
	}

	protected void AddResponse(SIP_Response response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (m_pResponses.Count < 15 || response.StatusCode >= 200)
		{
			m_pResponses.Add(response);
		}
	}

	private void OnStateChanged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs());
		}
	}

	protected void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}

	protected void OnTimedOut()
	{
		if (this.TimedOut != null)
		{
			this.TimedOut(this, new EventArgs());
		}
	}

	protected void OnTransportError(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (this.TransportError != null)
		{
			this.TransportError(this, new ExceptionEventArgs(exception));
		}
	}

	protected void OnTransactionError(string errorText)
	{
		if (this.TransactionError != null)
		{
			this.TransactionError(this, new EventArgs());
		}
	}
}
