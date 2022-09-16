using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm;

public class Tables4kGcmMultiplier : IGcmMultiplier
{
	private byte[] H;

	private ulong[] T;

	public void Init(byte[] H)
	{
		if (T == null)
		{
			T = new ulong[512];
		}
		else if (Arrays.AreEqual(this.H, H))
		{
			return;
		}
		this.H = Arrays.Clone(H);
		GcmUtilities.AsUlongs(this.H, T, 2);
		GcmUtilities.MultiplyP7(T, 2, T, 2);
		for (int i = 2; i < 256; i += 2)
		{
			GcmUtilities.DivideP(T, i, T, i << 1);
			GcmUtilities.Xor(T, i << 1, T, 2, T, i + 1 << 1);
		}
	}

	public void MultiplyH(byte[] x)
	{
		int num = x[15] << 1;
		ulong num2 = T[num];
		ulong num3 = T[num + 1];
		for (int num4 = 14; num4 >= 0; num4--)
		{
			num = x[num4] << 1;
			ulong num5 = num3 << 56;
			num3 = T[num + 1] ^ ((num3 >> 8) | (num2 << 56));
			num2 = T[num] ^ (num2 >> 8) ^ num5 ^ (num5 >> 1) ^ (num5 >> 2) ^ (num5 >> 7);
		}
		Pack.UInt64_To_BE(num2, x, 0);
		Pack.UInt64_To_BE(num3, x, 8);
	}
}
