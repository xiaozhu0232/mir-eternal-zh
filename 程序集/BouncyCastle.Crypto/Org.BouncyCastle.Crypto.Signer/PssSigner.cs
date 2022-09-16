using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class PssSigner : ISigner
{
	public const byte TrailerImplicit = 188;

	private readonly IDigest contentDigest1;

	private readonly IDigest contentDigest2;

	private readonly IDigest mgfDigest;

	private readonly IAsymmetricBlockCipher cipher;

	private SecureRandom random;

	private int hLen;

	private int mgfhLen;

	private int sLen;

	private bool sSet;

	private int emBits;

	private byte[] salt;

	private byte[] mDash;

	private byte[] block;

	private byte trailer;

	public virtual string AlgorithmName => mgfDigest.AlgorithmName + "withRSAandMGF1";

	public static PssSigner CreateRawSigner(IAsymmetricBlockCipher cipher, IDigest digest)
	{
		return new PssSigner(cipher, new NullDigest(), digest, digest, digest.GetDigestSize(), null, 188);
	}

	public static PssSigner CreateRawSigner(IAsymmetricBlockCipher cipher, IDigest contentDigest, IDigest mgfDigest, int saltLen, byte trailer)
	{
		return new PssSigner(cipher, new NullDigest(), contentDigest, mgfDigest, saltLen, null, trailer);
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest digest)
		: this(cipher, digest, digest.GetDigestSize())
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest digest, int saltLen)
		: this(cipher, digest, saltLen, 188)
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest digest, byte[] salt)
		: this(cipher, digest, digest, digest, salt.Length, salt, 188)
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest contentDigest, IDigest mgfDigest, int saltLen)
		: this(cipher, contentDigest, mgfDigest, saltLen, 188)
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest contentDigest, IDigest mgfDigest, byte[] salt)
		: this(cipher, contentDigest, contentDigest, mgfDigest, salt.Length, salt, 188)
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest digest, int saltLen, byte trailer)
		: this(cipher, digest, digest, saltLen, trailer)
	{
	}

	public PssSigner(IAsymmetricBlockCipher cipher, IDigest contentDigest, IDigest mgfDigest, int saltLen, byte trailer)
		: this(cipher, contentDigest, contentDigest, mgfDigest, saltLen, null, trailer)
	{
	}

	private PssSigner(IAsymmetricBlockCipher cipher, IDigest contentDigest1, IDigest contentDigest2, IDigest mgfDigest, int saltLen, byte[] salt, byte trailer)
	{
		this.cipher = cipher;
		this.contentDigest1 = contentDigest1;
		this.contentDigest2 = contentDigest2;
		this.mgfDigest = mgfDigest;
		hLen = contentDigest2.GetDigestSize();
		mgfhLen = mgfDigest.GetDigestSize();
		sLen = saltLen;
		sSet = salt != null;
		if (sSet)
		{
			this.salt = salt;
		}
		else
		{
			this.salt = new byte[saltLen];
		}
		mDash = new byte[8 + saltLen + hLen];
		this.trailer = trailer;
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		if (parameters is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
			parameters = parametersWithRandom.Parameters;
			random = parametersWithRandom.Random;
		}
		else if (forSigning)
		{
			random = new SecureRandom();
		}
		cipher.Init(forSigning, parameters);
		RsaKeyParameters rsaKeyParameters = ((!(parameters is RsaBlindingParameters)) ? ((RsaKeyParameters)parameters) : ((RsaBlindingParameters)parameters).PublicKey);
		emBits = rsaKeyParameters.Modulus.BitLength - 1;
		if (emBits < 8 * hLen + 8 * sLen + 9)
		{
			throw new ArgumentException("key too small for specified hash and salt lengths");
		}
		block = new byte[(emBits + 7) / 8];
	}

	private void ClearBlock(byte[] block)
	{
		Array.Clear(block, 0, block.Length);
	}

	public virtual void Update(byte input)
	{
		contentDigest1.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int length)
	{
		contentDigest1.BlockUpdate(input, inOff, length);
	}

	public virtual void Reset()
	{
		contentDigest1.Reset();
	}

	public virtual byte[] GenerateSignature()
	{
		contentDigest1.DoFinal(mDash, mDash.Length - hLen - sLen);
		if (sLen != 0)
		{
			if (!sSet)
			{
				random.NextBytes(salt);
			}
			salt.CopyTo(mDash, mDash.Length - sLen);
		}
		byte[] array = new byte[hLen];
		contentDigest2.BlockUpdate(mDash, 0, mDash.Length);
		contentDigest2.DoFinal(array, 0);
		block[block.Length - sLen - 1 - hLen - 1] = 1;
		salt.CopyTo(block, block.Length - sLen - hLen - 1);
		byte[] array2 = MaskGeneratorFunction1(array, 0, array.Length, block.Length - hLen - 1);
		byte[] array3;
		for (int i = 0; i != array2.Length; i++)
		{
			byte[] array4 = (array3 = block);
			int num = i;
			nint num2 = num;
			array4[num] = (byte)(array3[num2] ^ array2[i]);
		}
		array.CopyTo(block, block.Length - hLen - 1);
		uint num3 = 255u >> block.Length * 8 - emBits;
		(array3 = block)[0] = (byte)(array3[0] & (byte)num3);
		block[block.Length - 1] = trailer;
		byte[] result = cipher.ProcessBlock(block, 0, block.Length);
		ClearBlock(block);
		return result;
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		contentDigest1.DoFinal(mDash, mDash.Length - hLen - sLen);
		byte[] array = cipher.ProcessBlock(signature, 0, signature.Length);
		Arrays.Fill(block, 0, block.Length - array.Length, 0);
		array.CopyTo(block, block.Length - array.Length);
		uint num = 255u >> block.Length * 8 - emBits;
		if (block[0] != (byte)(block[0] & num) || block[block.Length - 1] != trailer)
		{
			ClearBlock(block);
			return false;
		}
		byte[] array2 = MaskGeneratorFunction1(block, block.Length - hLen - 1, hLen, block.Length - hLen - 1);
		byte[] array3;
		for (int i = 0; i != array2.Length; i++)
		{
			byte[] array4 = (array3 = block);
			int num2 = i;
			nint num3 = num2;
			array4[num2] = (byte)(array3[num3] ^ array2[i]);
		}
		(array3 = block)[0] = (byte)(array3[0] & (byte)num);
		for (int j = 0; j != block.Length - hLen - sLen - 2; j++)
		{
			if (block[j] != 0)
			{
				ClearBlock(block);
				return false;
			}
		}
		if (block[block.Length - hLen - sLen - 2] != 1)
		{
			ClearBlock(block);
			return false;
		}
		if (sSet)
		{
			Array.Copy(salt, 0, mDash, mDash.Length - sLen, sLen);
		}
		else
		{
			Array.Copy(block, block.Length - sLen - hLen - 1, mDash, mDash.Length - sLen, sLen);
		}
		contentDigest2.BlockUpdate(mDash, 0, mDash.Length);
		contentDigest2.DoFinal(mDash, mDash.Length - hLen);
		int num4 = block.Length - hLen - 1;
		for (int k = mDash.Length - hLen; k != mDash.Length; k++)
		{
			if ((block[num4] ^ mDash[k]) != 0)
			{
				ClearBlock(mDash);
				ClearBlock(block);
				return false;
			}
			num4++;
		}
		ClearBlock(mDash);
		ClearBlock(block);
		return true;
	}

	private void ItoOSP(int i, byte[] sp)
	{
		sp[0] = (byte)((uint)i >> 24);
		sp[1] = (byte)((uint)i >> 16);
		sp[2] = (byte)((uint)i >> 8);
		sp[3] = (byte)i;
	}

	private byte[] MaskGeneratorFunction1(byte[] Z, int zOff, int zLen, int length)
	{
		byte[] array = new byte[length];
		byte[] array2 = new byte[mgfhLen];
		byte[] array3 = new byte[4];
		int i = 0;
		mgfDigest.Reset();
		for (; i < length / mgfhLen; i++)
		{
			ItoOSP(i, array3);
			mgfDigest.BlockUpdate(Z, zOff, zLen);
			mgfDigest.BlockUpdate(array3, 0, array3.Length);
			mgfDigest.DoFinal(array2, 0);
			array2.CopyTo(array, i * mgfhLen);
		}
		if (i * mgfhLen < length)
		{
			ItoOSP(i, array3);
			mgfDigest.BlockUpdate(Z, zOff, zLen);
			mgfDigest.BlockUpdate(array3, 0, array3.Length);
			mgfDigest.DoFinal(array2, 0);
			Array.Copy(array2, 0, array, i * mgfhLen, array.Length - i * mgfhLen);
		}
		return array;
	}
}
