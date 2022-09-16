using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LumiSoft.Net.WebDav;

public class WebDav_MultiStatus
{
	private List<WebDav_Response> m_pResponses;

	public List<WebDav_Response> Responses => m_pResponses;

	public WebDav_MultiStatus()
	{
		m_pResponses = new List<WebDav_Response>();
	}

	internal static WebDav_MultiStatus Parse(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(stream);
		if (!string.Equals(xmlDocument.ChildNodes[1].NamespaceURI + xmlDocument.ChildNodes[1].LocalName, "DAV:multistatus", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Invalid DAV:multistatus value.");
		}
		WebDav_MultiStatus webDav_MultiStatus = new WebDav_MultiStatus();
		foreach (XmlNode childNode in xmlDocument.ChildNodes[1].ChildNodes)
		{
			webDav_MultiStatus.Responses.Add(WebDav_Response.Parse(childNode));
		}
		return webDav_MultiStatus;
	}
}
