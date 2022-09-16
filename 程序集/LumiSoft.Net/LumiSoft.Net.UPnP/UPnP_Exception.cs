using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LumiSoft.Net.UPnP;

public class UPnP_Exception : Exception
{
	private int m_ErrorCode;

	private string m_ErrorText = "";

	public int ErrorCode => m_ErrorCode;

	public string ErrorText => m_ErrorText;

	public UPnP_Exception(int errorCode, string errorText)
		: base("UPnP error: " + errorCode + " " + errorText + ".")
	{
		m_ErrorCode = errorCode;
		m_ErrorText = errorText;
	}

	public static UPnP_Exception Parse(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int num = -1;
		string text = null;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(stream);
			List<XmlNode> list = new List<XmlNode>();
			list.Add(xmlDocument);
			while (list.Count > 0)
			{
				XmlNode xmlNode = list[0];
				list.RemoveAt(0);
				if (string.Equals("UPnPError", xmlNode.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						if (string.Equals("errorCode", childNode.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							num = Convert.ToInt32(childNode.InnerText);
						}
						else if (string.Equals("errorDescription", childNode.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							text = childNode.InnerText;
						}
					}
					break;
				}
				if (xmlNode.ChildNodes.Count > 0)
				{
					for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
					{
						list.Insert(i, xmlNode.ChildNodes[i]);
					}
				}
			}
		}
		catch
		{
		}
		if (num == -1 || text == null)
		{
			throw new ParseException("Failed to parse UPnP error.");
		}
		return new UPnP_Exception(num, text);
	}
}
