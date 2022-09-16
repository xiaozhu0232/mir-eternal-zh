using System;
using System.Net;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_ValidateRequestEventArgs : EventArgs
{
	private SIP_Request m_pRequest;

	private IPEndPoint m_pRemoteEndPoint;

	private string m_ResponseCode;

	public SIP_Request Request => m_pRequest;

	public IPEndPoint RemoteEndPoint => m_pRemoteEndPoint;

	public string ResponseCode
	{
		get
		{
			return m_ResponseCode;
		}
		set
		{
			m_ResponseCode = value;
		}
	}

	public SIP_ValidateRequestEventArgs(SIP_Request request, IPEndPoint remoteEndpoint)
	{
		m_pRequest = request;
		m_pRemoteEndPoint = remoteEndpoint;
	}
}
