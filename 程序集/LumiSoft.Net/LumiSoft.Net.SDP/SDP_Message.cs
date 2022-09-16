using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LumiSoft.Net.SDP;

public class SDP_Message
{
	private string m_Version = "0";

	private SDP_Origin m_pOrigin;

	private string m_SessionName = "";

	private string m_SessionDescription = "";

	private string m_Uri = "";

	private SDP_Connection m_pConnectionData;

	private List<SDP_Time> m_pTimes;

	private string m_RepeatTimes = "";

	private List<SDP_Attribute> m_pAttributes;

	private List<SDP_MediaDescription> m_pMediaDescriptions;

	public string Version
	{
		get
		{
			return m_Version;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Version can't be null or empty !");
			}
			m_Version = value;
		}
	}

	public SDP_Origin Origin
	{
		get
		{
			return m_pOrigin;
		}
		set
		{
			m_pOrigin = value;
		}
	}

	public string SessionName
	{
		get
		{
			return m_SessionName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property SessionName can't be null or empty !");
			}
			m_SessionName = value;
		}
	}

	public string SessionDescription
	{
		get
		{
			return m_SessionDescription;
		}
		set
		{
			m_SessionDescription = value;
		}
	}

	public string Uri
	{
		get
		{
			return m_Uri;
		}
		set
		{
			m_Uri = value;
		}
	}

	public SDP_Connection Connection
	{
		get
		{
			return m_pConnectionData;
		}
		set
		{
			m_pConnectionData = value;
		}
	}

	public List<SDP_Time> Times => m_pTimes;

	public string RepeatTimes
	{
		get
		{
			return m_RepeatTimes;
		}
		set
		{
			m_RepeatTimes = value;
		}
	}

	public List<SDP_Attribute> Attributes => m_pAttributes;

	public List<SDP_MediaDescription> MediaDescriptions => m_pMediaDescriptions;

	public SDP_Message()
	{
		m_pTimes = new List<SDP_Time>();
		m_pAttributes = new List<SDP_Attribute>();
		m_pMediaDescriptions = new List<SDP_MediaDescription>();
	}

	public static SDP_Message Parse(string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SDP_Message sDP_Message = new SDP_Message();
		System.IO.StringReader stringReader = new System.IO.StringReader(data);
		string text = stringReader.ReadLine();
		while (text != null)
		{
			text = text.Trim();
			if (text.ToLower().StartsWith("m"))
			{
				SDP_MediaDescription sDP_MediaDescription = SDP_MediaDescription.Parse(text);
				sDP_Message.m_pMediaDescriptions.Add(sDP_MediaDescription);
				for (text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
				{
					text = text.Trim();
					if (text.ToLower().StartsWith("m"))
					{
						break;
					}
					if (text.ToLower().StartsWith("i"))
					{
						sDP_MediaDescription.Information = text.Split(new char[1] { '=' }, 2)[1].Trim();
					}
					else if (text.ToLower().StartsWith("c"))
					{
						sDP_MediaDescription.Connection = SDP_Connection.Parse(text);
					}
					else if (text.ToLower().StartsWith("a"))
					{
						sDP_MediaDescription.Attributes.Add(SDP_Attribute.Parse(text));
					}
				}
				if (text == null)
				{
					break;
				}
				continue;
			}
			if (text.ToLower().StartsWith("v"))
			{
				sDP_Message.Version = text.Split(new char[1] { '=' }, 2)[1].Trim();
			}
			else if (text.ToLower().StartsWith("o"))
			{
				sDP_Message.Origin = SDP_Origin.Parse(text);
			}
			else if (text.ToLower().StartsWith("s"))
			{
				sDP_Message.SessionName = text.Split(new char[1] { '=' }, 2)[1].Trim();
			}
			else if (text.ToLower().StartsWith("i"))
			{
				sDP_Message.SessionDescription = text.Split(new char[1] { '=' }, 2)[1].Trim();
			}
			else if (text.ToLower().StartsWith("u"))
			{
				sDP_Message.Uri = text.Split(new char[1] { '=' }, 2)[1].Trim();
			}
			else if (text.ToLower().StartsWith("c"))
			{
				sDP_Message.Connection = SDP_Connection.Parse(text);
			}
			else if (text.ToLower().StartsWith("t"))
			{
				sDP_Message.Times.Add(SDP_Time.Parse(text));
			}
			else if (text.ToLower().StartsWith("a"))
			{
				sDP_Message.Attributes.Add(SDP_Attribute.Parse(text));
			}
			text = stringReader.ReadLine().Trim();
		}
		return sDP_Message;
	}

	public SDP_Message Clone()
	{
		return (SDP_Message)MemberwiseClone();
	}

	public void ToFile(string fileName)
	{
		File.WriteAllText(fileName, ToStringData());
	}

	public string ToStringData()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("v=" + Version);
		if (Origin != null)
		{
			stringBuilder.Append(Origin.ToString());
		}
		if (!string.IsNullOrEmpty(SessionName))
		{
			stringBuilder.AppendLine("s=" + SessionName);
		}
		if (!string.IsNullOrEmpty(SessionDescription))
		{
			stringBuilder.AppendLine("i=" + SessionDescription);
		}
		if (!string.IsNullOrEmpty(Uri))
		{
			stringBuilder.AppendLine("u=" + Uri);
		}
		if (Connection != null)
		{
			stringBuilder.Append(Connection.ToValue());
		}
		foreach (SDP_Time time in Times)
		{
			stringBuilder.Append(time.ToValue());
		}
		foreach (SDP_Attribute attribute in Attributes)
		{
			stringBuilder.Append(attribute.ToValue());
		}
		foreach (SDP_MediaDescription mediaDescription in MediaDescriptions)
		{
			stringBuilder.Append(mediaDescription.ToValue());
		}
		return stringBuilder.ToString();
	}

	public byte[] ToByte()
	{
		return Encoding.UTF8.GetBytes(ToStringData());
	}
}
