using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Sha512Digest : LongDigest
{
	private const int DigestLength = 64;

	public override string AlgorithmName => "SHA-512";

	public Sha512Digest()
	{
	}

	public Sha512Digest(Sha512Digest t)
		: base(t)
	{
	}

	public override int GetDigestSize()
	{
		return 64;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		Finish();
		Pack.UInt64_To_BE(H1, output, outOff);
		Pack.UInt64_To_BE(H2, output, outOff + 8);
		Pack.UInt64_To_BE(H3, output, outOff + 16);
		Pack.UInt64_To_BE(H4, output, outOff + 24);
		Pack.UInt64_To_BE(H5, output, outOff + 32);
		Pack.UInt64_To_BE(H6, output, outOff + 40);
		Pack.UInt64_To_BE(H7, output, outOff + 48);
		Pack.UInt64_To_BE(H8, output, outOff + 56);
		Reset();
		return 64;
	}

	public override void Reset()
	{
		base.Reset();
		H1 = 7640891576956012808uL;
		H2 = 13503953896175478587uL;
		H3 = 4354685564936845355uL;
		H4 = 11912009170470909681uL;
		H5 = 5840696475078001361uL;
		H6 = 11170449401992604703uL;
		H7 = 2270897969802886507uL;
		H8 = 6620516959819538809uL;
	}

	public override IMemoable Copy()
	{
		return new Sha512Digest(this);
	}

	public override void Reset(IMemoable other)
	{
		Sha512Digest t = (Sha512Digest)other;
		CopyIn(t);
	}
}
