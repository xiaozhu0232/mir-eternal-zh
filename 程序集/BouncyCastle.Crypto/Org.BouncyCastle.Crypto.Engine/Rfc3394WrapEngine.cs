using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class Rfc3394WrapEngine : IWrapper
{
	private readonly IBlockCipher engine;

	private KeyParameter param;

	private bool forWrapping;

	private byte[] iv = new byte[8] { 166, 166, 166, 166, 166, 166, 166, 166 };

	public virtual string AlgorithmName => engine.AlgorithmName;

	public Rfc3394WrapEngine(IBlockCipher engine)
	{
		this.engine = engine;
	}

	public virtual void Init(bool forWrapping, ICipherParameters parameters)
	{
		this.forWrapping = forWrapping;
		if (parameters is ParametersWithRandom)
		{
			parameters = ((ParametersWithRandom)parameters).Parameters;
		}
		if (parameters is KeyParameter)
		{
			param = (KeyParameter)parameters;
		}
		else if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			byte[] iV = parametersWithIV.GetIV();
			if (iV.Length != 8)
			{
				throw new ArgumentException("IV length not equal to 8", "parameters");
			}
			iv = iV;
			param = (KeyParameter)parametersWithIV.Parameters;
		}
	}

	public virtual byte[] Wrap(byte[] input, int inOff, int inLen)
	{
		if (!forWrapping)
		{
			throw new InvalidOperationException("not set for wrapping");
		}
		int num = inLen / 8;
		if (num * 8 != inLen)
		{
			throw new DataLengthException("wrap data must be a multiple of 8 bytes");
		}
		byte[] array = new byte[inLen + iv.Length];
		byte[] array2 = new byte[8 + iv.Length];
		Array.Copy(iv, 0, array, 0, iv.Length);
		Array.Copy(input, inOff, array, iv.Length, inLen);
		engine.Init(forEncryption: true, param);
		for (int i = 0; i != 6; i++)
		{
			for (int j = 1; j <= num; j++)
			{
				Array.Copy(array, 0, array2, 0, iv.Length);
				Array.Copy(array, 8 * j, array2, iv.Length, 8);
				engine.ProcessBlock(array2, 0, array2, 0);
				int num2 = num * i + j;
				int num3 = 1;
				while (num2 != 0)
				{
					byte b = (byte)num2;
					byte[] array3;
					byte[] array4 = (array3 = array2);
					int num4 = iv.Length - num3;
					nint num5 = num4;
					array4[num4] = (byte)(array3[num5] ^ b);
					num2 = (int)((uint)num2 >> 8);
					num3++;
				}
				Array.Copy(array2, 0, array, 0, 8);
				Array.Copy(array2, 8, array, 8 * j, 8);
			}
		}
		return array;
	}

	public virtual byte[] Unwrap(byte[] input, int inOff, int inLen)
	{
		if (forWrapping)
		{
			throw new InvalidOperationException("not set for unwrapping");
		}
		int num = inLen / 8;
		if (num * 8 != inLen)
		{
			throw new InvalidCipherTextException("unwrap data must be a multiple of 8 bytes");
		}
		byte[] array = new byte[inLen - iv.Length];
		byte[] array2 = new byte[iv.Length];
		byte[] array3 = new byte[8 + iv.Length];
		Array.Copy(input, inOff, array2, 0, iv.Length);
		Array.Copy(input, inOff + iv.Length, array, 0, inLen - iv.Length);
		engine.Init(forEncryption: false, param);
		num--;
		for (int num2 = 5; num2 >= 0; num2--)
		{
			for (int num3 = num; num3 >= 1; num3--)
			{
				Array.Copy(array2, 0, array3, 0, iv.Length);
				Array.Copy(array, 8 * (num3 - 1), array3, iv.Length, 8);
				int num4 = num * num2 + num3;
				int num5 = 1;
				while (num4 != 0)
				{
					byte b = (byte)num4;
					byte[] array4;
					byte[] array5 = (array4 = array3);
					int num6 = iv.Length - num5;
					nint num7 = num6;
					array5[num6] = (byte)(array4[num7] ^ b);
					num4 = (int)((uint)num4 >> 8);
					num5++;
				}
				engine.ProcessBlock(array3, 0, array3, 0);
				Array.Copy(array3, 0, array2, 0, 8);
				Array.Copy(array3, 8, array, 8 * (num3 - 1), 8);
			}
		}
		if (!Arrays.ConstantTimeAreEqual(array2, iv))
		{
			throw new InvalidCipherTextException("checksum failed");
		}
		return array;
	}
}
