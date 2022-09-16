using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Net;

namespace Org.BouncyCastle.Asn1.X509;

public class GeneralName : Asn1Encodable, IAsn1Choice
{
	public const int OtherName = 0;

	public const int Rfc822Name = 1;

	public const int DnsName = 2;

	public const int X400Address = 3;

	public const int DirectoryName = 4;

	public const int EdiPartyName = 5;

	public const int UniformResourceIdentifier = 6;

	public const int IPAddress = 7;

	public const int RegisteredID = 8;

	internal readonly Asn1Encodable obj;

	internal readonly int tag;

	public int TagNo => tag;

	public Asn1Encodable Name => obj;

	public GeneralName(X509Name directoryName)
	{
		obj = directoryName;
		tag = 4;
	}

	public GeneralName(Asn1Object name, int tag)
	{
		obj = name;
		this.tag = tag;
	}

	public GeneralName(int tag, Asn1Encodable name)
	{
		obj = name;
		this.tag = tag;
	}

	public GeneralName(int tag, string name)
	{
		this.tag = tag;
		switch (tag)
		{
		case 1:
		case 2:
		case 6:
			obj = new DerIA5String(name);
			break;
		case 8:
			obj = new DerObjectIdentifier(name);
			break;
		case 4:
			obj = new X509Name(name);
			break;
		case 7:
		{
			byte[] array = toGeneralNameEncoding(name);
			if (array == null)
			{
				throw new ArgumentException("IP Address is invalid", "name");
			}
			obj = new DerOctetString(array);
			break;
		}
		default:
			throw new ArgumentException("can't process string for tag: " + tag, "tag");
		}
	}

	public static GeneralName GetInstance(object obj)
	{
		if (obj == null || obj is GeneralName)
		{
			return (GeneralName)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)obj;
			int tagNo = asn1TaggedObject.TagNo;
			switch (tagNo)
			{
			case 0:
			case 3:
			case 5:
				return new GeneralName(tagNo, Asn1Sequence.GetInstance(asn1TaggedObject, explicitly: false));
			case 1:
			case 2:
			case 6:
				return new GeneralName(tagNo, DerIA5String.GetInstance(asn1TaggedObject, isExplicit: false));
			case 4:
				return new GeneralName(tagNo, X509Name.GetInstance(asn1TaggedObject, explicitly: true));
			case 7:
				return new GeneralName(tagNo, Asn1OctetString.GetInstance(asn1TaggedObject, isExplicit: false));
			case 8:
				return new GeneralName(tagNo, DerObjectIdentifier.GetInstance(asn1TaggedObject, explicitly: false));
			default:
				throw new ArgumentException("unknown tag: " + tagNo);
			}
		}
		if (obj is byte[])
		{
			try
			{
				return GetInstance(Asn1Object.FromByteArray((byte[])obj));
			}
			catch (IOException)
			{
				throw new ArgumentException("unable to parse encoded general name");
			}
		}
		throw new ArgumentException("unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static GeneralName GetInstance(Asn1TaggedObject tagObj, bool explicitly)
	{
		return GetInstance(Asn1TaggedObject.GetInstance(tagObj, explicitly: true));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(tag);
		stringBuilder.Append(": ");
		switch (tag)
		{
		case 1:
		case 2:
		case 6:
			stringBuilder.Append(DerIA5String.GetInstance(obj).GetString());
			break;
		case 4:
			stringBuilder.Append(X509Name.GetInstance(obj).ToString());
			break;
		default:
			stringBuilder.Append(obj.ToString());
			break;
		}
		return stringBuilder.ToString();
	}

	private byte[] toGeneralNameEncoding(string ip)
	{
		if (Org.BouncyCastle.Utilities.Net.IPAddress.IsValidIPv6WithNetmask(ip) || Org.BouncyCastle.Utilities.Net.IPAddress.IsValidIPv6(ip))
		{
			int num = ip.IndexOf('/');
			if (num < 0)
			{
				byte[] array = new byte[16];
				int[] parsedIp = parseIPv6(ip);
				copyInts(parsedIp, array, 0);
				return array;
			}
			byte[] array2 = new byte[32];
			int[] parsedIp2 = parseIPv6(ip.Substring(0, num));
			copyInts(parsedIp2, array2, 0);
			string text = ip.Substring(num + 1);
			parsedIp2 = ((text.IndexOf(':') <= 0) ? parseMask(text) : parseIPv6(text));
			copyInts(parsedIp2, array2, 16);
			return array2;
		}
		if (Org.BouncyCastle.Utilities.Net.IPAddress.IsValidIPv4WithNetmask(ip) || Org.BouncyCastle.Utilities.Net.IPAddress.IsValidIPv4(ip))
		{
			int num2 = ip.IndexOf('/');
			if (num2 < 0)
			{
				byte[] array3 = new byte[4];
				parseIPv4(ip, array3, 0);
				return array3;
			}
			byte[] array4 = new byte[8];
			parseIPv4(ip.Substring(0, num2), array4, 0);
			string text2 = ip.Substring(num2 + 1);
			if (text2.IndexOf('.') > 0)
			{
				parseIPv4(text2, array4, 4);
			}
			else
			{
				parseIPv4Mask(text2, array4, 4);
			}
			return array4;
		}
		return null;
	}

	private void parseIPv4Mask(string mask, byte[] addr, int offset)
	{
		int num = int.Parse(mask);
		for (int i = 0; i != num; i++)
		{
			byte[] array;
			byte[] array2 = (array = addr);
			int num2 = i / 8 + offset;
			nint num3 = num2;
			array2[num2] = (byte)(array[num3] | (byte)(1 << i % 8));
		}
	}

	private void parseIPv4(string ip, byte[] addr, int offset)
	{
		string[] array = ip.Split('.', '/');
		foreach (string s in array)
		{
			addr[offset++] = (byte)int.Parse(s);
		}
	}

	private int[] parseMask(string mask)
	{
		int[] array = new int[8];
		int num = int.Parse(mask);
		for (int i = 0; i != num; i++)
		{
			int[] array2;
			int[] array3 = (array2 = array);
			int num2 = i / 16;
			nint num3 = num2;
			array3[num2] = array2[num3] | (1 << i % 16);
		}
		return array;
	}

	private void copyInts(int[] parsedIp, byte[] addr, int offSet)
	{
		for (int i = 0; i != parsedIp.Length; i++)
		{
			addr[i * 2 + offSet] = (byte)(parsedIp[i] >> 8);
			addr[i * 2 + 1 + offSet] = (byte)parsedIp[i];
		}
	}

	private int[] parseIPv6(string ip)
	{
		if (Platform.StartsWith(ip, "::"))
		{
			ip = ip.Substring(1);
		}
		else if (Platform.EndsWith(ip, "::"))
		{
			ip = ip.Substring(0, ip.Length - 1);
		}
		IEnumerator enumerator = ip.Split(':').GetEnumerator();
		int num = 0;
		int[] array = new int[8];
		int num2 = -1;
		while (enumerator.MoveNext())
		{
			string text = (string)enumerator.Current;
			if (text.Length == 0)
			{
				num2 = num;
				array[num++] = 0;
				continue;
			}
			if (text.IndexOf('.') < 0)
			{
				array[num++] = int.Parse(text, NumberStyles.AllowHexSpecifier);
				continue;
			}
			string[] array2 = text.Split('.');
			array[num++] = (int.Parse(array2[0]) << 8) | int.Parse(array2[1]);
			array[num++] = (int.Parse(array2[2]) << 8) | int.Parse(array2[3]);
		}
		if (num != array.Length)
		{
			Array.Copy(array, num2, array, array.Length - (num - num2), num - num2);
			for (int i = num2; i != array.Length - (num - num2); i++)
			{
				array[i] = 0;
			}
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		bool explicitly = tag == 4;
		return new DerTaggedObject(explicitly, tag, obj);
	}
}
