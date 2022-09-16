using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_Client_XOAuth : AUTH_SASL_Client
{
	private bool m_IsCompleted;

	private int m_State;

	private string m_UserName;

	private string m_AuthString;

	public override bool IsCompleted => m_IsCompleted;

	public override string Name => "XOAUTH";

	public override string UserName => m_UserName;

	public override bool SupportsInitialResponse => true;

	public AUTH_SASL_Client_XOAuth(string userName, string authString)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == "")
		{
			throw new ArgumentException("Argument 'userName' value must be specified.", "userName");
		}
		if (authString == null)
		{
			throw new ArgumentNullException("authString");
		}
		if (authString == "")
		{
			throw new ArgumentException("Argument 'authString' value must be specified.", "authString");
		}
		m_UserName = userName;
		m_AuthString = authString;
	}

	public override byte[] Continue(byte[] serverResponse)
	{
		if (m_IsCompleted)
		{
			throw new InvalidOperationException("Authentication is completed.");
		}
		if (m_State == 0)
		{
			m_IsCompleted = true;
			return Encoding.UTF8.GetBytes(m_AuthString);
		}
		return null;
	}
}
