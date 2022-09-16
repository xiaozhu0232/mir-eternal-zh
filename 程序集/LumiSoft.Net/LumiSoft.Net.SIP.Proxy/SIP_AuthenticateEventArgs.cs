using LumiSoft.Net.AUTH;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_AuthenticateEventArgs
{
	private Auth_HttpDigest m_pAuth;

	private bool m_Authenticated;

	public Auth_HttpDigest AuthContext => m_pAuth;

	public bool Authenticated
	{
		get
		{
			return m_Authenticated;
		}
		set
		{
			m_Authenticated = value;
		}
	}

	public SIP_AuthenticateEventArgs(Auth_HttpDigest auth)
	{
		m_pAuth = auth;
	}
}
