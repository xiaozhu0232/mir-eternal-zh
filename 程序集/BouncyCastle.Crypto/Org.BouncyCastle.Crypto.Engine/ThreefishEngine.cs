using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class ThreefishEngine : IBlockCipher
{
	private abstract class ThreefishCipher
	{
		protected readonly ulong[] t;

		protected readonly ulong[] kw;

		protected ThreefishCipher(ulong[] kw, ulong[] t)
		{
			this.kw = kw;
			this.t = t;
		}

		internal abstract void EncryptBlock(ulong[] block, ulong[] outWords);

		internal abstract void DecryptBlock(ulong[] block, ulong[] outWords);
	}

	private sealed class Threefish256Cipher : ThreefishCipher
	{
		private const int ROTATION_0_0 = 14;

		private const int ROTATION_0_1 = 16;

		private const int ROTATION_1_0 = 52;

		private const int ROTATION_1_1 = 57;

		private const int ROTATION_2_0 = 23;

		private const int ROTATION_2_1 = 40;

		private const int ROTATION_3_0 = 5;

		private const int ROTATION_3_1 = 37;

		private const int ROTATION_4_0 = 25;

		private const int ROTATION_4_1 = 33;

		private const int ROTATION_5_0 = 46;

		private const int ROTATION_5_1 = 12;

		private const int ROTATION_6_0 = 58;

		private const int ROTATION_6_1 = 22;

		private const int ROTATION_7_0 = 32;

		private const int ROTATION_7_1 = 32;

		public Threefish256Cipher(ulong[] kw, ulong[] t)
			: base(kw, t)
		{
		}

		internal override void EncryptBlock(ulong[] block, ulong[] outWords)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD5;
			int[] mOD2 = MOD3;
			if (array.Length != 9)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			num += array[0];
			num2 += array[1] + array2[0];
			num3 += array[2] + array2[1];
			num4 += array[3];
			for (int i = 1; i < 18; i += 2)
			{
				int num5 = mOD[i];
				int num6 = mOD2[i];
				num2 = RotlXor(num2, 14, num += num2);
				num4 = RotlXor(num4, 16, num3 += num4);
				num4 = RotlXor(num4, 52, num += num4);
				num2 = RotlXor(num2, 57, num3 += num2);
				num2 = RotlXor(num2, 23, num += num2);
				num4 = RotlXor(num4, 40, num3 += num4);
				num4 = RotlXor(num4, 5, num += num4);
				num2 = RotlXor(num2, 37, num3 += num2);
				num += array[num5];
				num2 += array[num5 + 1] + array2[num6];
				num3 += array[num5 + 2] + array2[num6 + 1];
				num4 += array[num5 + 3] + (uint)i;
				num2 = RotlXor(num2, 25, num += num2);
				num4 = RotlXor(num4, 33, num3 += num4);
				num4 = RotlXor(num4, 46, num += num4);
				num2 = RotlXor(num2, 12, num3 += num2);
				num2 = RotlXor(num2, 58, num += num2);
				num4 = RotlXor(num4, 22, num3 += num4);
				num4 = RotlXor(num4, 32, num += num4);
				num2 = RotlXor(num2, 32, num3 += num2);
				num += array[num5 + 1];
				num2 += array[num5 + 2] + array2[num6 + 1];
				num3 += array[num5 + 3] + array2[num6 + 2];
				num4 += array[num5 + 4] + (uint)i + 1;
			}
			outWords[0] = num;
			outWords[1] = num2;
			outWords[2] = num3;
			outWords[3] = num4;
		}

		internal override void DecryptBlock(ulong[] block, ulong[] state)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD5;
			int[] mOD2 = MOD3;
			if (array.Length != 9)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			for (int num5 = 17; num5 >= 1; num5 -= 2)
			{
				int num6 = mOD[num5];
				int num7 = mOD2[num5];
				num -= array[num6 + 1];
				num2 -= array[num6 + 2] + array2[num7 + 1];
				num3 -= array[num6 + 3] + array2[num7 + 2];
				num4 -= array[num6 + 4] + (uint)num5 + 1;
				num4 = XorRotr(num4, 32, num);
				num -= num4;
				num2 = XorRotr(num2, 32, num3);
				num3 -= num2;
				num2 = XorRotr(num2, 58, num);
				num -= num2;
				num4 = XorRotr(num4, 22, num3);
				num3 -= num4;
				num4 = XorRotr(num4, 46, num);
				num -= num4;
				num2 = XorRotr(num2, 12, num3);
				num3 -= num2;
				num2 = XorRotr(num2, 25, num);
				num -= num2;
				num4 = XorRotr(num4, 33, num3);
				num3 -= num4;
				num -= array[num6];
				num2 -= array[num6 + 1] + array2[num7];
				num3 -= array[num6 + 2] + array2[num7 + 1];
				num4 -= array[num6 + 3] + (uint)num5;
				num4 = XorRotr(num4, 5, num);
				num -= num4;
				num2 = XorRotr(num2, 37, num3);
				num3 -= num2;
				num2 = XorRotr(num2, 23, num);
				num -= num2;
				num4 = XorRotr(num4, 40, num3);
				num3 -= num4;
				num4 = XorRotr(num4, 52, num);
				num -= num4;
				num2 = XorRotr(num2, 57, num3);
				num3 -= num2;
				num2 = XorRotr(num2, 14, num);
				num -= num2;
				num4 = XorRotr(num4, 16, num3);
				num3 -= num4;
			}
			num -= array[0];
			num2 -= array[1] + array2[0];
			num3 -= array[2] + array2[1];
			num4 -= array[3];
			state[0] = num;
			state[1] = num2;
			state[2] = num3;
			state[3] = num4;
		}
	}

	private sealed class Threefish512Cipher : ThreefishCipher
	{
		private const int ROTATION_0_0 = 46;

		private const int ROTATION_0_1 = 36;

		private const int ROTATION_0_2 = 19;

		private const int ROTATION_0_3 = 37;

		private const int ROTATION_1_0 = 33;

		private const int ROTATION_1_1 = 27;

		private const int ROTATION_1_2 = 14;

		private const int ROTATION_1_3 = 42;

		private const int ROTATION_2_0 = 17;

		private const int ROTATION_2_1 = 49;

		private const int ROTATION_2_2 = 36;

		private const int ROTATION_2_3 = 39;

		private const int ROTATION_3_0 = 44;

		private const int ROTATION_3_1 = 9;

		private const int ROTATION_3_2 = 54;

		private const int ROTATION_3_3 = 56;

		private const int ROTATION_4_0 = 39;

		private const int ROTATION_4_1 = 30;

		private const int ROTATION_4_2 = 34;

		private const int ROTATION_4_3 = 24;

		private const int ROTATION_5_0 = 13;

		private const int ROTATION_5_1 = 50;

		private const int ROTATION_5_2 = 10;

		private const int ROTATION_5_3 = 17;

		private const int ROTATION_6_0 = 25;

		private const int ROTATION_6_1 = 29;

		private const int ROTATION_6_2 = 39;

		private const int ROTATION_6_3 = 43;

		private const int ROTATION_7_0 = 8;

		private const int ROTATION_7_1 = 35;

		private const int ROTATION_7_2 = 56;

		private const int ROTATION_7_3 = 22;

		internal Threefish512Cipher(ulong[] kw, ulong[] t)
			: base(kw, t)
		{
		}

		internal override void EncryptBlock(ulong[] block, ulong[] outWords)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD9;
			int[] mOD2 = MOD3;
			if (array.Length != 17)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			ulong num5 = block[4];
			ulong num6 = block[5];
			ulong num7 = block[6];
			ulong num8 = block[7];
			num += array[0];
			num2 += array[1];
			num3 += array[2];
			num4 += array[3];
			num5 += array[4];
			num6 += array[5] + array2[0];
			num7 += array[6] + array2[1];
			num8 += array[7];
			for (int i = 1; i < 18; i += 2)
			{
				int num9 = mOD[i];
				int num10 = mOD2[i];
				num2 = RotlXor(num2, 46, num += num2);
				num4 = RotlXor(num4, 36, num3 += num4);
				num6 = RotlXor(num6, 19, num5 += num6);
				num8 = RotlXor(num8, 37, num7 += num8);
				num2 = RotlXor(num2, 33, num3 += num2);
				num8 = RotlXor(num8, 27, num5 += num8);
				num6 = RotlXor(num6, 14, num7 += num6);
				num4 = RotlXor(num4, 42, num += num4);
				num2 = RotlXor(num2, 17, num5 += num2);
				num4 = RotlXor(num4, 49, num7 += num4);
				num6 = RotlXor(num6, 36, num += num6);
				num8 = RotlXor(num8, 39, num3 += num8);
				num2 = RotlXor(num2, 44, num7 += num2);
				num8 = RotlXor(num8, 9, num += num8);
				num6 = RotlXor(num6, 54, num3 += num6);
				num4 = RotlXor(num4, 56, num5 += num4);
				num += array[num9];
				num2 += array[num9 + 1];
				num3 += array[num9 + 2];
				num4 += array[num9 + 3];
				num5 += array[num9 + 4];
				num6 += array[num9 + 5] + array2[num10];
				num7 += array[num9 + 6] + array2[num10 + 1];
				num8 += array[num9 + 7] + (uint)i;
				num2 = RotlXor(num2, 39, num += num2);
				num4 = RotlXor(num4, 30, num3 += num4);
				num6 = RotlXor(num6, 34, num5 += num6);
				num8 = RotlXor(num8, 24, num7 += num8);
				num2 = RotlXor(num2, 13, num3 += num2);
				num8 = RotlXor(num8, 50, num5 += num8);
				num6 = RotlXor(num6, 10, num7 += num6);
				num4 = RotlXor(num4, 17, num += num4);
				num2 = RotlXor(num2, 25, num5 += num2);
				num4 = RotlXor(num4, 29, num7 += num4);
				num6 = RotlXor(num6, 39, num += num6);
				num8 = RotlXor(num8, 43, num3 += num8);
				num2 = RotlXor(num2, 8, num7 += num2);
				num8 = RotlXor(num8, 35, num += num8);
				num6 = RotlXor(num6, 56, num3 += num6);
				num4 = RotlXor(num4, 22, num5 += num4);
				num += array[num9 + 1];
				num2 += array[num9 + 2];
				num3 += array[num9 + 3];
				num4 += array[num9 + 4];
				num5 += array[num9 + 5];
				num6 += array[num9 + 6] + array2[num10 + 1];
				num7 += array[num9 + 7] + array2[num10 + 2];
				num8 += array[num9 + 8] + (uint)i + 1;
			}
			outWords[0] = num;
			outWords[1] = num2;
			outWords[2] = num3;
			outWords[3] = num4;
			outWords[4] = num5;
			outWords[5] = num6;
			outWords[6] = num7;
			outWords[7] = num8;
		}

		internal override void DecryptBlock(ulong[] block, ulong[] state)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD9;
			int[] mOD2 = MOD3;
			if (array.Length != 17)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			ulong num5 = block[4];
			ulong num6 = block[5];
			ulong num7 = block[6];
			ulong num8 = block[7];
			for (int num9 = 17; num9 >= 1; num9 -= 2)
			{
				int num10 = mOD[num9];
				int num11 = mOD2[num9];
				num -= array[num10 + 1];
				num2 -= array[num10 + 2];
				num3 -= array[num10 + 3];
				num4 -= array[num10 + 4];
				num5 -= array[num10 + 5];
				num6 -= array[num10 + 6] + array2[num11 + 1];
				num7 -= array[num10 + 7] + array2[num11 + 2];
				num8 -= array[num10 + 8] + (uint)num9 + 1;
				num2 = XorRotr(num2, 8, num7);
				num7 -= num2;
				num8 = XorRotr(num8, 35, num);
				num -= num8;
				num6 = XorRotr(num6, 56, num3);
				num3 -= num6;
				num4 = XorRotr(num4, 22, num5);
				num5 -= num4;
				num2 = XorRotr(num2, 25, num5);
				num5 -= num2;
				num4 = XorRotr(num4, 29, num7);
				num7 -= num4;
				num6 = XorRotr(num6, 39, num);
				num -= num6;
				num8 = XorRotr(num8, 43, num3);
				num3 -= num8;
				num2 = XorRotr(num2, 13, num3);
				num3 -= num2;
				num8 = XorRotr(num8, 50, num5);
				num5 -= num8;
				num6 = XorRotr(num6, 10, num7);
				num7 -= num6;
				num4 = XorRotr(num4, 17, num);
				num -= num4;
				num2 = XorRotr(num2, 39, num);
				num -= num2;
				num4 = XorRotr(num4, 30, num3);
				num3 -= num4;
				num6 = XorRotr(num6, 34, num5);
				num5 -= num6;
				num8 = XorRotr(num8, 24, num7);
				num7 -= num8;
				num -= array[num10];
				num2 -= array[num10 + 1];
				num3 -= array[num10 + 2];
				num4 -= array[num10 + 3];
				num5 -= array[num10 + 4];
				num6 -= array[num10 + 5] + array2[num11];
				num7 -= array[num10 + 6] + array2[num11 + 1];
				num8 -= array[num10 + 7] + (uint)num9;
				num2 = XorRotr(num2, 44, num7);
				num7 -= num2;
				num8 = XorRotr(num8, 9, num);
				num -= num8;
				num6 = XorRotr(num6, 54, num3);
				num3 -= num6;
				num4 = XorRotr(num4, 56, num5);
				num5 -= num4;
				num2 = XorRotr(num2, 17, num5);
				num5 -= num2;
				num4 = XorRotr(num4, 49, num7);
				num7 -= num4;
				num6 = XorRotr(num6, 36, num);
				num -= num6;
				num8 = XorRotr(num8, 39, num3);
				num3 -= num8;
				num2 = XorRotr(num2, 33, num3);
				num3 -= num2;
				num8 = XorRotr(num8, 27, num5);
				num5 -= num8;
				num6 = XorRotr(num6, 14, num7);
				num7 -= num6;
				num4 = XorRotr(num4, 42, num);
				num -= num4;
				num2 = XorRotr(num2, 46, num);
				num -= num2;
				num4 = XorRotr(num4, 36, num3);
				num3 -= num4;
				num6 = XorRotr(num6, 19, num5);
				num5 -= num6;
				num8 = XorRotr(num8, 37, num7);
				num7 -= num8;
			}
			num -= array[0];
			num2 -= array[1];
			num3 -= array[2];
			num4 -= array[3];
			num5 -= array[4];
			num6 -= array[5] + array2[0];
			num7 -= array[6] + array2[1];
			num8 -= array[7];
			state[0] = num;
			state[1] = num2;
			state[2] = num3;
			state[3] = num4;
			state[4] = num5;
			state[5] = num6;
			state[6] = num7;
			state[7] = num8;
		}
	}

	private sealed class Threefish1024Cipher : ThreefishCipher
	{
		private const int ROTATION_0_0 = 24;

		private const int ROTATION_0_1 = 13;

		private const int ROTATION_0_2 = 8;

		private const int ROTATION_0_3 = 47;

		private const int ROTATION_0_4 = 8;

		private const int ROTATION_0_5 = 17;

		private const int ROTATION_0_6 = 22;

		private const int ROTATION_0_7 = 37;

		private const int ROTATION_1_0 = 38;

		private const int ROTATION_1_1 = 19;

		private const int ROTATION_1_2 = 10;

		private const int ROTATION_1_3 = 55;

		private const int ROTATION_1_4 = 49;

		private const int ROTATION_1_5 = 18;

		private const int ROTATION_1_6 = 23;

		private const int ROTATION_1_7 = 52;

		private const int ROTATION_2_0 = 33;

		private const int ROTATION_2_1 = 4;

		private const int ROTATION_2_2 = 51;

		private const int ROTATION_2_3 = 13;

		private const int ROTATION_2_4 = 34;

		private const int ROTATION_2_5 = 41;

		private const int ROTATION_2_6 = 59;

		private const int ROTATION_2_7 = 17;

		private const int ROTATION_3_0 = 5;

		private const int ROTATION_3_1 = 20;

		private const int ROTATION_3_2 = 48;

		private const int ROTATION_3_3 = 41;

		private const int ROTATION_3_4 = 47;

		private const int ROTATION_3_5 = 28;

		private const int ROTATION_3_6 = 16;

		private const int ROTATION_3_7 = 25;

		private const int ROTATION_4_0 = 41;

		private const int ROTATION_4_1 = 9;

		private const int ROTATION_4_2 = 37;

		private const int ROTATION_4_3 = 31;

		private const int ROTATION_4_4 = 12;

		private const int ROTATION_4_5 = 47;

		private const int ROTATION_4_6 = 44;

		private const int ROTATION_4_7 = 30;

		private const int ROTATION_5_0 = 16;

		private const int ROTATION_5_1 = 34;

		private const int ROTATION_5_2 = 56;

		private const int ROTATION_5_3 = 51;

		private const int ROTATION_5_4 = 4;

		private const int ROTATION_5_5 = 53;

		private const int ROTATION_5_6 = 42;

		private const int ROTATION_5_7 = 41;

		private const int ROTATION_6_0 = 31;

		private const int ROTATION_6_1 = 44;

		private const int ROTATION_6_2 = 47;

		private const int ROTATION_6_3 = 46;

		private const int ROTATION_6_4 = 19;

		private const int ROTATION_6_5 = 42;

		private const int ROTATION_6_6 = 44;

		private const int ROTATION_6_7 = 25;

		private const int ROTATION_7_0 = 9;

		private const int ROTATION_7_1 = 48;

		private const int ROTATION_7_2 = 35;

		private const int ROTATION_7_3 = 52;

		private const int ROTATION_7_4 = 23;

		private const int ROTATION_7_5 = 31;

		private const int ROTATION_7_6 = 37;

		private const int ROTATION_7_7 = 20;

		public Threefish1024Cipher(ulong[] kw, ulong[] t)
			: base(kw, t)
		{
		}

		internal override void EncryptBlock(ulong[] block, ulong[] outWords)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD17;
			int[] mOD2 = MOD3;
			if (array.Length != 33)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			ulong num5 = block[4];
			ulong num6 = block[5];
			ulong num7 = block[6];
			ulong num8 = block[7];
			ulong num9 = block[8];
			ulong num10 = block[9];
			ulong num11 = block[10];
			ulong num12 = block[11];
			ulong num13 = block[12];
			ulong num14 = block[13];
			ulong num15 = block[14];
			ulong num16 = block[15];
			num += array[0];
			num2 += array[1];
			num3 += array[2];
			num4 += array[3];
			num5 += array[4];
			num6 += array[5];
			num7 += array[6];
			num8 += array[7];
			num9 += array[8];
			num10 += array[9];
			num11 += array[10];
			num12 += array[11];
			num13 += array[12];
			num14 += array[13] + array2[0];
			num15 += array[14] + array2[1];
			num16 += array[15];
			for (int i = 1; i < 20; i += 2)
			{
				int num17 = mOD[i];
				int num18 = mOD2[i];
				num2 = RotlXor(num2, 24, num += num2);
				num4 = RotlXor(num4, 13, num3 += num4);
				num6 = RotlXor(num6, 8, num5 += num6);
				num8 = RotlXor(num8, 47, num7 += num8);
				num10 = RotlXor(num10, 8, num9 += num10);
				num12 = RotlXor(num12, 17, num11 += num12);
				num14 = RotlXor(num14, 22, num13 += num14);
				num16 = RotlXor(num16, 37, num15 += num16);
				num10 = RotlXor(num10, 38, num += num10);
				num14 = RotlXor(num14, 19, num3 += num14);
				num12 = RotlXor(num12, 10, num7 += num12);
				num16 = RotlXor(num16, 55, num5 += num16);
				num8 = RotlXor(num8, 49, num11 += num8);
				num4 = RotlXor(num4, 18, num13 += num4);
				num6 = RotlXor(num6, 23, num15 += num6);
				num2 = RotlXor(num2, 52, num9 += num2);
				num8 = RotlXor(num8, 33, num += num8);
				num6 = RotlXor(num6, 4, num3 += num6);
				num4 = RotlXor(num4, 51, num5 += num4);
				num2 = RotlXor(num2, 13, num7 += num2);
				num16 = RotlXor(num16, 34, num13 += num16);
				num14 = RotlXor(num14, 41, num15 += num14);
				num12 = RotlXor(num12, 59, num9 += num12);
				num10 = RotlXor(num10, 17, num11 += num10);
				num16 = RotlXor(num16, 5, num += num16);
				num12 = RotlXor(num12, 20, num3 += num12);
				num14 = RotlXor(num14, 48, num7 += num14);
				num10 = RotlXor(num10, 41, num5 += num10);
				num2 = RotlXor(num2, 47, num15 += num2);
				num6 = RotlXor(num6, 28, num9 += num6);
				num4 = RotlXor(num4, 16, num11 += num4);
				num8 = RotlXor(num8, 25, num13 += num8);
				num += array[num17];
				num2 += array[num17 + 1];
				num3 += array[num17 + 2];
				num4 += array[num17 + 3];
				num5 += array[num17 + 4];
				num6 += array[num17 + 5];
				num7 += array[num17 + 6];
				num8 += array[num17 + 7];
				num9 += array[num17 + 8];
				num10 += array[num17 + 9];
				num11 += array[num17 + 10];
				num12 += array[num17 + 11];
				num13 += array[num17 + 12];
				num14 += array[num17 + 13] + array2[num18];
				num15 += array[num17 + 14] + array2[num18 + 1];
				num16 += array[num17 + 15] + (uint)i;
				num2 = RotlXor(num2, 41, num += num2);
				num4 = RotlXor(num4, 9, num3 += num4);
				num6 = RotlXor(num6, 37, num5 += num6);
				num8 = RotlXor(num8, 31, num7 += num8);
				num10 = RotlXor(num10, 12, num9 += num10);
				num12 = RotlXor(num12, 47, num11 += num12);
				num14 = RotlXor(num14, 44, num13 += num14);
				num16 = RotlXor(num16, 30, num15 += num16);
				num10 = RotlXor(num10, 16, num += num10);
				num14 = RotlXor(num14, 34, num3 += num14);
				num12 = RotlXor(num12, 56, num7 += num12);
				num16 = RotlXor(num16, 51, num5 += num16);
				num8 = RotlXor(num8, 4, num11 += num8);
				num4 = RotlXor(num4, 53, num13 += num4);
				num6 = RotlXor(num6, 42, num15 += num6);
				num2 = RotlXor(num2, 41, num9 += num2);
				num8 = RotlXor(num8, 31, num += num8);
				num6 = RotlXor(num6, 44, num3 += num6);
				num4 = RotlXor(num4, 47, num5 += num4);
				num2 = RotlXor(num2, 46, num7 += num2);
				num16 = RotlXor(num16, 19, num13 += num16);
				num14 = RotlXor(num14, 42, num15 += num14);
				num12 = RotlXor(num12, 44, num9 += num12);
				num10 = RotlXor(num10, 25, num11 += num10);
				num16 = RotlXor(num16, 9, num += num16);
				num12 = RotlXor(num12, 48, num3 += num12);
				num14 = RotlXor(num14, 35, num7 += num14);
				num10 = RotlXor(num10, 52, num5 += num10);
				num2 = RotlXor(num2, 23, num15 += num2);
				num6 = RotlXor(num6, 31, num9 += num6);
				num4 = RotlXor(num4, 37, num11 += num4);
				num8 = RotlXor(num8, 20, num13 += num8);
				num += array[num17 + 1];
				num2 += array[num17 + 2];
				num3 += array[num17 + 3];
				num4 += array[num17 + 4];
				num5 += array[num17 + 5];
				num6 += array[num17 + 6];
				num7 += array[num17 + 7];
				num8 += array[num17 + 8];
				num9 += array[num17 + 9];
				num10 += array[num17 + 10];
				num11 += array[num17 + 11];
				num12 += array[num17 + 12];
				num13 += array[num17 + 13];
				num14 += array[num17 + 14] + array2[num18 + 1];
				num15 += array[num17 + 15] + array2[num18 + 2];
				num16 += array[num17 + 16] + (uint)i + 1;
			}
			outWords[0] = num;
			outWords[1] = num2;
			outWords[2] = num3;
			outWords[3] = num4;
			outWords[4] = num5;
			outWords[5] = num6;
			outWords[6] = num7;
			outWords[7] = num8;
			outWords[8] = num9;
			outWords[9] = num10;
			outWords[10] = num11;
			outWords[11] = num12;
			outWords[12] = num13;
			outWords[13] = num14;
			outWords[14] = num15;
			outWords[15] = num16;
		}

		internal override void DecryptBlock(ulong[] block, ulong[] state)
		{
			ulong[] array = kw;
			ulong[] array2 = t;
			int[] mOD = MOD17;
			int[] mOD2 = MOD3;
			if (array.Length != 33)
			{
				throw new ArgumentException();
			}
			if (array2.Length != 5)
			{
				throw new ArgumentException();
			}
			ulong num = block[0];
			ulong num2 = block[1];
			ulong num3 = block[2];
			ulong num4 = block[3];
			ulong num5 = block[4];
			ulong num6 = block[5];
			ulong num7 = block[6];
			ulong num8 = block[7];
			ulong num9 = block[8];
			ulong num10 = block[9];
			ulong num11 = block[10];
			ulong num12 = block[11];
			ulong num13 = block[12];
			ulong num14 = block[13];
			ulong num15 = block[14];
			ulong num16 = block[15];
			for (int num17 = 19; num17 >= 1; num17 -= 2)
			{
				int num18 = mOD[num17];
				int num19 = mOD2[num17];
				num -= array[num18 + 1];
				num2 -= array[num18 + 2];
				num3 -= array[num18 + 3];
				num4 -= array[num18 + 4];
				num5 -= array[num18 + 5];
				num6 -= array[num18 + 6];
				num7 -= array[num18 + 7];
				num8 -= array[num18 + 8];
				num9 -= array[num18 + 9];
				num10 -= array[num18 + 10];
				num11 -= array[num18 + 11];
				num12 -= array[num18 + 12];
				num13 -= array[num18 + 13];
				num14 -= array[num18 + 14] + array2[num19 + 1];
				num15 -= array[num18 + 15] + array2[num19 + 2];
				num16 -= array[num18 + 16] + (uint)num17 + 1;
				num16 = XorRotr(num16, 9, num);
				num -= num16;
				num12 = XorRotr(num12, 48, num3);
				num3 -= num12;
				num14 = XorRotr(num14, 35, num7);
				num7 -= num14;
				num10 = XorRotr(num10, 52, num5);
				num5 -= num10;
				num2 = XorRotr(num2, 23, num15);
				num15 -= num2;
				num6 = XorRotr(num6, 31, num9);
				num9 -= num6;
				num4 = XorRotr(num4, 37, num11);
				num11 -= num4;
				num8 = XorRotr(num8, 20, num13);
				num13 -= num8;
				num8 = XorRotr(num8, 31, num);
				num -= num8;
				num6 = XorRotr(num6, 44, num3);
				num3 -= num6;
				num4 = XorRotr(num4, 47, num5);
				num5 -= num4;
				num2 = XorRotr(num2, 46, num7);
				num7 -= num2;
				num16 = XorRotr(num16, 19, num13);
				num13 -= num16;
				num14 = XorRotr(num14, 42, num15);
				num15 -= num14;
				num12 = XorRotr(num12, 44, num9);
				num9 -= num12;
				num10 = XorRotr(num10, 25, num11);
				num11 -= num10;
				num10 = XorRotr(num10, 16, num);
				num -= num10;
				num14 = XorRotr(num14, 34, num3);
				num3 -= num14;
				num12 = XorRotr(num12, 56, num7);
				num7 -= num12;
				num16 = XorRotr(num16, 51, num5);
				num5 -= num16;
				num8 = XorRotr(num8, 4, num11);
				num11 -= num8;
				num4 = XorRotr(num4, 53, num13);
				num13 -= num4;
				num6 = XorRotr(num6, 42, num15);
				num15 -= num6;
				num2 = XorRotr(num2, 41, num9);
				num9 -= num2;
				num2 = XorRotr(num2, 41, num);
				num -= num2;
				num4 = XorRotr(num4, 9, num3);
				num3 -= num4;
				num6 = XorRotr(num6, 37, num5);
				num5 -= num6;
				num8 = XorRotr(num8, 31, num7);
				num7 -= num8;
				num10 = XorRotr(num10, 12, num9);
				num9 -= num10;
				num12 = XorRotr(num12, 47, num11);
				num11 -= num12;
				num14 = XorRotr(num14, 44, num13);
				num13 -= num14;
				num16 = XorRotr(num16, 30, num15);
				num15 -= num16;
				num -= array[num18];
				num2 -= array[num18 + 1];
				num3 -= array[num18 + 2];
				num4 -= array[num18 + 3];
				num5 -= array[num18 + 4];
				num6 -= array[num18 + 5];
				num7 -= array[num18 + 6];
				num8 -= array[num18 + 7];
				num9 -= array[num18 + 8];
				num10 -= array[num18 + 9];
				num11 -= array[num18 + 10];
				num12 -= array[num18 + 11];
				num13 -= array[num18 + 12];
				num14 -= array[num18 + 13] + array2[num19];
				num15 -= array[num18 + 14] + array2[num19 + 1];
				num16 -= array[num18 + 15] + (uint)num17;
				num16 = XorRotr(num16, 5, num);
				num -= num16;
				num12 = XorRotr(num12, 20, num3);
				num3 -= num12;
				num14 = XorRotr(num14, 48, num7);
				num7 -= num14;
				num10 = XorRotr(num10, 41, num5);
				num5 -= num10;
				num2 = XorRotr(num2, 47, num15);
				num15 -= num2;
				num6 = XorRotr(num6, 28, num9);
				num9 -= num6;
				num4 = XorRotr(num4, 16, num11);
				num11 -= num4;
				num8 = XorRotr(num8, 25, num13);
				num13 -= num8;
				num8 = XorRotr(num8, 33, num);
				num -= num8;
				num6 = XorRotr(num6, 4, num3);
				num3 -= num6;
				num4 = XorRotr(num4, 51, num5);
				num5 -= num4;
				num2 = XorRotr(num2, 13, num7);
				num7 -= num2;
				num16 = XorRotr(num16, 34, num13);
				num13 -= num16;
				num14 = XorRotr(num14, 41, num15);
				num15 -= num14;
				num12 = XorRotr(num12, 59, num9);
				num9 -= num12;
				num10 = XorRotr(num10, 17, num11);
				num11 -= num10;
				num10 = XorRotr(num10, 38, num);
				num -= num10;
				num14 = XorRotr(num14, 19, num3);
				num3 -= num14;
				num12 = XorRotr(num12, 10, num7);
				num7 -= num12;
				num16 = XorRotr(num16, 55, num5);
				num5 -= num16;
				num8 = XorRotr(num8, 49, num11);
				num11 -= num8;
				num4 = XorRotr(num4, 18, num13);
				num13 -= num4;
				num6 = XorRotr(num6, 23, num15);
				num15 -= num6;
				num2 = XorRotr(num2, 52, num9);
				num9 -= num2;
				num2 = XorRotr(num2, 24, num);
				num -= num2;
				num4 = XorRotr(num4, 13, num3);
				num3 -= num4;
				num6 = XorRotr(num6, 8, num5);
				num5 -= num6;
				num8 = XorRotr(num8, 47, num7);
				num7 -= num8;
				num10 = XorRotr(num10, 8, num9);
				num9 -= num10;
				num12 = XorRotr(num12, 17, num11);
				num11 -= num12;
				num14 = XorRotr(num14, 22, num13);
				num13 -= num14;
				num16 = XorRotr(num16, 37, num15);
				num15 -= num16;
			}
			num -= array[0];
			num2 -= array[1];
			num3 -= array[2];
			num4 -= array[3];
			num5 -= array[4];
			num6 -= array[5];
			num7 -= array[6];
			num8 -= array[7];
			num9 -= array[8];
			num10 -= array[9];
			num11 -= array[10];
			num12 -= array[11];
			num13 -= array[12];
			num14 -= array[13] + array2[0];
			num15 -= array[14] + array2[1];
			num16 -= array[15];
			state[0] = num;
			state[1] = num2;
			state[2] = num3;
			state[3] = num4;
			state[4] = num5;
			state[5] = num6;
			state[6] = num7;
			state[7] = num8;
			state[8] = num9;
			state[9] = num10;
			state[10] = num11;
			state[11] = num12;
			state[12] = num13;
			state[13] = num14;
			state[14] = num15;
			state[15] = num16;
		}
	}

	public const int BLOCKSIZE_256 = 256;

	public const int BLOCKSIZE_512 = 512;

	public const int BLOCKSIZE_1024 = 1024;

	private const int TWEAK_SIZE_BYTES = 16;

	private const int TWEAK_SIZE_WORDS = 2;

	private const int ROUNDS_256 = 72;

	private const int ROUNDS_512 = 72;

	private const int ROUNDS_1024 = 80;

	private const int MAX_ROUNDS = 80;

	private const ulong C_240 = 2004413935125273122uL;

	private static readonly int[] MOD9;

	private static readonly int[] MOD17;

	private static readonly int[] MOD5;

	private static readonly int[] MOD3;

	private readonly int blocksizeBytes;

	private readonly int blocksizeWords;

	private readonly ulong[] currentBlock;

	private readonly ulong[] t = new ulong[5];

	private readonly ulong[] kw;

	private readonly ThreefishCipher cipher;

	private bool forEncryption;

	public virtual string AlgorithmName => "Threefish-" + blocksizeBytes * 8;

	public virtual bool IsPartialBlockOkay => false;

	static ThreefishEngine()
	{
		MOD9 = new int[80];
		MOD17 = new int[MOD9.Length];
		MOD5 = new int[MOD9.Length];
		MOD3 = new int[MOD9.Length];
		for (int i = 0; i < MOD9.Length; i++)
		{
			MOD17[i] = i % 17;
			MOD9[i] = i % 9;
			MOD5[i] = i % 5;
			MOD3[i] = i % 3;
		}
	}

	public ThreefishEngine(int blocksizeBits)
	{
		blocksizeBytes = blocksizeBits / 8;
		blocksizeWords = blocksizeBytes / 8;
		currentBlock = new ulong[blocksizeWords];
		kw = new ulong[2 * blocksizeWords + 1];
		switch (blocksizeBits)
		{
		case 256:
			cipher = new Threefish256Cipher(kw, t);
			break;
		case 512:
			cipher = new Threefish512Cipher(kw, t);
			break;
		case 1024:
			cipher = new Threefish1024Cipher(kw, t);
			break;
		default:
			throw new ArgumentException("Invalid blocksize - Threefish is defined with block size of 256, 512, or 1024 bits");
		}
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		byte[] key;
		byte[] array;
		if (parameters is TweakableBlockCipherParameters)
		{
			TweakableBlockCipherParameters tweakableBlockCipherParameters = (TweakableBlockCipherParameters)parameters;
			key = tweakableBlockCipherParameters.Key.GetKey();
			array = tweakableBlockCipherParameters.Tweak;
		}
		else
		{
			if (!(parameters is KeyParameter))
			{
				throw new ArgumentException("Invalid parameter passed to Threefish init - " + Platform.GetTypeName(parameters));
			}
			key = ((KeyParameter)parameters).GetKey();
			array = null;
		}
		ulong[] array2 = null;
		ulong[] tweak = null;
		if (key != null)
		{
			if (key.Length != blocksizeBytes)
			{
				throw new ArgumentException("Threefish key must be same size as block (" + blocksizeBytes + " bytes)");
			}
			array2 = new ulong[blocksizeWords];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = BytesToWord(key, i * 8);
			}
		}
		if (array != null)
		{
			if (array.Length != 16)
			{
				throw new ArgumentException("Threefish tweak must be " + 16 + " bytes");
			}
			tweak = new ulong[2]
			{
				BytesToWord(array, 0),
				BytesToWord(array, 8)
			};
		}
		Init(forEncryption, array2, tweak);
	}

	internal void Init(bool forEncryption, ulong[] key, ulong[] tweak)
	{
		this.forEncryption = forEncryption;
		if (key != null)
		{
			SetKey(key);
		}
		if (tweak != null)
		{
			SetTweak(tweak);
		}
	}

	private void SetKey(ulong[] key)
	{
		if (key.Length != blocksizeWords)
		{
			throw new ArgumentException("Threefish key must be same size as block (" + blocksizeWords + " words)");
		}
		ulong num = 2004413935125273122uL;
		for (int i = 0; i < blocksizeWords; i++)
		{
			kw[i] = key[i];
			num ^= kw[i];
		}
		kw[blocksizeWords] = num;
		Array.Copy(kw, 0, kw, blocksizeWords + 1, blocksizeWords);
	}

	private void SetTweak(ulong[] tweak)
	{
		if (tweak.Length != 2)
		{
			throw new ArgumentException("Tweak must be " + 2 + " words.");
		}
		t[0] = tweak[0];
		t[1] = tweak[1];
		t[2] = t[0] ^ t[1];
		t[3] = t[0];
		t[4] = t[1];
	}

	public virtual int GetBlockSize()
	{
		return blocksizeBytes;
	}

	public virtual void Reset()
	{
	}

	public virtual int ProcessBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
	{
		if (outOff + blocksizeBytes > outBytes.Length)
		{
			throw new DataLengthException("Output buffer too short");
		}
		if (inOff + blocksizeBytes > inBytes.Length)
		{
			throw new DataLengthException("Input buffer too short");
		}
		for (int i = 0; i < blocksizeBytes; i += 8)
		{
			currentBlock[i >> 3] = BytesToWord(inBytes, inOff + i);
		}
		ProcessBlock(currentBlock, currentBlock);
		for (int j = 0; j < blocksizeBytes; j += 8)
		{
			WordToBytes(currentBlock[j >> 3], outBytes, outOff + j);
		}
		return blocksizeBytes;
	}

	internal int ProcessBlock(ulong[] inWords, ulong[] outWords)
	{
		if (kw[blocksizeWords] == 0)
		{
			throw new InvalidOperationException("Threefish engine not initialised");
		}
		if (inWords.Length != blocksizeWords)
		{
			throw new DataLengthException("Input buffer too short");
		}
		if (outWords.Length != blocksizeWords)
		{
			throw new DataLengthException("Output buffer too short");
		}
		if (forEncryption)
		{
			cipher.EncryptBlock(inWords, outWords);
		}
		else
		{
			cipher.DecryptBlock(inWords, outWords);
		}
		return blocksizeWords;
	}

	internal static ulong BytesToWord(byte[] bytes, int off)
	{
		if (off + 8 > bytes.Length)
		{
			throw new ArgumentException();
		}
		ulong num = 0uL;
		int num2 = off;
		num = (ulong)bytes[num2++] & 0xFFuL;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 8;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 16;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 24;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 32;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 40;
		num |= ((ulong)bytes[num2++] & 0xFFuL) << 48;
		return num | (((ulong)bytes[num2++] & 0xFFuL) << 56);
	}

	internal static void WordToBytes(ulong word, byte[] bytes, int off)
	{
		if (off + 8 > bytes.Length)
		{
			throw new ArgumentException();
		}
		int num = off;
		bytes[num++] = (byte)word;
		bytes[num++] = (byte)(word >> 8);
		bytes[num++] = (byte)(word >> 16);
		bytes[num++] = (byte)(word >> 24);
		bytes[num++] = (byte)(word >> 32);
		bytes[num++] = (byte)(word >> 40);
		bytes[num++] = (byte)(word >> 48);
		bytes[num++] = (byte)(word >> 56);
	}

	private static ulong RotlXor(ulong x, int n, ulong xor)
	{
		return ((x << n) | (x >> 64 - n)) ^ xor;
	}

	private static ulong XorRotr(ulong x, int n, ulong xor)
	{
		ulong num = x ^ xor;
		return (num >> n) | (num << 64 - n);
	}
}
