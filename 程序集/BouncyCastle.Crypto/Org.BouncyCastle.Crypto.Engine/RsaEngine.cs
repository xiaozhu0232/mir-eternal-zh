using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Engines;

public class RsaEngine : IAsymmetricBlockCipher
{
	private readonly IRsa core;

	public virtual string AlgorithmName => "RSA";

	public RsaEngine()
		: this(new RsaCoreEngine())
	{
	}

	public RsaEngine(IRsa rsa)
	{
		core = rsa;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		core.Init(forEncryption, parameters);
	}

	public virtual int GetInputBlockSize()
	{
		return core.GetInputBlockSize();
	}

	public virtual int GetOutputBlockSize()
	{
		return core.GetOutputBlockSize();
	}

	public virtual byte[] ProcessBlock(byte[] inBuf, int inOff, int inLen)
	{
		BigInteger input = core.ConvertInput(inBuf, inOff, inLen);
		BigInteger result = core.ProcessBlock(input);
		return core.ConvertOutput(result);
	}
}
