using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm;

public class Tables64kGcmMultiplier : IGcmMultiplier
{
	private byte[] H;

	private ulong[][] T;

	public void Init(byte[] H)
	{
		if (T == null)
		{
			T = new ulong[16][];
		}
		else if (Arrays.AreEqual(this.H, H))
		{
			return;
		}
		this.H = Arrays.Clone(H);
		for (int i = 0; i < 16; i++)
		{
			ulong[] array = (T[i] = new ulong[512]);
			if (i == 0)
			{
				GcmUtilities.AsUlongs(this.H, array, 2);
				GcmUtilities.MultiplyP7(array, 2, array, 2);
			}
			else
			{
				GcmUtilities.MultiplyP8(T[i - 1], 2, array, 2);
			}
			for (int j = 2; j < 256; j += 2)
			{
				GcmUtilities.DivideP(array, j, array, j << 1);
				GcmUtilities.Xor(array, j << 1, array, 2, array, j + 1 << 1);
			}
		}
	}

	public void MultiplyH(byte[] x)
	{
		ulong[] array = T[15];
		int num = x[15] << 1;
		ulong num2 = array[num];
		ulong num3 = array[num + 1];
		for (int num4 = 14; num4 >= 0; num4--)
		{
			array = T[num4];
			num = x[num4] << 1;
			num2 ^= array[num];
			num3 ^= array[num + 1];
		}
		Pack.UInt64_To_BE(num2, x, 0);
		Pack.UInt64_To_BE(num3, x, 8);
	}
}
