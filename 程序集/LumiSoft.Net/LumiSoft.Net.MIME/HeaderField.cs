using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class HeaderField
{
	private string m_Name = "";

	private string m_Value = "";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (value == "")
			{
				throw new Exception("Header Field name can't be empty !");
			}
			if (!value.EndsWith(":"))
			{
				value += ":";
			}
			string text = value.Substring(0, value.Length - 1);
			foreach (char c in text)
			{
				if (c < '!' || c > '~')
				{
					throw new Exception("Invalid field name '" + value + "'. A field name MUST be composed of printable US-ASCII characters (i.e.,characters that have values between 33 and 126, inclusive),except\tcolon.");
				}
			}
			m_Name = value;
		}
	}

	public string Value
	{
		get
		{
			return MimeUtils.DecodeWords(m_Value);
		}
		set
		{
			m_Value = Core.CanonicalEncode(value, "utf-8");
		}
	}

	internal string EncodedValue => m_Value;

	public HeaderField()
	{
	}

	public HeaderField(string name, string value)
	{
		Name = name;
		Value = value;
	}
}
