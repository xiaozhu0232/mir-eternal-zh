using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RsaBlindingEngine : IAsymmetricBlockCipher
{
	private readonly IRsa core;

	private RsaKeyParameters key;

	private BigInteger blindingFactor;

	private bool forEncryption;

	public virtual string AlgorithmName => "RSA";

	public RsaBlindingEngine()
		: this(new RsaCoreEngine())
	{
	}

	public RsaBlindingEngine(IRsa rsa)
	{
		core = rsa;
	}

	public virtual void Init(bool forEncryption, ICipherParameters param)
	{
		RsaBlindingParameters rsaBlindingParameters;
		if (param is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)param;
			rsaBlindingParameters = (RsaBlindingParameters)parametersWithRandom.Parameters;
		}
		else
		{
			rsaBlindingParameters = (RsaBlindingParameters)param;
		}
		core.Init(forEncryption, rsaBlindingParameters.PublicKey);
		this.forEncryption = forEncryption;
		key = rsaBlindingParameters.PublicKey;
		blindingFactor = rsaBlindingParameters.BlindingFactor;
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
		BigInteger bigInteger = core.ConvertInput(inBuf, inOff, inLen);
		bigInteger = ((!forEncryption) ? UnblindMessage(bigInteger) : BlindMessage(bigInteger));
		return core.ConvertOutput(bigInteger);
	}

	private BigInteger BlindMessage(BigInteger msg)
	{
		BigInteger bigInteger = blindingFactor;
		bigInteger = msg.Multiply(bigInteger.ModPow(key.Exponent, key.Modulus));
		return bigInteger.Mod(key.Modulus);
	}

	private BigInteger UnblindMessage(BigInteger blindedMsg)
	{
		BigInteger modulus = key.Modulus;
		BigInteger bigInteger = blindedMsg;
		BigInteger val = BigIntegers.ModOddInverse(modulus, blindingFactor);
		bigInteger = bigInteger.Multiply(val);
		return bigInteger.Mod(modulus);
	}
}
