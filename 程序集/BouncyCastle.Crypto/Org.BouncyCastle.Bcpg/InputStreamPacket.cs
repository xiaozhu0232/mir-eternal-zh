namespace Org.BouncyCastle.Bcpg;

public class InputStreamPacket : Packet
{
	private readonly BcpgInputStream bcpgIn;

	public InputStreamPacket(BcpgInputStream bcpgIn)
	{
		this.bcpgIn = bcpgIn;
	}

	public BcpgInputStream GetInputStream()
	{
		return bcpgIn;
	}
}
