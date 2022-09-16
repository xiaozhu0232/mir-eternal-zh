using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC.Rfc7748;

public abstract class X25519
{
	private class F : X25519Field
	{
	}

	public const int PointSize = 32;

	public const int ScalarSize = 32;

	private const int C_A = 486662;

	private const int C_A24 = 121666;

	public static bool CalculateAgreement(byte[] k, int kOff, byte[] u, int uOff, byte[] r, int rOff)
	{
		ScalarMult(k, kOff, u, uOff, r, rOff);
		return !Arrays.AreAllZeroes(r, rOff, 32);
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
		for (int i = 0; i < 8; i++)
		{
			n[i] = Decode32(k, kOff + i * 4);
		}
		uint[] array;
		(array = n)[0] = array[0] & 0xFFFFFFF8u;
		(array = n)[7] = array[7] & 0x7FFFFFFFu;
		(array = n)[7] = array[7] | 0x40000000u;
	}

	public static void GeneratePrivateKey(SecureRandom random, byte[] k)
	{
		random.NextBytes(k);
		byte[] array;
		(array = k)[0] = (byte)(array[0] & 0xF8u);
		(array = k)[31] = (byte)(array[31] & 0x7Fu);
		(array = k)[31] = (byte)(array[31] | 0x40u);
	}

	public static void GeneratePublicKey(byte[] k, int kOff, byte[] r, int rOff)
	{
		ScalarMultBase(k, kOff, r, rOff);
	}

	private static void PointDouble(int[] x, int[] z)
	{
		int[] array = X25519Field.Create();
		int[] array2 = X25519Field.Create();
		X25519Field.Apm(x, z, array, array2);
		X25519Field.Sqr(array, array);
		X25519Field.Sqr(array2, array2);
		X25519Field.Mul(array, array2, x);
		X25519Field.Sub(array, array2, array);
		X25519Field.Mul(array, 121666, z);
		X25519Field.Add(z, array2, z);
		X25519Field.Mul(z, array, z);
	}

	public static void Precompute()
	{
		Ed25519.Precompute();
	}

	public static void ScalarMult(byte[] k, int kOff, byte[] u, int uOff, byte[] r, int rOff)
	{
		uint[] array = new uint[8];
		DecodeScalar(k, kOff, array);
		int[] array2 = X25519Field.Create();
		X25519Field.Decode(u, uOff, array2);
		int[] array3 = X25519Field.Create();
		X25519Field.Copy(array2, 0, array3, 0);
		int[] array4 = X25519Field.Create();
		array4[0] = 1;
		int[] array5 = X25519Field.Create();
		array5[0] = 1;
		int[] array6 = X25519Field.Create();
		int[] array7 = X25519Field.Create();
		int[] array8 = X25519Field.Create();
		int num = 254;
		int num2 = 1;
		do
		{
			X25519Field.Apm(array5, array6, array7, array5);
			X25519Field.Apm(array3, array4, array6, array3);
			X25519Field.Mul(array7, array3, array7);
			X25519Field.Mul(array5, array6, array5);
			X25519Field.Sqr(array6, array6);
			X25519Field.Sqr(array3, array3);
			X25519Field.Sub(array6, array3, array8);
			X25519Field.Mul(array8, 121666, array4);
			X25519Field.Add(array4, array3, array4);
			X25519Field.Mul(array4, array8, array4);
			X25519Field.Mul(array3, array6, array3);
			X25519Field.Apm(array7, array5, array5, array6);
			X25519Field.Sqr(array5, array5);
			X25519Field.Sqr(array6, array6);
			X25519Field.Mul(array6, array2, array6);
			num--;
			int num3 = num >> 5;
			int num4 = num & 0x1F;
			int num5 = (int)((array[num3] >> num4) & 1);
			num2 ^= num5;
			X25519Field.CSwap(num2, array3, array5);
			X25519Field.CSwap(num2, array4, array6);
			num2 = num5;
		}
		while (num >= 3);
		for (int i = 0; i < 3; i++)
		{
			PointDouble(array3, array4);
		}
		X25519Field.Inv(array4, array4);
		X25519Field.Mul(array3, array4, array3);
		X25519Field.Normalize(array3);
		X25519Field.Encode(array3, r, rOff);
	}

	public static void ScalarMultBase(byte[] k, int kOff, byte[] r, int rOff)
	{
		int[] array = X25519Field.Create();
		int[] array2 = X25519Field.Create();
		Ed25519.ScalarMultBaseYZ(k, kOff, array, array2);
		X25519Field.Apm(array2, array, array, array2);
		X25519Field.Inv(array2, array2);
		X25519Field.Mul(array, array2, array);
		X25519Field.Normalize(array);
		X25519Field.Encode(array, r, rOff);
	}
}
