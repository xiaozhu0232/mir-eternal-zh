using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public sealed class Cast6Engine : Cast5Engine
{
	private const int ROUNDS = 12;

	private const int BLOCK_SIZE = 16;

	private int[] _Kr = new int[48];

	private uint[] _Km = new uint[48];

	private int[] _Tr = new int[192];

	private uint[] _Tm = new uint[192];

	private uint[] _workingKey = new uint[8];

	public override string AlgorithmName => "CAST6";

	public override void Reset()
	{
	}

	public override int GetBlockSize()
	{
		return 16;
	}

	internal override void SetKey(byte[] key)
	{
		uint num = 1518500249u;
		uint num2 = 1859775393u;
		int num3 = 19;
		int num4 = 17;
		for (int i = 0; i < 24; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				_Tm[i * 8 + j] = num;
				num += num2;
				_Tr[i * 8 + j] = num3;
				num3 = (num3 + num4) & 0x1F;
			}
		}
		byte[] array = new byte[64];
		key.CopyTo(array, 0);
		for (int k = 0; k < 8; k++)
		{
			_workingKey[k] = Pack.BE_To_UInt32(array, k * 4);
		}
		for (int l = 0; l < 12; l++)
		{
			int num5 = l * 2 * 8;
			uint[] workingKey;
			(workingKey = _workingKey)[6] = workingKey[6] ^ Cast5Engine.F1(_workingKey[7], _Tm[num5], _Tr[num5]);
			(workingKey = _workingKey)[5] = workingKey[5] ^ Cast5Engine.F2(_workingKey[6], _Tm[num5 + 1], _Tr[num5 + 1]);
			(workingKey = _workingKey)[4] = workingKey[4] ^ Cast5Engine.F3(_workingKey[5], _Tm[num5 + 2], _Tr[num5 + 2]);
			(workingKey = _workingKey)[3] = workingKey[3] ^ Cast5Engine.F1(_workingKey[4], _Tm[num5 + 3], _Tr[num5 + 3]);
			(workingKey = _workingKey)[2] = workingKey[2] ^ Cast5Engine.F2(_workingKey[3], _Tm[num5 + 4], _Tr[num5 + 4]);
			(workingKey = _workingKey)[1] = workingKey[1] ^ Cast5Engine.F3(_workingKey[2], _Tm[num5 + 5], _Tr[num5 + 5]);
			(workingKey = _workingKey)[0] = workingKey[0] ^ Cast5Engine.F1(_workingKey[1], _Tm[num5 + 6], _Tr[num5 + 6]);
			(workingKey = _workingKey)[7] = workingKey[7] ^ Cast5Engine.F2(_workingKey[0], _Tm[num5 + 7], _Tr[num5 + 7]);
			num5 = (l * 2 + 1) * 8;
			(workingKey = _workingKey)[6] = workingKey[6] ^ Cast5Engine.F1(_workingKey[7], _Tm[num5], _Tr[num5]);
			(workingKey = _workingKey)[5] = workingKey[5] ^ Cast5Engine.F2(_workingKey[6], _Tm[num5 + 1], _Tr[num5 + 1]);
			(workingKey = _workingKey)[4] = workingKey[4] ^ Cast5Engine.F3(_workingKey[5], _Tm[num5 + 2], _Tr[num5 + 2]);
			(workingKey = _workingKey)[3] = workingKey[3] ^ Cast5Engine.F1(_workingKey[4], _Tm[num5 + 3], _Tr[num5 + 3]);
			(workingKey = _workingKey)[2] = workingKey[2] ^ Cast5Engine.F2(_workingKey[3], _Tm[num5 + 4], _Tr[num5 + 4]);
			(workingKey = _workingKey)[1] = workingKey[1] ^ Cast5Engine.F3(_workingKey[2], _Tm[num5 + 5], _Tr[num5 + 5]);
			(workingKey = _workingKey)[0] = workingKey[0] ^ Cast5Engine.F1(_workingKey[1], _Tm[num5 + 6], _Tr[num5 + 6]);
			(workingKey = _workingKey)[7] = workingKey[7] ^ Cast5Engine.F2(_workingKey[0], _Tm[num5 + 7], _Tr[num5 + 7]);
			_Kr[l * 4] = (int)(_workingKey[0] & 0x1F);
			_Kr[l * 4 + 1] = (int)(_workingKey[2] & 0x1F);
			_Kr[l * 4 + 2] = (int)(_workingKey[4] & 0x1F);
			_Kr[l * 4 + 3] = (int)(_workingKey[6] & 0x1F);
			_Km[l * 4] = _workingKey[7];
			_Km[l * 4 + 1] = _workingKey[5];
			_Km[l * 4 + 2] = _workingKey[3];
			_Km[l * 4 + 3] = _workingKey[1];
		}
	}

	internal override int EncryptBlock(byte[] src, int srcIndex, byte[] dst, int dstIndex)
	{
		uint a = Pack.BE_To_UInt32(src, srcIndex);
		uint b = Pack.BE_To_UInt32(src, srcIndex + 4);
		uint c = Pack.BE_To_UInt32(src, srcIndex + 8);
		uint d = Pack.BE_To_UInt32(src, srcIndex + 12);
		uint[] array = new uint[4];
		CAST_Encipher(a, b, c, d, array);
		Pack.UInt32_To_BE(array[0], dst, dstIndex);
		Pack.UInt32_To_BE(array[1], dst, dstIndex + 4);
		Pack.UInt32_To_BE(array[2], dst, dstIndex + 8);
		Pack.UInt32_To_BE(array[3], dst, dstIndex + 12);
		return 16;
	}

	internal override int DecryptBlock(byte[] src, int srcIndex, byte[] dst, int dstIndex)
	{
		uint a = Pack.BE_To_UInt32(src, srcIndex);
		uint b = Pack.BE_To_UInt32(src, srcIndex + 4);
		uint c = Pack.BE_To_UInt32(src, srcIndex + 8);
		uint d = Pack.BE_To_UInt32(src, srcIndex + 12);
		uint[] array = new uint[4];
		CAST_Decipher(a, b, c, d, array);
		Pack.UInt32_To_BE(array[0], dst, dstIndex);
		Pack.UInt32_To_BE(array[1], dst, dstIndex + 4);
		Pack.UInt32_To_BE(array[2], dst, dstIndex + 8);
		Pack.UInt32_To_BE(array[3], dst, dstIndex + 12);
		return 16;
	}

	private void CAST_Encipher(uint A, uint B, uint C, uint D, uint[] result)
	{
		for (int i = 0; i < 6; i++)
		{
			int num = i * 4;
			C ^= Cast5Engine.F1(D, _Km[num], _Kr[num]);
			B ^= Cast5Engine.F2(C, _Km[num + 1], _Kr[num + 1]);
			A ^= Cast5Engine.F3(B, _Km[num + 2], _Kr[num + 2]);
			D ^= Cast5Engine.F1(A, _Km[num + 3], _Kr[num + 3]);
		}
		for (int j = 6; j < 12; j++)
		{
			int num2 = j * 4;
			D ^= Cast5Engine.F1(A, _Km[num2 + 3], _Kr[num2 + 3]);
			A ^= Cast5Engine.F3(B, _Km[num2 + 2], _Kr[num2 + 2]);
			B ^= Cast5Engine.F2(C, _Km[num2 + 1], _Kr[num2 + 1]);
			C ^= Cast5Engine.F1(D, _Km[num2], _Kr[num2]);
		}
		result[0] = A;
		result[1] = B;
		result[2] = C;
		result[3] = D;
	}

	private void CAST_Decipher(uint A, uint B, uint C, uint D, uint[] result)
	{
		for (int i = 0; i < 6; i++)
		{
			int num = (11 - i) * 4;
			C ^= Cast5Engine.F1(D, _Km[num], _Kr[num]);
			B ^= Cast5Engine.F2(C, _Km[num + 1], _Kr[num + 1]);
			A ^= Cast5Engine.F3(B, _Km[num + 2], _Kr[num + 2]);
			D ^= Cast5Engine.F1(A, _Km[num + 3], _Kr[num + 3]);
		}
		for (int j = 6; j < 12; j++)
		{
			int num2 = (11 - j) * 4;
			D ^= Cast5Engine.F1(A, _Km[num2 + 3], _Kr[num2 + 3]);
			A ^= Cast5Engine.F3(B, _Km[num2 + 2], _Kr[num2 + 2]);
			B ^= Cast5Engine.F2(C, _Km[num2 + 1], _Kr[num2 + 1]);
			C ^= Cast5Engine.F1(D, _Km[num2], _Kr[num2]);
		}
		result[0] = A;
		result[1] = B;
		result[2] = C;
		result[3] = D;
	}
}
