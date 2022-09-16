using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LumiSoft.Net.WebDav;

public class WebDav_p_ResourceType : WebDav_p
{
	private List<string> m_pItems;

	public override string Namespace => "DAV:";

	public override string Name => "resourcetype";

	public override string Value
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < m_pItems.Count; i++)
			{
				if (i == m_pItems.Count - 1)
				{
					stringBuilder.Append(m_pItems[i]);
				}
				else
				{
					stringBuilder.Append(m_pItems[i] + ";");
				}
			}
			return stringBuilder.ToString();
		}
	}

	public string[] ResourceTypes => m_pItems.ToArray();

	public WebDav_p_ResourceType()
	{
		m_pItems = new List<string>();
	}

	public bool Contains(string resourceType)
	{
		foreach (string pItem in m_pItems)
		{
			if (string.Equals(resourceType, pItem, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	internal static WebDav_p_ResourceType Parse(XmlNode resourcetypeNode)
	{
		if (resourcetypeNode == null)
		{
			throw new ArgumentNullException("resourcetypeNode");
		}
		if (!string.Equals(resourcetypeNode.NamespaceURI + resourcetypeNode.LocalName, "DAV:resourcetype", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ParseException("Invalid DAV:resourcetype value.");
		}
		WebDav_p_ResourceType webDav_p_ResourceType = new WebDav_p_ResourceType();
		foreach (XmlNode childNode in resourcetypeNode.ChildNodes)
		{
			webDav_p_ResourceType.m_pItems.Add(childNode.NamespaceURI + childNode.LocalName);
		}
		return webDav_p_ResourceType;
	}
}
