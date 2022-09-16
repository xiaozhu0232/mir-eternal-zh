using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement;

public class SM2KeyExchange
{
	private readonly IDigest mDigest;

	private byte[] mUserID;

	private ECPrivateKeyParameters mStaticKey;

	private ECPoint mStaticPubPoint;

	private ECPoint mEphemeralPubPoint;

	private ECDomainParameters mECParams;

	private int mW;

	private ECPrivateKeyParameters mEphemeralKey;

	private bool mInitiator;

	public SM2KeyExchange()
		: this(new SM3Digest())
	{
	}

	public SM2KeyExchange(IDigest digest)
	{
		mDigest = digest;
	}

	public virtual void Init(ICipherParameters privParam)
	{
		SM2KeyExchangePrivateParameters sM2KeyExchangePrivateParameters;
		if (privParam is ParametersWithID)
		{
			sM2KeyExchangePrivateParameters = (SM2KeyExchangePrivateParameters)((ParametersWithID)privParam).Parameters;
			mUserID = ((ParametersWithID)privParam).GetID();
		}
		else
		{
			sM2KeyExchangePrivateParameters = (SM2KeyExchangePrivateParameters)privParam;
			mUserID = new byte[0];
		}
		mInitiator = sM2KeyExchangePrivateParameters.IsInitiator;
		mStaticKey = sM2KeyExchangePrivateParameters.StaticPrivateKey;
		mEphemeralKey = sM2KeyExchangePrivateParameters.EphemeralPrivateKey;
		mECParams = mStaticKey.Parameters;
		mStaticPubPoint = sM2KeyExchangePrivateParameters.StaticPublicPoint;
		mEphemeralPubPoint = sM2KeyExchangePrivateParameters.EphemeralPublicPoint;
		mW = mECParams.Curve.FieldSize / 2 - 1;
	}

	public virtual byte[] CalculateKey(int kLen, ICipherParameters pubParam)
	{
		SM2KeyExchangePublicParameters sM2KeyExchangePublicParameters;
		byte[] userID;
		if (pubParam is ParametersWithID)
		{
			sM2KeyExchangePublicParameters = (SM2KeyExchangePublicParameters)((ParametersWithID)pubParam).Parameters;
			userID = ((ParametersWithID)pubParam).GetID();
		}
		else
		{
			sM2KeyExchangePublicParameters = (SM2KeyExchangePublicParameters)pubParam;
			userID = new byte[0];
		}
		byte[] z = GetZ(mDigest, mUserID, mStaticPubPoint);
		byte[] z2 = GetZ(mDigest, userID, sM2KeyExchangePublicParameters.StaticPublicKey.Q);
		ECPoint u = CalculateU(sM2KeyExchangePublicParameters);
		if (mInitiator)
		{
			return Kdf(u, z, z2, kLen);
		}
		return Kdf(u, z2, z, kLen);
	}

	public virtual byte[][] CalculateKeyWithConfirmation(int kLen, byte[] confirmationTag, ICipherParameters pubParam)
	{
		SM2KeyExchangePublicParameters sM2KeyExchangePublicParameters;
		byte[] userID;
		if (pubParam is ParametersWithID)
		{
			sM2KeyExchangePublicParameters = (SM2KeyExchangePublicParameters)((ParametersWithID)pubParam).Parameters;
			userID = ((ParametersWithID)pubParam).GetID();
		}
		else
		{
			sM2KeyExchangePublicParameters = (SM2KeyExchangePublicParameters)pubParam;
			userID = new byte[0];
		}
		if (mInitiator && confirmationTag == null)
		{
			throw new ArgumentException("if initiating, confirmationTag must be set");
		}
		byte[] z = GetZ(mDigest, mUserID, mStaticPubPoint);
		byte[] z2 = GetZ(mDigest, userID, sM2KeyExchangePublicParameters.StaticPublicKey.Q);
		ECPoint u = CalculateU(sM2KeyExchangePublicParameters);
		byte[] array;
		if (mInitiator)
		{
			array = Kdf(u, z, z2, kLen);
			byte[] inner = CalculateInnerHash(mDigest, u, z, z2, mEphemeralPubPoint, sM2KeyExchangePublicParameters.EphemeralPublicKey.Q);
			byte[] a = S1(mDigest, u, inner);
			if (!Arrays.ConstantTimeAreEqual(a, confirmationTag))
			{
				throw new InvalidOperationException("confirmation tag mismatch");
			}
			return new byte[2][]
			{
				array,
				S2(mDigest, u, inner)
			};
		}
		array = Kdf(u, z2, z, kLen);
		byte[] inner2 = CalculateInnerHash(mDigest, u, z2, z, sM2KeyExchangePublicParameters.EphemeralPublicKey.Q, mEphemeralPubPoint);
		return new byte[3][]
		{
			array,
			S1(mDigest, u, inner2),
			S2(mDigest, u, inner2)
		};
	}

	protected virtual ECPoint CalculateU(SM2KeyExchangePublicParameters otherPub)
	{
		ECDomainParameters parameters = mStaticKey.Parameters;
		ECPoint p = ECAlgorithms.CleanPoint(parameters.Curve, otherPub.StaticPublicKey.Q);
		ECPoint eCPoint = ECAlgorithms.CleanPoint(parameters.Curve, otherPub.EphemeralPublicKey.Q);
		BigInteger bigInteger = Reduce(mEphemeralPubPoint.AffineXCoord.ToBigInteger());
		BigInteger val = Reduce(eCPoint.AffineXCoord.ToBigInteger());
		BigInteger val2 = mStaticKey.D.Add(bigInteger.Multiply(mEphemeralKey.D));
		BigInteger bigInteger2 = mECParams.H.Multiply(val2).Mod(mECParams.N);
		BigInteger b = bigInteger2.Multiply(val).Mod(mECParams.N);
		return ECAlgorithms.SumOfTwoMultiplies(p, bigInteger2, eCPoint, b).Normalize();
	}

	protected virtual byte[] Kdf(ECPoint u, byte[] za, byte[] zb, int klen)
	{
		int digestSize = mDigest.GetDigestSize();
		byte[] array = new byte[System.Math.Max(4, digestSize)];
		byte[] array2 = new byte[(klen + 7) / 8];
		int i = 0;
		IMemoable memoable = mDigest as IMemoable;
		IMemoable other = null;
		if (memoable != null)
		{
			AddFieldElement(mDigest, u.AffineXCoord);
			AddFieldElement(mDigest, u.AffineYCoord);
			mDigest.BlockUpdate(za, 0, za.Length);
			mDigest.BlockUpdate(zb, 0, zb.Length);
			other = memoable.Copy();
		}
		uint num = 0u;
		int num2;
		for (; i < array2.Length; i += num2)
		{
			if (memoable != null)
			{
				memoable.Reset(other);
			}
			else
			{
				AddFieldElement(mDigest, u.AffineXCoord);
				AddFieldElement(mDigest, u.AffineYCoord);
				mDigest.BlockUpdate(za, 0, za.Length);
				mDigest.BlockUpdate(zb, 0, zb.Length);
			}
			Pack.UInt32_To_BE(++num, array, 0);
			mDigest.BlockUpdate(array, 0, 4);
			mDigest.DoFinal(array, 0);
			num2 = System.Math.Min(digestSize, array2.Length - i);
			Array.Copy(array, 0, array2, i, num2);
		}
		return array2;
	}

	private BigInteger Reduce(BigInteger x)
	{
		return x.And(BigInteger.One.ShiftLeft(mW).Subtract(BigInteger.One)).SetBit(mW);
	}

	private byte[] S1(IDigest digest, ECPoint u, byte[] inner)
	{
		digest.Update(2);
		AddFieldElement(digest, u.AffineYCoord);
		digest.BlockUpdate(inner, 0, inner.Length);
		return DigestUtilities.DoFinal(digest);
	}

	private byte[] CalculateInnerHash(IDigest digest, ECPoint u, byte[] za, byte[] zb, ECPoint p1, ECPoint p2)
	{
		AddFieldElement(digest, u.AffineXCoord);
		digest.BlockUpdate(za, 0, za.Length);
		digest.BlockUpdate(zb, 0, zb.Length);
		AddFieldElement(digest, p1.AffineXCoord);
		AddFieldElement(digest, p1.AffineYCoord);
		AddFieldElement(digest, p2.AffineXCoord);
		AddFieldElement(digest, p2.AffineYCoord);
		return DigestUtilities.DoFinal(digest);
	}

	private byte[] S2(IDigest digest, ECPoint u, byte[] inner)
	{
		digest.Update(3);
		AddFieldElement(digest, u.AffineYCoord);
		digest.BlockUpdate(inner, 0, inner.Length);
		return DigestUtilities.DoFinal(digest);
	}

	private byte[] GetZ(IDigest digest, byte[] userID, ECPoint pubPoint)
	{
		AddUserID(digest, userID);
		AddFieldElement(digest, mECParams.Curve.A);
		AddFieldElement(digest, mECParams.Curve.B);
		AddFieldElement(digest, mECParams.G.AffineXCoord);
		AddFieldElement(digest, mECParams.G.AffineYCoord);
		AddFieldElement(digest, pubPoint.AffineXCoord);
		AddFieldElement(digest, pubPoint.AffineYCoord);
		return DigestUtilities.DoFinal(digest);
	}

	private void AddUserID(IDigest digest, byte[] userID)
	{
		uint num = (uint)(userID.Length * 8);
		digest.Update((byte)(num >> 8));
		digest.Update((byte)num);
		digest.BlockUpdate(userID, 0, userID.Length);
	}

	private void AddFieldElement(IDigest digest, ECFieldElement v)
	{
		byte[] encoded = v.GetEncoded();
		digest.BlockUpdate(encoded, 0, encoded.Length);
	}
}
