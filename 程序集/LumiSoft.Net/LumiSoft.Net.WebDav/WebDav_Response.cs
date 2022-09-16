using System;
using System.Collections.Generic;
using System.Xml;

namespace LumiSoft.Net.WebDav;

public class WebDav_Response
{
	private string m_HRef;

	private List<WebDav_PropStat> m_pPropStats;

	public string HRef => m_HRef;

	public WebDav_PropStat[] PropStats => m_pPropStats.ToArray();

	internal WebDav_Response()
	{
		m_pPropStats = new List<WebDav_PropStat>();
	}

	internal static WebDav_Response Parse(XmlNode reponseNode)
	{
		if (reponseNode == null)
		{
			throw new ArgumentNullException("responseNode");
		}
		if (!string.Equals(reponseNode.NamespaceURI + reponseNode.LocalName, "DAV:response", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Invalid DAV:response value.");
		}
		WebDav_Response webDav_Response = new WebDav_Response();
		foreach (XmlNode childNode in reponseNode.ChildNodes)
		{
			if (string.Equals(childNode.LocalName, "href", StringComparison.InvariantCultureIgnoreCase))
			{
				webDav_Response.m_HRef = childNode.ChildNodes[0].Value;
			}
			else if (string.Equals(childNode.LocalName, "propstat", StringComparison.InvariantCultureIgnoreCase))
			{
				webDav_Response.m_pPropStats.Add(WebDav_PropStat.Parse(childNode));
			}
		}
		return webDav_Response;
	}
}
