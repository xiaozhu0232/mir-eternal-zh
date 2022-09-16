using System;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_MsgFlags
{
	public static readonly string Seen = "\\Seen";

	public static readonly string Answered = "\\Answered";

	public static readonly string Flagged = "\\Flagged";

	public static readonly string Deleted = "\\Deleted";

	public static readonly string Draft = "\\Draft";

	public static readonly string Recent = "\\Recent";

	private KeyValueCollection<string, string> m_pFlags;

	public int Count => m_pFlags.Count;

	public IMAP_t_MsgFlags(params string[] flags)
	{
		m_pFlags = new KeyValueCollection<string, string>();
		if (flags == null)
		{
			return;
		}
		foreach (string text in flags)
		{
			if (!string.IsNullOrEmpty(text))
			{
				m_pFlags.Add(text.ToLower(), text);
			}
		}
	}

	public static IMAP_t_MsgFlags Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value = value.Trim();
		if (value.StartsWith("(") && value.EndsWith(")"))
		{
			value = value.Substring(1, value.Length - 2);
		}
		string[] flags = new string[0];
		if (!string.IsNullOrEmpty(value))
		{
			flags = value.Split(' ');
		}
		return new IMAP_t_MsgFlags(flags);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(array[i]);
		}
		return stringBuilder.ToString();
	}

	public bool Contains(string flag)
	{
		if (flag == null)
		{
			throw new ArgumentNullException("flag");
		}
		return m_pFlags.ContainsKey(flag.ToLower());
	}

	public string[] ToArray()
	{
		return m_pFlags.ToArray();
	}
}
