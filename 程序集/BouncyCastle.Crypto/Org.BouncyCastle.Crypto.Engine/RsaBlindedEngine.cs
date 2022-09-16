using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class RsaBlindedEngine : IAsymmetricBlockCipher
{
	private readonly IRsa core;

	private RsaKeyParameters key;

	private SecureRandom random;

	public virtual string AlgorithmName => "RSA";

	public RsaBlindedEngine()
		: this(new RsaCoreEngine())
	{
	}

	public RsaBlindedEngine(IRsa rsa)
	{
		core = rsa;
	}

	public virtual void Init(bool forEncryption, ICipherParameters param)
	{
		core.Init(forEncryption, param);
		if (param is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)param;
			key = (RsaKeyParameters)parametersWithRandom.Parameters;
			if (key is RsaPrivateCrtKeyParameters)
			{
				random = parametersWithRandom.Random;
			}
			else
			{
				random = null;
			}
		}
		else
		{
			key = (RsaKeyParameters)param;
			if (key is RsaPrivateCrtKeyParameters)
			{
				random = new SecureRandom();
			}
			else
			{
				random = null;
			}
		}
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
		if (key == null)
		{
			throw new InvalidOperationException("RSA engine not initialised");
		}
		BigInteger bigInteger = core.ConvertInput(inBuf, inOff, inLen);
		BigInteger bigInteger4;
		if (key is RsaPrivateCrtKeyParameters)
		{
			RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)key;
			BigInteger publicExponent = rsaPrivateCrtKeyParameters.PublicExponent;
			if (publicExponent != null)
			{
				BigInteger modulus = rsaPrivateCrtKeyParameters.Modulus;
				BigInteger bigInteger2 = BigIntegers.CreateRandomInRange(BigInteger.One, modulus.Subtract(BigInteger.One), random);
				BigInteger input = bigInteger2.ModPow(publicExponent, modulus).Multiply(bigInteger).Mod(modulus);
				BigInteger bigInteger3 = core.ProcessBlock(input);
				BigInteger val = BigIntegers.ModOddInverse(modulus, bigInteger2);
				bigInteger4 = bigInteger3.Multiply(val).Mod(modulus);
				if (!bigInteger.Equals(bigInteger4.ModPow(publicExponent, modulus)))
				{
					throw new InvalidOperationException("RSA engine faulty decryption/signing detected");
				}
			}
			else
			{
				bigInteger4 = core.ProcessBlock(bigInteger);
			}
		}
		else
		{
			bigInteger4 = core.ProcessBlock(bigInteger);
		}
		return core.ConvertOutput(bigInteger4);
	}
}
