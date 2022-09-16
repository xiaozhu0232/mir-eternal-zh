using System;

namespace LumiSoft.Net.IMAP.Client;

public class IMAP_ClientException : Exception
{
	private IMAP_r_ServerStatus m_pResponse;

	public IMAP_r_ServerStatus Response => m_pResponse;

	public string StatusCode => m_pResponse.ResponseCode;

	public string ResponseText => m_pResponse.ResponseText;

	public IMAP_ClientException(IMAP_r_ServerStatus response)
		: base(response.ToString())
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
	}

	public IMAP_ClientException(string responseLine)
		: base(responseLine)
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		m_pResponse = IMAP_r_ServerStatus.Parse(responseLine);
	}

	public IMAP_ClientException(string responseCode, string responseText)
		: base(responseCode + " " + responseText)
	{
		if (responseCode == null)
		{
			throw new ArgumentNullException("responseCode");
		}
		if (responseCode == string.Empty)
		{
			throw new ArgumentException("Argument 'responseCode' value must be specified.", "responseCode");
		}
		if (responseText == null)
		{
			throw new ArgumentNullException("responseText");
		}
		if (responseText == string.Empty)
		{
			throw new ArgumentException("Argument 'responseText' value must be specified.", "responseText");
		}
		m_pResponse = IMAP_r_ServerStatus.Parse(responseCode + " " + responseText);
	}
}
