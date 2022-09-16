using System;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class ChaCha7539Engine : Salsa20Engine
{
	public override string AlgorithmName => "ChaCha7539";

	protected override int NonceSize => 12;

	protected override void AdvanceCounter()
	{
		uint[] array;
		if (((array = engineState)[12] = array[12] + 1) == 0)
		{
			throw new InvalidOperationException("attempt to increase counter past 2^32.");
		}
	}

	protected override void ResetCounter()
	{
		engineState[12] = 0u;
	}

	protected override void SetKey(byte[] keyBytes, byte[] ivBytes)
	{
		if (keyBytes != null)
		{
			if (keyBytes.Length != 32)
			{
				throw new ArgumentException(AlgorithmName + " requires 256 bit key");
			}
			PackTauOrSigma(keyBytes.Length, engineState, 0);
			Pack.LE_To_UInt32(keyBytes, 0, engineState, 4, 8);
		}
		Pack.LE_To_UInt32(ivBytes, 0, engineState, 13, 3);
	}

	protected override void GenerateKeyStream(byte[] output)
	{
		ChaChaEngine.ChachaCore(rounds, engineState, x);
		Pack.UInt32_To_LE(x, output, 0);
	}
}
