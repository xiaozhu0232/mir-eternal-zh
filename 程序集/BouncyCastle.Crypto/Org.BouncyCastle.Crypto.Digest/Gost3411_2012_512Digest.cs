using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Gost3411_2012_512Digest : Gost3411_2012Digest
{
	private static readonly byte[] IV;

	public override string AlgorithmName => "GOST3411-2012-512";

	public Gost3411_2012_512Digest()
		: base(IV)
	{
	}

	public Gost3411_2012_512Digest(Gost3411_2012_512Digest other)
		: base(IV)
	{
		Reset(other);
	}

	public override int GetDigestSize()
	{
		return 64;
	}

	public override IMemoable Copy()
	{
		return new Gost3411_2012_512Digest(this);
	}

	static Gost3411_2012_512Digest()
	{
		byte[] array = (IV = new byte[64]);
	}
}
