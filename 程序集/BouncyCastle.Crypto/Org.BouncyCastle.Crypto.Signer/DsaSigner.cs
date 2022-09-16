using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class DsaSigner : IDsaExt, IDsa
{
	protected readonly IDsaKCalculator kCalculator;

	protected DsaKeyParameters key = null;

	protected SecureRandom random = null;

	public virtual string AlgorithmName => "DSA";

	public virtual BigInteger Order => key.Parameters.Q;

	public DsaSigner()
	{
		kCalculator = new RandomDsaKCalculator();
	}

	public DsaSigner(IDsaKCalculator kCalculator)
	{
		this.kCalculator = kCalculator;
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		SecureRandom provided = null;
		if (forSigning)
		{
			if (parameters is ParametersWithRandom)
			{
				ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
				provided = parametersWithRandom.Random;
				parameters = parametersWithRandom.Parameters;
			}
			if (!(parameters is DsaPrivateKeyParameters))
			{
				throw new InvalidKeyException("DSA private key required for signing");
			}
			key = (DsaPrivateKeyParameters)parameters;
		}
		else
		{
			if (!(parameters is DsaPublicKeyParameters))
			{
				throw new InvalidKeyException("DSA public key required for verification");
			}
			key = (DsaPublicKeyParameters)parameters;
		}
		random = InitSecureRandom(forSigning && !kCalculator.IsDeterministic, provided);
	}

	public virtual BigInteger[] GenerateSignature(byte[] message)
	{
		DsaParameters parameters = key.Parameters;
		BigInteger q = parameters.Q;
		BigInteger bigInteger = CalculateE(q, message);
		BigInteger x = ((DsaPrivateKeyParameters)key).X;
		if (kCalculator.IsDeterministic)
		{
			kCalculator.Init(q, x, message);
		}
		else
		{
			kCalculator.Init(q, random);
		}
		BigInteger bigInteger2 = kCalculator.NextK();
		BigInteger bigInteger3 = parameters.G.ModPow(bigInteger2, parameters.P).Mod(q);
		bigInteger2 = BigIntegers.ModOddInverse(q, bigInteger2).Multiply(bigInteger.Add(x.Multiply(bigInteger3)));
		BigInteger bigInteger4 = bigInteger2.Mod(q);
		return new BigInteger[2] { bigInteger3, bigInteger4 };
	}

	public virtual bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
	{
		DsaParameters parameters = key.Parameters;
		BigInteger q = parameters.Q;
		BigInteger bigInteger = CalculateE(q, message);
		if (r.SignValue <= 0 || q.CompareTo(r) <= 0)
		{
			return false;
		}
		if (s.SignValue <= 0 || q.CompareTo(s) <= 0)
		{
			return false;
		}
		BigInteger val = BigIntegers.ModOddInverseVar(q, s);
		BigInteger e = bigInteger.Multiply(val).Mod(q);
		BigInteger e2 = r.Multiply(val).Mod(q);
		BigInteger p = parameters.P;
		e = parameters.G.ModPow(e, p);
		e2 = ((DsaPublicKeyParameters)key).Y.ModPow(e2, p);
		BigInteger bigInteger2 = e.Multiply(e2).Mod(p).Mod(q);
		return bigInteger2.Equals(r);
	}

	protected virtual BigInteger CalculateE(BigInteger n, byte[] message)
	{
		int length = System.Math.Min(message.Length, n.BitLength / 8);
		return new BigInteger(1, message, 0, length);
	}

	protected virtual SecureRandom InitSecureRandom(bool needed, SecureRandom provided)
	{
		if (needed)
		{
			if (provided == null)
			{
				return new SecureRandom();
			}
			return provided;
		}
		return null;
	}
}
