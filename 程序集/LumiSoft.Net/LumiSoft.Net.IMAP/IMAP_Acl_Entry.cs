using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_Acl_Entry
{
	private string m_Identifier = "";

	private string m_Rights = "";

	public string Identifier => m_Identifier;

	public string Rights => m_Rights;

	public IMAP_Acl_Entry(string identifier, string rights)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (identifier == string.Empty)
		{
			throw new ArgumentException("Argument 'identifier' value must be specified.", "identifier");
		}
		if (rights == null)
		{
			throw new ArgumentNullException("rights");
		}
		m_Identifier = identifier;
		m_Rights = rights;
	}
}
