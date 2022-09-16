using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Namespace : IMAP_r_u
{
	private IMAP_Namespace_Entry[] m_pPersonalNamespaces;

	private IMAP_Namespace_Entry[] m_pOtherUsersNamespaces;

	private IMAP_Namespace_Entry[] m_pSharedNamespaces;

	public IMAP_Namespace_Entry[] PersonalNamespaces => m_pPersonalNamespaces;

	public IMAP_Namespace_Entry[] OtherUsersNamespaces => m_pOtherUsersNamespaces;

	public IMAP_Namespace_Entry[] SharedNamespaces => m_pSharedNamespaces;

	public IMAP_r_u_Namespace(IMAP_Namespace_Entry[] personalNamespaces, IMAP_Namespace_Entry[] otherUsersNamespaces, IMAP_Namespace_Entry[] sharedNamespaces)
	{
		if (personalNamespaces == null)
		{
			throw new ArgumentNullException("personalNamespaces");
		}
		m_pPersonalNamespaces = personalNamespaces;
		m_pOtherUsersNamespaces = otherUsersNamespaces;
		m_pSharedNamespaces = sharedNamespaces;
	}

	public static IMAP_r_u_Namespace Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		StringReader stringReader = new StringReader(response);
		stringReader.ReadWord();
		stringReader.ReadWord();
		stringReader.ReadToFirstChar();
		List<IMAP_Namespace_Entry> list = new List<IMAP_Namespace_Entry>();
		if (stringReader.SourceString.StartsWith("("))
		{
			StringReader stringReader2 = new StringReader(stringReader.ReadParenthesized());
			while (stringReader2.Available > 0)
			{
				string[] array = TextUtils.SplitQuotedString(stringReader2.ReadParenthesized(), ' ', unquote: true);
				list.Add(new IMAP_Namespace_Entry(array[0], array[1][0]));
			}
		}
		else
		{
			stringReader.ReadWord();
		}
		stringReader.ReadToFirstChar();
		List<IMAP_Namespace_Entry> list2 = new List<IMAP_Namespace_Entry>();
		if (stringReader.SourceString.StartsWith("("))
		{
			StringReader stringReader3 = new StringReader(stringReader.ReadParenthesized());
			while (stringReader3.Available > 0)
			{
				string[] array2 = TextUtils.SplitQuotedString(stringReader3.ReadParenthesized(), ' ', unquote: true);
				list2.Add(new IMAP_Namespace_Entry(array2[0], array2[1][0]));
			}
		}
		else
		{
			stringReader.ReadWord();
		}
		stringReader.ReadToFirstChar();
		List<IMAP_Namespace_Entry> list3 = new List<IMAP_Namespace_Entry>();
		if (stringReader.SourceString.StartsWith("("))
		{
			StringReader stringReader4 = new StringReader(stringReader.ReadParenthesized());
			while (stringReader4.Available > 0)
			{
				string[] array3 = TextUtils.SplitQuotedString(stringReader4.ReadParenthesized(), ' ', unquote: true);
				list3.Add(new IMAP_Namespace_Entry(array3[0], array3[1][0]));
			}
		}
		else
		{
			stringReader.ReadWord();
		}
		return new IMAP_r_u_Namespace(list.ToArray(), list2.ToArray(), list3.ToArray());
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* NAMESPACE ");
		if (m_pPersonalNamespaces != null && m_pPersonalNamespaces.Length != 0)
		{
			stringBuilder.Append("(");
			for (int i = 0; i < m_pPersonalNamespaces.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append("(\"" + m_pPersonalNamespaces[i].NamespaceName + "\" \"" + m_pPersonalNamespaces[i].HierarchyDelimiter + "\")");
			}
			stringBuilder.Append(")");
		}
		else
		{
			stringBuilder.Append("NIL");
		}
		stringBuilder.Append(" ");
		if (m_pOtherUsersNamespaces != null && m_pOtherUsersNamespaces.Length != 0)
		{
			stringBuilder.Append("(");
			for (int j = 0; j < m_pOtherUsersNamespaces.Length; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append("(\"" + m_pOtherUsersNamespaces[j].NamespaceName + "\" \"" + m_pOtherUsersNamespaces[j].HierarchyDelimiter + "\")");
			}
			stringBuilder.Append(")");
		}
		else
		{
			stringBuilder.Append("NIL");
		}
		stringBuilder.Append(" ");
		if (m_pSharedNamespaces != null && m_pSharedNamespaces.Length != 0)
		{
			stringBuilder.Append("(");
			for (int k = 0; k < m_pSharedNamespaces.Length; k++)
			{
				if (k > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append("(\"" + m_pSharedNamespaces[k].NamespaceName + "\" \"" + m_pSharedNamespaces[k].HierarchyDelimiter + "\")");
			}
			stringBuilder.Append(")");
		}
		else
		{
			stringBuilder.Append("NIL");
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
