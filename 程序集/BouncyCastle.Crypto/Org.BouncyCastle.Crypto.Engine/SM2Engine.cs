using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class SM2Engine
{
	private readonly IDigest mDigest;

	private bool mForEncryption;

	private ECKeyParameters mECKey;

	private ECDomainParameters mECParams;

	private int mCurveLength;

	private SecureRandom mRandom;

	public SM2Engine()
		: this(new SM3Digest())
	{
	}

	public SM2Engine(IDigest digest)
	{
		mDigest = digest;
	}

	public virtual void Init(bool forEncryption, ICipherParameters param)
	{
		mForEncryption = forEncryption;
		if (forEncryption)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)param;
			mECKey = (ECKeyParameters)parametersWithRandom.Parameters;
			mECParams = mECKey.Parameters;
			ECPoint eCPoint = ((ECPublicKeyParameters)mECKey).Q.Multiply(mECParams.H);
			if (eCPoint.IsInfinity)
			{
				throw new ArgumentException("invalid key: [h]Q at infinity");
			}
			mRandom = parametersWithRandom.Random;
		}
		else
		{
			mECKey = (ECKeyParameters)param;
			mECParams = mECKey.Parameters;
		}
		mCurveLength = (mECParams.Curve.FieldSize + 7) / 8;
	}

	public virtual byte[] ProcessBlock(byte[] input, int inOff, int inLen)
	{
		if (mForEncryption)
		{
			return Encrypt(input, inOff, inLen);
		}
		return Decrypt(input, inOff, inLen);
	}

	protected virtual ECMultiplier CreateBasePointMultiplier()
	{
		return new FixedPointCombMultiplier();
	}

	private byte[] Encrypt(byte[] input, int inOff, int inLen)
	{
		byte[] array = new byte[inLen];
		Array.Copy(input, inOff, array, 0, array.Length);
		ECMultiplier eCMultiplier = CreateBasePointMultiplier();
		byte[] encoded;
		ECPoint eCPoint2;
		do
		{
			BigInteger bigInteger = NextK();
			ECPoint eCPoint = eCMultiplier.Multiply(mECParams.G, bigInteger).Normalize();
			encoded = eCPoint.GetEncoded(compressed: false);
			eCPoint2 = ((ECPublicKeyParameters)mECKey).Q.Multiply(bigInteger).Normalize();
			Kdf(mDigest, eCPoint2, array);
		}
		while (NotEncrypted(array, input, inOff));
		AddFieldElement(mDigest, eCPoint2.AffineXCoord);
		mDigest.BlockUpdate(input, inOff, inLen);
		AddFieldElement(mDigest, eCPoint2.AffineYCoord);
		byte[] array2 = DigestUtilities.DoFinal(mDigest);
		return Arrays.ConcatenateAll(encoded, array, array2);
	}

	private byte[] Decrypt(byte[] input, int inOff, int inLen)
	{
		byte[] array = new byte[mCurveLength * 2 + 1];
		Array.Copy(input, inOff, array, 0, array.Length);
		ECPoint eCPoint = mECParams.Curve.DecodePoint(array);
		ECPoint eCPoint2 = eCPoint.Multiply(mECParams.H);
		if (eCPoint2.IsInfinity)
		{
			throw new InvalidCipherTextException("[h]C1 at infinity");
		}
		eCPoint = eCPoint.Multiply(((ECPrivateKeyParameters)mECKey).D).Normalize();
		byte[] array2 = new byte[inLen - array.Length - mDigest.GetDigestSize()];
		Array.Copy(input, inOff + array.Length, array2, 0, array2.Length);
		Kdf(mDigest, eCPoint, array2);
		AddFieldElement(mDigest, eCPoint.AffineXCoord);
		mDigest.BlockUpdate(array2, 0, array2.Length);
		AddFieldElement(mDigest, eCPoint.AffineYCoord);
		byte[] array3 = DigestUtilities.DoFinal(mDigest);
		int num = 0;
		for (int i = 0; i != array3.Length; i++)
		{
			num |= array3[i] ^ input[inOff + array.Length + array2.Length + i];
		}
		Arrays.Fill(array, 0);
		Arrays.Fill(array3, 0);
		if (num != 0)
		{
			Arrays.Fill(array2, 0);
			throw new InvalidCipherTextException("invalid cipher text");
		}
		return array2;
	}

	private bool NotEncrypted(byte[] encData, byte[] input, int inOff)
	{
		for (int i = 0; i != encData.Length; i++)
		{
			if (encData[i] != input[inOff + i])
			{
				return false;
			}
		}
		return true;
	}

	private void Kdf(IDigest digest, ECPoint c1, byte[] encData)
	{
		int digestSize = digest.GetDigestSize();
		byte[] array = new byte[System.Math.Max(4, digestSize)];
		int i = 0;
		IMemoable memoable = digest as IMemoable;
		IMemoable other = null;
		if (memoable != null)
		{
			AddFieldElement(digest, c1.AffineXCoord);
			AddFieldElement(digest, c1.AffineYCoord);
			other = memoable.Copy();
		}
		uint num = 0u;
		int num2;
		for (; i < encData.Length; i += num2)
		{
			if (memoable != null)
			{
				memoable.Reset(other);
			}
			else
			{
				AddFieldElement(digest, c1.AffineXCoord);
				AddFieldElement(digest, c1.AffineYCoord);
			}
			Pack.UInt32_To_BE(++num, array, 0);
			digest.BlockUpdate(array, 0, 4);
			digest.DoFinal(array, 0);
			num2 = System.Math.Min(digestSize, encData.Length - i);
			Xor(encData, array, i, num2);
		}
	}

	private void Xor(byte[] data, byte[] kdfOut, int dOff, int dRemaining)
	{
		for (int i = 0; i != dRemaining; i++)
		{
			byte[] array;
			byte[] array2 = (array = data);
			int num = dOff + i;
			nint num2 = num;
			array2[num] = (byte)(array[num2] ^ kdfOut[i]);
		}
	}

	private BigInteger NextK()
	{
		int bitLength = mECParams.N.BitLength;
		BigInteger bigInteger;
		do
		{
			bigInteger = new BigInteger(bitLength, mRandom);
		}
		while (bigInteger.SignValue == 0 || bigInteger.CompareTo(mECParams.N) >= 0);
		return bigInteger;
	}

	private void AddFieldElement(IDigest digest, ECFieldElement v)
	{
		byte[] encoded = v.GetEncoded();
		digest.BlockUpdate(encoded, 0, encoded.Length);
	}
}
