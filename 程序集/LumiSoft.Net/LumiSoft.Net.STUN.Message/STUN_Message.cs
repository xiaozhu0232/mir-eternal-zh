using System;
using System.Net;
using System.Text;

namespace LumiSoft.Net.STUN.Message;

public class STUN_Message
{
	private enum AttributeType
	{
		MappedAddress = 1,
		ResponseAddress = 2,
		ChangeRequest = 3,
		SourceAddress = 4,
		ChangedAddress = 5,
		Username = 6,
		Password = 7,
		MessageIntegrity = 8,
		ErrorCode = 9,
		UnknownAttribute = 10,
		ReflectedFrom = 11,
		XorMappedAddress = 32800,
		XorOnly = 33,
		ServerName = 32802
	}

	private enum IPFamily
	{
		IPv4 = 1,
		IPv6
	}

	private STUN_MessageType m_Type = STUN_MessageType.BindingRequest;

	private int m_MagicCookie;

	private byte[] m_pTransactionID;

	private IPEndPoint m_pMappedAddress;

	private IPEndPoint m_pResponseAddress;

	private STUN_t_ChangeRequest m_pChangeRequest;

	private IPEndPoint m_pSourceAddress;

	private IPEndPoint m_pChangedAddress;

	private string m_UserName;

	private string m_Password;

	private STUN_t_ErrorCode m_pErrorCode;

	private IPEndPoint m_pReflectedFrom;

	private string m_ServerName;

	public STUN_MessageType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public int MagicCookie => m_MagicCookie;

	public byte[] TransactionID => m_pTransactionID;

	public IPEndPoint MappedAddress
	{
		get
		{
			return m_pMappedAddress;
		}
		set
		{
			m_pMappedAddress = value;
		}
	}

	public IPEndPoint ResponseAddress
	{
		get
		{
			return m_pResponseAddress;
		}
		set
		{
			m_pResponseAddress = value;
		}
	}

	public STUN_t_ChangeRequest ChangeRequest
	{
		get
		{
			return m_pChangeRequest;
		}
		set
		{
			m_pChangeRequest = value;
		}
	}

	public IPEndPoint SourceAddress
	{
		get
		{
			return m_pSourceAddress;
		}
		set
		{
			m_pSourceAddress = value;
		}
	}

	public IPEndPoint ChangedAddress
	{
		get
		{
			return m_pChangedAddress;
		}
		set
		{
			m_pChangedAddress = value;
		}
	}

	public string UserName
	{
		get
		{
			return m_UserName;
		}
		set
		{
			m_UserName = value;
		}
	}

	public string Password
	{
		get
		{
			return m_Password;
		}
		set
		{
			m_Password = value;
		}
	}

	public STUN_t_ErrorCode ErrorCode
	{
		get
		{
			return m_pErrorCode;
		}
		set
		{
			m_pErrorCode = value;
		}
	}

	public IPEndPoint ReflectedFrom
	{
		get
		{
			return m_pReflectedFrom;
		}
		set
		{
			m_pReflectedFrom = value;
		}
	}

	public string ServerName
	{
		get
		{
			return m_ServerName;
		}
		set
		{
			m_ServerName = value;
		}
	}

	public STUN_Message()
	{
		m_pTransactionID = new byte[12];
		new Random().NextBytes(m_pTransactionID);
	}

	public void Parse(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < 20)
		{
			throw new ArgumentException("Invalid STUN message value !");
		}
		int num = 0;
		switch ((data[num++] << 8) | data[num++])
		{
		case 273:
			m_Type = STUN_MessageType.BindingErrorResponse;
			break;
		case 1:
			m_Type = STUN_MessageType.BindingRequest;
			break;
		case 257:
			m_Type = STUN_MessageType.BindingResponse;
			break;
		case 274:
			m_Type = STUN_MessageType.SharedSecretErrorResponse;
			break;
		case 2:
			m_Type = STUN_MessageType.SharedSecretRequest;
			break;
		case 258:
			m_Type = STUN_MessageType.SharedSecretResponse;
			break;
		default:
			throw new ArgumentException("Invalid STUN message type value !");
		}
		int num2 = (data[num++] << 8) | data[num++];
		m_MagicCookie = (data[num++] << 24) | (data[num++] << 16) | (data[num++] << 8) | data[num++];
		m_pTransactionID = new byte[12];
		Array.Copy(data, num, m_pTransactionID, 0, 12);
		num += 12;
		while (num - 20 < num2)
		{
			ParseAttribute(data, ref num);
		}
	}

	public byte[] ToByteData()
	{
		byte[] array = new byte[512];
		int num = 0;
		array[num++] = (byte)((uint)((int)Type >> 8) & 0x3Fu);
		array[num++] = (byte)(Type & (STUN_MessageType)255);
		array[num++] = 0;
		array[num++] = 0;
		array[num++] = (byte)((uint)(MagicCookie >> 24) & 0xFFu);
		array[num++] = (byte)((uint)(MagicCookie >> 16) & 0xFFu);
		array[num++] = (byte)((uint)(MagicCookie >> 8) & 0xFFu);
		array[num++] = (byte)((uint)MagicCookie & 0xFFu);
		Array.Copy(m_pTransactionID, 0, array, num, 12);
		num += 12;
		if (MappedAddress != null)
		{
			StoreEndPoint(AttributeType.MappedAddress, MappedAddress, array, ref num);
		}
		else if (ResponseAddress != null)
		{
			StoreEndPoint(AttributeType.ResponseAddress, ResponseAddress, array, ref num);
		}
		else if (ChangeRequest != null)
		{
			array[num++] = 0;
			array[num++] = 3;
			array[num++] = 0;
			array[num++] = 4;
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = (byte)((Convert.ToInt32(ChangeRequest.ChangeIP) << 2) | (Convert.ToInt32(ChangeRequest.ChangePort) << 1));
		}
		else if (SourceAddress != null)
		{
			StoreEndPoint(AttributeType.SourceAddress, SourceAddress, array, ref num);
		}
		else if (ChangedAddress != null)
		{
			StoreEndPoint(AttributeType.ChangedAddress, ChangedAddress, array, ref num);
		}
		else if (UserName != null)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(UserName);
			array[num++] = 0;
			array[num++] = 6;
			array[num++] = (byte)(bytes.Length >> 8);
			array[num++] = (byte)((uint)bytes.Length & 0xFFu);
			Array.Copy(bytes, 0, array, num, bytes.Length);
			num += bytes.Length;
		}
		else if (Password != null)
		{
			byte[] bytes2 = Encoding.ASCII.GetBytes(UserName);
			array[num++] = 0;
			array[num++] = 7;
			array[num++] = (byte)(bytes2.Length >> 8);
			array[num++] = (byte)((uint)bytes2.Length & 0xFFu);
			Array.Copy(bytes2, 0, array, num, bytes2.Length);
			num += bytes2.Length;
		}
		else if (ErrorCode != null)
		{
			byte[] bytes3 = Encoding.ASCII.GetBytes(ErrorCode.ReasonText);
			array[num++] = 0;
			array[num++] = 9;
			array[num++] = 0;
			array[num++] = (byte)(4 + bytes3.Length);
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = (byte)Math.Floor((double)(ErrorCode.Code / 100));
			array[num++] = (byte)((uint)ErrorCode.Code & 0xFFu);
			Array.Copy(bytes3, array, bytes3.Length);
			num += bytes3.Length;
		}
		else if (ReflectedFrom != null)
		{
			StoreEndPoint(AttributeType.ReflectedFrom, ReflectedFrom, array, ref num);
		}
		array[2] = (byte)(num - 20 >> 8);
		array[3] = (byte)((uint)(num - 20) & 0xFFu);
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, array2.Length);
		return array2;
	}

	private void ParseAttribute(byte[] data, ref int offset)
	{
		AttributeType attributeType = (AttributeType)((data[offset++] << 8) | data[offset++]);
		int num = (data[offset++] << 8) | data[offset++];
		switch (attributeType)
		{
		case AttributeType.MappedAddress:
			m_pMappedAddress = ParseEndPoint(data, ref offset);
			break;
		case AttributeType.ResponseAddress:
			m_pResponseAddress = ParseEndPoint(data, ref offset);
			break;
		case AttributeType.ChangeRequest:
			offset += 3;
			m_pChangeRequest = new STUN_t_ChangeRequest((data[offset] & 4) != 0, (data[offset] & 2) != 0);
			offset++;
			break;
		case AttributeType.SourceAddress:
			m_pSourceAddress = ParseEndPoint(data, ref offset);
			break;
		case AttributeType.ChangedAddress:
			m_pChangedAddress = ParseEndPoint(data, ref offset);
			break;
		case AttributeType.Username:
			m_UserName = Encoding.Default.GetString(data, offset, num);
			offset += num;
			break;
		case AttributeType.Password:
			m_Password = Encoding.Default.GetString(data, offset, num);
			offset += num;
			break;
		case AttributeType.MessageIntegrity:
			offset += num;
			break;
		case AttributeType.ErrorCode:
		{
			int code = (data[offset + 2] & 7) * 100 + (data[offset + 3] & 0xFF);
			m_pErrorCode = new STUN_t_ErrorCode(code, Encoding.Default.GetString(data, offset + 4, num - 4));
			offset += num;
			break;
		}
		case AttributeType.UnknownAttribute:
			offset += num;
			break;
		case AttributeType.ReflectedFrom:
			m_pReflectedFrom = ParseEndPoint(data, ref offset);
			break;
		case AttributeType.ServerName:
			m_ServerName = Encoding.Default.GetString(data, offset, num);
			offset += num;
			break;
		default:
			offset += num;
			break;
		}
	}

	private IPEndPoint ParseEndPoint(byte[] data, ref int offset)
	{
		offset++;
		offset++;
		int port = (data[offset++] << 8) | data[offset++];
		return new IPEndPoint(new IPAddress(new byte[4]
		{
			data[offset++],
			data[offset++],
			data[offset++],
			data[offset++]
		}), port);
	}

	private void StoreEndPoint(AttributeType type, IPEndPoint endPoint, byte[] message, ref int offset)
	{
		message[offset++] = (byte)((int)type >> 8);
		message[offset++] = (byte)(type & (AttributeType)255);
		message[offset++] = 0;
		message[offset++] = 8;
		message[offset++] = 0;
		message[offset++] = 1;
		message[offset++] = (byte)(endPoint.Port >> 8);
		message[offset++] = (byte)((uint)endPoint.Port & 0xFFu);
		byte[] addressBytes = endPoint.Address.GetAddressBytes();
		message[offset++] = addressBytes[0];
		message[offset++] = addressBytes[1];
		message[offset++] = addressBytes[2];
		message[offset++] = addressBytes[3];
	}
}
