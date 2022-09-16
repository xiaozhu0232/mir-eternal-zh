using System;

namespace Org.BouncyCastle.Bcpg.Sig;

public class KeyFlags : SignatureSubpacket
{
	public const int CertifyOther = 1;

	public const int SignData = 2;

	public const int EncryptComms = 4;

	public const int EncryptStorage = 8;

	public const int Split = 16;

	public const int Authentication = 32;

	public const int Shared = 128;

	public int Flags
	{
		get
		{
			int num = 0;
			for (int i = 0; i != data.Length; i++)
			{
				num |= (data[i] & 0xFF) << i * 8;
			}
			return num;
		}
	}

	private static byte[] IntToByteArray(int v)
	{
		byte[] array = new byte[4];
		int num = 0;
		for (int i = 0; i != 4; i++)
		{
			array[i] = (byte)(v >> i * 8);
			if (array[i] != 0)
			{
				num = i;
			}
		}
		byte[] array2 = new byte[num + 1];
		Array.Copy(array, 0, array2, 0, array2.Length);
		return array2;
	}

	public KeyFlags(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.KeyFlags, critical, isLongLength, data)
	{
	}

	public KeyFlags(bool critical, int flags)
		: base(SignatureSubpacketTag.KeyFlags, critical, isLongLength: false, IntToByteArray(flags))
	{
	}
}
