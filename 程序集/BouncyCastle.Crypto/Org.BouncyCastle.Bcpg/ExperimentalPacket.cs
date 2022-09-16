namespace Org.BouncyCastle.Bcpg;

public class ExperimentalPacket : ContainedPacket
{
	private readonly PacketTag tag;

	private readonly byte[] contents;

	public PacketTag Tag => tag;

	internal ExperimentalPacket(PacketTag tag, BcpgInputStream bcpgIn)
	{
		this.tag = tag;
		contents = bcpgIn.ReadAll();
	}

	public byte[] GetContents()
	{
		return (byte[])contents.Clone();
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(tag, contents, oldFormat: true);
	}
}
