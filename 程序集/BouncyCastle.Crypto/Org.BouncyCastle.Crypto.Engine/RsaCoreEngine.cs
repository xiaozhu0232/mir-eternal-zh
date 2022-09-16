using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Engines;

public class RsaCoreEngine : IRsa
{
	private RsaKeyParameters key;

	private bool forEncryption;

	private int bitSize;

	private void CheckInitialised()
	{
		if (key == null)
		{
			throw new InvalidOperationException("RSA engine not initialised");
		}
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (parameters is ParametersWithRandom)
		{
			parameters = ((ParametersWithRandom)parameters).Parameters;
		}
		if (!(parameters is RsaKeyParameters))
		{
			throw new InvalidKeyException("Not an RSA key");
		}
		key = (RsaKeyParameters)parameters;
		this.forEncryption = forEncryption;
		bitSize = key.Modulus.BitLength;
	}

	public virtual int GetInputBlockSize()
	{
		CheckInitialised();
		if (forEncryption)
		{
			return (bitSize - 1) / 8;
		}
		return (bitSize + 7) / 8;
	}

	public virtual int GetOutputBlockSize()
	{
		CheckInitialised();
		if (forEncryption)
		{
			return (bitSize + 7) / 8;
		}
		return (bitSize - 1) / 8;
	}

	public virtual BigInteger ConvertInput(byte[] inBuf, int inOff, int inLen)
	{
		CheckInitialised();
		int num = (bitSize + 7) / 8;
		if (inLen > num)
		{
			throw new DataLengthException("input too large for RSA cipher.");
		}
		BigInteger bigInteger = new BigInteger(1, inBuf, inOff, inLen);
		if (bigInteger.CompareTo(key.Modulus) >= 0)
		{
			throw new DataLengthException("input too large for RSA cipher.");
		}
		return bigInteger;
	}

	public virtual byte[] ConvertOutput(BigInteger result)
	{
		CheckInitialised();
		byte[] array = result.ToByteArrayUnsigned();
		if (forEncryption)
		{
			int outputBlockSize = GetOutputBlockSize();
			if (array.Length < outputBlockSize)
			{
				byte[] array2 = new byte[outputBlockSize];
				array.CopyTo(array2, array2.Length - array.Length);
				array = array2;
			}
		}
		return array;
	}

	public virtual BigInteger ProcessBlock(BigInteger input)
	{
		CheckInitialised();
		if (key is RsaPrivateCrtKeyParameters)
		{
			RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)key;
			BigInteger p = rsaPrivateCrtKeyParameters.P;
			BigInteger q = rsaPrivateCrtKeyParameters.Q;
			BigInteger dP = rsaPrivateCrtKeyParameters.DP;
			BigInteger dQ = rsaPrivateCrtKeyParameters.DQ;
			BigInteger qInv = rsaPrivateCrtKeyParameters.QInv;
			BigInteger bigInteger = input.Remainder(p).ModPow(dP, p);
			BigInteger bigInteger2 = input.Remainder(q).ModPow(dQ, q);
			BigInteger bigInteger3 = bigInteger.Subtract(bigInteger2);
			bigInteger3 = bigInteger3.Multiply(qInv);
			bigInteger3 = bigInteger3.Mod(p);
			BigInteger bigInteger4 = bigInteger3.Multiply(q);
			return bigInteger4.Add(bigInteger2);
		}
		return input.ModPow(key.Exponent, key.Modulus);
	}
}
