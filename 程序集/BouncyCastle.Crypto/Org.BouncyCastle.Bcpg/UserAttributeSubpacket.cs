using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg;

public class UserAttributeSubpacket
{
	internal readonly UserAttributeSubpacketTag type;

	private readonly bool longLength;

	protected readonly byte[] data;

	public virtual UserAttributeSubpacketTag SubpacketType => type;

	protected internal UserAttributeSubpacket(UserAttributeSubpacketTag type, byte[] data)
		: this(type, forceLongLength: false, data)
	{
	}

	protected internal UserAttributeSubpacket(UserAttributeSubpacketTag type, bool forceLongLength, byte[] data)
	{
		this.type = type;
		longLength = forceLongLength;
		this.data = data;
	}

	public virtual byte[] GetData()
	{
		return data;
	}

	public virtual void Encode(Stream os)
	{
		int num = data.Length + 1;
		if (num < 192 && !longLength)
		{
			os.WriteByte((byte)num);
		}
		else if (num <= 8383 && !longLength)
		{
			num -= 192;
			os.WriteByte((byte)(((num >> 8) & 0xFF) + 192));
			os.WriteByte((byte)num);
		}
		else
		{
			os.WriteByte(byte.MaxValue);
			os.WriteByte((byte)(num >> 24));
			os.WriteByte((byte)(num >> 16));
			os.WriteByte((byte)(num >> 8));
			os.WriteByte((byte)num);
		}
		os.WriteByte((byte)type);
		os.Write(data, 0, data.Length);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is UserAttributeSubpacket userAttributeSubpacket))
		{
			return false;
		}
		if (type == userAttributeSubpacket.type)
		{
			return Arrays.AreEqual(data, userAttributeSubpacket.data);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return type.GetHashCode() ^ Arrays.GetHashCode(data);
	}
}
