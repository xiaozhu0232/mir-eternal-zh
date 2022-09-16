using System;

namespace LumiSoft.Net.FTP.Client;

public class FTP_ClientException : Exception
{
	private int m_StatusCode = 500;

	private string m_ResponseText = "";

	public int StatusCode => m_StatusCode;

	public string ResponseText => m_ResponseText;

	public bool IsPermanentError
	{
		get
		{
			if (m_StatusCode >= 500 && m_StatusCode <= 599)
			{
				return true;
			}
			return false;
		}
	}

	public FTP_ClientException(string responseLine)
		: base(responseLine)
	{
		if (responseLine == null)
		{
			throw new ArgumentNullException("responseLine");
		}
		string[] array = responseLine.Split(new char[1] { ' ' }, 2);
		try
		{
			m_StatusCode = Convert.ToInt32(array[0]);
		}
		catch
		{
		}
		if (array.Length == 2)
		{
			m_ResponseText = array[1];
		}
	}
}
