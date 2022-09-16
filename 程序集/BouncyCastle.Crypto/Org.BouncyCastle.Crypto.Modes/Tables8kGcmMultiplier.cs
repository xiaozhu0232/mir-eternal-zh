using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm;

public class Tables8kGcmMultiplier : IGcmMultiplier
{
	private byte[] H;

	private ulong[][] T;

	public void Init(byte[] H)
	{
		if (T == null)
		{
			T = new ulong[32][];
		}
		else if (Arrays.AreEqual(this.H, H))
		{
			return;
		}
		this.H = Arrays.Clone(H);
		for (int i = 0; i < 32; i++)
		{
			ulong[] array = (T[i] = new ulong[32]);
			if (i == 0)
			{
				GcmUtilities.AsUlongs(this.H, array, 2);
				GcmUtilities.MultiplyP3(array, 2, array, 2);
			}
			else
			{
				GcmUtilities.MultiplyP4(T[i - 1], 2, array, 2);
			}
			for (int j = 2; j < 16; j += 2)
			{
				GcmUtilities.DivideP(array, j, array, j << 1);
				GcmUtilities.Xor(array, j << 1, array, 2, array, j + 1 << 1);
			}
		}
	}

	public void MultiplyH(byte[] x)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		for (int num3 = 15; num3 >= 0; num3--)
		{
			ulong[] array = T[num3 + num3 + 1];
			ulong[] array2 = T[num3 + num3];
			int num4 = (x[num3] & 0xF) << 1;
			int num5 = (x[num3] & 0xF0) >> 3;
			num ^= array[num4] ^ array2[num5];
			num2 ^= array[num4 + 1] ^ array2[num5 + 1];
		}
		Pack.UInt64_To_BE(num, x, 0);
		Pack.UInt64_To_BE(num2, x, 8);
	}
}
