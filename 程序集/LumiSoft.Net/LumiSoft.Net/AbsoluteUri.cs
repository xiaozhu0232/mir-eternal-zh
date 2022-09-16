using System;

namespace LumiSoft.Net;

public class AbsoluteUri
{
	private string m_Scheme = "";

	private string m_Value = "";

	public virtual string Scheme => m_Scheme;

	public string Value => ToString().Split(new char[1] { ':' }, 2)[1];

	internal AbsoluteUri()
	{
	}

	public static AbsoluteUri Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value == "")
		{
			throw new ArgumentException("Argument 'value' value must be specified.");
		}
		string[] array = value.Split(new char[1] { ':' }, 2);
		if (array[0].ToLower() == "sip" || array[0].ToLower() == "sips")
		{
			SIP_Uri sIP_Uri = new SIP_Uri();
			sIP_Uri.ParseInternal(value);
			return sIP_Uri;
		}
		AbsoluteUri absoluteUri = new AbsoluteUri();
		absoluteUri.ParseInternal(value);
		return absoluteUri;
	}

	protected virtual void ParseInternal(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		string[] array = value.Split(new char[1] { ':' }, 1);
		m_Scheme = array[0].ToLower();
		if (array.Length == 2)
		{
			m_Value = array[1];
		}
	}

	public override string ToString()
	{
		return m_Scheme + ":" + m_Value;
	}
}
