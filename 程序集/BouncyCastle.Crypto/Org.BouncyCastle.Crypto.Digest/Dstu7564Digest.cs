using System;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Dstu7564Digest : IDigest, IMemoable
{
	private const int NB_512 = 8;

	private const int NB_1024 = 16;

	private const int NR_512 = 10;

	private const int NR_1024 = 14;

	private int hashSize;

	private int blockSize;

	private int columns;

	private int rounds;

	private ulong[] state;

	private ulong[] tempState1;

	private ulong[] tempState2;

	private ulong inputBlocks;

	private int bufOff;

	private byte[] buf;

	private static readonly byte[] S0 = new byte[256]
	{
		168, 67, 95, 6, 107, 117, 108, 89, 113, 223,
		135, 149, 23, 240, 216, 9, 109, 243, 29, 203,
		201, 77, 44, 175, 121, 224, 151, 253, 111, 75,
		69, 57, 62, 221, 163, 79, 180, 182, 154, 14,
		31, 191, 21, 225, 73, 210, 147, 198, 146, 114,
		158, 97, 209, 99, 250, 238, 244, 25, 213, 173,
		88, 164, 187, 161, 220, 242, 131, 55, 66, 228,
		122, 50, 156, 204, 171, 74, 143, 110, 4, 39,
		46, 231, 226, 90, 150, 22, 35, 43, 194, 101,
		102, 15, 188, 169, 71, 65, 52, 72, 252, 183,
		106, 136, 165, 83, 134, 249, 91, 219, 56, 123,
		195, 30, 34, 51, 36, 40, 54, 199, 178, 59,
		142, 119, 186, 245, 20, 159, 8, 85, 155, 76,
		254, 96, 92, 218, 24, 70, 205, 125, 33, 176,
		63, 27, 137, 255, 235, 132, 105, 58, 157, 215,
		211, 112, 103, 64, 181, 222, 93, 48, 145, 177,
		120, 17, 1, 229, 0, 104, 152, 160, 197, 2,
		166, 116, 45, 11, 162, 118, 179, 190, 206, 189,
		174, 233, 138, 49, 28, 236, 241, 153, 148, 170,
		246, 38, 47, 239, 232, 140, 53, 3, 212, 127,
		251, 5, 193, 94, 144, 32, 61, 130, 247, 234,
		10, 13, 126, 248, 80, 26, 196, 7, 87, 184,
		60, 98, 227, 200, 172, 82, 100, 16, 208, 217,
		19, 12, 18, 41, 81, 185, 207, 214, 115, 141,
		129, 84, 192, 237, 78, 68, 167, 42, 133, 37,
		230, 202, 124, 139, 86, 128
	};

	private static readonly byte[] S1 = new byte[256]
	{
		206, 187, 235, 146, 234, 203, 19, 193, 233, 58,
		214, 178, 210, 144, 23, 248, 66, 21, 86, 180,
		101, 28, 136, 67, 197, 92, 54, 186, 245, 87,
		103, 141, 49, 246, 100, 88, 158, 244, 34, 170,
		117, 15, 2, 177, 223, 109, 115, 77, 124, 38,
		46, 247, 8, 93, 68, 62, 159, 20, 200, 174,
		84, 16, 216, 188, 26, 107, 105, 243, 189, 51,
		171, 250, 209, 155, 104, 78, 22, 149, 145, 238,
		76, 99, 142, 91, 204, 60, 25, 161, 129, 73,
		123, 217, 111, 55, 96, 202, 231, 43, 72, 253,
		150, 69, 252, 65, 18, 13, 121, 229, 137, 140,
		227, 32, 48, 220, 183, 108, 74, 181, 63, 151,
		212, 98, 45, 6, 164, 165, 131, 95, 42, 218,
		201, 0, 126, 162, 85, 191, 17, 213, 156, 207,
		14, 10, 61, 81, 125, 147, 27, 254, 196, 71,
		9, 134, 11, 143, 157, 106, 7, 185, 176, 152,
		24, 50, 113, 75, 239, 59, 112, 160, 228, 64,
		255, 195, 169, 230, 120, 249, 139, 70, 128, 30,
		56, 225, 184, 168, 224, 12, 35, 118, 29, 37,
		36, 5, 241, 110, 148, 40, 154, 132, 232, 163,
		79, 119, 211, 133, 226, 82, 242, 130, 80, 122,
		47, 116, 83, 179, 97, 175, 57, 53, 222, 205,
		31, 153, 172, 173, 114, 44, 221, 208, 135, 190,
		94, 166, 236, 4, 198, 3, 52, 251, 219, 89,
		182, 194, 1, 240, 90, 237, 167, 102, 33, 127,
		138, 39, 199, 192, 41, 215
	};

	private static readonly byte[] S2 = new byte[256]
	{
		147, 217, 154, 181, 152, 34, 69, 252, 186, 106,
		223, 2, 159, 220, 81, 89, 74, 23, 43, 194,
		148, 244, 187, 163, 98, 228, 113, 212, 205, 112,
		22, 225, 73, 60, 192, 216, 92, 155, 173, 133,
		83, 161, 122, 200, 45, 224, 209, 114, 166, 44,
		196, 227, 118, 120, 183, 180, 9, 59, 14, 65,
		76, 222, 178, 144, 37, 165, 215, 3, 17, 0,
		195, 46, 146, 239, 78, 18, 157, 125, 203, 53,
		16, 213, 79, 158, 77, 169, 85, 198, 208, 123,
		24, 151, 211, 54, 230, 72, 86, 129, 143, 119,
		204, 156, 185, 226, 172, 184, 47, 21, 164, 124,
		218, 56, 30, 11, 5, 214, 20, 110, 108, 126,
		102, 253, 177, 229, 96, 175, 94, 51, 135, 201,
		240, 93, 109, 63, 136, 141, 199, 247, 29, 233,
		236, 237, 128, 41, 39, 207, 153, 168, 80, 15,
		55, 36, 40, 48, 149, 210, 62, 91, 64, 131,
		179, 105, 87, 31, 7, 28, 138, 188, 32, 235,
		206, 142, 171, 238, 49, 162, 115, 249, 202, 58,
		26, 251, 13, 193, 254, 250, 242, 111, 189, 150,
		221, 67, 82, 182, 8, 243, 174, 190, 25, 137,
		50, 38, 176, 234, 75, 100, 132, 130, 107, 245,
		121, 191, 1, 95, 117, 99, 27, 35, 61, 104,
		42, 101, 232, 145, 246, 255, 19, 88, 241, 71,
		10, 127, 197, 167, 231, 97, 90, 6, 70, 68,
		66, 4, 160, 219, 57, 134, 84, 170, 140, 52,
		33, 139, 248, 12, 116, 103
	};

	private static readonly byte[] S3 = new byte[256]
	{
		104, 141, 202, 77, 115, 75, 78, 42, 212, 82,
		38, 179, 84, 30, 25, 31, 34, 3, 70, 61,
		45, 74, 83, 131, 19, 138, 183, 213, 37, 121,
		245, 189, 88, 47, 13, 2, 237, 81, 158, 17,
		242, 62, 85, 94, 209, 22, 60, 102, 112, 93,
		243, 69, 64, 204, 232, 148, 86, 8, 206, 26,
		58, 210, 225, 223, 181, 56, 110, 14, 229, 244,
		249, 134, 233, 79, 214, 133, 35, 207, 50, 153,
		49, 20, 174, 238, 200, 72, 211, 48, 161, 146,
		65, 177, 24, 196, 44, 113, 114, 68, 21, 253,
		55, 190, 95, 170, 155, 136, 216, 171, 137, 156,
		250, 96, 234, 188, 98, 12, 36, 166, 168, 236,
		103, 32, 219, 124, 40, 221, 172, 91, 52, 126,
		16, 241, 123, 143, 99, 160, 5, 154, 67, 119,
		33, 191, 39, 9, 195, 159, 182, 215, 41, 194,
		235, 192, 164, 139, 140, 29, 251, 255, 193, 178,
		151, 46, 248, 101, 246, 117, 7, 4, 73, 51,
		228, 217, 185, 208, 66, 199, 108, 144, 0, 142,
		111, 80, 1, 197, 218, 71, 63, 205, 105, 162,
		226, 122, 167, 198, 147, 15, 10, 6, 230, 43,
		150, 163, 28, 175, 106, 18, 132, 57, 231, 176,
		130, 247, 254, 157, 135, 92, 129, 53, 222, 180,
		165, 252, 128, 239, 203, 187, 107, 118, 186, 90,
		125, 120, 11, 149, 227, 173, 116, 152, 59, 54,
		100, 109, 220, 240, 89, 169, 76, 23, 127, 145,
		184, 201, 87, 27, 224, 97
	};

	public virtual string AlgorithmName => "DSTU7564";

	public Dstu7564Digest(Dstu7564Digest digest)
	{
		CopyIn(digest);
	}

	private void CopyIn(Dstu7564Digest digest)
	{
		hashSize = digest.hashSize;
		blockSize = digest.blockSize;
		rounds = digest.rounds;
		if (columns > 0 && columns == digest.columns)
		{
			Array.Copy(digest.state, 0, state, 0, columns);
			Array.Copy(digest.buf, 0, buf, 0, blockSize);
		}
		else
		{
			columns = digest.columns;
			state = Arrays.Clone(digest.state);
			tempState1 = new ulong[columns];
			tempState2 = new ulong[columns];
			buf = Arrays.Clone(digest.buf);
		}
		inputBlocks = digest.inputBlocks;
		bufOff = digest.bufOff;
	}

	public Dstu7564Digest(int hashSizeBits)
	{
		if (hashSizeBits == 256 || hashSizeBits == 384 || hashSizeBits == 512)
		{
			hashSize = hashSizeBits / 8;
			if (hashSizeBits > 256)
			{
				columns = 16;
				rounds = 14;
			}
			else
			{
				columns = 8;
				rounds = 10;
			}
			blockSize = columns << 3;
			state = new ulong[columns];
			state[0] = (ulong)blockSize;
			tempState1 = new ulong[columns];
			tempState2 = new ulong[columns];
			buf = new byte[blockSize];
			return;
		}
		throw new ArgumentException("Hash size is not recommended. Use 256/384/512 instead");
	}

	public virtual int GetDigestSize()
	{
		return hashSize;
	}

	public virtual int GetByteLength()
	{
		return blockSize;
	}

	public virtual void Update(byte input)
	{
		buf[bufOff++] = input;
		if (bufOff == blockSize)
		{
			ProcessBlock(buf, 0);
			bufOff = 0;
			inputBlocks++;
		}
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (bufOff != 0 && length > 0)
		{
			Update(input[inOff++]);
			length--;
		}
		if (length > 0)
		{
			while (length >= blockSize)
			{
				ProcessBlock(input, inOff);
				inOff += blockSize;
				length -= blockSize;
				inputBlocks++;
			}
			while (length > 0)
			{
				Update(input[inOff++]);
				length--;
			}
		}
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		int num = bufOff;
		buf[bufOff++] = 128;
		int num2 = blockSize - 12;
		if (bufOff > num2)
		{
			while (bufOff < blockSize)
			{
				buf[bufOff++] = 0;
			}
			bufOff = 0;
			ProcessBlock(buf, 0);
		}
		while (bufOff < num2)
		{
			buf[bufOff++] = 0;
		}
		ulong num3 = (ulong)((long)(inputBlocks & 0xFFFFFFFFu) * (long)blockSize + (uint)num << 3);
		Pack.UInt32_To_LE((uint)num3, buf, bufOff);
		bufOff += 4;
		num3 >>= 32;
		num3 += (ulong)((long)(inputBlocks >> 32) * (long)blockSize << 3);
		Pack.UInt64_To_LE(num3, buf, bufOff);
		ProcessBlock(buf, 0);
		Array.Copy(state, 0, tempState1, 0, columns);
		P(tempState1);
		for (int i = 0; i < columns; i++)
		{
			ulong[] array;
			ulong[] array2 = (array = state);
			int num4 = i;
			nint num5 = num4;
			array2[num4] = array[num5] ^ tempState1[i];
		}
		int num6 = hashSize / 8;
		for (int j = columns - num6; j < columns; j++)
		{
			Pack.UInt64_To_LE(state[j], output, outOff);
			outOff += 8;
		}
		Reset();
		return hashSize;
	}

	public virtual void Reset()
	{
		Array.Clear(state, 0, state.Length);
		state[0] = (ulong)blockSize;
		inputBlocks = 0uL;
		bufOff = 0;
	}

	protected virtual void ProcessBlock(byte[] input, int inOff)
	{
		int num = inOff;
		for (int i = 0; i < columns; i++)
		{
			ulong num2 = Pack.LE_To_UInt64(input, num);
			num += 8;
			tempState1[i] = state[i] ^ num2;
			tempState2[i] = num2;
		}
		P(tempState1);
		Q(tempState2);
		for (int j = 0; j < columns; j++)
		{
			ulong[] array;
			ulong[] array2 = (array = state);
			int num3 = j;
			nint num4 = num3;
			array2[num3] = array[num4] ^ (tempState1[j] ^ tempState2[j]);
		}
	}

	private void P(ulong[] s)
	{
		for (int i = 0; i < rounds; i++)
		{
			ulong num = (ulong)i;
			for (int j = 0; j < columns; j++)
			{
				ulong[] array;
				ulong[] array2 = (array = s);
				int num2 = j;
				nint num3 = num2;
				array2[num2] = array[num3] ^ num;
				num += 16;
			}
			ShiftRows(s);
			SubBytes(s);
			MixColumns(s);
		}
	}

	private void Q(ulong[] s)
	{
		for (int i = 0; i < rounds; i++)
		{
			ulong num = (ulong)((long)((columns - 1 << 4) ^ i) << 56) | 0xF0F0F0F0F0F0F3uL;
			for (int j = 0; j < columns; j++)
			{
				ulong[] array;
				ulong[] array2 = (array = s);
				int num2 = j;
				nint num3 = num2;
				array2[num2] = array[num3] + num;
				num -= 1152921504606846976L;
			}
			ShiftRows(s);
			SubBytes(s);
			MixColumns(s);
		}
	}

	private static ulong MixColumn(ulong c)
	{
		ulong num = ((c & 0x7F7F7F7F7F7F7F7FL) << 1) ^ (((c & 0x8080808080808080uL) >> 7) * 29);
		ulong num2 = Rotate(8, c) ^ c;
		num2 ^= Rotate(16, num2);
		num2 ^= Rotate(48, c);
		ulong num3 = num2 ^ c ^ num;
		num3 = ((num3 & 0x3F3F3F3F3F3F3F3FL) << 2) ^ (((num3 & 0x8080808080808080uL) >> 6) * 29) ^ (((num3 & 0x4040404040404040L) >> 6) * 29);
		return num2 ^ Rotate(32, num3) ^ Rotate(40, num) ^ Rotate(48, num);
	}

	private void MixColumns(ulong[] s)
	{
		for (int i = 0; i < columns; i++)
		{
			s[i] = MixColumn(s[i]);
		}
	}

	private static ulong Rotate(int n, ulong x)
	{
		return (x >> n) | (x << -n);
	}

	private void ShiftRows(ulong[] s)
	{
		switch (columns)
		{
		case 8:
		{
			ulong num18 = s[0];
			ulong num19 = s[1];
			ulong num20 = s[2];
			ulong num21 = s[3];
			ulong num22 = s[4];
			ulong num23 = s[5];
			ulong num24 = s[6];
			ulong num25 = s[7];
			ulong num26 = (num18 ^ num22) & 0xFFFFFFFF00000000uL;
			num18 ^= num26;
			num22 ^= num26;
			num26 = (num19 ^ num23) & 0xFFFFFFFF000000uL;
			num19 ^= num26;
			num23 ^= num26;
			num26 = (num20 ^ num24) & 0xFFFFFFFF0000uL;
			num20 ^= num26;
			num24 ^= num26;
			num26 = (num21 ^ num25) & 0xFFFFFFFF00uL;
			num21 ^= num26;
			num25 ^= num26;
			num26 = (num18 ^ num20) & 0xFFFF0000FFFF0000uL;
			num18 ^= num26;
			num20 ^= num26;
			num26 = (num19 ^ num21) & 0xFFFF0000FFFF00uL;
			num19 ^= num26;
			num21 ^= num26;
			num26 = (num22 ^ num24) & 0xFFFF0000FFFF0000uL;
			num22 ^= num26;
			num24 ^= num26;
			num26 = (num23 ^ num25) & 0xFFFF0000FFFF00uL;
			num23 ^= num26;
			num25 ^= num26;
			num26 = (num18 ^ num19) & 0xFF00FF00FF00FF00uL;
			num18 ^= num26;
			num19 ^= num26;
			num26 = (num20 ^ num21) & 0xFF00FF00FF00FF00uL;
			num20 ^= num26;
			num21 ^= num26;
			num26 = (num22 ^ num23) & 0xFF00FF00FF00FF00uL;
			num22 ^= num26;
			num23 ^= num26;
			num26 = (num24 ^ num25) & 0xFF00FF00FF00FF00uL;
			num24 ^= num26;
			num25 ^= num26;
			s[0] = num18;
			s[1] = num19;
			s[2] = num20;
			s[3] = num21;
			s[4] = num22;
			s[5] = num23;
			s[6] = num24;
			s[7] = num25;
			break;
		}
		case 16:
		{
			ulong num = s[0];
			ulong num2 = s[1];
			ulong num3 = s[2];
			ulong num4 = s[3];
			ulong num5 = s[4];
			ulong num6 = s[5];
			ulong num7 = s[6];
			ulong num8 = s[7];
			ulong num9 = s[8];
			ulong num10 = s[9];
			ulong num11 = s[10];
			ulong num12 = s[11];
			ulong num13 = s[12];
			ulong num14 = s[13];
			ulong num15 = s[14];
			ulong num16 = s[15];
			ulong num17 = (num ^ num9) & 0xFF00000000000000uL;
			num ^= num17;
			num9 ^= num17;
			num17 = (num2 ^ num10) & 0xFF00000000000000uL;
			num2 ^= num17;
			num10 ^= num17;
			num17 = (num3 ^ num11) & 0xFFFF000000000000uL;
			num3 ^= num17;
			num11 ^= num17;
			num17 = (num4 ^ num12) & 0xFFFFFF0000000000uL;
			num4 ^= num17;
			num12 ^= num17;
			num17 = (num5 ^ num13) & 0xFFFFFFFF00000000uL;
			num5 ^= num17;
			num13 ^= num17;
			num17 = (num6 ^ num14) & 0xFFFFFFFF000000uL;
			num6 ^= num17;
			num14 ^= num17;
			num17 = (num7 ^ num15) & 0xFFFFFFFFFF0000uL;
			num7 ^= num17;
			num15 ^= num17;
			num17 = (num8 ^ num16) & 0xFFFFFFFFFFFF00uL;
			num8 ^= num17;
			num16 ^= num17;
			num17 = (num ^ num5) & 0xFFFFFF00000000uL;
			num ^= num17;
			num5 ^= num17;
			num17 = (num2 ^ num6) & 0xFFFFFFFFFF000000uL;
			num2 ^= num17;
			num6 ^= num17;
			num17 = (num3 ^ num7) & 0xFF00FFFFFFFF0000uL;
			num3 ^= num17;
			num7 ^= num17;
			num17 = (num4 ^ num8) & 0xFF0000FFFFFFFF00uL;
			num4 ^= num17;
			num8 ^= num17;
			num17 = (num9 ^ num13) & 0xFFFFFF00000000uL;
			num9 ^= num17;
			num13 ^= num17;
			num17 = (num10 ^ num14) & 0xFFFFFFFFFF000000uL;
			num10 ^= num17;
			num14 ^= num17;
			num17 = (num11 ^ num15) & 0xFF00FFFFFFFF0000uL;
			num11 ^= num17;
			num15 ^= num17;
			num17 = (num12 ^ num16) & 0xFF0000FFFFFFFF00uL;
			num12 ^= num17;
			num16 ^= num17;
			num17 = (num ^ num3) & 0xFFFF0000FFFF0000uL;
			num ^= num17;
			num3 ^= num17;
			num17 = (num2 ^ num4) & 0xFFFF0000FFFF00uL;
			num2 ^= num17;
			num4 ^= num17;
			num17 = (num5 ^ num7) & 0xFFFF0000FFFF0000uL;
			num5 ^= num17;
			num7 ^= num17;
			num17 = (num6 ^ num8) & 0xFFFF0000FFFF00uL;
			num6 ^= num17;
			num8 ^= num17;
			num17 = (num9 ^ num11) & 0xFFFF0000FFFF0000uL;
			num9 ^= num17;
			num11 ^= num17;
			num17 = (num10 ^ num12) & 0xFFFF0000FFFF00uL;
			num10 ^= num17;
			num12 ^= num17;
			num17 = (num13 ^ num15) & 0xFFFF0000FFFF0000uL;
			num13 ^= num17;
			num15 ^= num17;
			num17 = (num14 ^ num16) & 0xFFFF0000FFFF00uL;
			num14 ^= num17;
			num16 ^= num17;
			num17 = (num ^ num2) & 0xFF00FF00FF00FF00uL;
			num ^= num17;
			num2 ^= num17;
			num17 = (num3 ^ num4) & 0xFF00FF00FF00FF00uL;
			num3 ^= num17;
			num4 ^= num17;
			num17 = (num5 ^ num6) & 0xFF00FF00FF00FF00uL;
			num5 ^= num17;
			num6 ^= num17;
			num17 = (num7 ^ num8) & 0xFF00FF00FF00FF00uL;
			num7 ^= num17;
			num8 ^= num17;
			num17 = (num9 ^ num10) & 0xFF00FF00FF00FF00uL;
			num9 ^= num17;
			num10 ^= num17;
			num17 = (num11 ^ num12) & 0xFF00FF00FF00FF00uL;
			num11 ^= num17;
			num12 ^= num17;
			num17 = (num13 ^ num14) & 0xFF00FF00FF00FF00uL;
			num13 ^= num17;
			num14 ^= num17;
			num17 = (num15 ^ num16) & 0xFF00FF00FF00FF00uL;
			num15 ^= num17;
			num16 ^= num17;
			s[0] = num;
			s[1] = num2;
			s[2] = num3;
			s[3] = num4;
			s[4] = num5;
			s[5] = num6;
			s[6] = num7;
			s[7] = num8;
			s[8] = num9;
			s[9] = num10;
			s[10] = num11;
			s[11] = num12;
			s[12] = num13;
			s[13] = num14;
			s[14] = num15;
			s[15] = num16;
			break;
		}
		default:
			throw new InvalidOperationException("unsupported state size: only 512/1024 are allowed");
		}
	}

	private void SubBytes(ulong[] s)
	{
		for (int i = 0; i < columns; i++)
		{
			ulong num = s[i];
			uint num2 = (uint)num;
			uint num3 = (uint)(num >> 32);
			byte b = S0[num2 & 0xFF];
			byte b2 = S1[(num2 >> 8) & 0xFF];
			byte b3 = S2[(num2 >> 16) & 0xFF];
			byte b4 = S3[num2 >> 24];
			num2 = (uint)(b | (b2 << 8) | (b3 << 16) | (b4 << 24));
			byte b5 = S0[num3 & 0xFF];
			byte b6 = S1[(num3 >> 8) & 0xFF];
			byte b7 = S2[(num3 >> 16) & 0xFF];
			byte b8 = S3[num3 >> 24];
			num3 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));
			s[i] = num2 | ((ulong)num3 << 32);
		}
	}

	public virtual IMemoable Copy()
	{
		return new Dstu7564Digest(this);
	}

	public virtual void Reset(IMemoable other)
	{
		Dstu7564Digest digest = (Dstu7564Digest)other;
		CopyIn(digest);
	}
}
