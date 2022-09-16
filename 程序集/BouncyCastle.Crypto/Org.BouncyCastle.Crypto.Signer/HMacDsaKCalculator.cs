using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class HMacDsaKCalculator : IDsaKCalculator
{
	private readonly HMac hMac;

	private readonly byte[] K;

	private readonly byte[] V;

	private BigInteger n;

	public virtual bool IsDeterministic => true;

	public HMacDsaKCalculator(IDigest digest)
	{
		hMac = new HMac(digest);
		V = new byte[hMac.GetMacSize()];
		K = new byte[hMac.GetMacSize()];
	}

	public virtual void Init(BigInteger n, SecureRandom random)
	{
		throw new InvalidOperationException("Operation not supported");
	}

	public void Init(BigInteger n, BigInteger d, byte[] message)
	{
		this.n = n;
		Arrays.Fill(V, 1);
		Arrays.Fill(K, 0);
		int unsignedByteLength = BigIntegers.GetUnsignedByteLength(n);
		byte[] array = new byte[unsignedByteLength];
		byte[] array2 = BigIntegers.AsUnsignedByteArray(d);
		Array.Copy(array2, 0, array, array.Length - array2.Length, array2.Length);
		byte[] array3 = new byte[unsignedByteLength];
		BigInteger bigInteger = BitsToInt(message);
		if (bigInteger.CompareTo(n) >= 0)
		{
			bigInteger = bigInteger.Subtract(n);
		}
		byte[] array4 = BigIntegers.AsUnsignedByteArray(bigInteger);
		Array.Copy(array4, 0, array3, array3.Length - array4.Length, array4.Length);
		hMac.Init(new KeyParameter(K));
		hMac.BlockUpdate(V, 0, V.Length);
		hMac.Update(0);
		hMac.BlockUpdate(array, 0, array.Length);
		hMac.BlockUpdate(array3, 0, array3.Length);
		hMac.DoFinal(K, 0);
		hMac.Init(new KeyParameter(K));
		hMac.BlockUpdate(V, 0, V.Length);
		hMac.DoFinal(V, 0);
		hMac.BlockUpdate(V, 0, V.Length);
		hMac.Update(1);
		hMac.BlockUpdate(array, 0, array.Length);
		hMac.BlockUpdate(array3, 0, array3.Length);
		hMac.DoFinal(K, 0);
		hMac.Init(new KeyParameter(K));
		hMac.BlockUpdate(V, 0, V.Length);
		hMac.DoFinal(V, 0);
	}

	public virtual BigInteger NextK()
	{
		byte[] array = new byte[BigIntegers.GetUnsignedByteLength(n)];
		BigInteger bigInteger;
		while (true)
		{
			int num;
			for (int i = 0; i < array.Length; i += num)
			{
				hMac.BlockUpdate(V, 0, V.Length);
				hMac.DoFinal(V, 0);
				num = System.Math.Min(array.Length - i, V.Length);
				Array.Copy(V, 0, array, i, num);
			}
			bigInteger = BitsToInt(array);
			if (bigInteger.SignValue > 0 && bigInteger.CompareTo(n) < 0)
			{
				break;
			}
			hMac.BlockUpdate(V, 0, V.Length);
			hMac.Update(0);
			hMac.DoFinal(K, 0);
			hMac.Init(new KeyParameter(K));
			hMac.BlockUpdate(V, 0, V.Length);
			hMac.DoFinal(V, 0);
		}
		return bigInteger;
	}

	private BigInteger BitsToInt(byte[] t)
	{
		BigInteger bigInteger = new BigInteger(1, t);
		if (t.Length * 8 > n.BitLength)
		{
			bigInteger = bigInteger.ShiftRight(t.Length * 8 - n.BitLength);
		}
		return bigInteger;
	}
}
