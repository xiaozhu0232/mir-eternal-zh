using System;

namespace LumiSoft.Net.POP3.Client;

public class POP3_ClientException : Exception
{
	private string m_StatusCode = "";

	private string m_ResponseText = "";

	public string StatusCode => m_StatusCode;

	public string ResponseText => m_ResponseText;

	public POP3_ClientException(string responseLine)
		: base(responseLine)
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		string[] array = responseLine.Split(new char[0], 2);
		m_StatusCode = array[0];
		if (array.Length == 2)
		{
			m_ResponseText = array[1];
		}
	}
}
