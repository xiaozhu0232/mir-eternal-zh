using System;

namespace LumiSoft.Net.SDP;

public class SDP_Origin
{
	private string m_UserName;

	private long m_SessionID;

	private long m_SessionVersion;

	private string m_NetType;

	private string m_AddressType;

	private string m_UnicastAddress;

	public string UserName => m_UserName;

	public long SessionID => m_SessionID;

	public long SessionVersion
	{
		get
		{
			return m_SessionVersion;
		}
		set
		{
			m_SessionVersion = value;
		}
	}

	public string NetType => m_NetType;

	public string AddressType => m_AddressType;

	public string UnicastAddress => m_UnicastAddress;

	public SDP_Origin(string userName, long sessionID, long sessionVersion, string netType, string addressType, string unicastAddress)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (userName == string.Empty)
		{
			throw new ArgumentException("Argument 'userName' value must be specified.");
		}
		if (netType == null)
		{
			throw new ArgumentNullException("netType");
		}
		if (netType == string.Empty)
		{
			throw new ArgumentException("Argument 'netType' value must be specified.");
		}
		if (addressType == null)
		{
			throw new ArgumentNullException("addressType");
		}
		if (addressType == string.Empty)
		{
			throw new ArgumentException("Argument 'addressType' value must be specified.");
		}
		if (unicastAddress == null)
		{
			throw new ArgumentNullException("unicastAddress");
		}
		if (unicastAddress == string.Empty)
		{
			throw new ArgumentException("Argument 'unicastAddress' value must be specified.");
		}
		m_UserName = userName;
		m_SessionID = sessionID;
		m_SessionVersion = sessionVersion;
		m_NetType = netType;
		m_AddressType = addressType;
		m_UnicastAddress = unicastAddress;
	}

	public static SDP_Origin Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value = value.Trim();
		if (!value.ToLower().StartsWith("o="))
		{
			throw new ParseException("Invalid SDP Origin('o=') value '" + value + "'.");
		}
		value = value.Substring(2);
		string[] array = value.Split(' ');
		if (array.Length != 6)
		{
			throw new ParseException("Invalid SDP Origin('o=') value '" + value + "'.");
		}
		return new SDP_Origin(array[0], Convert.ToInt64(array[1]), Convert.ToInt64(array[2]), array[3], array[4], array[5]);
	}

	public override string ToString()
	{
		return "o=" + m_UserName + " " + m_SessionID + " " + m_SessionVersion + " " + m_NetType + " " + m_AddressType + " " + m_UnicastAddress + "\r\n";
	}
}
