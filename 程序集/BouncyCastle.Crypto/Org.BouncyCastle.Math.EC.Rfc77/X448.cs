using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Rfc7748;

public abstract class X448
{
	private class F : X448Field
	{
	}

	public const int PointSize = 56;

	public const int ScalarSize = 56;

	private const uint C_A = 156326u;

	private const uint C_A24 = 39082u;

	public static bool CalculateAgreement(byte[] k, int kOff, byte[] u, int uOff, byte[] r, int rOff)
	{
		ScalarMult(k, kOff, u, uOff, r, rOff);
		return !Arrays.AreAllZeroes(r, rOff, 56);
	}

	private static uint Decode32(byte[] bs, int off)
	{
		uint num = bs[off];
		num |= (uint)(bs[++off] << 8);
		num |= (uint)(bs[++off] << 16);
		return num | (uint)(bs[++off] << 24);
	}

	private static void DecodeScalar(byte[] k, int kOff, uint[] n)
	{
		for (int i = 0; i < 14; i++)
		{
			n[i] = Decode32(k, kOff + i * 4);
		}
		uint[] array;
		(array = n)[0] = array[0] & 0xFFFFFFFCu;
		(array = n)[13] = array[13] | 0x80000000u;
	}

	public static void GeneratePrivateKey(SecureRandom random, byte[] k)
	{
		random.NextBytes(k);
		byte[] array;
		(array = k)[0] = (byte)(array[0] & 0xFCu);
		(array = k)[55] = (byte)(array[55] | 0x80u);
	}

	public static void GeneratePublicKey(byte[] k, int kOff, byte[] r, int rOff)
	{
		ScalarMultBase(k, kOff, r, rOff);
	}

	private static void PointDouble(uint[] x, uint[] z)
	{
		uint[] array = X448Field.Create();
		uint[] array2 = X448Field.Create();
		X448Field.Add(x, z, array);
		X448Field.Sub(x, z, array2);
		X448Field.Sqr(array, array);
		X448Field.Sqr(array2, array2);
		X448Field.Mul(array, array2, x);
		X448Field.Sub(array, array2, array);
		X448Field.Mul(array, 39082u, z);
		X448Field.Add(z, array2, z);
		X448Field.Mul(z, array, z);
	}

	public static void Precompute()
	{
		Ed448.Precompute();
	}

	public static void ScalarMult(byte[] k, int kOff, byte[] u, int uOff, byte[] r, int rOff)
	{
		uint[] array = new uint[14];
		DecodeScalar(k, kOff, array);
		uint[] array2 = X448Field.Create();
		X448Field.Decode(u, uOff, array2);
		uint[] array3 = X448Field.Create();
		X448Field.Copy(array2, 0, array3, 0);
		uint[] array4 = X448Field.Create();
		array4[0] = 1u;
		uint[] array5 = X448Field.Create();
		array5[0] = 1u;
		uint[] array6 = X448Field.Create();
		uint[] array7 = X448Field.Create();
		uint[] array8 = X448Field.Create();
		int num = 447;
		int num2 = 1;
		do
		{
			X448Field.Add(array5, array6, array7);
			X448Field.Sub(array5, array6, array5);
			X448Field.Add(array3, array4, array6);
			X448Field.Sub(array3, array4, array3);
			X448Field.Mul(array7, array3, array7);
			X448Field.Mul(array5, array6, array5);
			X448Field.Sqr(array6, array6);
			X448Field.Sqr(array3, array3);
			X448Field.Sub(array6, array3, array8);
			X448Field.Mul(array8, 39082u, array4);
			X448Field.Add(array4, array3, array4);
			X448Field.Mul(array4, array8, array4);
			X448Field.Mul(array3, array6, array3);
			X448Field.Sub(array7, array5, array6);
			X448Field.Add(array7, array5, array5);
			X448Field.Sqr(array5, array5);
			X448Field.Sqr(array6, array6);
			X448Field.Mul(array6, array2, array6);
			num--;
			int num3 = num >> 5;
			int num4 = num & 0x1F;
			int num5 = (int)((array[num3] >> num4) & 1);
			num2 ^= num5;
			X448Field.CSwap(num2, array3, array5);
			X448Field.CSwap(num2, array4, array6);
			num2 = num5;
		}
		while (num >= 2);
		for (int i = 0; i < 2; i++)
		{
			PointDouble(array3, array4);
		}
		X448Field.Inv(array4, array4);
		X448Field.Mul(array3, array4, array3);
		X448Field.Normalize(array3);
		X448Field.Encode(array3, r, rOff);
	}

	public static void ScalarMultBase(byte[] k, int kOff, byte[] r, int rOff)
	{
		uint[] array = X448Field.Create();
		uint[] y = X448Field.Create();
		Ed448.ScalarMultBaseXY(k, kOff, array, y);
		X448Field.Inv(array, array);
		X448Field.Mul(array, y, array);
		X448Field.Sqr(array, array);
		X448Field.Normalize(array);
		X448Field.Encode(array, r, rOff);
	}
}
