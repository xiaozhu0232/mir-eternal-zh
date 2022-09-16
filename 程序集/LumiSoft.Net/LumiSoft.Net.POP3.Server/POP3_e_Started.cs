using System;

namespace LumiSoft.Net.POP3.Server;

public class POP3_e_Started : EventArgs
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

	internal POP3_e_Started(string response)
	{
		m_Response = response;
	}
}
