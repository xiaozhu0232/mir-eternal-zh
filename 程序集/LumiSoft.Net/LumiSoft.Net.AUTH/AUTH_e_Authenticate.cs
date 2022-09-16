using System;

namespace LumiSoft.Net.AUTH;

public class AUTH_e_Authenticate : EventArgs
{
	private bool m_IsAuthenticated;

	private string m_AuthorizationID = "";

	private string m_UserName = "";

	private string m_Password = "";

	public bool IsAuthenticated
	{
		get
		{
			return m_IsAuthenticated;
		}
		set
		{
			m_IsAuthenticated = value;
		}
	}

	public string AuthorizationID => m_AuthorizationID;

	public string UserName => m_UserName;

	public string Password => m_Password;

	public AUTH_e_Authenticate(string authorizationID, string userName, string password)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == string.Empty)
		{
			throw new ArgumentException("Argument 'userName' value must be specified.", "userName");
		}
		m_AuthorizationID = authorizationID;
		m_UserName = userName;
		m_Password = password;
	}
}
