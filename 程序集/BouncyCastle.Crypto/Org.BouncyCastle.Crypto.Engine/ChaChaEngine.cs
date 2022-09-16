using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class ChaChaEngine : Salsa20Engine
{
	public override string AlgorithmName => "ChaCha" + rounds;

	public ChaChaEngine()
	{
	}

	public ChaChaEngine(int rounds)
		: base(rounds)
	{
	}

	protected override void AdvanceCounter()
	{
		uint[] array;
		if (((array = engineState)[12] = array[12] + 1) == 0)
		{
			(array = engineState)[13] = array[13] + 1;
		}
	}

	protected override void ResetCounter()
	{
		engineState[12] = (engineState[13] = 0u);
	}

	protected override void SetKey(byte[] keyBytes, byte[] ivBytes)
	{
		if (keyBytes != null)
		{
			if (keyBytes.Length != 16 && keyBytes.Length != 32)
			{
				throw new ArgumentException(AlgorithmName + " requires 128 bit or 256 bit key");
			}
			PackTauOrSigma(keyBytes.Length, engineState, 0);
			Pack.LE_To_UInt32(keyBytes, 0, engineState, 4, 4);
			Pack.LE_To_UInt32(keyBytes, keyBytes.Length - 16, engineState, 8, 4);
		}
		Pack.LE_To_UInt32(ivBytes, 0, engineState, 14, 2);
	}

	protected override void GenerateKeyStream(byte[] output)
	{
		ChachaCore(rounds, engineState, x);
		Pack.UInt32_To_LE(x, output, 0);
	}

	internal static void ChachaCore(int rounds, uint[] input, uint[] x)
	{
		if (input.Length != 16)
		{
			throw new ArgumentException();
		}
		if (x.Length != 16)
		{
			throw new ArgumentException();
		}
		if (rounds % 2 != 0)
		{
			throw new ArgumentException("Number of rounds must be even");
		}
		uint num = input[0];
		uint num2 = input[1];
		uint num3 = input[2];
		uint num4 = input[3];
		uint num5 = input[4];
		uint num6 = input[5];
		uint num7 = input[6];
		uint num8 = input[7];
		uint num9 = input[8];
		uint num10 = input[9];
		uint num11 = input[10];
		uint num12 = input[11];
		uint num13 = input[12];
		uint num14 = input[13];
		uint num15 = input[14];
		uint num16 = input[15];
		for (int num17 = rounds; num17 > 0; num17 -= 2)
		{
			num += num5;
			num13 = Integers.RotateLeft(num13 ^ num, 16);
			num9 += num13;
			num5 = Integers.RotateLeft(num5 ^ num9, 12);
			num += num5;
			num13 = Integers.RotateLeft(num13 ^ num, 8);
			num9 += num13;
			num5 = Integers.RotateLeft(num5 ^ num9, 7);
			num2 += num6;
			num14 = Integers.RotateLeft(num14 ^ num2, 16);
			num10 += num14;
			num6 = Integers.RotateLeft(num6 ^ num10, 12);
			num2 += num6;
			num14 = Integers.RotateLeft(num14 ^ num2, 8);
			num10 += num14;
			num6 = Integers.RotateLeft(num6 ^ num10, 7);
			num3 += num7;
			num15 = Integers.RotateLeft(num15 ^ num3, 16);
			num11 += num15;
			num7 = Integers.RotateLeft(num7 ^ num11, 12);
			num3 += num7;
			num15 = Integers.RotateLeft(num15 ^ num3, 8);
			num11 += num15;
			num7 = Integers.RotateLeft(num7 ^ num11, 7);
			num4 += num8;
			num16 = Integers.RotateLeft(num16 ^ num4, 16);
			num12 += num16;
			num8 = Integers.RotateLeft(num8 ^ num12, 12);
			num4 += num8;
			num16 = Integers.RotateLeft(num16 ^ num4, 8);
			num12 += num16;
			num8 = Integers.RotateLeft(num8 ^ num12, 7);
			num += num6;
			num16 = Integers.RotateLeft(num16 ^ num, 16);
			num11 += num16;
			num6 = Integers.RotateLeft(num6 ^ num11, 12);
			num += num6;
			num16 = Integers.RotateLeft(num16 ^ num, 8);
			num11 += num16;
			num6 = Integers.RotateLeft(num6 ^ num11, 7);
			num2 += num7;
			num13 = Integers.RotateLeft(num13 ^ num2, 16);
			num12 += num13;
			num7 = Integers.RotateLeft(num7 ^ num12, 12);
			num2 += num7;
			num13 = Integers.RotateLeft(num13 ^ num2, 8);
			num12 += num13;
			num7 = Integers.RotateLeft(num7 ^ num12, 7);
			num3 += num8;
			num14 = Integers.RotateLeft(num14 ^ num3, 16);
			num9 += num14;
			num8 = Integers.RotateLeft(num8 ^ num9, 12);
			num3 += num8;
			num14 = Integers.RotateLeft(num14 ^ num3, 8);
			num9 += num14;
			num8 = Integers.RotateLeft(num8 ^ num9, 7);
			num4 += num5;
			num15 = Integers.RotateLeft(num15 ^ num4, 16);
			num10 += num15;
			num5 = Integers.RotateLeft(num5 ^ num10, 12);
			num4 += num5;
			num15 = Integers.RotateLeft(num15 ^ num4, 8);
			num10 += num15;
			num5 = Integers.RotateLeft(num5 ^ num10, 7);
		}
		x[0] = num + input[0];
		x[1] = num2 + input[1];
		x[2] = num3 + input[2];
		x[3] = num4 + input[3];
		x[4] = num5 + input[4];
		x[5] = num6 + input[5];
		x[6] = num7 + input[6];
		x[7] = num8 + input[7];
		x[8] = num9 + input[8];
		x[9] = num10 + input[9];
		x[10] = num11 + input[10];
		x[11] = num12 + input[11];
		x[12] = num13 + input[12];
		x[13] = num14 + input[13];
		x[14] = num15 + input[14];
		x[15] = num16 + input[15];
	}
}
