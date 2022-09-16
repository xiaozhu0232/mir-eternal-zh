using System;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_B2BUA_Call
{
	private SIP_B2BUA m_pOwner;

	private DateTime m_StartTime;

	private SIP_Dialog m_pCaller;

	private SIP_Dialog m_pCallee;

	private string m_CallID = "";

	private bool m_IsTerminated;

	public DateTime StartTime => m_StartTime;

	public string CallID => m_CallID;

	public SIP_Dialog CallerDialog => m_pCaller;

	public SIP_Dialog CalleeDialog => m_pCallee;

	public bool IsTimedOut => false;

	internal SIP_B2BUA_Call(SIP_B2BUA owner, SIP_Dialog caller, SIP_Dialog callee)
	{
		m_pOwner = owner;
		m_pCaller = caller;
		m_pCallee = callee;
		m_StartTime = DateTime.Now;
		m_CallID = Guid.NewGuid().ToString().Replace("-", "");
	}

	private void m_pCaller_RequestReceived(SIP_RequestReceivedEventArgs e)
	{
	}

	private void m_pCaller_Terminated(object sender, EventArgs e)
	{
		Terminate();
	}

	private void m_pCallee_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		_ = (SIP_ServerTransaction)e.ClientTransaction.Tag;
	}

	private void m_pCallee_RequestReceived(SIP_RequestReceivedEventArgs e)
	{
	}

	private void m_pCallee_Terminated(object sender, EventArgs e)
	{
		Terminate();
	}

	private void m_pCaller_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		_ = (SIP_ServerTransaction)e.ClientTransaction.Tag;
	}

	public void Terminate()
	{
		if (!m_IsTerminated)
		{
			m_IsTerminated = true;
			m_pOwner.RemoveCall(this);
			if (m_pCaller != null)
			{
				m_pCaller.Dispose();
				m_pCaller = null;
			}
			if (m_pCallee != null)
			{
				m_pCallee.Dispose();
				m_pCallee = null;
			}
			m_pOwner.OnCallTerminated(this);
		}
	}

	private void CopyMessage(SIP_Message source, SIP_Message destination, string[] exceptHeaders)
	{
		foreach (SIP_HeaderField item in source.Header)
		{
			bool flag = true;
			for (int i = 0; i < exceptHeaders.Length; i++)
			{
				if (exceptHeaders[i].ToLower() == item.Name.ToLower())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				destination.Header.Add(item.Name, item.Value);
			}
		}
		destination.Data = source.Data;
	}
}
