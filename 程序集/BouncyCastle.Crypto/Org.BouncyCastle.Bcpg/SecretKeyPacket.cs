using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg;

public class SecretKeyPacket : ContainedPacket
{
	public const int UsageNone = 0;

	public const int UsageChecksum = 255;

	public const int UsageSha1 = 254;

	private PublicKeyPacket pubKeyPacket;

	private readonly byte[] secKeyData;

	private int s2kUsage;

	private SymmetricKeyAlgorithmTag encAlgorithm;

	private S2k s2k;

	private byte[] iv;

	public SymmetricKeyAlgorithmTag EncAlgorithm => encAlgorithm;

	public int S2kUsage => s2kUsage;

	public S2k S2k => s2k;

	public PublicKeyPacket PublicKeyPacket => pubKeyPacket;

	internal SecretKeyPacket(BcpgInputStream bcpgIn)
	{
		if (this is SecretSubkeyPacket)
		{
			pubKeyPacket = new PublicSubkeyPacket(bcpgIn);
		}
		else
		{
			pubKeyPacket = new PublicKeyPacket(bcpgIn);
		}
		s2kUsage = bcpgIn.ReadByte();
		if (s2kUsage == 255 || s2kUsage == 254)
		{
			encAlgorithm = (SymmetricKeyAlgorithmTag)bcpgIn.ReadByte();
			s2k = new S2k(bcpgIn);
		}
		else
		{
			encAlgorithm = (SymmetricKeyAlgorithmTag)s2kUsage;
		}
		if ((s2k == null || s2k.Type != 101 || s2k.ProtectionMode != 1) && s2kUsage != 0)
		{
			if (encAlgorithm < SymmetricKeyAlgorithmTag.Aes128)
			{
				iv = new byte[8];
			}
			else
			{
				iv = new byte[16];
			}
			bcpgIn.ReadFully(iv);
		}
		secKeyData = bcpgIn.ReadAll();
	}

	public SecretKeyPacket(PublicKeyPacket pubKeyPacket, SymmetricKeyAlgorithmTag encAlgorithm, S2k s2k, byte[] iv, byte[] secKeyData)
	{
		this.pubKeyPacket = pubKeyPacket;
		this.encAlgorithm = encAlgorithm;
		if (encAlgorithm != 0)
		{
			s2kUsage = 255;
		}
		else
		{
			s2kUsage = 0;
		}
		this.s2k = s2k;
		this.iv = Arrays.Clone(iv);
		this.secKeyData = secKeyData;
	}

	public SecretKeyPacket(PublicKeyPacket pubKeyPacket, SymmetricKeyAlgorithmTag encAlgorithm, int s2kUsage, S2k s2k, byte[] iv, byte[] secKeyData)
	{
		this.pubKeyPacket = pubKeyPacket;
		this.encAlgorithm = encAlgorithm;
		this.s2kUsage = s2kUsage;
		this.s2k = s2k;
		this.iv = Arrays.Clone(iv);
		this.secKeyData = secKeyData;
	}

	public byte[] GetIV()
	{
		return Arrays.Clone(iv);
	}

	public byte[] GetSecretKeyData()
	{
		return secKeyData;
	}

	public byte[] GetEncodedContents()
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.Write(pubKeyPacket.GetEncodedContents());
		bcpgOutputStream.WriteByte((byte)s2kUsage);
		if (s2kUsage == 255 || s2kUsage == 254)
		{
			bcpgOutputStream.WriteByte((byte)encAlgorithm);
			bcpgOutputStream.WriteObject(s2k);
		}
		if (iv != null)
		{
			bcpgOutputStream.Write(iv);
		}
		if (secKeyData != null && secKeyData.Length > 0)
		{
			bcpgOutputStream.Write(secKeyData);
		}
		return memoryStream.ToArray();
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(PacketTag.SecretKey, GetEncodedContents(), oldFormat: true);
	}
}
