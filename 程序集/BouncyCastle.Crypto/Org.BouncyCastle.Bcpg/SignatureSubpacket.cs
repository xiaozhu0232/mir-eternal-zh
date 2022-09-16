using System.IO;

namespace Org.BouncyCastle.Bcpg;

public class SignatureSubpacket
{
	private readonly SignatureSubpacketTag type;

	private readonly bool critical;

	private readonly bool isLongLength;

	internal byte[] data;

	public SignatureSubpacketTag SubpacketType => type;

	protected internal SignatureSubpacket(SignatureSubpacketTag type, bool critical, bool isLongLength, byte[] data)
	{
		this.type = type;
		this.critical = critical;
		this.isLongLength = isLongLength;
		this.data = data;
	}

	public bool IsCritical()
	{
		return critical;
	}

	public bool IsLongLength()
	{
		return isLongLength;
	}

	public byte[] GetData()
	{
		return (byte[])data.Clone();
	}

	public void Encode(Stream os)
	{
		int num = data.Length + 1;
		if (isLongLength)
		{
			os.WriteByte(byte.MaxValue);
			os.WriteByte((byte)(num >> 24));
			os.WriteByte((byte)(num >> 16));
			os.WriteByte((byte)(num >> 8));
			os.WriteByte((byte)num);
		}
		else if (num < 192)
		{
			os.WriteByte((byte)num);
		}
		else if (num <= 8383)
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
		if (critical)
		{
			os.WriteByte((byte)((SignatureSubpacketTag)128 | type));
		}
		else
		{
			os.WriteByte((byte)type);
		}
		os.Write(data, 0, data.Length);
	}
}
