using System;
using System.Security.Cryptography;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_Client_CramMd5 : AUTH_SASL_Client
{
	private bool m_IsCompleted;

	private int m_State;

	private string m_UserName;

	private string m_Password;

	public override bool IsCompleted => m_IsCompleted;

	public override string Name => "CRAM-MD5";

	public override string UserName => m_UserName;

	public AUTH_SASL_Client_CramMd5(string userName, string password)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == string.Empty)
		{
			throw new ArgumentException("Argument 'username' value must be specified.", "userName");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		m_UserName = userName;
		m_Password = password;
	}

	public override byte[] Continue(byte[] serverResponse)
	{
		if (serverResponse == null)
		{
			throw new ArgumentNullException("serverResponse");
		}
		if (m_IsCompleted)
		{
			throw new InvalidOperationException("Authentication is completed.");
		}
		if (m_State == 0)
		{
			m_State++;
			m_IsCompleted = true;
			string text = Net_Utils.ToHex(new HMACMD5(Encoding.UTF8.GetBytes(m_Password)).ComputeHash(serverResponse)).ToLower();
			return Encoding.UTF8.GetBytes(m_UserName + " " + text);
		}
		throw new InvalidOperationException("Authentication is completed.");
	}
}
