using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class S2k : BcpgObject
{
	private const int ExpBias = 6;

	public const int Simple = 0;

	public const int Salted = 1;

	public const int SaltedAndIterated = 3;

	public const int GnuDummyS2K = 101;

	public const int GnuProtectionModeNoPrivateKey = 1;

	public const int GnuProtectionModeDivertToCard = 2;

	internal int type;

	internal HashAlgorithmTag algorithm;

	internal byte[] iv;

	internal int itCount = -1;

	internal int protectionMode = -1;

	public virtual int Type => type;

	public virtual HashAlgorithmTag HashAlgorithm => algorithm;

	public virtual long IterationCount => 16 + (itCount & 0xF) << (itCount >> 4) + 6;

	public virtual int ProtectionMode => protectionMode;

	internal S2k(Stream inStr)
	{
		type = inStr.ReadByte();
		algorithm = (HashAlgorithmTag)inStr.ReadByte();
		if (type != 101)
		{
			if (type != 0)
			{
				iv = new byte[8];
				if (Streams.ReadFully(inStr, iv, 0, iv.Length) < iv.Length)
				{
					throw new EndOfStreamException();
				}
				if (type == 3)
				{
					itCount = inStr.ReadByte();
				}
			}
		}
		else
		{
			inStr.ReadByte();
			inStr.ReadByte();
			inStr.ReadByte();
			protectionMode = inStr.ReadByte();
		}
	}

	public S2k(HashAlgorithmTag algorithm)
	{
		type = 0;
		this.algorithm = algorithm;
	}

	public S2k(HashAlgorithmTag algorithm, byte[] iv)
	{
		type = 1;
		this.algorithm = algorithm;
		this.iv = iv;
	}

	public S2k(HashAlgorithmTag algorithm, byte[] iv, int itCount)
	{
		type = 3;
		this.algorithm = algorithm;
		this.iv = iv;
		this.itCount = itCount;
	}

	public virtual byte[] GetIV()
	{
		return Arrays.Clone(iv);
	}

	[Obsolete("Use 'IterationCount' property instead")]
	public long GetIterationCount()
	{
		return IterationCount;
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		bcpgOut.WriteByte((byte)type);
		bcpgOut.WriteByte((byte)algorithm);
		if (type != 101)
		{
			if (type != 0)
			{
				bcpgOut.Write(iv);
			}
			if (type == 3)
			{
				bcpgOut.WriteByte((byte)itCount);
			}
		}
		else
		{
			bcpgOut.WriteByte(71);
			bcpgOut.WriteByte(78);
			bcpgOut.WriteByte(85);
			bcpgOut.WriteByte((byte)protectionMode);
		}
	}
}
