using System;
using System.Threading;

namespace LumiSoft.Net;

internal class AsyncResultState : IAsyncResult
{
	private object m_pAsyncObject;

	private Delegate m_pAsyncDelegate;

	private AsyncCallback m_pCallback;

	private object m_pState;

	private IAsyncResult m_pAsyncResult;

	private bool m_IsEndCalled;

	public object AsyncObject => m_pAsyncObject;

	public Delegate AsyncDelegate => m_pAsyncDelegate;

	public IAsyncResult AsyncResult => m_pAsyncResult;

	public bool IsEndCalled
	{
		get
		{
			return m_IsEndCalled;
		}
		set
		{
			m_IsEndCalled = value;
		}
	}

	public object AsyncState => m_pState;

	public WaitHandle AsyncWaitHandle => m_pAsyncResult.AsyncWaitHandle;

	public bool CompletedSynchronously => m_pAsyncResult.CompletedSynchronously;

	public bool IsCompleted => m_pAsyncResult.IsCompleted;

	public AsyncResultState(object asyncObject, Delegate asyncDelegate, AsyncCallback callback, object state)
	{
		m_pAsyncObject = asyncObject;
		m_pAsyncDelegate = asyncDelegate;
		m_pCallback = callback;
		m_pState = state;
	}

	public void SetAsyncResult(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		m_pAsyncResult = asyncResult;
	}

	public void CompletedCallback(IAsyncResult ar)
	{
		if (m_pCallback != null)
		{
			m_pCallback(this);
		}
	}
}
