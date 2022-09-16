using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Generators;

public class KdfCounterBytesGenerator : IMacDerivationFunction, IDerivationFunction
{
	private static readonly BigInteger IntegerMax = BigInteger.ValueOf(2147483647L);

	private static readonly BigInteger Two = BigInteger.Two;

	private readonly IMac prf;

	private readonly int h;

	private byte[] fixedInputDataCtrPrefix;

	private byte[] fixedInputData_afterCtr;

	private int maxSizeExcl;

	private byte[] ios;

	private int generatedBytes;

	private byte[] k;

	public IDigest Digest
	{
		get
		{
			if (!(prf is HMac))
			{
				return null;
			}
			return ((HMac)prf).GetUnderlyingDigest();
		}
	}

	public KdfCounterBytesGenerator(IMac prf)
	{
		this.prf = prf;
		h = prf.GetMacSize();
		k = new byte[h];
	}

	public void Init(IDerivationParameters param)
	{
		if (!(param is KdfCounterParameters kdfCounterParameters))
		{
			throw new ArgumentException("Wrong type of arguments given");
		}
		prf.Init(new KeyParameter(kdfCounterParameters.Ki));
		fixedInputDataCtrPrefix = kdfCounterParameters.FixedInputDataCounterPrefix;
		fixedInputData_afterCtr = kdfCounterParameters.FixedInputDataCounterSuffix;
		int r = kdfCounterParameters.R;
		ios = new byte[r / 8];
		BigInteger bigInteger = Two.Pow(r).Multiply(BigInteger.ValueOf(h));
		maxSizeExcl = ((bigInteger.CompareTo(IntegerMax) == 1) ? int.MaxValue : bigInteger.IntValue);
		generatedBytes = 0;
	}

	public IMac GetMac()
	{
		return prf;
	}

	public int GenerateBytes(byte[] output, int outOff, int length)
	{
		int num = generatedBytes + length;
		if (num < 0 || num >= maxSizeExcl)
		{
			throw new DataLengthException("Current KDFCTR may only be used for " + maxSizeExcl + " bytes");
		}
		if (generatedBytes % h == 0)
		{
			generateNext();
		}
		int num2 = length;
		int sourceIndex = generatedBytes % h;
		int val = h - generatedBytes % h;
		int num3 = System.Math.Min(val, num2);
		Array.Copy(k, sourceIndex, output, outOff, num3);
		generatedBytes += num3;
		num2 -= num3;
		outOff += num3;
		while (num2 > 0)
		{
			generateNext();
			num3 = System.Math.Min(h, num2);
			Array.Copy(k, 0, output, outOff, num3);
			generatedBytes += num3;
			num2 -= num3;
			outOff += num3;
		}
		return length;
	}

	private void generateNext()
	{
		int num = generatedBytes / h + 1;
		switch (ios.Length)
		{
		case 4:
			ios[0] = (byte)(num >> 24);
			goto case 3;
		case 3:
			ios[ios.Length - 3] = (byte)(num >> 16);
			goto case 2;
		case 2:
			ios[ios.Length - 2] = (byte)(num >> 8);
			goto case 1;
		case 1:
			ios[ios.Length - 1] = (byte)num;
			prf.BlockUpdate(fixedInputDataCtrPrefix, 0, fixedInputDataCtrPrefix.Length);
			prf.BlockUpdate(ios, 0, ios.Length);
			prf.BlockUpdate(fixedInputData_afterCtr, 0, fixedInputData_afterCtr.Length);
			prf.DoFinal(k, 0);
			break;
		default:
			throw new InvalidOperationException("Unsupported size of counter i");
		}
	}
}
