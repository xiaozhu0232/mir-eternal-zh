using System;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DtlsReplayWindow
{
	private const long VALID_SEQ_MASK = 281474976710655L;

	private const long WINDOW_SIZE = 64L;

	private long mLatestConfirmedSeq = -1L;

	private long mBitmap = 0L;

	internal bool ShouldDiscard(long seq)
	{
		if ((seq & 0xFFFFFFFFFFFFL) != seq)
		{
			return true;
		}
		if (seq <= mLatestConfirmedSeq)
		{
			long num = mLatestConfirmedSeq - seq;
			if (num >= 64)
			{
				return true;
			}
			if ((mBitmap & (1L << (int)num)) != 0)
			{
				return true;
			}
		}
		return false;
	}

	internal void ReportAuthenticated(long seq)
	{
		if ((seq & 0xFFFFFFFFFFFFL) != seq)
		{
			throw new ArgumentException("out of range", "seq");
		}
		if (seq <= mLatestConfirmedSeq)
		{
			long num = mLatestConfirmedSeq - seq;
			if (num < 64)
			{
				mBitmap |= 1L << (int)num;
			}
			return;
		}
		long num2 = seq - mLatestConfirmedSeq;
		if (num2 >= 64)
		{
			mBitmap = 1L;
		}
		else
		{
			mBitmap <<= (int)num2;
			mBitmap |= 1L;
		}
		mLatestConfirmedSeq = seq;
	}

	internal void Reset()
	{
		mLatestConfirmedSeq = -1L;
		mBitmap = 0L;
	}
}
