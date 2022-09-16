using System.IO;

namespace Org.BouncyCastle.Bcpg;

public abstract class ContainedPacket : Packet
{
	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.WritePacket(this);
		return memoryStream.ToArray();
	}

	public abstract void Encode(BcpgOutputStream bcpgOut);
}
