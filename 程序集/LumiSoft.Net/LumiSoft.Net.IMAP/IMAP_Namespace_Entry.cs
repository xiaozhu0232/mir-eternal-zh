using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_Namespace_Entry
{
	private string m_NamespaceName = "";

	private char m_Delimiter = '/';

	public string NamespaceName => m_NamespaceName;

	public char HierarchyDelimiter => m_Delimiter;

	public IMAP_Namespace_Entry(string name, char delimiter)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_NamespaceName = name;
	}
}
