using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use class IMAP_r_u_MyRights instead.")]
public class IMAP_Response_MyRights : IMAP_r_u
{
	private string m_FolderName = "";

	private string m_pRights;

	public string FolderName => m_FolderName;

	public string Rights => m_pRights;

	public IMAP_Response_MyRights(string folder, string rights)
	{
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		m_FolderName = folder;
		m_pRights = rights;
	}

	public static IMAP_Response_MyRights Parse(string myRightsResponse)
	{
		if (myRightsResponse == null)
		{
			throw new ArgumentNullException("myRightsResponse");
		}
		StringReader stringReader = new StringReader(myRightsResponse);
		stringReader.ReadWord();
		stringReader.ReadWord();
		string folder = IMAP_Utils.Decode_IMAP_UTF7_String(stringReader.ReadWord(unQuote: true));
		string rights = stringReader.ReadToEnd().Trim();
		return new IMAP_Response_MyRights(folder, rights);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* MYRIGHTS \"" + m_FolderName + "\" \"" + m_pRights + "\"\r\n");
		return stringBuilder.ToString();
	}
}
