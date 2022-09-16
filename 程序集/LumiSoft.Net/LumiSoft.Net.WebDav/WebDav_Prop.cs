using System;
using System.Collections.Generic;
using System.Xml;

namespace LumiSoft.Net.WebDav;

public class WebDav_Prop
{
	private List<WebDav_p> m_pProperties;

	public WebDav_p[] Properties => m_pProperties.ToArray();

	public WebDav_p_ResourceType Prop_ResourceType
	{
		get
		{
			foreach (WebDav_p pProperty in m_pProperties)
			{
				if (pProperty is WebDav_p_ResourceType)
				{
					return (WebDav_p_ResourceType)pProperty;
				}
			}
			return null;
		}
	}

	public WebDav_Prop()
	{
		m_pProperties = new List<WebDav_p>();
	}

	internal static WebDav_Prop Parse(XmlNode propNode)
	{
		if (propNode == null)
		{
			throw new ArgumentNullException("propNode");
		}
		if (!string.Equals(propNode.NamespaceURI + propNode.LocalName, "DAV:prop", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Invalid DAV:prop value.");
		}
		WebDav_Prop webDav_Prop = new WebDav_Prop();
		foreach (XmlNode childNode in propNode.ChildNodes)
		{
			if (string.Equals(childNode.LocalName, "resourcetype", StringComparison.InvariantCultureIgnoreCase))
			{
				webDav_Prop.m_pProperties.Add(WebDav_p_ResourceType.Parse(childNode));
			}
			else
			{
				webDav_Prop.m_pProperties.Add(new WebDav_p_Default(childNode.NamespaceURI, childNode.LocalName, childNode.InnerXml));
			}
		}
		return webDav_Prop;
	}
}
