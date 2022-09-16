using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Crypto.Generators;

public class OpenBsdBCrypt
{
	private static readonly byte[] EncodingTable;

	private static readonly byte[] DecodingTable;

	private static readonly string DefaultVersion;

	private static readonly ISet AllowedVersions;

	static OpenBsdBCrypt()
	{
		EncodingTable = new byte[64]
		{
			46, 47, 65, 66, 67, 68, 69, 70, 71, 72,
			73, 74, 75, 76, 77, 78, 79, 80, 81, 82,
			83, 84, 85, 86, 87, 88, 89, 90, 97, 98,
			99, 100, 101, 102, 103, 104, 105, 106, 107, 108,
			109, 110, 111, 112, 113, 114, 115, 116, 117, 118,
			119, 120, 121, 122, 48, 49, 50, 51, 52, 53,
			54, 55, 56, 57
		};
		DecodingTable = new byte[128];
		DefaultVersion = "2y";
		AllowedVersions = new HashSet();
		AllowedVersions.Add("2a");
		AllowedVersions.Add("2y");
		AllowedVersions.Add("2b");
		for (int i = 0; i < DecodingTable.Length; i++)
		{
			DecodingTable[i] = byte.MaxValue;
		}
		for (int j = 0; j < EncodingTable.Length; j++)
		{
			DecodingTable[EncodingTable[j]] = (byte)j;
		}
	}

	private static string CreateBcryptString(string version, byte[] password, byte[] salt, int cost)
	{
		if (!AllowedVersions.Contains(version))
		{
			throw new ArgumentException("Version " + version + " is not accepted by this implementation.", "version");
		}
		StringBuilder stringBuilder = new StringBuilder(60);
		stringBuilder.Append('$');
		stringBuilder.Append(version);
		stringBuilder.Append('$');
		stringBuilder.Append((cost < 10) ? ("0" + cost) : cost.ToString());
		stringBuilder.Append('$');
		stringBuilder.Append(EncodeData(salt));
		byte[] data = BCrypt.Generate(password, salt, cost);
		stringBuilder.Append(EncodeData(data));
		return stringBuilder.ToString();
	}

	public static string Generate(char[] password, byte[] salt, int cost)
	{
		return Generate(DefaultVersion, password, salt, cost);
	}

	public static string Generate(string version, char[] password, byte[] salt, int cost)
	{
		if (!AllowedVersions.Contains(version))
		{
			throw new ArgumentException("Version " + version + " is not accepted by this implementation.", "version");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (salt == null)
		{
			throw new ArgumentNullException("salt");
		}
		if (salt.Length != 16)
		{
			throw new DataLengthException("16 byte salt required: " + salt.Length);
		}
		if (cost < 4 || cost > 31)
		{
			throw new ArgumentException("Invalid cost factor.", "cost");
		}
		byte[] array = Strings.ToUtf8ByteArray(password);
		byte[] array2 = new byte[(array.Length >= 72) ? 72 : (array.Length + 1)];
		int length = System.Math.Min(array.Length, array2.Length);
		Array.Copy(array, 0, array2, 0, length);
		Array.Clear(array, 0, array.Length);
		string result = CreateBcryptString(version, array2, salt, cost);
		Array.Clear(array2, 0, array2.Length);
		return result;
	}

	public static bool CheckPassword(string bcryptString, char[] password)
	{
		if (bcryptString.Length != 60)
		{
			throw new DataLengthException("Bcrypt String length: " + bcryptString.Length + ", 60 required.");
		}
		if (bcryptString[0] != '$' || bcryptString[3] != '$' || bcryptString[6] != '$')
		{
			throw new ArgumentException("Invalid Bcrypt String format.", "bcryptString");
		}
		string text = bcryptString.Substring(1, 2);
		if (!AllowedVersions.Contains(text))
		{
			throw new ArgumentException("Bcrypt version '" + text + "' is not supported by this implementation", "bcryptString");
		}
		int num = 0;
		try
		{
			num = int.Parse(bcryptString.Substring(4, 2));
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("Invalid cost factor: " + bcryptString.Substring(4, 2), "bcryptString", innerException);
		}
		if (num < 4 || num > 31)
		{
			throw new ArgumentException("Invalid cost factor: " + num + ", 4 < cost < 31 expected.");
		}
		if (password == null)
		{
			throw new ArgumentNullException("Missing password.");
		}
		int num2 = bcryptString.LastIndexOf('$') + 1;
		int num3 = bcryptString.Length - 31;
		byte[] salt = DecodeSaltString(bcryptString.Substring(num2, num3 - num2));
		string value = Generate(text, password, salt, num);
		return bcryptString.Equals(value);
	}

	private static string EncodeData(byte[] data)
	{
		if (data.Length != 24 && data.Length != 16)
		{
			throw new DataLengthException("Invalid length: " + data.Length + ", 24 for key or 16 for salt expected");
		}
		bool flag = false;
		if (data.Length == 16)
		{
			flag = true;
			byte[] array = new byte[18];
			Array.Copy(data, 0, array, 0, data.Length);
			data = array;
		}
		else
		{
			data[data.Length - 1] = 0;
		}
		MemoryStream memoryStream = new MemoryStream();
		int num = data.Length;
		for (int i = 0; i < num; i += 3)
		{
			uint num2 = data[i];
			uint num3 = data[i + 1];
			uint num4 = data[i + 2];
			memoryStream.WriteByte(EncodingTable[(num2 >> 2) & 0x3F]);
			memoryStream.WriteByte(EncodingTable[((num2 << 4) | (num3 >> 4)) & 0x3F]);
			memoryStream.WriteByte(EncodingTable[((num3 << 2) | (num4 >> 6)) & 0x3F]);
			memoryStream.WriteByte(EncodingTable[num4 & 0x3F]);
		}
		string text = Strings.FromByteArray(memoryStream.ToArray());
		int length = (flag ? 22 : (text.Length - 1));
		return text.Substring(0, length);
	}

	private static byte[] DecodeSaltString(string saltString)
	{
		char[] array = saltString.ToCharArray();
		MemoryStream memoryStream = new MemoryStream(16);
		if (array.Length != 22)
		{
			throw new DataLengthException("Invalid base64 salt length: " + array.Length + " , 22 required.");
		}
		foreach (int num in array)
		{
			switch (num)
			{
			case 46:
			case 47:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 71:
			case 72:
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
			case 78:
			case 79:
			case 80:
			case 81:
			case 82:
			case 83:
			case 84:
			case 85:
			case 86:
			case 87:
			case 88:
			case 89:
			case 90:
			case 91:
			case 92:
			case 93:
			case 94:
			case 95:
			case 96:
			case 97:
			case 98:
			case 99:
			case 100:
			case 101:
			case 102:
			case 103:
			case 104:
			case 105:
			case 106:
			case 107:
			case 108:
			case 109:
			case 110:
			case 111:
			case 112:
			case 113:
			case 114:
			case 115:
			case 116:
			case 117:
			case 118:
			case 119:
			case 120:
			case 121:
			case 122:
				continue;
			}
			throw new ArgumentException("Salt string contains invalid character: " + num, "saltString");
		}
		char[] array2 = new char[24];
		Array.Copy(array, 0, array2, 0, array.Length);
		array = array2;
		int num2 = array.Length;
		for (int j = 0; j < num2; j += 4)
		{
			byte b = DecodingTable[(uint)array[j]];
			byte b2 = DecodingTable[(uint)array[j + 1]];
			byte b3 = DecodingTable[(uint)array[j + 2]];
			byte b4 = DecodingTable[(uint)array[j + 3]];
			memoryStream.WriteByte((byte)((b << 2) | (b2 >> 4)));
			memoryStream.WriteByte((byte)((b2 << 4) | (b3 >> 2)));
			memoryStream.WriteByte((byte)((b3 << 6) | b4));
		}
		byte[] sourceArray = memoryStream.ToArray();
		byte[] array3 = new byte[16];
		Array.Copy(sourceArray, 0, array3, 0, array3.Length);
		return array3;
	}
}
