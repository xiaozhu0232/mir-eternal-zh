using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_ServerMechanism_Plain : AUTH_SASL_ServerMechanism
{
	private bool m_IsCompleted;

	private bool m_IsAuthenticated;

	private bool m_RequireSSL;

	private string m_UserName = "";

	public override bool IsCompleted => m_IsCompleted;

	public override bool IsAuthenticated => m_IsAuthenticated;

	public override string Name => "PLAIN";

	public override bool RequireSSL => m_RequireSSL;

	public override string UserName => m_UserName;

	public event EventHandler<AUTH_e_Authenticate> Authenticate;

	public AUTH_SASL_ServerMechanism_Plain(bool requireSSL)
	{
		m_RequireSSL = requireSSL;
	}

	public override void Reset()
	{
		m_IsCompleted = false;
		m_IsAuthenticated = false;
		m_UserName = "";
	}

	public override byte[] Continue(byte[] clientResponse)
	{
		if (clientResponse == null)
		{
			throw new ArgumentNullException("clientResponse");
		}
		if (clientResponse.Length == 0)
		{
			return new byte[0];
		}
		string[] array = Encoding.UTF8.GetString(clientResponse).Split(default(char));
		if (array.Length == 3 && !string.IsNullOrEmpty(array[1]))
		{
			m_UserName = array[1];
			AUTH_e_Authenticate aUTH_e_Authenticate = OnAuthenticate(array[0], array[1], array[2]);
			m_IsAuthenticated = aUTH_e_Authenticate.IsAuthenticated;
		}
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
