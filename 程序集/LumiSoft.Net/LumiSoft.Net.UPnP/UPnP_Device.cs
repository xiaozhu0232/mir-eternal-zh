using System;
using System.IO;
using System.Xml;

namespace LumiSoft.Net.UPnP;

public class UPnP_Device
{
	private string m_BaseUrl = "";

	private string m_DeviceType = "";

	private string m_FriendlyName = "";

	private string m_Manufacturer = "";

	private string m_ManufacturerUrl = "";

	private string m_ModelDescription = "";

	private string m_ModelName = "";

	private string m_ModelNumber = "";

	private string m_ModelUrl = "";

	private string m_SerialNumber = "";

	private string m_UDN = "";

	private string m_PresentationUrl = "";

	private string m_DeviceXml;

	public string BaseUrl => m_BaseUrl;

	public string DeviceType => m_DeviceType;

	public string FriendlyName => m_FriendlyName;

	public string Manufacturer => m_Manufacturer;

	public string ManufacturerUrl => m_ManufacturerUrl;

	public string ModelDescription => m_ModelDescription;

	public string ModelName => m_ModelName;

	public string ModelNumber => m_ModelNumber;

	public string ModelUrl => m_ModelUrl;

	public string SerialNumber => m_SerialNumber;

	public string UDN => m_UDN;

	public string PresentationUrl => m_PresentationUrl;

	public string DeviceXml => m_DeviceXml;

	internal UPnP_Device(string url)
	{
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		Init(url);
	}

	private void Init(string url)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(url);
		StringWriter stringWriter = new StringWriter();
		xmlDocument.WriteTo(new XmlTextWriter(stringWriter));
		m_DeviceXml = stringWriter.ToString();
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		xmlNamespaceManager.AddNamespace("n", xmlDocument.ChildNodes[1].NamespaceURI);
		m_BaseUrl = ((xmlDocument.SelectSingleNode("n:root/n:URLBase", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:URLBase", xmlNamespaceManager).InnerText : url.Substring(0, url.LastIndexOf("/")));
		m_DeviceType = ((xmlDocument.SelectSingleNode("n:root/n:device/n:deviceType", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:deviceType", xmlNamespaceManager).InnerText : "");
		m_FriendlyName = ((xmlDocument.SelectSingleNode("n:root/n:device/n:friendlyName", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:friendlyName", xmlNamespaceManager).InnerText : "");
		m_Manufacturer = ((xmlDocument.SelectSingleNode("n:root/n:device/n:manufacturer", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:manufacturer", xmlNamespaceManager).InnerText : "");
		m_ManufacturerUrl = ((xmlDocument.SelectSingleNode("n:root/n:device/n:manufacturerURL", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:manufacturerURL", xmlNamespaceManager).InnerText : "");
		m_ModelDescription = ((xmlDocument.SelectSingleNode("n:root/n:device/n:modelDescription", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:modelDescription", xmlNamespaceManager).InnerText : "");
		m_ModelName = ((xmlDocument.SelectSingleNode("n:root/n:device/n:modelName", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:modelName", xmlNamespaceManager).InnerText : "");
		m_ModelNumber = ((xmlDocument.SelectSingleNode("n:root/n:device/n:modelNumber", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:modelNumber", xmlNamespaceManager).InnerText : "");
		m_ModelUrl = ((xmlDocument.SelectSingleNode("n:root/n:device/n:modelURL", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:modelURL", xmlNamespaceManager).InnerText : "");
		m_SerialNumber = ((xmlDocument.SelectSingleNode("n:root/n:device/n:serialNumber", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:serialNumber", xmlNamespaceManager).InnerText : "");
		m_UDN = ((xmlDocument.SelectSingleNode("n:root/n:device/n:UDN", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:UDN", xmlNamespaceManager).InnerText : "");
		m_PresentationUrl = ((xmlDocument.SelectSingleNode("n:root/n:device/n:presentationURL", xmlNamespaceManager) != null) ? xmlDocument.SelectSingleNode("n:root/n:device/n:presentationURL", xmlNamespaceManager).InnerText : "");
	}
}
