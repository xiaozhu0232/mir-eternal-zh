using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Srp;

public class Srp6Utilities
{
	public static BigInteger CalculateK(IDigest digest, BigInteger N, BigInteger g)
	{
		return HashPaddedPair(digest, N, N, g);
	}

	public static BigInteger CalculateU(IDigest digest, BigInteger N, BigInteger A, BigInteger B)
	{
		return HashPaddedPair(digest, N, A, B);
	}

	public static BigInteger CalculateX(IDigest digest, BigInteger N, byte[] salt, byte[] identity, byte[] password)
	{
		byte[] array = new byte[digest.GetDigestSize()];
		digest.BlockUpdate(identity, 0, identity.Length);
		digest.Update(58);
		digest.BlockUpdate(password, 0, password.Length);
		digest.DoFinal(array, 0);
		digest.BlockUpdate(salt, 0, salt.Length);
		digest.BlockUpdate(array, 0, array.Length);
		digest.DoFinal(array, 0);
		return new BigInteger(1, array);
	}

	public static BigInteger GeneratePrivateValue(IDigest digest, BigInteger N, BigInteger g, SecureRandom random)
	{
		int num = System.Math.Min(256, N.BitLength / 2);
		BigInteger min = BigInteger.One.ShiftLeft(num - 1);
		BigInteger max = N.Subtract(BigInteger.One);
		return BigIntegers.CreateRandomInRange(min, max, random);
	}

	public static BigInteger ValidatePublicValue(BigInteger N, BigInteger val)
	{
		val = val.Mod(N);
		if (val.Equals(BigInteger.Zero))
		{
			throw new CryptoException("Invalid public value: 0");
		}
		return val;
	}

	public static BigInteger CalculateM1(IDigest digest, BigInteger N, BigInteger A, BigInteger B, BigInteger S)
	{
		return HashPaddedTriplet(digest, N, A, B, S);
	}

	public static BigInteger CalculateM2(IDigest digest, BigInteger N, BigInteger A, BigInteger M1, BigInteger S)
	{
		return HashPaddedTriplet(digest, N, A, M1, S);
	}

	public static BigInteger CalculateKey(IDigest digest, BigInteger N, BigInteger S)
	{
		int length = (N.BitLength + 7) / 8;
		byte[] padded = GetPadded(S, length);
		digest.BlockUpdate(padded, 0, padded.Length);
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		return new BigInteger(1, array);
	}

	private static BigInteger HashPaddedTriplet(IDigest digest, BigInteger N, BigInteger n1, BigInteger n2, BigInteger n3)
	{
		int length = (N.BitLength + 7) / 8;
		byte[] padded = GetPadded(n1, length);
		byte[] padded2 = GetPadded(n2, length);
		byte[] padded3 = GetPadded(n3, length);
		digest.BlockUpdate(padded, 0, padded.Length);
		digest.BlockUpdate(padded2, 0, padded2.Length);
		digest.BlockUpdate(padded3, 0, padded3.Length);
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		return new BigInteger(1, array);
	}

	private static BigInteger HashPaddedPair(IDigest digest, BigInteger N, BigInteger n1, BigInteger n2)
	{
		int length = (N.BitLength + 7) / 8;
		byte[] padded = GetPadded(n1, length);
		byte[] padded2 = GetPadded(n2, length);
		digest.BlockUpdate(padded, 0, padded.Length);
		digest.BlockUpdate(padded2, 0, padded2.Length);
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		return new BigInteger(1, array);
	}

	private static byte[] GetPadded(BigInteger n, int length)
	{
		byte[] array = BigIntegers.AsUnsignedByteArray(n);
		if (array.Length < length)
		{
			byte[] array2 = new byte[length];
			Array.Copy(array, 0, array2, length - array.Length, array.Length);
			array = array2;
		}
		return array;
	}
}
