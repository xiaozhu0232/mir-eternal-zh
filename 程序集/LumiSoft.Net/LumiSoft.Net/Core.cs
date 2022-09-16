using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using LumiSoft.Net.DNS;
using LumiSoft.Net.DNS.Client;

namespace LumiSoft.Net;

[Obsolete("")]
public class Core
{
	public static string GetHostName(IPAddress ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		string result = ip.ToString();
		try
		{
			DnsServerResponse dnsServerResponse = new Dns_Client().Query(ip.ToString(), DNS_QType.PTR);
			if (dnsServerResponse.ResponseCode == DNS_RCode.NO_ERROR)
			{
				DNS_rr_PTR[] pTRRecords = dnsServerResponse.GetPTRRecords();
				if (pTRRecords.Length != 0)
				{
					result = pTRRecords[0].DomainName;
					return result;
				}
				return result;
			}
			return result;
		}
		catch
		{
			return result;
		}
	}

	public static string GetArgsText(string input, string cmdTxtToRemove)
	{
		string text = input.Trim();
		if (text.Length >= cmdTxtToRemove.Length)
		{
			text = text.Substring(cmdTxtToRemove.Length);
		}
		return text.Trim();
	}

	[Obsolete("Use Net_Utils.IsInteger instead of it")]
	public static bool IsNumber(string str)
	{
		try
		{
			Convert.ToInt64(str);
			return true;
		}
		catch
		{
			return false;
		}
	}

	[Obsolete("Use Net_Utils.ReverseArray instead of it")]
	public static Array ReverseArray(Array array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Array.Reverse(array);
		return array;
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

	[Obsolete("Use Net_Utils.FromBase64 instead of it")]
	public static byte[] Base64Decode(byte[] base64Data)
	{
		return Base64DecodeEx(base64Data, null);
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

	public static byte[] QuotedPrintableEncode(byte[] data)
	{
		int num = 0;
		MemoryStream memoryStream = new MemoryStream();
		foreach (byte b in data)
		{
			if (num > 75)
			{
				memoryStream.Write(new byte[3] { 61, 13, 10 }, 0, 3);
				num = 0;
			}
			if (b <= 33 || b >= 126 || b == 61)
			{
				memoryStream.Write(new byte[1] { 61 }, 0, 1);
				memoryStream.Write(ToHex(b), 0, 2);
				num += 3;
			}
			else
			{
				memoryStream.WriteByte(b);
				num++;
			}
		}
		return memoryStream.ToArray();
	}

	[Obsolete("Use MIME_Utils.QuotedPrintableDecode instead of it")]
	public static byte[] QuotedPrintableDecode(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream();
		MemoryStream memoryStream2 = new MemoryStream(data);
		for (int num = memoryStream2.ReadByte(); num > -1; num = memoryStream2.ReadByte())
		{
			if (num == 61)
			{
				byte[] array = new byte[2];
				int num2 = memoryStream2.Read(array, 0, 2);
				if (num2 == 2)
				{
					if (array[0] != 13 || array[1] != 10)
					{
						try
						{
							memoryStream.Write(FromHex(array), 0, 1);
						}
						catch
						{
							memoryStream.WriteByte(61);
							memoryStream.Write(array, 0, 2);
						}
					}
				}
				else
				{
					memoryStream.Write(array, 0, num2);
				}
			}
			else
			{
				memoryStream.WriteByte((byte)num);
			}
		}
		return memoryStream.ToArray();
	}

	[Obsolete("Use MIME_Utils.QDecode instead of it")]
	public static string QDecode(Encoding encoding, string data)
	{
		return encoding.GetString(QuotedPrintableDecode(Encoding.ASCII.GetBytes(data.Replace("_", " "))));
	}

	[Obsolete("Use MimeUtils.DecodeWords method instead.")]
	public static string CanonicalDecode(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < text.Length)
		{
			int num2 = text.IndexOf("=?", num);
			int num3 = -1;
			if (num2 > -1)
			{
				num3 = text.IndexOf("?=", num2 + 2);
			}
			if (num2 > -1 && num3 > -1)
			{
				if (num2 - num > 0)
				{
					stringBuilder.Append(text.Substring(num, num2 - num));
				}
				while (true)
				{
					string[] array = text.Substring(num2 + 2, num3 - num2 - 2).Split('?');
					if (array.Length == 3)
					{
						try
						{
							Encoding encoding = Encoding.GetEncoding(array[0]);
							if (array[1].ToLower() == "q")
							{
								stringBuilder.Append(QDecode(encoding, array[2]));
							}
							else
							{
								stringBuilder.Append(encoding.GetString(Base64Decode(Encoding.Default.GetBytes(array[2]))));
							}
						}
						catch
						{
							stringBuilder.Append(text.Substring(num2, num3 - num2 + 2));
						}
						num = num3 + 2;
						break;
					}
					if (array.Length < 3)
					{
						num3 = text.IndexOf("?=", num3 + 2);
						if (num3 == -1)
						{
							stringBuilder.Append("=?");
							num = num2 + 2;
							break;
						}
						continue;
					}
					stringBuilder.Append("=?");
					num = num2 + 2;
					break;
				}
			}
			else if (text.Length > num)
			{
				stringBuilder.Append(text.Substring(num));
				num = text.Length;
			}
		}
		return stringBuilder.ToString();
	}

	public static string CanonicalEncode(string str, string charSet)
	{
		if (!IsAscii(str))
		{
			return string.Concat(string.Concat("=?" + charSet + "?B?", Convert.ToBase64String(Encoding.GetEncoding(charSet).GetBytes(str))), "?=");
		}
		return str;
	}

	[Obsolete("Use IMAP_Utils.Encode_IMAP_UTF7_String instead of it")]
	public static string Encode_IMAP_UTF7_String(string text)
	{
		char[] base64Chars = new char[64]
		{
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
			'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
			'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
			'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
			'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
			'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
			'8', '9', '+', ','
		};
		MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '&':
				memoryStream.Write(new byte[2] { 38, 45 }, 0, 2);
				continue;
			default:
				if (c < '\'' || c > '~')
				{
					break;
				}
				goto case ' ';
			case ' ':
			case '!':
			case '"':
			case '#':
			case '$':
			case '%':
				memoryStream.WriteByte((byte)c);
				continue;
			}
			MemoryStream memoryStream2 = new MemoryStream();
			for (int j = i; j < text.Length; j++)
			{
				char c2 = text[j];
				if ((c2 >= ' ' && c2 <= '%') || (c2 >= '\'' && c2 <= '~'))
				{
					break;
				}
				memoryStream2.WriteByte((byte)((c2 & 0xFF00) >> 8));
				memoryStream2.WriteByte((byte)(c2 & 0xFFu));
				i = j;
			}
			byte[] array = Base64EncodeEx(memoryStream2.ToArray(), base64Chars, padd: false);
			memoryStream.WriteByte(38);
			memoryStream.Write(array, 0, array.Length);
			memoryStream.WriteByte(45);
		}
		return Encoding.Default.GetString(memoryStream.ToArray());
	}

	[Obsolete("Use IMAP_Utils.Decode_IMAP_UTF7_String instead of it")]
	public static string Decode_IMAP_UTF7_String(string text)
	{
		char[] base64Chars = new char[64]
		{
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
			'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
			'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
			'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
			'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
			'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
			'8', '9', '+', ','
		};
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '&')
			{
				int num = -1;
				for (int j = i + 1; j < text.Length; j++)
				{
					if (text[j] == '-')
					{
						num = j;
						break;
					}
					if (text[j] == '&')
					{
						break;
					}
				}
				if (num == -1)
				{
					stringBuilder.Append(c);
					continue;
				}
				if (num - i == 1)
				{
					stringBuilder.Append(c);
					i++;
					continue;
				}
				byte[] bytes = Encoding.Default.GetBytes(text.Substring(i + 1, num - i - 1));
				byte[] array = Base64DecodeEx(bytes, base64Chars);
				char[] array2 = new char[array.Length / 2];
				for (int k = 0; k < array2.Length; k++)
				{
					array2[k] = (char)((array[k * 2] << 8) | array[k * 2 + 1]);
				}
				stringBuilder.Append(array2);
				i += bytes.Length + 1;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	[Obsolete("Use Net_Utils.IsAscii instead of it")]
	public static bool IsAscii(string data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i] > '\u007f')
			{
				return false;
			}
		}
		return true;
	}

	public static string GetFileNameFromPath(string filePath)
	{
		return Path.GetFileName(filePath);
	}

	public static bool IsIP(string value)
	{
		try
		{
			IPAddress address = null;
			return IPAddress.TryParse(value, out address);
		}
		catch
		{
			return false;
		}
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

	[Obsolete("Use Net_Utils.IsPrivateIP instead of it")]
	public static bool IsPrivateIP(string ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		return IsPrivateIP(IPAddress.Parse(ip));
	}

	[Obsolete("Use Net_Utils.IsPrivateIP instead of it")]
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

	[Obsolete("Use Net_Utils.CreateSocket instead of it")]
	public static Socket CreateSocket(IPEndPoint localEP, ProtocolType protocolType)
	{
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

	public static string ToHexString(string data)
	{
		return Encoding.Default.GetString(ToHex(Encoding.Default.GetBytes(data)));
	}

	public static string ToHexString(byte[] data)
	{
		return Encoding.Default.GetString(ToHex(data));
	}

	public static byte[] ToHex(byte byteValue)
	{
		return ToHex(new byte[1] { byteValue });
	}

	public static byte[] ToHex(byte[] data)
	{
		char[] array = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};
		MemoryStream memoryStream = new MemoryStream(data.Length * 2);
		foreach (byte b in data)
		{
			memoryStream.Write(new byte[2]
			{
				(byte)array[(b & 0xF0) >> 4],
				(byte)array[b & 0xF]
			}, 0, 2);
		}
		return memoryStream.ToArray();
	}

	[Obsolete("Use Net_Utils.FromHex instead of it")]
	public static byte[] FromHex(byte[] hexData)
	{
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

	[Obsolete("Use Net_Utils.ComputeMd5 instead of it")]
	public static string ComputeMd5(string text, bool hex)
	{
		using MD5 mD = new MD5CryptoServiceProvider();
		byte[] bytes = mD.ComputeHash(Encoding.Default.GetBytes(text));
		if (hex)
		{
			return ToHexString(Encoding.Default.GetString(bytes)).ToLower();
		}
		return Encoding.Default.GetString(bytes);
	}
}
