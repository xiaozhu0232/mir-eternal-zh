namespace Org.BouncyCastle.Bcpg;

public class MarkerPacket : ContainedPacket
{
	private byte[] marker = new byte[3] { 80, 71, 80 };

	public MarkerPacket(BcpgInputStream bcpgIn)
	{
		bcpgIn.ReadFully(marker);
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(PacketTag.Marker, marker, oldFormat: true);
	}
}
