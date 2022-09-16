using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg;

public class LiteralDataPacket : InputStreamPacket
{
	private int format;

	private byte[] fileName;

	private long modDate;

	public int Format => format;

	public long ModificationTime => modDate;

	public string FileName => Strings.FromUtf8ByteArray(fileName);

	internal LiteralDataPacket(BcpgInputStream bcpgIn)
		: base(bcpgIn)
	{
		format = bcpgIn.ReadByte();
		int num = bcpgIn.ReadByte();
		fileName = new byte[num];
		for (int i = 0; i != num; i++)
		{
			int num2 = bcpgIn.ReadByte();
			if (num2 < 0)
			{
				throw new IOException("literal data truncated in header");
			}
			fileName[i] = (byte)num2;
		}
		modDate = (long)(uint)((bcpgIn.ReadByte() << 24) | (bcpgIn.ReadByte() << 16) | (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte()) * 1000L;
	}

	public byte[] GetRawFileName()
	{
		return Arrays.Clone(fileName);
	}
}
