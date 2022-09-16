using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators;

public class Gost3410ParametersGenerator
{
	private int size;

	private int typeproc;

	private SecureRandom init_random;

	public void Init(int size, int typeProcedure, SecureRandom random)
	{
		this.size = size;
		typeproc = typeProcedure;
		init_random = random;
	}

	private int procedure_A(int x0, int c, BigInteger[] pq, int size)
	{
		while (x0 < 0 || x0 > 65536)
		{
			x0 = init_random.NextInt() / 32768;
		}
		while (c < 0 || c > 65536 || c / 2 == 0)
		{
			c = init_random.NextInt() / 32768 + 1;
		}
		BigInteger value = BigInteger.ValueOf(c);
		BigInteger val = BigInteger.ValueOf(19381L);
		BigInteger[] array = new BigInteger[1] { BigInteger.ValueOf(x0) };
		int[] array2 = new int[1] { size };
		int num = 0;
		for (int i = 0; array2[i] >= 17; i++)
		{
			int[] array3 = new int[array2.Length + 1];
			Array.Copy(array2, 0, array3, 0, array2.Length);
			array2 = new int[array3.Length];
			Array.Copy(array3, 0, array2, 0, array3.Length);
			array2[i + 1] = array2[i] / 2;
			num = i + 1;
		}
		BigInteger[] array4 = new BigInteger[num + 1];
		array4[num] = new BigInteger("8003", 16);
		int num2 = num - 1;
		for (int j = 0; j < num; j++)
		{
			int num3 = array2[num2] / 16;
			while (true)
			{
				BigInteger[] array5 = new BigInteger[array.Length];
				Array.Copy(array, 0, array5, 0, array.Length);
				array = new BigInteger[num3 + 1];
				Array.Copy(array5, 0, array, 0, array5.Length);
				for (int k = 0; k < num3; k++)
				{
					array[k + 1] = array[k].Multiply(val).Add(value).Mod(BigInteger.Two.Pow(16));
				}
				BigInteger bigInteger = BigInteger.Zero;
				for (int l = 0; l < num3; l++)
				{
					bigInteger = bigInteger.Add(array[l].ShiftLeft(16 * l));
				}
				array[0] = array[num3];
				BigInteger bigInteger2 = BigInteger.One.ShiftLeft(array2[num2] - 1).Divide(array4[num2 + 1]).Add(bigInteger.ShiftLeft(array2[num2] - 1).Divide(array4[num2 + 1].ShiftLeft(16 * num3)));
				if (bigInteger2.TestBit(0))
				{
					bigInteger2 = bigInteger2.Add(BigInteger.One);
				}
				while (true)
				{
					BigInteger bigInteger3 = bigInteger2.Multiply(array4[num2 + 1]);
					if (bigInteger3.BitLength > array2[num2])
					{
						break;
					}
					array4[num2] = bigInteger3.Add(BigInteger.One);
					if (BigInteger.Two.ModPow(bigInteger3, array4[num2]).CompareTo(BigInteger.One) == 0 && BigInteger.Two.ModPow(bigInteger2, array4[num2]).CompareTo(BigInteger.One) != 0)
					{
						goto end_IL_0106;
					}
					bigInteger2 = bigInteger2.Add(BigInteger.Two);
				}
				continue;
				end_IL_0106:
				break;
			}
			if (--num2 < 0)
			{
				pq[0] = array4[0];
				pq[1] = array4[1];
				return array[0].IntValue;
			}
		}
		return array[0].IntValue;
	}

	private long procedure_Aa(long x0, long c, BigInteger[] pq, int size)
	{
		while (x0 < 0 || x0 > 4294967296L)
		{
			x0 = init_random.NextInt() * 2;
		}
		while (c < 0 || c > 4294967296L || c / 2 == 0)
		{
			c = init_random.NextInt() * 2 + 1;
		}
		BigInteger value = BigInteger.ValueOf(c);
		BigInteger val = BigInteger.ValueOf(97781173L);
		BigInteger[] array = new BigInteger[1] { BigInteger.ValueOf(x0) };
		int[] array2 = new int[1] { size };
		int num = 0;
		for (int i = 0; array2[i] >= 33; i++)
		{
			int[] array3 = new int[array2.Length + 1];
			Array.Copy(array2, 0, array3, 0, array2.Length);
			array2 = new int[array3.Length];
			Array.Copy(array3, 0, array2, 0, array3.Length);
			array2[i + 1] = array2[i] / 2;
			num = i + 1;
		}
		BigInteger[] array4 = new BigInteger[num + 1];
		array4[num] = new BigInteger("8000000B", 16);
		int num2 = num - 1;
		for (int j = 0; j < num; j++)
		{
			int num3 = array2[num2] / 32;
			while (true)
			{
				BigInteger[] array5 = new BigInteger[array.Length];
				Array.Copy(array, 0, array5, 0, array.Length);
				array = new BigInteger[num3 + 1];
				Array.Copy(array5, 0, array, 0, array5.Length);
				for (int k = 0; k < num3; k++)
				{
					array[k + 1] = array[k].Multiply(val).Add(value).Mod(BigInteger.Two.Pow(32));
				}
				BigInteger bigInteger = BigInteger.Zero;
				for (int l = 0; l < num3; l++)
				{
					bigInteger = bigInteger.Add(array[l].ShiftLeft(32 * l));
				}
				array[0] = array[num3];
				BigInteger bigInteger2 = BigInteger.One.ShiftLeft(array2[num2] - 1).Divide(array4[num2 + 1]).Add(bigInteger.ShiftLeft(array2[num2] - 1).Divide(array4[num2 + 1].ShiftLeft(32 * num3)));
				if (bigInteger2.TestBit(0))
				{
					bigInteger2 = bigInteger2.Add(BigInteger.One);
				}
				while (true)
				{
					BigInteger bigInteger3 = bigInteger2.Multiply(array4[num2 + 1]);
					if (bigInteger3.BitLength > array2[num2])
					{
						break;
					}
					array4[num2] = bigInteger3.Add(BigInteger.One);
					if (BigInteger.Two.ModPow(bigInteger3, array4[num2]).CompareTo(BigInteger.One) == 0 && BigInteger.Two.ModPow(bigInteger2, array4[num2]).CompareTo(BigInteger.One) != 0)
					{
						goto end_IL_010b;
					}
					bigInteger2 = bigInteger2.Add(BigInteger.Two);
				}
				continue;
				end_IL_010b:
				break;
			}
			if (--num2 < 0)
			{
				pq[0] = array4[0];
				pq[1] = array4[1];
				return array[0].LongValue;
			}
		}
		return array[0].LongValue;
	}

	private void procedure_B(int x0, int c, BigInteger[] pq)
	{
		while (x0 < 0 || x0 > 65536)
		{
			x0 = init_random.NextInt() / 32768;
		}
		while (c < 0 || c > 65536 || c / 2 == 0)
		{
			c = init_random.NextInt() / 32768 + 1;
		}
		BigInteger[] array = new BigInteger[2];
		BigInteger bigInteger = null;
		BigInteger bigInteger2 = null;
		BigInteger bigInteger3 = null;
		BigInteger value = BigInteger.ValueOf(c);
		BigInteger val = BigInteger.ValueOf(19381L);
		x0 = procedure_A(x0, c, array, 256);
		bigInteger = array[0];
		x0 = procedure_A(x0, c, array, 512);
		bigInteger2 = array[0];
		BigInteger[] array2 = new BigInteger[65];
		array2[0] = BigInteger.ValueOf(x0);
		BigInteger bigInteger4 = bigInteger.Multiply(bigInteger2);
		while (true)
		{
			for (int i = 0; i < 64; i++)
			{
				array2[i + 1] = array2[i].Multiply(val).Add(value).Mod(BigInteger.Two.Pow(16));
			}
			BigInteger bigInteger5 = BigInteger.Zero;
			for (int j = 0; j < 64; j++)
			{
				bigInteger5 = bigInteger5.Add(array2[j].ShiftLeft(16 * j));
			}
			array2[0] = array2[64];
			BigInteger bigInteger6 = BigInteger.One.ShiftLeft(1023).Divide(bigInteger4).Add(bigInteger5.ShiftLeft(1023).Divide(bigInteger4.ShiftLeft(1024)));
			if (bigInteger6.TestBit(0))
			{
				bigInteger6 = bigInteger6.Add(BigInteger.One);
			}
			while (true)
			{
				BigInteger bigInteger7 = bigInteger4.Multiply(bigInteger6);
				if (bigInteger7.BitLength > 1024)
				{
					break;
				}
				bigInteger3 = bigInteger7.Add(BigInteger.One);
				if (BigInteger.Two.ModPow(bigInteger7, bigInteger3).CompareTo(BigInteger.One) == 0 && BigInteger.Two.ModPow(bigInteger.Multiply(bigInteger6), bigInteger3).CompareTo(BigInteger.One) != 0)
				{
					pq[0] = bigInteger3;
					pq[1] = bigInteger;
					return;
				}
				bigInteger6 = bigInteger6.Add(BigInteger.Two);
			}
		}
	}

	private void procedure_Bb(long x0, long c, BigInteger[] pq)
	{
		while (x0 < 0 || x0 > 4294967296L)
		{
			x0 = init_random.NextInt() * 2;
		}
		while (c < 0 || c > 4294967296L || c / 2 == 0)
		{
			c = init_random.NextInt() * 2 + 1;
		}
		BigInteger[] array = new BigInteger[2];
		BigInteger bigInteger = null;
		BigInteger bigInteger2 = null;
		BigInteger bigInteger3 = null;
		BigInteger value = BigInteger.ValueOf(c);
		BigInteger val = BigInteger.ValueOf(97781173L);
		x0 = procedure_Aa(x0, c, array, 256);
		bigInteger = array[0];
		x0 = procedure_Aa(x0, c, array, 512);
		bigInteger2 = array[0];
		BigInteger[] array2 = new BigInteger[33];
		array2[0] = BigInteger.ValueOf(x0);
		BigInteger bigInteger4 = bigInteger.Multiply(bigInteger2);
		while (true)
		{
			for (int i = 0; i < 32; i++)
			{
				array2[i + 1] = array2[i].Multiply(val).Add(value).Mod(BigInteger.Two.Pow(32));
			}
			BigInteger bigInteger5 = BigInteger.Zero;
			for (int j = 0; j < 32; j++)
			{
				bigInteger5 = bigInteger5.Add(array2[j].ShiftLeft(32 * j));
			}
			array2[0] = array2[32];
			BigInteger bigInteger6 = BigInteger.One.ShiftLeft(1023).Divide(bigInteger4).Add(bigInteger5.ShiftLeft(1023).Divide(bigInteger4.ShiftLeft(1024)));
			if (bigInteger6.TestBit(0))
			{
				bigInteger6 = bigInteger6.Add(BigInteger.One);
			}
			while (true)
			{
				BigInteger bigInteger7 = bigInteger4.Multiply(bigInteger6);
				if (bigInteger7.BitLength > 1024)
				{
					break;
				}
				bigInteger3 = bigInteger7.Add(BigInteger.One);
				if (BigInteger.Two.ModPow(bigInteger7, bigInteger3).CompareTo(BigInteger.One) == 0 && BigInteger.Two.ModPow(bigInteger.Multiply(bigInteger6), bigInteger3).CompareTo(BigInteger.One) != 0)
				{
					pq[0] = bigInteger3;
					pq[1] = bigInteger;
					return;
				}
				bigInteger6 = bigInteger6.Add(BigInteger.Two);
			}
		}
	}

	private BigInteger procedure_C(BigInteger p, BigInteger q)
	{
		BigInteger bigInteger = p.Subtract(BigInteger.One);
		BigInteger e = bigInteger.Divide(q);
		BigInteger bigInteger3;
		while (true)
		{
			BigInteger bigInteger2 = new BigInteger(p.BitLength, init_random);
			if (bigInteger2.CompareTo(BigInteger.One) > 0 && bigInteger2.CompareTo(bigInteger) < 0)
			{
				bigInteger3 = bigInteger2.ModPow(e, p);
				if (bigInteger3.CompareTo(BigInteger.One) != 0)
				{
					break;
				}
			}
		}
		return bigInteger3;
	}

	public Gost3410Parameters GenerateParameters()
	{
		BigInteger[] array = new BigInteger[2];
		BigInteger bigInteger = null;
		BigInteger bigInteger2 = null;
		BigInteger bigInteger3 = null;
		if (typeproc == 1)
		{
			int x = init_random.NextInt();
			int c = init_random.NextInt();
			switch (size)
			{
			case 512:
				procedure_A(x, c, array, 512);
				break;
			case 1024:
				procedure_B(x, c, array);
				break;
			default:
				throw new ArgumentException("Ooops! key size 512 or 1024 bit.");
			}
			bigInteger2 = array[0];
			bigInteger = array[1];
			bigInteger3 = procedure_C(bigInteger2, bigInteger);
			return new Gost3410Parameters(bigInteger2, bigInteger, bigInteger3, new Gost3410ValidationParameters(x, c));
		}
		long num = init_random.NextLong();
		long num2 = init_random.NextLong();
		switch (size)
		{
		case 512:
			procedure_Aa(num, num2, array, 512);
			break;
		case 1024:
			procedure_Bb(num, num2, array);
			break;
		default:
			throw new InvalidOperationException("Ooops! key size 512 or 1024 bit.");
		}
		bigInteger2 = array[0];
		bigInteger = array[1];
		bigInteger3 = procedure_C(bigInteger2, bigInteger);
		return new Gost3410Parameters(bigInteger2, bigInteger, bigInteger3, new Gost3410ValidationParameters(num, num2));
	}
}
