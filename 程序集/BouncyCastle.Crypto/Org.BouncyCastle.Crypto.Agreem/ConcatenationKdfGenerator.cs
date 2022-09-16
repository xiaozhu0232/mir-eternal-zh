using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Kdf;

public class ConcatenationKdfGenerator : IDerivationFunction
{
	private readonly IDigest mDigest;

	private byte[] mShared;

	private byte[] mOtherInfo;

	private int mHLen;

	public virtual IDigest Digest => mDigest;

	public ConcatenationKdfGenerator(IDigest digest)
	{
		mDigest = digest;
		mHLen = digest.GetDigestSize();
	}

	public virtual void Init(IDerivationParameters param)
	{
		if (!(param is KdfParameters))
		{
			throw new ArgumentException("KDF parameters required for ConcatenationKdfGenerator");
		}
		KdfParameters kdfParameters = (KdfParameters)param;
		mShared = kdfParameters.GetSharedSecret();
		mOtherInfo = kdfParameters.GetIV();
	}

	public virtual int GenerateBytes(byte[] outBytes, int outOff, int len)
	{
		if (outBytes.Length - len < outOff)
		{
			throw new DataLengthException("output buffer too small");
		}
		byte[] array = new byte[mHLen];
		byte[] array2 = new byte[4];
		uint n = 1u;
		int num = 0;
		mDigest.Reset();
		if (len > mHLen)
		{
			do
			{
				Pack.UInt32_To_BE(n, array2);
				mDigest.BlockUpdate(array2, 0, array2.Length);
				mDigest.BlockUpdate(mShared, 0, mShared.Length);
				mDigest.BlockUpdate(mOtherInfo, 0, mOtherInfo.Length);
				mDigest.DoFinal(array, 0);
				Array.Copy(array, 0, outBytes, outOff + num, mHLen);
				num += mHLen;
			}
			while (n++ < len / mHLen);
		}
		if (num < len)
		{
			Pack.UInt32_To_BE(n, array2);
			mDigest.BlockUpdate(array2, 0, array2.Length);
			mDigest.BlockUpdate(mShared, 0, mShared.Length);
			mDigest.BlockUpdate(mOtherInfo, 0, mOtherInfo.Length);
			mDigest.DoFinal(array, 0);
			Array.Copy(array, 0, outBytes, outOff + num, len - num);
		}
		return len;
	}
}
