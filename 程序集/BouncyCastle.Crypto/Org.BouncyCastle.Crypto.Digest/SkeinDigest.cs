using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class SkeinDigest : IDigest, IMemoable
{
	public const int SKEIN_256 = 256;

	public const int SKEIN_512 = 512;

	public const int SKEIN_1024 = 1024;

	private readonly SkeinEngine engine;

	public string AlgorithmName => "Skein-" + engine.BlockSize * 8 + "-" + engine.OutputSize * 8;

	public SkeinDigest(int stateSizeBits, int digestSizeBits)
	{
		engine = new SkeinEngine(stateSizeBits, digestSizeBits);
		Init(null);
	}

	public SkeinDigest(SkeinDigest digest)
	{
		engine = new SkeinEngine(digest.engine);
	}

	public void Reset(IMemoable other)
	{
		SkeinDigest skeinDigest = (SkeinDigest)other;
		engine.Reset(skeinDigest.engine);
	}

	public IMemoable Copy()
	{
		return new SkeinDigest(this);
	}

	public int GetDigestSize()
	{
		return engine.OutputSize;
	}

	public int GetByteLength()
	{
		return engine.BlockSize;
	}

	public void Init(SkeinParameters parameters)
	{
		engine.Init(parameters);
	}

	public void Reset()
	{
		engine.Reset();
	}

	public void Update(byte inByte)
	{
		engine.Update(inByte);
	}

	public void BlockUpdate(byte[] inBytes, int inOff, int len)
	{
		engine.Update(inBytes, inOff, len);
	}

	public int DoFinal(byte[] outBytes, int outOff)
	{
		return engine.DoFinal(outBytes, outOff);
	}
}
