using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_ServerMechanism_Login : AUTH_SASL_ServerMechanism
{
	private bool m_IsCompleted;

	private bool m_IsAuthenticated;

	private bool m_RequireSSL;

	private string m_UserName;

	private string m_Password;

	private int m_State;

	public override bool IsCompleted => m_IsCompleted;

	public override bool IsAuthenticated => m_IsAuthenticated;

	public override string Name => "LOGIN";

	public override bool RequireSSL => m_RequireSSL;

	public override string UserName => m_UserName;

	public event EventHandler<AUTH_e_Authenticate> Authenticate;

	public AUTH_SASL_ServerMechanism_Login(bool requireSSL)
	{
		m_RequireSSL = requireSSL;
	}

	public override void Reset()
	{
		m_IsCompleted = false;
		m_IsAuthenticated = false;
		m_UserName = null;
		m_Password = null;
		m_State = 0;
	}

	public override byte[] Continue(byte[] clientResponse)
	{
		if (clientResponse == null)
		{
			throw new ArgumentNullException("clientResponse");
		}
		if (m_State == 0 && clientResponse.Length != 0)
		{
			m_State++;
		}
		if (m_State == 0)
		{
			m_State++;
			return Encoding.ASCII.GetBytes("UserName:");
		}
		if (m_State == 1)
		{
			m_State++;
			m_UserName = Encoding.UTF8.GetString(clientResponse);
			return Encoding.ASCII.GetBytes("Password:");
		}
		m_Password = Encoding.UTF8.GetString(clientResponse);
		AUTH_e_Authenticate aUTH_e_Authenticate = OnAuthenticate("", m_UserName, m_Password);
		m_IsAuthenticated = aUTH_e_Authenticate.IsAuthenticated;
		m_IsCompleted = true;
		return null;
	}

	private AUTH_e_Authenticate OnAuthenticate(string authorizationID, string userName, string password)
	{
		AUTH_e_Authenticate aUTH_e_Authenticate = new AUTH_e_Authenticate(authorizationID, userName, password);
		if (this.Authenticate != null)
		{
			this.Authenticate(this, aUTH_e_Authenticate);
		}
		return aUTH_e_Authenticate;
	}
}
