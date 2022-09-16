using System;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DtlsEpoch
{
	private readonly DtlsReplayWindow mReplayWindow = new DtlsReplayWindow();

	private readonly int mEpoch;

	private readonly TlsCipher mCipher;

	private long mSequenceNumber = 0L;

	internal TlsCipher Cipher => mCipher;

	internal int Epoch => mEpoch;

	internal DtlsReplayWindow ReplayWindow => mReplayWindow;

	internal long SequenceNumber
	{
		get
		{
			lock (this)
			{
				return mSequenceNumber;
			}
		}
	}

	internal DtlsEpoch(int epoch, TlsCipher cipher)
	{
		if (epoch < 0)
		{
			throw new ArgumentException("must be >= 0", "epoch");
		}
		if (cipher == null)
		{
			throw new ArgumentNullException("cipher");
		}
		mEpoch = epoch;
		mCipher = cipher;
	}

	internal long AllocateSequenceNumber()
	{
		lock (this)
		{
			if (mSequenceNumber >= 281474976710656L)
			{
				throw new TlsFatalAlert(80);
			}
			return mSequenceNumber++;
		}
	}
}
