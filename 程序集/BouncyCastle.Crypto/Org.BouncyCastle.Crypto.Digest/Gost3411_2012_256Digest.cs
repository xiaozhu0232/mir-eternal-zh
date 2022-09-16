using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class Gost3411_2012_256Digest : Gost3411_2012Digest
{
	private static readonly byte[] IV = new byte[64]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1
	};

	public override string AlgorithmName => "GOST3411-2012-256";

	public Gost3411_2012_256Digest()
		: base(IV)
	{
	}

	public Gost3411_2012_256Digest(Gost3411_2012_256Digest other)
		: base(IV)
	{
		Reset(other);
	}

	public override int GetDigestSize()
	{
		return 32;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		byte[] array = new byte[64];
		base.DoFinal(array, 0);
		Array.Copy(array, 32, output, outOff, 32);
		return 32;
	}

	public override IMemoable Copy()
	{
		return new Gost3411_2012_256Digest(this);
	}
}
