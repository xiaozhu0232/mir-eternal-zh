using System;

namespace LumiSoft.Net.AUTH;

public class AUTH_e_UserInfo : EventArgs
{
	private bool m_UserExists;

	private string m_UserName = "";

	private string m_Password = "";

	public bool UserExists
	{
		get
		{
			return m_UserExists;
		}
		set
		{
			m_UserExists = value;
		}
	}

	public string UserName => m_UserName;

	public string Password
	{
		get
		{
			return m_Password;
		}
		set
		{
			m_Password = value;
		}
	}

	public AUTH_e_UserInfo(string userName)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == string.Empty)
		{
			throw new ArgumentException("Argument 'userName' value must be specified.", "userName");
		}
		m_UserName = userName;
	}
}
