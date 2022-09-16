using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class PublicKeyEncSessionPacket : ContainedPacket
{
	private int version;

	private long keyId;

	private PublicKeyAlgorithmTag algorithm;

	private byte[][] data;

	public int Version => version;

	public long KeyId => keyId;

	public PublicKeyAlgorithmTag Algorithm => algorithm;

	internal PublicKeyEncSessionPacket(BcpgInputStream bcpgIn)
	{
		version = bcpgIn.ReadByte();
		keyId |= (long)bcpgIn.ReadByte() << 56;
		keyId |= (long)bcpgIn.ReadByte() << 48;
		keyId |= (long)bcpgIn.ReadByte() << 40;
		keyId |= (long)bcpgIn.ReadByte() << 32;
		keyId |= (long)bcpgIn.ReadByte() << 24;
		keyId |= (long)bcpgIn.ReadByte() << 16;
		keyId |= (long)bcpgIn.ReadByte() << 8;
		keyId |= (uint)bcpgIn.ReadByte();
		algorithm = (PublicKeyAlgorithmTag)bcpgIn.ReadByte();
		switch (algorithm)
		{
		case PublicKeyAlgorithmTag.RsaGeneral:
		case PublicKeyAlgorithmTag.RsaEncrypt:
			data = new byte[1][] { new MPInteger(bcpgIn).GetEncoded() };
			break;
		case PublicKeyAlgorithmTag.ElGamalEncrypt:
		case PublicKeyAlgorithmTag.ElGamalGeneral:
		{
			MPInteger mPInteger = new MPInteger(bcpgIn);
			MPInteger mPInteger2 = new MPInteger(bcpgIn);
			data = new byte[2][]
			{
				mPInteger.GetEncoded(),
				mPInteger2.GetEncoded()
			};
			break;
		}
		case PublicKeyAlgorithmTag.EC:
			data = new byte[1][] { Streams.ReadAll(bcpgIn) };
			break;
		default:
			throw new IOException("unknown PGP public key algorithm encountered");
		}
	}

	public PublicKeyEncSessionPacket(long keyId, PublicKeyAlgorithmTag algorithm, byte[][] data)
	{
		version = 3;
		this.keyId = keyId;
		this.algorithm = algorithm;
		this.data = new byte[data.Length][];
		for (int i = 0; i < data.Length; i++)
		{
			this.data[i] = Arrays.Clone(data[i]);
		}
	}

	public byte[][] GetEncSessionKey()
	{
		return data;
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.WriteByte((byte)version);
		bcpgOutputStream.WriteLong(keyId);
		bcpgOutputStream.WriteByte((byte)algorithm);
		for (int i = 0; i < data.Length; i++)
		{
			bcpgOutputStream.Write(data[i]);
		}
		Platform.Dispose(bcpgOutputStream);
		bcpgOut.WritePacket(PacketTag.PublicKeyEncryptedSession, memoryStream.ToArray(), oldFormat: true);
	}
}
