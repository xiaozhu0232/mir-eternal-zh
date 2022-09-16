using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SDP;

public class SDP_MediaDescription
{
	private string m_MediaType = "";

	private int m_Port;

	private int m_NumberOfPorts = 1;

	private string m_Protocol = "";

	private List<string> m_pMediaFormats;

	private string m_Information;

	private SDP_Connection m_pConnection;

	private string m_Bandwidth;

	private List<SDP_Attribute> m_pAttributes;

	private Dictionary<string, object> m_pTags;

	public string MediaType => m_MediaType;

	public int Port
	{
		get
		{
			return m_Port;
		}
		set
		{
			m_Port = value;
		}
	}

	public int NumberOfPorts
	{
		get
		{
			return m_NumberOfPorts;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Property NumberOfPorts must be >= 1 !");
			}
			m_NumberOfPorts = value;
		}
	}

	public string Protocol
	{
		get
		{
			return m_Protocol;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Protocol cant be null or empty !");
			}
			m_Protocol = value;
		}
	}

	public List<string> MediaFormats => m_pMediaFormats;

	public string Information
	{
		get
		{
			return m_Information;
		}
		set
		{
			m_Information = value;
		}
	}

	public SDP_Connection Connection
	{
		get
		{
			return m_pConnection;
		}
		set
		{
			m_pConnection = value;
		}
	}

	public string Bandwidth
	{
		get
		{
			return m_Bandwidth;
		}
		set
		{
			m_Bandwidth = value;
		}
	}

	public List<SDP_Attribute> Attributes => m_pAttributes;

	public Dictionary<string, object> Tags => m_pTags;

	public SDP_MediaDescription(string mediaType, int port, int ports, string protocol, string[] mediaFormats)
	{
		if (mediaType == null)
		{
			throw new ArgumentNullException("mediaType");
		}
		if (mediaType == string.Empty)
		{
			throw new ArgumentException("Argument 'mediaType' value must be specified.");
		}
		if (port < 0)
		{
			throw new ArgumentException("Argument 'port' value must be >= 0.");
		}
		if (ports < 0)
		{
			throw new ArgumentException("Argument 'ports' value must be >= 0.");
		}
		if (protocol == null)
		{
			throw new ArgumentNullException("protocol");
		}
		if (protocol == string.Empty)
		{
			throw new ArgumentException("Argument 'protocol' value msut be specified.");
		}
		m_MediaType = mediaType;
		m_Port = port;
		m_NumberOfPorts = ports;
		m_Protocol = protocol;
		m_pMediaFormats = new List<string>();
		m_pAttributes = new List<SDP_Attribute>();
		m_pTags = new Dictionary<string, object>();
		if (mediaFormats != null)
		{
			m_pMediaFormats.AddRange(mediaFormats);
		}
	}

	private SDP_MediaDescription()
	{
		m_pMediaFormats = new List<string>();
		m_pAttributes = new List<SDP_Attribute>();
		m_pTags = new Dictionary<string, object>();
	}

	public static SDP_MediaDescription Parse(string mValue)
	{
		SDP_MediaDescription sDP_MediaDescription = new SDP_MediaDescription();
		StringReader stringReader = new StringReader(mValue);
		stringReader.QuotedReadToDelimiter('=');
		string text = stringReader.ReadWord();
		if (text == null)
		{
			throw new Exception("SDP message \"m\" field <media> value is missing !");
		}
		sDP_MediaDescription.m_MediaType = text;
		text = stringReader.ReadWord();
		if (text == null)
		{
			throw new Exception("SDP message \"m\" field <port> value is missing !");
		}
		if (text.IndexOf('/') > -1)
		{
			string[] array = text.Split('/');
			sDP_MediaDescription.m_Port = Convert.ToInt32(array[0]);
			sDP_MediaDescription.m_NumberOfPorts = Convert.ToInt32(array[1]);
		}
		else
		{
			sDP_MediaDescription.m_Port = Convert.ToInt32(text);
			sDP_MediaDescription.m_NumberOfPorts = 1;
		}
		text = stringReader.ReadWord();
		if (text == null)
		{
			throw new Exception("SDP message \"m\" field <proto> value is missing !");
		}
		sDP_MediaDescription.m_Protocol = text;
		for (text = stringReader.ReadWord(); text != null; text = stringReader.ReadWord())
		{
			sDP_MediaDescription.MediaFormats.Add(text);
		}
		return sDP_MediaDescription;
	}

	public void SetStreamMode(string streamMode)
	{
		if (streamMode == null)
		{
			throw new ArgumentNullException("streamMode");
		}
		for (int i = 0; i < m_pAttributes.Count; i++)
		{
			SDP_Attribute sDP_Attribute = m_pAttributes[i];
			if (string.Equals(sDP_Attribute.Name, "sendrecv", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pAttributes.RemoveAt(i);
				i--;
			}
			else if (string.Equals(sDP_Attribute.Name, "sendonly", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pAttributes.RemoveAt(i);
				i--;
			}
			else if (string.Equals(sDP_Attribute.Name, "recvonly", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pAttributes.RemoveAt(i);
				i--;
			}
			else if (string.Equals(sDP_Attribute.Name, "inactive", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pAttributes.RemoveAt(i);
				i--;
			}
		}
		if (streamMode != "")
		{
			m_pAttributes.Add(new SDP_Attribute(streamMode, ""));
		}
	}

	public string ToValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (NumberOfPorts > 1)
		{
			stringBuilder.Append("m=" + MediaType + " " + Port + "/" + NumberOfPorts + " " + Protocol);
		}
		else
		{
			stringBuilder.Append("m=" + MediaType + " " + Port + " " + Protocol);
		}
		foreach (string mediaFormat in MediaFormats)
		{
			stringBuilder.Append(" " + mediaFormat);
		}
		stringBuilder.Append("\r\n");
		if (!string.IsNullOrEmpty(m_Information))
		{
			stringBuilder.Append("i=" + m_Information + "\r\n");
		}
		if (!string.IsNullOrEmpty(m_Bandwidth))
		{
			stringBuilder.Append("b=" + m_Bandwidth + "\r\n");
		}
		if (m_pConnection != null)
		{
			stringBuilder.Append(m_pConnection.ToValue());
		}
		foreach (SDP_Attribute attribute in Attributes)
		{
			stringBuilder.Append(attribute.ToValue());
		}
		return stringBuilder.ToString();
	}
}
