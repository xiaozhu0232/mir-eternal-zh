namespace Org.BouncyCastle.Bcpg;

public class SymmetricEncDataPacket : InputStreamPacket
{
	public SymmetricEncDataPacket(BcpgInputStream bcpgIn)
		: base(bcpgIn)
	{
	}
}
