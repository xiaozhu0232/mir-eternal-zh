using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Login : EventArgs
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

	public string UserName => m_User;

	public string Password => m_Password;

	internal IMAP_e_Login(string user, string password)
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
