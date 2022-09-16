using System;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_RegistrationEventArgs : EventArgs
{
	private SIP_Registration m_pRegistration;

	public SIP_Registration Registration => m_pRegistration;

	public SIP_RegistrationEventArgs(SIP_Registration registration)
	{
		if (registration == null)
		{
			throw new ArgumentNullException("registration");
		}
		m_pRegistration = registration;
	}
}
