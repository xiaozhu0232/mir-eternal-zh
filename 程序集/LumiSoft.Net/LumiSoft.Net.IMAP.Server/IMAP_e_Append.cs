using System;
using System.IO;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Append : EventArgs
{
	private IMAP_r_ServerStatus m_pResponse;

	private string m_Folder;

	private string[] m_pFlags;

	private DateTime m_Date = DateTime.MinValue;

	private int m_Size;

	private Stream m_pStream;

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

	public string Folder => m_Folder;

	public string[] Flags => m_pFlags;

	public DateTime InternalDate => m_Date;

	public int Size => m_Size;

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

	public event EventHandler Completed;

	internal IMAP_e_Append(string folder, string[] flags, DateTime date, int size, IMAP_r_ServerStatus response)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_Folder = folder;
		m_pFlags = flags;
		m_Date = date;
		m_Size = size;
		m_pResponse = response;
	}

	internal void OnCompleted()
	{
		if (this.Completed != null)
		{
			this.Completed(this, new EventArgs());
		}
		this.Completed = null;
	}
}
