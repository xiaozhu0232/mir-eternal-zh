using System;
using System.Collections;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DeferredHash : TlsHandshakeHash, IDigest
{
	protected const int BUFFERING_HASH_LIMIT = 4;

	protected TlsContext mContext;

	private DigestInputBuffer mBuf;

	private IDictionary mHashes;

	private int mPrfHashAlgorithm;

	public virtual string AlgorithmName
	{
		get
		{
			throw new InvalidOperationException("Use Fork() to get a definite IDigest");
		}
	}

	internal DeferredHash()
	{
		mBuf = new DigestInputBuffer();
		mHashes = Platform.CreateHashtable();
		mPrfHashAlgorithm = -1;
	}

	private DeferredHash(byte prfHashAlgorithm, IDigest prfHash)
	{
		mBuf = null;
		mHashes = Platform.CreateHashtable();
		mPrfHashAlgorithm = prfHashAlgorithm;
		mHashes[prfHashAlgorithm] = prfHash;
	}

	public virtual void Init(TlsContext context)
	{
		mContext = context;
	}

	public virtual TlsHandshakeHash NotifyPrfDetermined()
	{
		int prfAlgorithm = mContext.SecurityParameters.PrfAlgorithm;
		if (prfAlgorithm == 0)
		{
			CombinedHash combinedHash = new CombinedHash();
			combinedHash.Init(mContext);
			mBuf.UpdateDigest(combinedHash);
			return combinedHash.NotifyPrfDetermined();
		}
		mPrfHashAlgorithm = TlsUtilities.GetHashAlgorithmForPrfAlgorithm(prfAlgorithm);
		CheckTrackingHash((byte)mPrfHashAlgorithm);
		return this;
	}

	public virtual void TrackHashAlgorithm(byte hashAlgorithm)
	{
		if (mBuf == null)
		{
			throw new InvalidOperationException("Too late to track more hash algorithms");
		}
		CheckTrackingHash(hashAlgorithm);
	}

	public virtual void SealHashAlgorithms()
	{
		CheckStopBuffering();
	}

	public virtual TlsHandshakeHash StopTracking()
	{
		byte b = (byte)mPrfHashAlgorithm;
		IDigest digest = TlsUtilities.CloneHash(b, (IDigest)mHashes[b]);
		if (mBuf != null)
		{
			mBuf.UpdateDigest(digest);
		}
		DeferredHash deferredHash = new DeferredHash(b, digest);
		deferredHash.Init(mContext);
		return deferredHash;
	}

	public virtual IDigest ForkPrfHash()
	{
		CheckStopBuffering();
		byte b = (byte)mPrfHashAlgorithm;
		if (mBuf != null)
		{
			IDigest digest = TlsUtilities.CreateHash(b);
			mBuf.UpdateDigest(digest);
			return digest;
		}
		return TlsUtilities.CloneHash(b, (IDigest)mHashes[b]);
	}

	public virtual byte[] GetFinalHash(byte hashAlgorithm)
	{
		IDigest digest = (IDigest)mHashes[hashAlgorithm];
		if (digest == null)
		{
			throw new InvalidOperationException("HashAlgorithm." + HashAlgorithm.GetText(hashAlgorithm) + " is not being tracked");
		}
		digest = TlsUtilities.CloneHash(hashAlgorithm, digest);
		if (mBuf != null)
		{
			mBuf.UpdateDigest(digest);
		}
		return DigestUtilities.DoFinal(digest);
	}

	public virtual int GetByteLength()
	{
		throw new InvalidOperationException("Use Fork() to get a definite IDigest");
	}

	public virtual int GetDigestSize()
	{
		throw new InvalidOperationException("Use Fork() to get a definite IDigest");
	}

	public virtual void Update(byte input)
	{
		if (mBuf != null)
		{
			mBuf.WriteByte(input);
			return;
		}
		foreach (IDigest value in mHashes.Values)
		{
			value.Update(input);
		}
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int len)
	{
		if (mBuf != null)
		{
			mBuf.Write(input, inOff, len);
			return;
		}
		foreach (IDigest value in mHashes.Values)
		{
			value.BlockUpdate(input, inOff, len);
		}
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		throw new InvalidOperationException("Use Fork() to get a definite IDigest");
	}

	public virtual void Reset()
	{
		if (mBuf != null)
		{
			mBuf.SetLength(0L);
			return;
		}
		foreach (IDigest value in mHashes.Values)
		{
			value.Reset();
		}
	}

	protected virtual void CheckStopBuffering()
	{
		if (mBuf == null || mHashes.Count > 4)
		{
			return;
		}
		foreach (IDigest value in mHashes.Values)
		{
			mBuf.UpdateDigest(value);
		}
		mBuf = null;
	}

	protected virtual void CheckTrackingHash(byte hashAlgorithm)
	{
		if (!mHashes.Contains(hashAlgorithm))
		{
			IDigest value = TlsUtilities.CreateHash(hashAlgorithm);
			mHashes[hashAlgorithm] = value;
		}
	}
}
