using System;
using System.Security.Cryptography;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_ServerMechanism_CramMd5 : AUTH_SASL_ServerMechanism
{
	private bool m_IsCompleted;

	private bool m_IsAuthenticated;

	private bool m_RequireSSL;

	private string m_UserName = "";

	private int m_State;

	private string m_Key = "";

	public override bool IsCompleted => m_IsCompleted;

	public override bool IsAuthenticated => m_IsAuthenticated;

	public override string Name => "CRAM-MD5";

	public override bool RequireSSL => m_RequireSSL;

	public override string UserName => m_UserName;

	public event EventHandler<AUTH_e_UserInfo> GetUserInfo;

	public AUTH_SASL_ServerMechanism_CramMd5(bool requireSSL)
	{
		m_RequireSSL = requireSSL;
	}

	public override void Reset()
	{
		m_IsCompleted = false;
		m_IsAuthenticated = false;
		m_UserName = "";
		m_State = 0;
		m_Key = "";
	}

	public override byte[] Continue(byte[] clientResponse)
	{
		if (clientResponse == null)
		{
			throw new ArgumentNullException("clientResponse");
		}
		if (m_State == 0)
		{
			m_State++;
			m_Key = "<" + Guid.NewGuid().ToString() + "@host>";
			return Encoding.UTF8.GetBytes(m_Key);
		}
		string[] array = Encoding.UTF8.GetString(clientResponse).Split(' ');
		if (array.Length == 2 && !string.IsNullOrEmpty(array[0]))
		{
			m_UserName = array[0];
			AUTH_e_UserInfo aUTH_e_UserInfo = OnGetUserInfo(array[0]);
			if (aUTH_e_UserInfo.UserExists && Net_Utils.ToHex(HmacMd5(m_Key, aUTH_e_UserInfo.Password)) == array[1])
			{
				m_IsAuthenticated = true;
			}
		}
		m_IsCompleted = true;
		return null;
	}

	private byte[] HmacMd5(string hashKey, string text)
	{
		return new HMACMD5(Encoding.Default.GetBytes(text)).ComputeHash(Encoding.ASCII.GetBytes(hashKey));
	}

	private AUTH_e_UserInfo OnGetUserInfo(string userName)
	{
		AUTH_e_UserInfo aUTH_e_UserInfo = new AUTH_e_UserInfo(userName);
		if (this.GetUserInfo != null)
		{
			this.GetUserInfo(this, aUTH_e_UserInfo);
		}
		return aUTH_e_UserInfo;
	}
}
