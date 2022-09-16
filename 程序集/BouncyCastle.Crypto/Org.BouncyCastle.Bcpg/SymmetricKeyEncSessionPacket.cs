using System.IO;

namespace Org.BouncyCastle.Bcpg;

public class SymmetricKeyEncSessionPacket : ContainedPacket
{
	private int version;

	private SymmetricKeyAlgorithmTag encAlgorithm;

	private S2k s2k;

	private readonly byte[] secKeyData;

	public SymmetricKeyAlgorithmTag EncAlgorithm => encAlgorithm;

	public S2k S2k => s2k;

	public int Version => version;

	public SymmetricKeyEncSessionPacket(BcpgInputStream bcpgIn)
	{
		version = bcpgIn.ReadByte();
		encAlgorithm = (SymmetricKeyAlgorithmTag)bcpgIn.ReadByte();
		s2k = new S2k(bcpgIn);
		secKeyData = bcpgIn.ReadAll();
	}

	public SymmetricKeyEncSessionPacket(SymmetricKeyAlgorithmTag encAlgorithm, S2k s2k, byte[] secKeyData)
	{
		version = 4;
		this.encAlgorithm = encAlgorithm;
		this.s2k = s2k;
		this.secKeyData = secKeyData;
	}

	public byte[] GetSecKeyData()
	{
		return secKeyData;
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.Write((byte)version, (byte)encAlgorithm);
		bcpgOutputStream.WriteObject(s2k);
		if (secKeyData != null && secKeyData.Length > 0)
		{
			bcpgOutputStream.Write(secKeyData);
		}
		bcpgOut.WritePacket(PacketTag.SymmetricKeyEncryptedSessionKey, memoryStream.ToArray(), oldFormat: true);
	}
}
