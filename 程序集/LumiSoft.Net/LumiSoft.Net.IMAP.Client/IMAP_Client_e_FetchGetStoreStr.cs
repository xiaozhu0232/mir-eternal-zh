using System;
using System.IO;

namespace LumiSoft.Net.IMAP.Client;

public class IMAP_Client_e_FetchGetStoreStream : EventArgs
{
	private IMAP_r_u_Fetch m_pFetchResponse;

	private IMAP_t_Fetch_r_i m_pDataItem;

	private Stream m_pStream;

	public IMAP_r_u_Fetch FetchResponse => m_pFetchResponse;

	public IMAP_t_Fetch_r_i DataItem => m_pDataItem;

	public Stream Stream
	{
		get
		{
			return m_pStream;
		}
		set
		{
			m_pStream = value;
		}
	}

	public IMAP_Client_e_FetchGetStoreStream(IMAP_r_u_Fetch fetch, IMAP_t_Fetch_r_i dataItem)
	{
		if (fetch == null)
		{
			throw new ArgumentNullException("fetch");
		}
		if (dataItem == null)
		{
			throw new ArgumentNullException("dataItem");
		}
		m_pFetchResponse = fetch;
		m_pDataItem = dataItem;
	}
}
