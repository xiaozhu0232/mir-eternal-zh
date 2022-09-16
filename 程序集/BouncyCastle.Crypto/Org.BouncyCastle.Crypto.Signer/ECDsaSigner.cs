using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class ECDsaSigner : IDsaExt, IDsa
{
	private static readonly BigInteger Eight = BigInteger.ValueOf(8L);

	protected readonly IDsaKCalculator kCalculator;

	protected ECKeyParameters key = null;

	protected SecureRandom random = null;

	public virtual string AlgorithmName => "ECDSA";

	public virtual BigInteger Order => key.Parameters.N;

	public ECDsaSigner()
	{
		kCalculator = new RandomDsaKCalculator();
	}

	public ECDsaSigner(IDsaKCalculator kCalculator)
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
		random = InitSecureRandom(forSigning && !kCalculator.IsDeterministic, provided);
	}

	public virtual BigInteger[] GenerateSignature(byte[] message)
	{
		ECDomainParameters parameters = key.Parameters;
		BigInteger n = parameters.N;
		BigInteger bigInteger = CalculateE(n, message);
		BigInteger d = ((ECPrivateKeyParameters)key).D;
		if (kCalculator.IsDeterministic)
		{
			kCalculator.Init(n, d, message);
		}
		else
		{
			kCalculator.Init(n, random);
		}
		ECMultiplier eCMultiplier = CreateBasePointMultiplier();
		BigInteger bigInteger3;
		BigInteger bigInteger4;
		while (true)
		{
			BigInteger bigInteger2 = kCalculator.NextK();
			ECPoint eCPoint = eCMultiplier.Multiply(parameters.G, bigInteger2).Normalize();
			bigInteger3 = eCPoint.AffineXCoord.ToBigInteger().Mod(n);
			if (bigInteger3.SignValue != 0)
			{
				bigInteger4 = BigIntegers.ModOddInverse(n, bigInteger2).Multiply(bigInteger.Add(d.Multiply(bigInteger3))).Mod(n);
				if (bigInteger4.SignValue != 0)
				{
					break;
				}
			}
		}
		return new BigInteger[2] { bigInteger3, bigInteger4 };
	}

	public virtual bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
	{
		BigInteger n = key.Parameters.N;
		if (r.SignValue < 1 || s.SignValue < 1 || r.CompareTo(n) >= 0 || s.CompareTo(n) >= 0)
		{
			return false;
		}
		BigInteger bigInteger = CalculateE(n, message);
		BigInteger val = BigIntegers.ModOddInverseVar(n, s);
		BigInteger a = bigInteger.Multiply(val).Mod(n);
		BigInteger b = r.Multiply(val).Mod(n);
		ECPoint g = key.Parameters.G;
		ECPoint q = ((ECPublicKeyParameters)key).Q;
		ECPoint eCPoint = ECAlgorithms.SumOfTwoMultiplies(g, a, q, b);
		if (eCPoint.IsInfinity)
		{
			return false;
		}
		ECCurve curve = eCPoint.Curve;
		if (curve != null)
		{
			BigInteger cofactor = curve.Cofactor;
			if (cofactor != null && cofactor.CompareTo(Eight) <= 0)
			{
				ECFieldElement denominator = GetDenominator(curve.CoordinateSystem, eCPoint);
				if (denominator != null && !denominator.IsZero)
				{
					ECFieldElement xCoord = eCPoint.XCoord;
					while (curve.IsValidFieldElement(r))
					{
						ECFieldElement eCFieldElement = curve.FromBigInteger(r).Multiply(denominator);
						if (eCFieldElement.Equals(xCoord))
						{
							return true;
						}
						r = r.Add(n);
					}
					return false;
				}
			}
		}
		BigInteger bigInteger2 = eCPoint.Normalize().AffineXCoord.ToBigInteger().Mod(n);
		return bigInteger2.Equals(r);
	}

	protected virtual BigInteger CalculateE(BigInteger n, byte[] message)
	{
		int num = message.Length * 8;
		BigInteger bigInteger = new BigInteger(1, message);
		if (n.BitLength < num)
		{
			bigInteger = bigInteger.ShiftRight(num - n.BitLength);
		}
		return bigInteger;
	}

	protected virtual ECMultiplier CreateBasePointMultiplier()
	{
		return new FixedPointCombMultiplier();
	}

	protected virtual ECFieldElement GetDenominator(int coordinateSystem, ECPoint p)
	{
		switch (coordinateSystem)
		{
		case 1:
		case 6:
		case 7:
			return p.GetZCoord(0);
		case 2:
		case 3:
		case 4:
			return p.GetZCoord(0).Square();
		default:
			return null;
		}
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
