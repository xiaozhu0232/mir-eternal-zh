using System;

namespace LumiSoft.Net.FTP;

public class FTP_ListItem
{
	private string m_Name = "";

	private long m_Size;

	private DateTime m_Modified;

	private bool m_IsDir;

	public bool IsDir => m_IsDir;

	public bool IsFile => !m_IsDir;

	public string Name => m_Name;

	public long Size => m_Size;

	public DateTime Modified => m_Modified;

	public FTP_ListItem(string name, long size, DateTime modified, bool isDir)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == "")
		{
			throw new ArgumentException("Argument 'name' value must be specified.");
		}
		m_Name = name;
		m_Size = size;
		m_Modified = modified;
		m_IsDir = isDir;
	}
}
