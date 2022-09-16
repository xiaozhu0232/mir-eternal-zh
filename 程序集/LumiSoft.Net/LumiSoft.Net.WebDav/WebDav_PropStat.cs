using System;
using System.Xml;

namespace LumiSoft.Net.WebDav;

public class WebDav_PropStat
{
	private string m_Status;

	private string m_ResponseDescription;

	private WebDav_Prop m_pProp;

	public string Status => m_Status;

	public string ResponseDescription => m_ResponseDescription;

	public WebDav_Prop Prop => m_pProp;

	internal WebDav_PropStat()
	{
	}

	internal static WebDav_PropStat Parse(XmlNode propstatNode)
	{
		if (propstatNode == null)
		{
			throw new ArgumentNullException("propstatNode");
		}
		if (!string.Equals(propstatNode.NamespaceURI + propstatNode.LocalName, "DAV:propstat", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Invalid DAV:propstat value.");
		}
		WebDav_PropStat webDav_PropStat = new WebDav_PropStat();
		foreach (XmlNode childNode in propstatNode.ChildNodes)
		{
			if (string.Equals(childNode.LocalName, "status", StringComparison.InvariantCultureIgnoreCase))
			{
				webDav_PropStat.m_Status = childNode.ChildNodes[0].Value;
			}
			else if (string.Equals(childNode.LocalName, "prop", StringComparison.InvariantCultureIgnoreCase))
			{
				webDav_PropStat.m_pProp = WebDav_Prop.Parse(childNode);
			}
		}
		return webDav_PropStat;
	}
}
