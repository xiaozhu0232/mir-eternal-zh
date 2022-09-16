using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class ECGost3410Signer : IDsaExt, IDsa
{
	private ECKeyParameters key;

	private SecureRandom random;

	private bool forSigning;

	public virtual string AlgorithmName => key.AlgorithmName;

	public virtual BigInteger Order => key.Parameters.N;

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		this.forSigning = forSigning;
		if (forSigning)
		{
			if (parameters is ParametersWithRandom)
			{
				ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
				random = parametersWithRandom.Random;
				parameters = parametersWithRandom.Parameters;
			}
			else
			{
				random = new SecureRandom();
			}
			if (!(parameters is ECPrivateKeyParameters))
			{
				throw new InvalidKeyException("EC private key required for signing");
			}
			key = (ECPrivateKeyParameters)parameters;
		}
		else
		{
			if (!(parameters is ECPublicKeyParameters))
			{
				throw new InvalidKeyException("EC public key required for verification");
			}
			key = (ECPublicKeyParameters)parameters;
		}
	}

	public virtual BigInteger[] GenerateSignature(byte[] message)
	{
		if (!forSigning)
		{
			throw new InvalidOperationException("not initialized for signing");
		}
		byte[] bytes = Arrays.Reverse(message);
		BigInteger val = new BigInteger(1, bytes);
		ECDomainParameters parameters = key.Parameters;
		BigInteger n = parameters.N;
		BigInteger d = ((ECPrivateKeyParameters)key).D;
		BigInteger bigInteger = null;
		ECMultiplier eCMultiplier = CreateBasePointMultiplier();
		BigInteger bigInteger3;
		while (true)
		{
			BigInteger bigInteger2 = new BigInteger(n.BitLength, random);
			if (bigInteger2.SignValue == 0)
			{
				continue;
			}
			ECPoint eCPoint = eCMultiplier.Multiply(parameters.G, bigInteger2).Normalize();
			bigInteger3 = eCPoint.AffineXCoord.ToBigInteger().Mod(n);
			if (bigInteger3.SignValue != 0)
			{
				bigInteger = bigInteger2.Multiply(val).Add(d.Multiply(bigInteger3)).Mod(n);
				if (bigInteger.SignValue != 0)
				{
					break;
				}
			}
		}
		return new BigInteger[2] { bigInteger3, bigInteger };
	}

	public virtual bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
	{
		if (forSigning)
		{
			throw new InvalidOperationException("not initialized for verification");
		}
		byte[] bytes = Arrays.Reverse(message);
		BigInteger x = new BigInteger(1, bytes);
		BigInteger n = key.Parameters.N;
		if (r.CompareTo(BigInteger.One) < 0 || r.CompareTo(n) >= 0)
		{
			return false;
		}
		if (s.CompareTo(BigInteger.One) < 0 || s.CompareTo(n) >= 0)
		{
			return false;
		}
		BigInteger val = BigIntegers.ModOddInverseVar(n, x);
		BigInteger a = s.Multiply(val).Mod(n);
		BigInteger b = n.Subtract(r).Multiply(val).Mod(n);
		ECPoint g = key.Parameters.G;
		ECPoint q = ((ECPublicKeyParameters)key).Q;
		ECPoint eCPoint = ECAlgorithms.SumOfTwoMultiplies(g, a, q, b).Normalize();
		if (eCPoint.IsInfinity)
		{
			return false;
		}
		BigInteger bigInteger = eCPoint.AffineXCoord.ToBigInteger().Mod(n);
		return bigInteger.Equals(r);
	}

	protected virtual ECMultiplier CreateBasePointMultiplier()
	{
		return new FixedPointCombMultiplier();
	}
}
