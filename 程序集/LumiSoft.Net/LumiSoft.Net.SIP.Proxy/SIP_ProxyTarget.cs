using System;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_ProxyTarget
{
	private SIP_Uri m_pTargetUri;

	private SIP_Flow m_pFlow;

	public SIP_Uri TargetUri => m_pTargetUri;

	public SIP_Flow Flow => m_pFlow;

	public SIP_ProxyTarget(SIP_Uri targetUri)
		: this(targetUri, null)
	{
	}

	public SIP_ProxyTarget(SIP_Uri targetUri, SIP_Flow flow)
	{
		if (targetUri == null)
		{
			throw new ArgumentNullException("targetUri");
		}
		m_pTargetUri = targetUri;
		m_pFlow = flow;
	}
}
