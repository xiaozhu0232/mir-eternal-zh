using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class DesEdeWrapEngine : IWrapper
{
	private CbcBlockCipher engine;

	private KeyParameter param;

	private ParametersWithIV paramPlusIV;

	private byte[] iv;

	private bool forWrapping;

	private static readonly byte[] IV2 = new byte[8] { 74, 221, 162, 44, 121, 232, 33, 5 };

	private readonly IDigest sha1 = new Sha1Digest();

	private readonly byte[] digest = new byte[20];

	public virtual string AlgorithmName => "DESede";

	public virtual void Init(bool forWrapping, ICipherParameters parameters)
	{
		this.forWrapping = forWrapping;
		engine = new CbcBlockCipher(new DesEdeEngine());
		SecureRandom secureRandom;
		if (parameters is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
			parameters = parametersWithRandom.Parameters;
			secureRandom = parametersWithRandom.Random;
		}
		else
		{
			secureRandom = new SecureRandom();
		}
		if (parameters is KeyParameter)
		{
			param = (KeyParameter)parameters;
			if (this.forWrapping)
			{
				iv = new byte[8];
				secureRandom.NextBytes(iv);
				paramPlusIV = new ParametersWithIV(param, iv);
			}
		}
		else if (parameters is ParametersWithIV)
		{
			if (!forWrapping)
			{
				throw new ArgumentException("You should not supply an IV for unwrapping");
			}
			paramPlusIV = (ParametersWithIV)parameters;
			iv = paramPlusIV.GetIV();
			param = (KeyParameter)paramPlusIV.Parameters;
			if (iv.Length != 8)
			{
				throw new ArgumentException("IV is not 8 octets", "parameters");
			}
		}
	}

	public virtual byte[] Wrap(byte[] input, int inOff, int length)
	{
		if (!forWrapping)
		{
			throw new InvalidOperationException("Not initialized for wrapping");
		}
		byte[] array = new byte[length];
		Array.Copy(input, inOff, array, 0, length);
		byte[] array2 = CalculateCmsKeyChecksum(array);
		byte[] array3 = new byte[array.Length + array2.Length];
		Array.Copy(array, 0, array3, 0, array.Length);
		Array.Copy(array2, 0, array3, array.Length, array2.Length);
		int blockSize = engine.GetBlockSize();
		if (array3.Length % blockSize != 0)
		{
			throw new InvalidOperationException("Not multiple of block length");
		}
		engine.Init(forEncryption: true, paramPlusIV);
		byte[] array4 = new byte[array3.Length];
		for (int i = 0; i != array3.Length; i += blockSize)
		{
			engine.ProcessBlock(array3, i, array4, i);
		}
		byte[] array5 = new byte[iv.Length + array4.Length];
		Array.Copy(iv, 0, array5, 0, iv.Length);
		Array.Copy(array4, 0, array5, iv.Length, array4.Length);
		byte[] array6 = reverse(array5);
		ParametersWithIV parameters = new ParametersWithIV(param, IV2);
		engine.Init(forEncryption: true, parameters);
		for (int j = 0; j != array6.Length; j += blockSize)
		{
			engine.ProcessBlock(array6, j, array6, j);
		}
		return array6;
	}

	public virtual byte[] Unwrap(byte[] input, int inOff, int length)
	{
		if (forWrapping)
		{
			throw new InvalidOperationException("Not set for unwrapping");
		}
		if (input == null)
		{
			throw new InvalidCipherTextException("Null pointer as ciphertext");
		}
		int blockSize = engine.GetBlockSize();
		if (length % blockSize != 0)
		{
			throw new InvalidCipherTextException("Ciphertext not multiple of " + blockSize);
		}
		ParametersWithIV parameters = new ParametersWithIV(param, IV2);
		engine.Init(forEncryption: false, parameters);
		byte[] array = new byte[length];
		for (int i = 0; i != array.Length; i += blockSize)
		{
			engine.ProcessBlock(input, inOff + i, array, i);
		}
		byte[] array2 = reverse(array);
		iv = new byte[8];
		byte[] array3 = new byte[array2.Length - 8];
		Array.Copy(array2, 0, iv, 0, 8);
		Array.Copy(array2, 8, array3, 0, array2.Length - 8);
		paramPlusIV = new ParametersWithIV(param, iv);
		engine.Init(forEncryption: false, paramPlusIV);
		byte[] array4 = new byte[array3.Length];
		for (int j = 0; j != array4.Length; j += blockSize)
		{
			engine.ProcessBlock(array3, j, array4, j);
		}
		byte[] array5 = new byte[array4.Length - 8];
		byte[] array6 = new byte[8];
		Array.Copy(array4, 0, array5, 0, array4.Length - 8);
		Array.Copy(array4, array4.Length - 8, array6, 0, 8);
		if (!CheckCmsKeyChecksum(array5, array6))
		{
			throw new InvalidCipherTextException("Checksum inside ciphertext is corrupted");
		}
		return array5;
	}

	private byte[] CalculateCmsKeyChecksum(byte[] key)
	{
		sha1.BlockUpdate(key, 0, key.Length);
		sha1.DoFinal(digest, 0);
		byte[] array = new byte[8];
		Array.Copy(digest, 0, array, 0, 8);
		return array;
	}

	private bool CheckCmsKeyChecksum(byte[] key, byte[] checksum)
	{
		return Arrays.ConstantTimeAreEqual(CalculateCmsKeyChecksum(key), checksum);
	}

	private static byte[] reverse(byte[] bs)
	{
		byte[] array = new byte[bs.Length];
		for (int i = 0; i < bs.Length; i++)
		{
			array[i] = bs[bs.Length - (i + 1)];
		}
		return array;
	}
}
