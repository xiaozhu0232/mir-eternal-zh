using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_Client_XOAuth2 : AUTH_SASL_Client
{
	private bool m_IsCompleted;

	private int m_State;

	private string m_UserName;

	private string m_AccessToken;

	public override bool IsCompleted => m_IsCompleted;

	public override string Name => "XOAUTH2";

	public override string UserName => m_UserName;

	public override bool SupportsInitialResponse => true;

	public AUTH_SASL_Client_XOAuth2(string userName, string accessToken)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == "")
		{
			throw new ArgumentException("Argument 'userName' value must be specified.", "userName");
		}
		if (accessToken == null)
		{
			throw new ArgumentNullException("accessToken");
		}
		if (accessToken == "")
		{
			throw new ArgumentException("Argument 'accessToken' value must be specified.", "accessToken");
		}
		m_UserName = userName;
		m_AccessToken = accessToken;
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
			string s = "user=" + m_UserName + "\u0001auth=Bearer " + m_AccessToken + "\u0001\u0001";
			return Encoding.UTF8.GetBytes(s);
		}
		return null;
	}
}
