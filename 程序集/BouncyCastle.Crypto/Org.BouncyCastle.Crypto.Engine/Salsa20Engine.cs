using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class Salsa20Engine : IStreamCipher
{
	private const int StateSize = 16;

	public static readonly int DEFAULT_ROUNDS = 20;

	private static readonly uint[] TAU_SIGMA = Pack.LE_To_UInt32(Strings.ToAsciiByteArray("expand 16-byte kexpand 32-byte k"), 0, 8);

	[Obsolete]
	protected static readonly byte[] sigma = Strings.ToAsciiByteArray("expand 32-byte k");

	[Obsolete]
	protected static readonly byte[] tau = Strings.ToAsciiByteArray("expand 16-byte k");

	protected int rounds;

	private int index = 0;

	internal uint[] engineState = new uint[16];

	internal uint[] x = new uint[16];

	private byte[] keyStream = new byte[64];

	private bool initialised = false;

	private uint cW0;

	private uint cW1;

	private uint cW2;

	protected virtual int NonceSize => 8;

	public virtual string AlgorithmName
	{
		get
		{
			string text = "Salsa20";
			if (rounds != DEFAULT_ROUNDS)
			{
				text = text + "/" + rounds;
			}
			return text;
		}
	}

	internal void PackTauOrSigma(int keyLength, uint[] state, int stateOffset)
	{
		int num = (keyLength - 16) / 4;
		state[stateOffset] = TAU_SIGMA[num];
		state[stateOffset + 1] = TAU_SIGMA[num + 1];
		state[stateOffset + 2] = TAU_SIGMA[num + 2];
		state[stateOffset + 3] = TAU_SIGMA[num + 3];
	}

	public Salsa20Engine()
		: this(DEFAULT_ROUNDS)
	{
	}

	public Salsa20Engine(int rounds)
	{
		if (rounds <= 0 || ((uint)rounds & (true ? 1u : 0u)) != 0)
		{
			throw new ArgumentException("'rounds' must be a positive, even number");
		}
		this.rounds = rounds;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is ParametersWithIV parametersWithIV))
		{
			throw new ArgumentException(AlgorithmName + " Init requires an IV", "parameters");
		}
		byte[] iV = parametersWithIV.GetIV();
		if (iV == null || iV.Length != NonceSize)
		{
			throw new ArgumentException(AlgorithmName + " requires exactly " + NonceSize + " bytes of IV");
		}
		ICipherParameters parameters2 = parametersWithIV.Parameters;
		if (parameters2 == null)
		{
			if (!initialised)
			{
				throw new InvalidOperationException(AlgorithmName + " KeyParameter can not be null for first initialisation");
			}
			SetKey(null, iV);
		}
		else
		{
			if (!(parameters2 is KeyParameter))
			{
				throw new ArgumentException(AlgorithmName + " Init parameters must contain a KeyParameter (or null for re-init)");
			}
			SetKey(((KeyParameter)parameters2).GetKey(), iV);
		}
		Reset();
		initialised = true;
	}

	public virtual byte ReturnByte(byte input)
	{
		if (LimitExceeded())
		{
			throw new MaxBytesExceededException("2^70 byte limit per IV; Change IV");
		}
		if (index == 0)
		{
			GenerateKeyStream(keyStream);
			AdvanceCounter();
		}
		byte result = (byte)(keyStream[index] ^ input);
		index = (index + 1) & 0x3F;
		return result;
	}

	protected virtual void AdvanceCounter()
	{
		uint[] array;
		if (((array = engineState)[8] = array[8] + 1) == 0)
		{
			(array = engineState)[9] = array[9] + 1;
		}
	}

	public virtual void ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff)
	{
		if (!initialised)
		{
			throw new InvalidOperationException(AlgorithmName + " not initialised");
		}
		Check.DataLength(inBytes, inOff, len, "input buffer too short");
		Check.OutputLength(outBytes, outOff, len, "output buffer too short");
		if (LimitExceeded((uint)len))
		{
			throw new MaxBytesExceededException("2^70 byte limit per IV would be exceeded; Change IV");
		}
		for (int i = 0; i < len; i++)
		{
			if (index == 0)
			{
				GenerateKeyStream(keyStream);
				AdvanceCounter();
			}
			outBytes[i + outOff] = (byte)(keyStream[index] ^ inBytes[i + inOff]);
			index = (index + 1) & 0x3F;
		}
	}

	public virtual void Reset()
	{
		index = 0;
		ResetLimitCounter();
		ResetCounter();
	}

	protected virtual void ResetCounter()
	{
		engineState[8] = (engineState[9] = 0u);
	}

	protected virtual void SetKey(byte[] keyBytes, byte[] ivBytes)
	{
		if (keyBytes != null)
		{
			if (keyBytes.Length != 16 && keyBytes.Length != 32)
			{
				throw new ArgumentException(AlgorithmName + " requires 128 bit or 256 bit key");
			}
			int num = (keyBytes.Length - 16) / 4;
			engineState[0] = TAU_SIGMA[num];
			engineState[5] = TAU_SIGMA[num + 1];
			engineState[10] = TAU_SIGMA[num + 2];
			engineState[15] = TAU_SIGMA[num + 3];
			Pack.LE_To_UInt32(keyBytes, 0, engineState, 1, 4);
			Pack.LE_To_UInt32(keyBytes, keyBytes.Length - 16, engineState, 11, 4);
		}
		Pack.LE_To_UInt32(ivBytes, 0, engineState, 6, 2);
	}

	protected virtual void GenerateKeyStream(byte[] output)
	{
		SalsaCore(rounds, engineState, x);
		Pack.UInt32_To_LE(x, output, 0);
	}

	internal static void SalsaCore(int rounds, uint[] input, uint[] x)
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
			num5 ^= Integers.RotateLeft(num + num13, 7);
			num9 ^= Integers.RotateLeft(num5 + num, 9);
			num13 ^= Integers.RotateLeft(num9 + num5, 13);
			num ^= Integers.RotateLeft(num13 + num9, 18);
			num10 ^= Integers.RotateLeft(num6 + num2, 7);
			num14 ^= Integers.RotateLeft(num10 + num6, 9);
			num2 ^= Integers.RotateLeft(num14 + num10, 13);
			num6 ^= Integers.RotateLeft(num2 + num14, 18);
			num15 ^= Integers.RotateLeft(num11 + num7, 7);
			num3 ^= Integers.RotateLeft(num15 + num11, 9);
			num7 ^= Integers.RotateLeft(num3 + num15, 13);
			num11 ^= Integers.RotateLeft(num7 + num3, 18);
			num4 ^= Integers.RotateLeft(num16 + num12, 7);
			num8 ^= Integers.RotateLeft(num4 + num16, 9);
			num12 ^= Integers.RotateLeft(num8 + num4, 13);
			num16 ^= Integers.RotateLeft(num12 + num8, 18);
			num2 ^= Integers.RotateLeft(num + num4, 7);
			num3 ^= Integers.RotateLeft(num2 + num, 9);
			num4 ^= Integers.RotateLeft(num3 + num2, 13);
			num ^= Integers.RotateLeft(num4 + num3, 18);
			num7 ^= Integers.RotateLeft(num6 + num5, 7);
			num8 ^= Integers.RotateLeft(num7 + num6, 9);
			num5 ^= Integers.RotateLeft(num8 + num7, 13);
			num6 ^= Integers.RotateLeft(num5 + num8, 18);
			num12 ^= Integers.RotateLeft(num11 + num10, 7);
			num9 ^= Integers.RotateLeft(num12 + num11, 9);
			num10 ^= Integers.RotateLeft(num9 + num12, 13);
			num11 ^= Integers.RotateLeft(num10 + num9, 18);
			num13 ^= Integers.RotateLeft(num16 + num15, 7);
			num14 ^= Integers.RotateLeft(num13 + num16, 9);
			num15 ^= Integers.RotateLeft(num14 + num13, 13);
			num16 ^= Integers.RotateLeft(num15 + num14, 18);
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

	private void ResetLimitCounter()
	{
		cW0 = 0u;
		cW1 = 0u;
		cW2 = 0u;
	}

	private bool LimitExceeded()
	{
		if (++cW0 == 0 && ++cW1 == 0)
		{
			return (++cW2 & 0x20) != 0;
		}
		return false;
	}

	private bool LimitExceeded(uint len)
	{
		uint num = cW0;
		cW0 += len;
		if (cW0 < num && ++cW1 == 0)
		{
			return (++cW2 & 0x20) != 0;
		}
		return false;
	}
}
