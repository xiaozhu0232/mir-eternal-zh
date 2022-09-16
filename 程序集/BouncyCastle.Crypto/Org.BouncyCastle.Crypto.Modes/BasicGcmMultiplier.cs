namespace Org.BouncyCastle.Crypto.Modes.Gcm;

public class BasicGcmMultiplier : IGcmMultiplier
{
	private ulong[] H;

	public void Init(byte[] H)
	{
		this.H = GcmUtilities.AsUlongs(H);
	}

	public void MultiplyH(byte[] x)
	{
		ulong[] x2 = GcmUtilities.AsUlongs(x);
		GcmUtilities.Multiply(x2, H);
		GcmUtilities.AsBytes(x2, x);
	}
}
