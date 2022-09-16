using System;

namespace LumiSoft.Net.SDP;

public class SDP_Connection
{
	private string m_NetType = "IN";

	private string m_AddressType = "";

	private string m_Address = "";

	public string NetType => m_NetType;

	public string AddressType
	{
		get
		{
			return m_AddressType;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property AddressType can't be null or empty !");
			}
			m_AddressType = value;
		}
	}

	public string Address
	{
		get
		{
			return m_Address;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Property Address can't be null or empty !");
			}
			m_Address = value;
		}
	}

	public SDP_Connection(string netType, string addressType, string address)
	{
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
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address == string.Empty)
		{
			throw new ArgumentException("Argument 'address' value must be specified.");
		}
		m_NetType = netType;
		m_AddressType = addressType;
		m_Address = address;
	}

	public static SDP_Connection Parse(string cValue)
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		StringReader stringReader = new StringReader(cValue);
		stringReader.QuotedReadToDelimiter('=');
		text = stringReader.ReadWord() ?? throw new Exception("SDP message \"c\" field <nettype> value is missing !");
		text2 = stringReader.ReadWord() ?? throw new Exception("SDP message \"c\" field <addrtype> value is missing !");
		text3 = stringReader.ReadWord() ?? throw new Exception("SDP message \"c\" field <connection-address> value is missing !");
		return new SDP_Connection(text, text2, text3);
	}

	public string ToValue()
	{
		return "c=" + NetType + " " + AddressType + " " + Address + "\r\n";
	}
}
