using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_ServerMechanism_DigestMd5 : AUTH_SASL_ServerMechanism
{
	private bool m_IsCompleted;

	private bool m_IsAuthenticated;

	private bool m_RequireSSL;

	private string m_Realm = "";

	private string m_Nonce = "";

	private string m_UserName = "";

	private int m_State;

	public override bool IsCompleted => m_IsCompleted;

	public override bool IsAuthenticated => m_IsAuthenticated;

	public override string Name => "DIGEST-MD5";

	public override bool RequireSSL => m_RequireSSL;

	public string Realm
	{
		get
		{
			return m_Realm;
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			m_Realm = value;
		}
	}

	public override string UserName => m_UserName;

	public event EventHandler<AUTH_e_UserInfo> GetUserInfo;

	public AUTH_SASL_ServerMechanism_DigestMd5(bool requireSSL)
	{
		m_RequireSSL = requireSSL;
		m_Nonce = Auth_HttpDigest.CreateNonce();
	}

	public override void Reset()
	{
		m_IsCompleted = false;
		m_IsAuthenticated = false;
		m_UserName = "";
		m_State = 0;
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
			AUTH_SASL_DigestMD5_Challenge aUTH_SASL_DigestMD5_Challenge = new AUTH_SASL_DigestMD5_Challenge(new string[1] { m_Realm }, m_Nonce, new string[1] { "auth" }, stale: false);
			return Encoding.UTF8.GetBytes(aUTH_SASL_DigestMD5_Challenge.ToChallenge());
		}
		if (m_State == 1)
		{
			m_State++;
			try
			{
				AUTH_SASL_DigestMD5_Response aUTH_SASL_DigestMD5_Response = AUTH_SASL_DigestMD5_Response.Parse(Encoding.UTF8.GetString(clientResponse));
				if (m_Realm != aUTH_SASL_DigestMD5_Response.Realm || m_Nonce != aUTH_SASL_DigestMD5_Response.Nonce)
				{
					return Encoding.UTF8.GetBytes("rspauth=\"\"");
				}
				m_UserName = aUTH_SASL_DigestMD5_Response.UserName;
				AUTH_e_UserInfo aUTH_e_UserInfo = OnGetUserInfo(aUTH_SASL_DigestMD5_Response.UserName);
				if (aUTH_e_UserInfo.UserExists && aUTH_SASL_DigestMD5_Response.Authenticate(aUTH_e_UserInfo.UserName, aUTH_e_UserInfo.Password))
				{
					m_IsAuthenticated = true;
					return Encoding.UTF8.GetBytes(aUTH_SASL_DigestMD5_Response.ToRspauthResponse(aUTH_e_UserInfo.UserName, aUTH_e_UserInfo.Password));
				}
			}
			catch
			{
			}
			return Encoding.UTF8.GetBytes("rspauth=\"\"");
		}
		m_IsCompleted = true;
		return null;
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
