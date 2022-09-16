using System;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Namespace : EventArgs
{
	private IMAP_r_u_Namespace m_pNamespaceResponse;

	private IMAP_r_ServerStatus m_pResponse;

	public IMAP_r_u_Namespace NamespaceResponse
	{
		get
		{
			return m_pNamespaceResponse;
		}
		set
		{
			m_pNamespaceResponse = value;
		}
	}

	public IMAP_r_ServerStatus Response
	{
		get
		{
			return m_pResponse;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pResponse = value;
		}
	}

	internal IMAP_e_Namespace(IMAP_r_ServerStatus response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pResponse = response;
	}
}
