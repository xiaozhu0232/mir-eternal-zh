using System;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_Client_DigestMd5 : AUTH_SASL_Client
{
	private bool m_IsCompleted;

	private int m_State;

	private string m_Protocol;

	private string m_ServerName;

	private string m_UserName;

	private string m_Password;

	private AUTH_SASL_DigestMD5_Response m_pResponse;

	public override bool IsCompleted => m_IsCompleted;

	public override string Name => "DIGEST-MD5";

	public override string UserName => m_UserName;

	public AUTH_SASL_Client_DigestMd5(string protocol, string server, string userName, string password)
	{
		if (protocol == null)
		{
			throw new ArgumentNullException("protocol");
		}
		if (protocol == string.Empty)
		{
			throw new ArgumentException("Argument 'protocol' value must be specified.", "userName");
		}
		if (server == null)
		{
			throw new ArgumentNullException("protocol");
		}
		if (server == string.Empty)
		{
			throw new ArgumentException("Argument 'server' value must be specified.", "userName");
		}
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
		m_Protocol = protocol;
		m_ServerName = server;
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
			AUTH_SASL_DigestMD5_Challenge aUTH_SASL_DigestMD5_Challenge = AUTH_SASL_DigestMD5_Challenge.Parse(Encoding.UTF8.GetString(serverResponse));
			m_pResponse = new AUTH_SASL_DigestMD5_Response(aUTH_SASL_DigestMD5_Challenge, aUTH_SASL_DigestMD5_Challenge.Realm[0], m_UserName, m_Password, Guid.NewGuid().ToString().Replace("-", ""), 1, aUTH_SASL_DigestMD5_Challenge.QopOptions[0], m_Protocol + "/" + m_ServerName);
			return Encoding.UTF8.GetBytes(m_pResponse.ToResponse());
		}
		if (m_State == 1)
		{
			m_State++;
			m_IsCompleted = true;
			if (!string.Equals(Encoding.UTF8.GetString(serverResponse), m_pResponse.ToRspauthResponse(m_UserName, m_Password), StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Server server 'rspauth' value mismatch with local 'rspauth' value.");
			}
			return new byte[0];
		}
		throw new InvalidOperationException("Authentication is completed.");
	}
}
