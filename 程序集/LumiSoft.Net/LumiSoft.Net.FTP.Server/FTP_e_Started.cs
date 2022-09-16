using System;

namespace LumiSoft.Net.FTP.Server;

public class FTP_e_Started : EventArgs
{
	private string m_Response;

	public string Response
	{
		get
		{
			return m_Response;
		}
		set
		{
			m_Response = value;
		}
	}

	internal FTP_e_Started(string response)
	{
		m_Response = response;
	}
}
