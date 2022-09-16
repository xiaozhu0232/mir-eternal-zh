using System;

namespace LumiSoft.Net.SIP.UA;

[Obsolete("Use SIP stack instead.")]
public class SIP_UA_Call_EventArgs : EventArgs
{
	private SIP_UA_Call m_pCall;

	public SIP_UA_Call Call => m_pCall;

	public SIP_UA_Call_EventArgs(SIP_UA_Call call)
	{
		if (call == null)
		{
			throw new ArgumentNullException("call");
		}
		m_pCall = call;
	}
}
