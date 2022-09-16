using System;

namespace Org.BouncyCastle.Bcpg;

public class PublicSubkeyPacket : PublicKeyPacket
{
	internal PublicSubkeyPacket(BcpgInputStream bcpgIn)
		: base(bcpgIn)
	{
	}

	public PublicSubkeyPacket(PublicKeyAlgorithmTag algorithm, DateTime time, IBcpgKey key)
		: base(algorithm, time, key)
	{
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(PacketTag.PublicSubkey, GetEncodedContents(), oldFormat: true);
	}
}
