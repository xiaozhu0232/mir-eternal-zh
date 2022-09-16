using System.IO;

namespace Org.BouncyCastle.Bcpg;

public class TrustPacket : ContainedPacket
{
	private readonly byte[] levelAndTrustAmount;

	public TrustPacket(BcpgInputStream bcpgIn)
	{
		MemoryStream memoryStream = new MemoryStream();
		int num;
		while ((num = bcpgIn.ReadByte()) >= 0)
		{
			memoryStream.WriteByte((byte)num);
		}
		levelAndTrustAmount = memoryStream.ToArray();
	}

	public TrustPacket(int trustCode)
	{
		levelAndTrustAmount = new byte[1] { (byte)trustCode };
	}

	public byte[] GetLevelAndTrustAmount()
	{
		return (byte[])levelAndTrustAmount.Clone();
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(PacketTag.Trust, levelAndTrustAmount, oldFormat: true);
	}
}
