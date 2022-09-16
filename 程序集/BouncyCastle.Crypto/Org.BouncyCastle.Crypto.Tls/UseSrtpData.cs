using System;

namespace Org.BouncyCastle.Crypto.Tls;

public class UseSrtpData
{
	protected readonly int[] mProtectionProfiles;

	protected readonly byte[] mMki;

	public virtual int[] ProtectionProfiles => mProtectionProfiles;

	public virtual byte[] Mki => mMki;

	public UseSrtpData(int[] protectionProfiles, byte[] mki)
	{
		if (protectionProfiles == null || protectionProfiles.Length < 1 || protectionProfiles.Length >= 32768)
		{
			throw new ArgumentException("must have length from 1 to (2^15 - 1)", "protectionProfiles");
		}
		if (mki == null)
		{
			mki = TlsUtilities.EmptyBytes;
		}
		else if (mki.Length > 255)
		{
			throw new ArgumentException("cannot be longer than 255 bytes", "mki");
		}
		mProtectionProfiles = protectionProfiles;
		mMki = mki;
	}
}
