using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1;

public class DerObjectIdentifier : Asn1Object
{
	private const long LONG_LIMIT = 72057594037927808L;

	private readonly string identifier;

	private byte[] body = null;

	private static readonly DerObjectIdentifier[] cache = new DerObjectIdentifier[1024];

	public string Id => identifier;

	public static DerObjectIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is DerObjectIdentifier)
		{
			return (DerObjectIdentifier)obj;
		}
		if (obj is Asn1Encodable)
		{
			Asn1Object asn1Object = ((Asn1Encodable)obj).ToAsn1Object();
			if (asn1Object is DerObjectIdentifier)
			{
				return (DerObjectIdentifier)asn1Object;
			}
		}
		if (obj is byte[])
		{
			return FromOctetString((byte[])obj);
		}
		throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static DerObjectIdentifier GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		Asn1Object @object = obj.GetObject();
		if (explicitly || @object is DerObjectIdentifier)
		{
			return GetInstance(@object);
		}
		return FromOctetString(Asn1OctetString.GetInstance(@object).GetOctets());
	}

	public DerObjectIdentifier(string identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (!IsValidIdentifier(identifier))
		{
			throw new FormatException("string " + identifier + " not an OID");
		}
		this.identifier = identifier;
	}

	internal DerObjectIdentifier(DerObjectIdentifier oid, string branchID)
	{
		if (!IsValidBranchID(branchID, 0))
		{
			throw new ArgumentException("string " + branchID + " not a valid OID branch", "branchID");
		}
		identifier = oid.Id + "." + branchID;
	}

	public virtual DerObjectIdentifier Branch(string branchID)
	{
		return new DerObjectIdentifier(this, branchID);
	}

	public virtual bool On(DerObjectIdentifier stem)
	{
		string id = Id;
		string id2 = stem.Id;
		if (id.Length > id2.Length && id[id2.Length] == '.')
		{
			return Platform.StartsWith(id, id2);
		}
		return false;
	}

	internal DerObjectIdentifier(byte[] bytes)
	{
		identifier = MakeOidStringFromBytes(bytes);
		body = Arrays.Clone(bytes);
	}

	private void WriteField(Stream outputStream, long fieldValue)
	{
		byte[] array = new byte[9];
		int num = 8;
		array[num] = (byte)(fieldValue & 0x7F);
		while (fieldValue >= 128)
		{
			fieldValue >>= 7;
			array[--num] = (byte)((fieldValue & 0x7F) | 0x80);
		}
		outputStream.Write(array, num, 9 - num);
	}

	private void WriteField(Stream outputStream, BigInteger fieldValue)
	{
		int num = (fieldValue.BitLength + 6) / 7;
		if (num == 0)
		{
			outputStream.WriteByte(0);
			return;
		}
		BigInteger bigInteger = fieldValue;
		byte[] array = new byte[num];
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			array[num2] = (byte)(((uint)bigInteger.IntValue & 0x7Fu) | 0x80u);
			bigInteger = bigInteger.ShiftRight(7);
		}
		byte[] array2;
		byte[] array3 = (array2 = array);
		int num3 = num - 1;
		nint num4 = num3;
		array3[num3] = (byte)(array2[num4] & 0x7Fu);
		outputStream.Write(array, 0, array.Length);
	}

	private void DoOutput(MemoryStream bOut)
	{
		OidTokenizer oidTokenizer = new OidTokenizer(identifier);
		string s = oidTokenizer.NextToken();
		int num = int.Parse(s) * 40;
		s = oidTokenizer.NextToken();
		if (s.Length <= 18)
		{
			WriteField(bOut, num + long.Parse(s));
		}
		else
		{
			WriteField(bOut, new BigInteger(s).Add(BigInteger.ValueOf(num)));
		}
		while (oidTokenizer.HasMoreTokens)
		{
			s = oidTokenizer.NextToken();
			if (s.Length <= 18)
			{
				WriteField(bOut, long.Parse(s));
			}
			else
			{
				WriteField(bOut, new BigInteger(s));
			}
		}
	}

	internal byte[] GetBody()
	{
		lock (this)
		{
			if (body == null)
			{
				MemoryStream memoryStream = new MemoryStream();
				DoOutput(memoryStream);
				body = memoryStream.ToArray();
			}
		}
		return body;
	}

	internal override void Encode(DerOutputStream derOut)
	{
		derOut.WriteEncoded(6, GetBody());
	}

	protected override int Asn1GetHashCode()
	{
		return identifier.GetHashCode();
	}

	protected override bool Asn1Equals(Asn1Object asn1Object)
	{
		if (!(asn1Object is DerObjectIdentifier derObjectIdentifier))
		{
			return false;
		}
		return identifier.Equals(derObjectIdentifier.identifier);
	}

	public override string ToString()
	{
		return identifier;
	}

	private static bool IsValidBranchID(string branchID, int start)
	{
		int num = 0;
		int num2 = branchID.Length;
		while (--num2 >= start)
		{
			char c = branchID[num2];
			if (c == '.')
			{
				if (num == 0 || (num > 1 && branchID[num2 + 1] == '0'))
				{
					return false;
				}
				num = 0;
			}
			else
			{
				if ('0' > c || c > '9')
				{
					return false;
				}
				num++;
			}
		}
		if (num == 0 || (num > 1 && branchID[num2 + 1] == '0'))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidIdentifier(string identifier)
	{
		if (identifier.Length < 3 || identifier[1] != '.')
		{
			return false;
		}
		char c = identifier[0];
		if (c < '0' || c > '2')
		{
			return false;
		}
		return IsValidBranchID(identifier, 2);
	}

	private static string MakeOidStringFromBytes(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		long num = 0L;
		BigInteger bigInteger = null;
		bool flag = true;
		for (int i = 0; i != bytes.Length; i++)
		{
			int num2 = bytes[i];
			if (num <= 72057594037927808L)
			{
				num += num2 & 0x7F;
				if ((num2 & 0x80) == 0)
				{
					if (flag)
					{
						if (num < 40)
						{
							stringBuilder.Append('0');
						}
						else if (num < 80)
						{
							stringBuilder.Append('1');
							num -= 40;
						}
						else
						{
							stringBuilder.Append('2');
							num -= 80;
						}
						flag = false;
					}
					stringBuilder.Append('.');
					stringBuilder.Append(num);
					num = 0L;
				}
				else
				{
					num <<= 7;
				}
				continue;
			}
			if (bigInteger == null)
			{
				bigInteger = BigInteger.ValueOf(num);
			}
			bigInteger = bigInteger.Or(BigInteger.ValueOf(num2 & 0x7F));
			if ((num2 & 0x80) == 0)
			{
				if (flag)
				{
					stringBuilder.Append('2');
					bigInteger = bigInteger.Subtract(BigInteger.ValueOf(80L));
					flag = false;
				}
				stringBuilder.Append('.');
				stringBuilder.Append(bigInteger);
				bigInteger = null;
				num = 0L;
			}
			else
			{
				bigInteger = bigInteger.ShiftLeft(7);
			}
		}
		return stringBuilder.ToString();
	}

	internal static DerObjectIdentifier FromOctetString(byte[] enc)
	{
		int hashCode = Arrays.GetHashCode(enc);
		int num = hashCode & 0x3FF;
		lock (cache)
		{
			DerObjectIdentifier derObjectIdentifier = cache[num];
			if (derObjectIdentifier != null && Arrays.AreEqual(enc, derObjectIdentifier.GetBody()))
			{
				return derObjectIdentifier;
			}
			return cache[num] = new DerObjectIdentifier(enc);
		}
	}
}
