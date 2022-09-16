using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net;

public class Net_Utils
{
	public static string GetLocalHostName(string hostName)
	{
		if (string.IsNullOrEmpty(hostName))
		{
			return Dns.GetHostName();
		}
		return hostName;
	}

	public static bool CompareArray(Array array1, Array array2)
	{
		return CompareArray(array1, array2, array2.Length);
	}

	public static bool CompareArray(Array array1, Array array2, int array2Count)
	{
		if (array1 == null && array2 == null)
		{
			return true;
		}
		if (array1 == null && array2 != null)
		{
			return false;
		}
		if (array1 != null && array2 == null)
		{
			return false;
		}
		if (array1.Length != array2Count)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!array1.GetValue(i).Equals(array2.GetValue(i)))
			{
				return false;
			}
		}
		return true;
	}

	public static Array ReverseArray(Array array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Array.Reverse(array);
		return array;
	}

	public static string ArrayToString(string[] values, string delimiter)
	{
		if (values == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < values.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(delimiter);
			}
			stringBuilder.Append(values[i]);
		}
		return stringBuilder.ToString();
	}

	public static long StreamCopy(Stream source, Stream target, int blockSize)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (blockSize < 1024)
		{
			throw new ArgumentException("Argument 'blockSize' value must be >= 1024.");
		}
		byte[] array = new byte[blockSize];
		long num = 0L;
		while (true)
		{
			int num2 = source.Read(array, 0, array.Length);
			if (num2 == 0)
			{
				break;
			}
			target.Write(array, 0, num2);
			num += num2;
		}
		return num;
	}

	public static int CompareIP(IPAddress source, IPAddress destination)
	{
		byte[] addressBytes = source.GetAddressBytes();
		byte[] addressBytes2 = destination.GetAddressBytes();
		if (addressBytes.Length < addressBytes2.Length)
		{
			return 1;
		}
		if (addressBytes.Length > addressBytes2.Length)
		{
			return -1;
		}
		for (int i = 0; i < addressBytes.Length; i++)
		{
			if (addressBytes[i] < addressBytes2[i])
			{
				return 1;
			}
			if (addressBytes[i] > addressBytes2[i])
			{
				return -1;
			}
		}
		return 0;
	}

	public static bool IsIPAddress(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		IPAddress address = null;
		return IPAddress.TryParse(value, out address);
	}

	public static bool IsMulticastAddress(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (ip.IsIPv6Multicast)
		{
			return true;
		}
		if (ip.AddressFamily == AddressFamily.InterNetwork)
		{
			byte[] addressBytes = ip.GetAddressBytes();
			if (addressBytes[0] >= 224 && addressBytes[0] <= 239)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPrivateIP(string ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		return IsPrivateIP(IPAddress.Parse(ip));
	}

	public static bool IsPrivateIP(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		if (ip.AddressFamily == AddressFamily.InterNetwork)
		{
			byte[] addressBytes = ip.GetAddressBytes();
			if (addressBytes[0] == 192 && addressBytes[1] == 168)
			{
				return true;
			}
			if (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31)
			{
				return true;
			}
			if (addressBytes[0] == 10)
			{
				return true;
			}
			if (addressBytes[0] == 169 && addressBytes[1] == 254)
			{
				return true;
			}
		}
		return false;
	}

	public static IPEndPoint ParseIPEndPoint(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		try
		{
			string[] array = value.Split(':');
			return new IPEndPoint(IPAddress.Parse(array[0]), Convert.ToInt32(array[1]));
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("Invalid IPEndPoint value.", "value", innerException);
		}
	}

	public static bool IsInteger(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		long result = 0L;
		return long.TryParse(value, out result);
	}

	public static bool IsAscii(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] > '\u007f')
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSocketAsyncSupported()
	{
		try
		{
			using (new SocketAsyncEventArgs())
			{
				return true;
			}
		}
		catch (NotSupportedException ex)
		{
			_ = ex.Message;
			return false;
		}
	}

	public static Socket CreateSocket(IPEndPoint localEP, ProtocolType protocolType)
	{
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		SocketType socketType = SocketType.Stream;
		if (protocolType == ProtocolType.Udp)
		{
			socketType = SocketType.Dgram;
		}
		if (localEP.AddressFamily == AddressFamily.InterNetwork)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
			socket.Bind(localEP);
			return socket;
		}
		if (localEP.AddressFamily == AddressFamily.InterNetworkV6)
		{
			Socket socket2 = new Socket(AddressFamily.InterNetworkV6, socketType, protocolType);
			socket2.Bind(localEP);
			return socket2;
		}
		throw new ArgumentException("Invalid IPEndPoint address family.");
	}

	public static string ToHex(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return BitConverter.ToString(data).ToLower().Replace("-", "");
	}

	public static string ToHex(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return BitConverter.ToString(Encoding.Default.GetBytes(text)).ToLower().Replace("-", "");
	}

	public static byte[] FromHex(byte[] hexData)
	{
		if (hexData == null)
		{
			throw new ArgumentNullException("hexData");
		}
		if (hexData.Length < 2 || (double)hexData.Length / 2.0 != Math.Floor((double)hexData.Length / 2.0))
		{
			throw new Exception("Illegal hex data, hex data must be in two bytes pairs, for example: 0F,FF,A3,... .");
		}
		MemoryStream memoryStream = new MemoryStream(hexData.Length / 2);
		for (int i = 0; i < hexData.Length; i += 2)
		{
			byte[] array = new byte[2];
			for (int j = 0; j < 2; j++)
			{
				if (hexData[i + j] == 48)
				{
					array[j] = 0;
				}
				else if (hexData[i + j] == 49)
				{
					array[j] = 1;
				}
				else if (hexData[i + j] == 50)
				{
					array[j] = 2;
				}
				else if (hexData[i + j] == 51)
				{
					array[j] = 3;
				}
				else if (hexData[i + j] == 52)
				{
					array[j] = 4;
				}
				else if (hexData[i + j] == 53)
				{
					array[j] = 5;
				}
				else if (hexData[i + j] == 54)
				{
					array[j] = 6;
				}
				else if (hexData[i + j] == 55)
				{
					array[j] = 7;
				}
				else if (hexData[i + j] == 56)
				{
					array[j] = 8;
				}
				else if (hexData[i + j] == 57)
				{
					array[j] = 9;
				}
				else if (hexData[i + j] == 65 || hexData[i + j] == 97)
				{
					array[j] = 10;
				}
				else if (hexData[i + j] == 66 || hexData[i + j] == 98)
				{
					array[j] = 11;
				}
				else if (hexData[i + j] == 67 || hexData[i + j] == 99)
				{
					array[j] = 12;
				}
				else if (hexData[i + j] == 68 || hexData[i + j] == 100)
				{
					array[j] = 13;
				}
				else if (hexData[i + j] == 69 || hexData[i + j] == 101)
				{
					array[j] = 14;
				}
				else if (hexData[i + j] == 70 || hexData[i + j] == 102)
				{
					array[j] = 15;
				}
			}
			memoryStream.WriteByte((byte)((array[0] << 4) | array[1]));
		}
		return memoryStream.ToArray();
	}

	public static byte[] FromBase64(string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new Base64().Decode(data, ignoreNonBase64Chars: true);
	}

	public static byte[] FromBase64(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new Base64().Decode(data, 0, data.Length, ignoreNonBase64Chars: true);
	}

	public static byte[] Base64Encode(byte[] data)
	{
		return Base64EncodeEx(data, null, padd: true);
	}

	public static byte[] Base64EncodeEx(byte[] data, char[] base64Chars, bool padd)
	{
		if (base64Chars != null && base64Chars.Length != 64)
		{
			throw new Exception("There must be 64 chars in base64Chars char array !");
		}
		if (base64Chars == null)
		{
			base64Chars = new char[64]
			{
				'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
				'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
				'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
				'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
				'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
				'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
				'8', '9', '+', '/'
			};
		}
		byte[] array = new byte[64];
		for (int i = 0; i < 64; i++)
		{
			array[i] = (byte)base64Chars[i];
		}
		int num = (int)Math.Ceiling((double)(data.Length * 8) / 6.0);
		if (padd && (double)num / 4.0 != Math.Ceiling((double)num / 4.0))
		{
			num += (int)(Math.Ceiling((double)num / 4.0) * 4.0) - num;
		}
		int num2 = 0;
		if (num > 76)
		{
			num2 = (int)Math.Ceiling((double)num / 76.0) - 1;
		}
		byte[] array2 = new byte[num + num2 * 2];
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < data.Length; j += 3)
		{
			if (num3 >= 76)
			{
				array2[num4] = 13;
				array2[num4 + 1] = 10;
				num4 += 2;
				num3 = 0;
			}
			if (data.Length - j >= 3)
			{
				array2[num4] = array[data[j] >> 2];
				array2[num4 + 1] = array[((data[j] & 3) << 4) | (data[j + 1] >> 4)];
				array2[num4 + 2] = array[((data[j + 1] & 0xF) << 2) | (data[j + 2] >> 6)];
				array2[num4 + 3] = array[data[j + 2] & 0x3F];
				num4 += 4;
				num3 += 4;
			}
			else if (data.Length - j == 2)
			{
				array2[num4] = array[data[j] >> 2];
				array2[num4 + 1] = array[((data[j] & 3) << 4) | (data[j + 1] >> 4)];
				array2[num4 + 2] = array[(data[j + 1] & 0xF) << 2];
				if (padd)
				{
					array2[num4 + 3] = 61;
				}
			}
			else if (data.Length - j == 1)
			{
				array2[num4] = array[data[j] >> 2];
				array2[num4 + 1] = array[(data[j] & 3) << 4];
				if (padd)
				{
					array2[num4 + 2] = 61;
					array2[num4 + 3] = 61;
				}
			}
		}
		return array2;
	}

	public static byte[] Base64DecodeEx(byte[] base64Data, char[] base64Chars)
	{
		if (base64Chars != null && base64Chars.Length != 64)
		{
			throw new Exception("There must be 64 chars in base64Chars char array !");
		}
		if (base64Chars == null)
		{
			base64Chars = new char[64]
			{
				'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
				'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
				'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
				'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
				'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
				'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
				'8', '9', '+', '/'
			};
		}
		byte[] array = new byte[128];
		for (int i = 0; i < 128; i++)
		{
			int num = -1;
			for (int j = 0; j < base64Chars.Length; j++)
			{
				if (i == base64Chars[j])
				{
					num = j;
					break;
				}
			}
			if (num > -1)
			{
				array[i] = (byte)num;
			}
			else
			{
				array[i] = byte.MaxValue;
			}
		}
		byte[] array2 = new byte[base64Data.Length * 6 / 8 + 4];
		int num2 = 0;
		int num3 = 0;
		_ = new byte[3];
		byte[] array3 = new byte[4];
		for (int k = 0; k < base64Data.Length; k++)
		{
			byte b = base64Data[k];
			if (b == 61)
			{
				array3[num3] = byte.MaxValue;
			}
			else
			{
				byte b2 = array[b & 0x7F];
				if (b2 != byte.MaxValue)
				{
					array3[num3] = b2;
					num3++;
				}
			}
			int num4 = -1;
			if (num3 == 4)
			{
				num4 = 3;
			}
			else if (k == base64Data.Length - 1)
			{
				switch (num3)
				{
				case 1:
					num4 = 0;
					break;
				case 2:
					num4 = 1;
					break;
				case 3:
					num4 = 2;
					break;
				}
			}
			if (num4 > -1)
			{
				array2[num2] = (byte)((array3[0] << 2) | (array3[1] >> 4));
				array2[num2 + 1] = (byte)(((array3[1] & 0xF) << 4) | (array3[2] >> 2));
				array2[num2 + 2] = (byte)(((array3[2] & 3) << 6) | array3[3]);
				num2 += num4;
				num3 = 0;
			}
		}
		if (num2 > -1)
		{
			byte[] array4 = new byte[num2];
			Array.Copy(array2, 0, array4, 0, num2);
			return array4;
		}
		return new byte[0];
	}

	public static string ComputeMd5(string text, bool hex)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using MD5 mD = new MD5CryptoServiceProvider();
		byte[] array = mD.ComputeHash(Encoding.Default.GetBytes(text));
		if (hex)
		{
			return ToHex(array).ToLower();
		}
		return Encoding.Default.GetString(array);
	}

	[Obsolete("Use method 'IsSocketAsyncSupported' instead.")]
	public static bool IsIoCompletionPortsSupported()
	{
		try
		{
			using (new SocketAsyncEventArgs())
			{
				return true;
			}
		}
		catch (NotSupportedException ex)
		{
			_ = ex.Message;
			return false;
		}
	}
}
