using System;
using System.IO;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg;

public class PublicKeyPacket : ContainedPacket
{
	private int version;

	private long time;

	private int validDays;

	private PublicKeyAlgorithmTag algorithm;

	private IBcpgKey key;

	public virtual int Version => version;

	public virtual PublicKeyAlgorithmTag Algorithm => algorithm;

	public virtual int ValidDays => validDays;

	public virtual IBcpgKey Key => key;

	internal PublicKeyPacket(BcpgInputStream bcpgIn)
	{
		version = bcpgIn.ReadByte();
		time = (uint)((bcpgIn.ReadByte() << 24) | (bcpgIn.ReadByte() << 16) | (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte());
		if (version <= 3)
		{
			validDays = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
		}
		algorithm = (PublicKeyAlgorithmTag)bcpgIn.ReadByte();
		switch (algorithm)
		{
		case PublicKeyAlgorithmTag.RsaGeneral:
		case PublicKeyAlgorithmTag.RsaEncrypt:
		case PublicKeyAlgorithmTag.RsaSign:
			key = new RsaPublicBcpgKey(bcpgIn);
			break;
		case PublicKeyAlgorithmTag.Dsa:
			key = new DsaPublicBcpgKey(bcpgIn);
			break;
		case PublicKeyAlgorithmTag.ElGamalEncrypt:
		case PublicKeyAlgorithmTag.ElGamalGeneral:
			key = new ElGamalPublicBcpgKey(bcpgIn);
			break;
		case PublicKeyAlgorithmTag.EC:
			key = new ECDHPublicBcpgKey(bcpgIn);
			break;
		case PublicKeyAlgorithmTag.ECDsa:
			key = new ECDsaPublicBcpgKey(bcpgIn);
			break;
		default:
			throw new IOException("unknown PGP public key algorithm encountered");
		}
	}

	public PublicKeyPacket(PublicKeyAlgorithmTag algorithm, DateTime time, IBcpgKey key)
	{
		version = 4;
		this.time = DateTimeUtilities.DateTimeToUnixMs(time) / 1000;
		this.algorithm = algorithm;
		this.key = key;
	}

	public virtual DateTime GetTime()
	{
		return DateTimeUtilities.UnixMsToDateTime(time * 1000);
	}

	public virtual byte[] GetEncodedContents()
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.WriteByte((byte)version);
		bcpgOutputStream.WriteInt((int)time);
		if (version <= 3)
		{
			bcpgOutputStream.WriteShort((short)validDays);
		}
		bcpgOutputStream.WriteByte((byte)algorithm);
		bcpgOutputStream.WriteObject((BcpgObject)key);
		return memoryStream.ToArray();
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WritePacket(PacketTag.PublicKey, GetEncodedContents(), oldFormat: true);
	}
}
