using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls;

internal class CombinedHash : TlsHandshakeHash, IDigest
{
	protected TlsContext mContext;

	protected IDigest mMd5;

	protected IDigest mSha1;

	public virtual string AlgorithmName => mMd5.AlgorithmName + " and " + mSha1.AlgorithmName;

	internal CombinedHash()
	{
		mMd5 = TlsUtilities.CreateHash(1);
		mSha1 = TlsUtilities.CreateHash(2);
	}

	internal CombinedHash(CombinedHash t)
	{
		mContext = t.mContext;
		mMd5 = TlsUtilities.CloneHash(1, t.mMd5);
		mSha1 = TlsUtilities.CloneHash(2, t.mSha1);
	}

	public virtual void Init(TlsContext context)
	{
		mContext = context;
	}

	public virtual TlsHandshakeHash NotifyPrfDetermined()
	{
		return this;
	}

	public virtual void TrackHashAlgorithm(byte hashAlgorithm)
	{
		throw new InvalidOperationException("CombinedHash only supports calculating the legacy PRF for handshake hash");
	}

	public virtual void SealHashAlgorithms()
	{
	}

	public virtual TlsHandshakeHash StopTracking()
	{
		return new CombinedHash(this);
	}

	public virtual IDigest ForkPrfHash()
	{
		return new CombinedHash(this);
	}

	public virtual byte[] GetFinalHash(byte hashAlgorithm)
	{
		throw new InvalidOperationException("CombinedHash doesn't support multiple hashes");
	}

	public virtual int GetByteLength()
	{
		return System.Math.Max(mMd5.GetByteLength(), mSha1.GetByteLength());
	}

	public virtual int GetDigestSize()
	{
		return mMd5.GetDigestSize() + mSha1.GetDigestSize();
	}

	public virtual void Update(byte input)
	{
		mMd5.Update(input);
		mSha1.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		mMd5.BlockUpdate(input, inOff, len);
		mSha1.BlockUpdate(input, inOff, len);
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		if (mContext != null && TlsUtilities.IsSsl(mContext))
		{
			Ssl3Complete(mMd5, Ssl3Mac.IPAD, Ssl3Mac.OPAD, 48);
			Ssl3Complete(mSha1, Ssl3Mac.IPAD, Ssl3Mac.OPAD, 40);
		}
		int num = mMd5.DoFinal(output, outOff);
		int num2 = mSha1.DoFinal(output, outOff + num);
		return num + num2;
	}

	public virtual void Reset()
	{
		mMd5.Reset();
		mSha1.Reset();
	}

	protected virtual void Ssl3Complete(IDigest d, byte[] ipad, byte[] opad, int padLength)
	{
		byte[] masterSecret = mContext.SecurityParameters.masterSecret;
		d.BlockUpdate(masterSecret, 0, masterSecret.Length);
		d.BlockUpdate(ipad, 0, padLength);
		byte[] array = DigestUtilities.DoFinal(d);
		d.BlockUpdate(masterSecret, 0, masterSecret.Length);
		d.BlockUpdate(opad, 0, padLength);
		d.BlockUpdate(array, 0, array.Length);
	}
}
