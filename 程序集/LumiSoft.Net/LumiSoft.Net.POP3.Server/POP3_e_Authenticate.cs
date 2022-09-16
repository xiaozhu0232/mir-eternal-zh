using System;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_Authenticate : EventArgs
{
	private bool m_IsAuthenticated;

	private string m_User = "";

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

	public string User => m_User;

	public string Password => m_Password;

	internal POP3_e_Authenticate(string user, string password)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		m_User = user;
		m_Password = password;
	}
}
