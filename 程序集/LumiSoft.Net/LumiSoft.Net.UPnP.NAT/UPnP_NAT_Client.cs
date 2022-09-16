using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using LumiSoft.Net.UPnP.Client;

namespace LumiSoft.Net.UPnP.NAT;

public class UPnP_NAT_Client
{
	private static string m_BaseUrl;

	private static string m_ServiceType;

	private static string m_ControlUrl;

	public bool IsSupported => m_ControlUrl != null;

	public UPnP_NAT_Client()
	{
		Init();
	}

	private void Init()
	{
		try
		{
			UPnP_Client uPnP_Client = new UPnP_Client();
			UPnP_Device[] array = null;
			try
			{
				IPAddress ip = null;
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					if (networkInterface.OperationalStatus != OperationalStatus.Up)
					{
						continue;
					}
					using (IEnumerator<GatewayIPAddressInformation> enumerator = networkInterface.GetIPProperties().GatewayAddresses.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							ip = enumerator.Current.Address;
						}
					}
					break;
				}
				array = uPnP_Client.Search(ip, "urn:schemas-upnp-org:device:InternetGatewayDevice:1", 1200);
			}
			catch
			{
			}
			if (array.Length == 0)
			{
				array = uPnP_Client.Search("urn:schemas-upnp-org:device:InternetGatewayDevice:1", 1200);
			}
			if (array.Length == 0)
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(array[0].DeviceXml);
			List<XmlNode> list = new List<XmlNode>();
			list.Add(xmlDocument);
			while (list.Count > 0)
			{
				XmlNode xmlNode = list[0];
				list.RemoveAt(0);
				if (string.Equals("urn:schemas-upnp-org:service:WANPPPConnection:1", xmlNode.InnerText, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (XmlNode childNode in xmlNode.ParentNode.ChildNodes)
					{
						if (string.Equals("controlURL", childNode.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							m_BaseUrl = array[0].BaseUrl;
							m_ServiceType = "urn:schemas-upnp-org:service:WANPPPConnection:1";
							m_ControlUrl = childNode.InnerText;
							return;
						}
					}
				}
				else if (string.Equals("urn:schemas-upnp-org:service:WANIPConnection:1", xmlNode.InnerText, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (XmlNode childNode2 in xmlNode.ParentNode.ChildNodes)
					{
						if (string.Equals("controlURL", childNode2.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							m_BaseUrl = array[0].BaseUrl;
							m_ServiceType = "urn:schemas-upnp-org:service:WANIPConnection:1";
							m_ControlUrl = childNode2.InnerText;
							return;
						}
					}
				}
				else if (xmlNode.ChildNodes.Count > 0)
				{
					for (int j = 0; j < xmlNode.ChildNodes.Count; j++)
					{
						list.Insert(j, xmlNode.ChildNodes[j]);
					}
				}
			}
		}
		catch
		{
		}
	}

	public IPAddress GetExternalIPAddress()
	{
		string soapData = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<s:Body>\r\n<u:GetExternalIPAddress xmlns:u=\"" + m_ServiceType + "\"></u:GetExternalIPAddress>\r\n</s:Body>\r\n</s:Envelope>\r\n";
		XmlReader xmlReader = XmlReader.Create(new System.IO.StringReader(SendCommand("GetExternalIPAddress", soapData)));
		while (xmlReader.Read())
		{
			if (string.Equals("NewExternalIPAddress", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return IPAddress.Parse(xmlReader.ReadString());
			}
		}
		return null;
	}

	public UPnP_NAT_Map[] GetPortMappings()
	{
		List<UPnP_NAT_Map> list = new List<UPnP_NAT_Map>();
		for (int i = 0; i < 100; i++)
		{
			try
			{
				string soapData = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<s:Body>\r\n<u:GetGenericPortMappingEntry xmlns:u=\"" + m_ServiceType + "\">\r\n<NewPortMappingIndex>" + i + "</NewPortMappingIndex>\r\n</u:GetGenericPortMappingEntry>\r\n</s:Body>\r\n</s:Envelope>\r\n";
				string s = SendCommand("GetGenericPortMappingEntry", soapData);
				bool enabled = false;
				string protocol = "";
				string remoteHost = "";
				string externalPort = "";
				string internalHost = "";
				int internalPort = 0;
				string description = "";
				int leaseDuration = 0;
				XmlReader xmlReader = XmlReader.Create(new System.IO.StringReader(s));
				while (xmlReader.Read())
				{
					if (string.Equals("NewRemoteHost", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						remoteHost = xmlReader.ReadString();
					}
					else if (string.Equals("NewExternalPort", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						externalPort = xmlReader.ReadString();
					}
					else if (string.Equals("NewProtocol", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						protocol = xmlReader.ReadString();
					}
					else if (string.Equals("NewInternalPort", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						internalPort = Convert.ToInt32(xmlReader.ReadString());
					}
					else if (string.Equals("NewInternalClient", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						internalHost = xmlReader.ReadString();
					}
					else if (string.Equals("NewEnabled", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						enabled = Convert.ToBoolean(Convert.ToInt32(xmlReader.ReadString()));
					}
					else if (string.Equals("NewPortMappingDescription", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						description = xmlReader.ReadString();
					}
					else if (string.Equals("NewLeaseDuration", xmlReader.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						leaseDuration = Convert.ToInt32(xmlReader.ReadString());
					}
				}
				list.Add(new UPnP_NAT_Map(enabled, protocol, remoteHost, externalPort, internalHost, internalPort, description, leaseDuration));
			}
			catch (WebException ex)
			{
				if (ex.Response.ContentType.ToLower().IndexOf("text/xml") > -1)
				{
					UPnP_Exception ex2 = UPnP_Exception.Parse(ex.Response.GetResponseStream());
					if (ex2.ErrorCode != 713)
					{
						throw ex2;
					}
					break;
				}
				throw ex;
			}
		}
		return list.ToArray();
	}

	public void AddPortMapping(bool enabled, string description, string protocol, string remoteHost, int publicPort, IPEndPoint localEP, int leaseDuration)
	{
		if (description == null)
		{
			throw new ArgumentNullException("description");
		}
		if (protocol == null)
		{
			throw new ArgumentNullException("protocol");
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		try
		{
			string soapData = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<s:Body>\r\n<u:AddPortMapping xmlns:u=\"" + m_ServiceType + "\">\r\n<NewRemoteHost>" + remoteHost + "</NewRemoteHost>\r\n<NewExternalPort>" + publicPort + "</NewExternalPort>\r\n<NewProtocol>" + protocol + "</NewProtocol>\r\n<NewInternalPort>" + localEP.Port + "</NewInternalPort>\r\n<NewInternalClient>" + localEP.Address.ToString() + "</NewInternalClient>\r\n<NewEnabled>" + Convert.ToInt32(enabled) + "</NewEnabled>\r\n<NewPortMappingDescription>" + description + "</NewPortMappingDescription>\r\n<NewLeaseDuration>" + leaseDuration + "</NewLeaseDuration>\r\n</u:AddPortMapping>\r\n</s:Body>\r\n</s:Envelope>\r\n";
			SendCommand("AddPortMapping", soapData);
		}
		catch (WebException ex)
		{
			if (ex.Response.ContentType.ToLower().IndexOf("text/xml") > -1)
			{
				throw UPnP_Exception.Parse(ex.Response.GetResponseStream());
			}
		}
	}

	public void DeletePortMapping(UPnP_NAT_Map map)
	{
		if (map == null)
		{
			throw new ArgumentNullException("map");
		}
		DeletePortMapping(map.Protocol, map.RemoteHost, Convert.ToInt32(map.ExternalPort));
	}

	public void DeletePortMapping(string protocol, string remoteHost, int publicPort)
	{
		if (protocol == null)
		{
			throw new ArgumentNullException("protocol");
		}
		try
		{
			string soapData = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<s:Body>\r\n<u:DeletePortMapping xmlns:u=\"" + m_ServiceType + "\">\r\n<NewRemoteHost>" + remoteHost + "</NewRemoteHost>\r\n<NewExternalPort>" + publicPort + "</NewExternalPort>\r\n<NewProtocol>" + protocol + "</NewProtocol>\r\n</u:DeletePortMapping>\r\n</s:Body>\r\n</s:Envelope>\r\n";
			SendCommand("DeletePortMapping", soapData);
		}
		catch (WebException ex)
		{
			if (ex.Response.ContentType.ToLower().IndexOf("text/xml") > -1)
			{
				throw UPnP_Exception.Parse(ex.Response.GetResponseStream());
			}
		}
	}

	private string SendCommand(string method, string soapData)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(soapData);
		WebRequest webRequest = WebRequest.Create(m_BaseUrl + m_ControlUrl);
		webRequest.Method = "POST";
		webRequest.Headers.Add("SOAPAction", m_ServiceType + "#" + method);
		webRequest.ContentType = "text/xml; charset=\"utf-8\";";
		webRequest.ContentLength = bytes.Length;
		webRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
		webRequest.GetRequestStream().Close();
		using TextReader textReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
		return textReader.ReadToEnd();
	}
}
