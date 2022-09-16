using System;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg.Sig;

public class SignatureCreationTime : SignatureSubpacket
{
	protected static byte[] TimeToBytes(DateTime time)
	{
		long num = DateTimeUtilities.DateTimeToUnixMs(time) / 1000;
		return new byte[4]
		{
			(byte)(num >> 24),
			(byte)(num >> 16),
			(byte)(num >> 8),
			(byte)num
		};
	}

	public SignatureCreationTime(bool critical, bool isLongLength, byte[] data)
		: base(SignatureSubpacketTag.CreationTime, critical, isLongLength, data)
	{
	}

	public SignatureCreationTime(bool critical, DateTime date)
		: base(SignatureSubpacketTag.CreationTime, critical, isLongLength: false, TimeToBytes(date))
	{
	}

	public DateTime GetTime()
	{
		long num = (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
		return DateTimeUtilities.UnixMsToDateTime(num * 1000);
	}
}
