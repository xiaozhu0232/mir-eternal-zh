using System;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Crypto.Tls;

internal class Timeout
{
	private long durationMillis;

	private long startMillis;

	internal Timeout(long durationMillis)
		: this(durationMillis, DateTimeUtilities.CurrentUnixMs())
	{
	}

	internal Timeout(long durationMillis, long currentTimeMillis)
	{
		this.durationMillis = System.Math.Max(0L, durationMillis);
		startMillis = System.Math.Max(0L, currentTimeMillis);
	}

	internal long RemainingMillis(long currentTimeMillis)
	{
		lock (this)
		{
			if (startMillis > currentTimeMillis)
			{
				startMillis = currentTimeMillis;
				return durationMillis;
			}
			long num = currentTimeMillis - startMillis;
			long num2 = durationMillis - num;
			if (num2 <= 0)
			{
				return durationMillis = 0L;
			}
			return num2;
		}
	}

	internal static int ConstrainWaitMillis(int waitMillis, Timeout timeout, long currentTimeMillis)
	{
		if (waitMillis < 0)
		{
			return -1;
		}
		int waitMillis2 = GetWaitMillis(timeout, currentTimeMillis);
		if (waitMillis2 < 0)
		{
			return -1;
		}
		if (waitMillis == 0)
		{
			return waitMillis2;
		}
		if (waitMillis2 == 0)
		{
			return waitMillis;
		}
		return System.Math.Min(waitMillis, waitMillis2);
	}

	internal static Timeout ForWaitMillis(int waitMillis)
	{
		return ForWaitMillis(waitMillis, DateTimeUtilities.CurrentUnixMs());
	}

	internal static Timeout ForWaitMillis(int waitMillis, long currentTimeMillis)
	{
		if (waitMillis < 0)
		{
			throw new ArgumentException("cannot be negative", "waitMillis");
		}
		if (waitMillis > 0)
		{
			return new Timeout(waitMillis, currentTimeMillis);
		}
		return null;
	}

	internal static int GetWaitMillis(Timeout timeout, long currentTimeMillis)
	{
		if (timeout == null)
		{
			return 0;
		}
		long num = timeout.RemainingMillis(currentTimeMillis);
		if (num < 1)
		{
			return -1;
		}
		if (num > int.MaxValue)
		{
			return int.MaxValue;
		}
		return (int)num;
	}

	internal static bool HasExpired(Timeout timeout, long currentTimeMillis)
	{
		if (timeout != null)
		{
			return timeout.RemainingMillis(currentTimeMillis) < 1;
		}
		return false;
	}
}
