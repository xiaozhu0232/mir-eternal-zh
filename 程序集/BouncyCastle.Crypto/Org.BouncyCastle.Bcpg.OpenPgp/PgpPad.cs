using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public sealed class PgpPad
{
	private PgpPad()
	{
	}

	public static byte[] PadSessionData(byte[] sessionInfo)
	{
		return PadSessionData(sessionInfo, obfuscate: true);
	}

	public static byte[] PadSessionData(byte[] sessionInfo, bool obfuscate)
	{
		int num = sessionInfo.Length;
		int num2 = (num >> 3) + 1 << 3;
		if (obfuscate)
		{
			num2 = System.Math.Max(40, num2);
		}
		int num3 = num2 - num;
		byte b = (byte)num3;
		byte[] array = new byte[num2];
		Array.Copy(sessionInfo, 0, array, 0, num);
		for (int i = num; i < num2; i++)
		{
			array[i] = b;
		}
		return array;
	}

	public static byte[] UnpadSessionData(byte[] encoded)
	{
		int num = encoded.Length;
		byte b = encoded[num - 1];
		int num2 = b;
		int num3 = num - num2;
		int num4 = num3 - 1;
		int num5 = 0;
		for (int i = 0; i < num; i++)
		{
			int num6 = num4 - i >> 31;
			num5 |= (b ^ encoded[i]) & num6;
		}
		num5 |= num & 7;
		if ((num5 | (40 - num >> 31)) != 0)
		{
			throw new PgpException("bad padding found in session data");
		}
		byte[] array = new byte[num3];
		Array.Copy(encoded, 0, array, 0, num3);
		return array;
	}
}
