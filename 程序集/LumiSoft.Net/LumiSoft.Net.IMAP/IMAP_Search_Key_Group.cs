using System;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.IMAP.Client;

namespace LumiSoft.Net.IMAP;

public class IMAP_Search_Key_Group : IMAP_Search_Key
{
	private List<IMAP_Search_Key> m_pKeys;

	public List<IMAP_Search_Key> Keys => m_pKeys;

	public IMAP_Search_Key_Group()
	{
		m_pKeys = new List<IMAP_Search_Key>();
	}

	public static IMAP_Search_Key_Group Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (r.StartsWith("("))
		{
			r = new StringReader(r.ReadParenthesized());
		}
		IMAP_Search_Key_Group iMAP_Search_Key_Group = new IMAP_Search_Key_Group();
		r.ReadToFirstChar();
		while (r.Available > 0)
		{
			iMAP_Search_Key_Group.m_pKeys.Add(IMAP_Search_Key.ParseKey(r));
		}
		return iMAP_Search_Key_Group;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		for (int i = 0; i < m_pKeys.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(m_pKeys[i].ToString());
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	internal override void ToCmdParts(List<IMAP_Client_CmdPart> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, "("));
		for (int i = 0; i < m_pKeys.Count; i++)
		{
			if (i > 0)
			{
				list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, " "));
			}
			m_pKeys[i].ToCmdParts(list);
		}
		list.Add(new IMAP_Client_CmdPart(IMAP_Client_CmdPart_Type.Constant, ")"));
	}
}
