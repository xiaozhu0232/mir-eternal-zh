using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm;

public class Tables1kGcmExponentiator : IGcmExponentiator
{
	private IList lookupPowX2;

	public void Init(byte[] x)
	{
		ulong[] array = GcmUtilities.AsUlongs(x);
		if (lookupPowX2 == null || !Arrays.AreEqual(array, (ulong[])lookupPowX2[0]))
		{
			lookupPowX2 = Platform.CreateArrayList(8);
			lookupPowX2.Add(array);
		}
	}

	public void ExponentiateX(long pow, byte[] output)
	{
		ulong[] x = GcmUtilities.OneAsUlongs();
		int num = 0;
		while (pow > 0)
		{
			if ((pow & 1) != 0)
			{
				EnsureAvailable(num);
				GcmUtilities.Multiply(x, (ulong[])lookupPowX2[num]);
			}
			num++;
			pow >>= 1;
		}
		GcmUtilities.AsBytes(x, output);
	}

	private void EnsureAvailable(int bit)
	{
		int num = lookupPowX2.Count;
		if (num <= bit)
		{
			ulong[] array = (ulong[])lookupPowX2[num - 1];
			do
			{
				array = Arrays.Clone(array);
				GcmUtilities.Square(array, array);
				lookupPowX2.Add(array);
			}
			while (++num <= bit);
		}
	}
}
