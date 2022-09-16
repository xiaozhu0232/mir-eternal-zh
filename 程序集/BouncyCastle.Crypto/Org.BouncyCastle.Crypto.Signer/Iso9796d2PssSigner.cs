using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class Iso9796d2PssSigner : ISignerWithRecovery, ISigner
{
	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerImplicit = 188;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerRipeMD160 = 12748;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerRipeMD128 = 13004;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerSha1 = 13260;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerSha256 = 13516;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerSha512 = 13772;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerSha384 = 14028;

	[Obsolete("Use 'IsoTrailers' instead")]
	public const int TrailerWhirlpool = 14284;

	private IDigest digest;

	private IAsymmetricBlockCipher cipher;

	private SecureRandom random;

	private byte[] standardSalt;

	private int hLen;

	private int trailer;

	private int keyBits;

	private byte[] block;

	private byte[] mBuf;

	private int messageLength;

	private readonly int saltLength;

	private bool fullMessage;

	private byte[] recoveredMessage;

	private byte[] preSig;

	private byte[] preBlock;

	private int preMStart;

	private int preTLength;

	public virtual string AlgorithmName => digest.AlgorithmName + "withISO9796-2S2";

	public byte[] GetRecoveredMessage()
	{
		return recoveredMessage;
	}

	public Iso9796d2PssSigner(IAsymmetricBlockCipher cipher, IDigest digest, int saltLength, bool isImplicit)
	{
		this.cipher = cipher;
		this.digest = digest;
		hLen = digest.GetDigestSize();
		this.saltLength = saltLength;
		if (isImplicit)
		{
			trailer = 188;
			return;
		}
		if (IsoTrailers.NoTrailerAvailable(digest))
		{
			throw new ArgumentException("no valid trailer", "digest");
		}
		trailer = IsoTrailers.GetTrailer(digest);
	}

	public Iso9796d2PssSigner(IAsymmetricBlockCipher cipher, IDigest digest, int saltLength)
		: this(cipher, digest, saltLength, isImplicit: false)
	{
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		RsaKeyParameters rsaKeyParameters;
		if (parameters is ParametersWithRandom)
		{
			ParametersWithRandom parametersWithRandom = (ParametersWithRandom)parameters;
			rsaKeyParameters = (RsaKeyParameters)parametersWithRandom.Parameters;
			if (forSigning)
			{
				random = parametersWithRandom.Random;
			}
		}
		else if (parameters is ParametersWithSalt)
		{
			if (!forSigning)
			{
				throw new ArgumentException("ParametersWithSalt only valid for signing", "parameters");
			}
			ParametersWithSalt parametersWithSalt = (ParametersWithSalt)parameters;
			rsaKeyParameters = (RsaKeyParameters)parametersWithSalt.Parameters;
			standardSalt = parametersWithSalt.GetSalt();
			if (standardSalt.Length != saltLength)
			{
				throw new ArgumentException("Fixed salt is of wrong length");
			}
		}
		else
		{
			rsaKeyParameters = (RsaKeyParameters)parameters;
			if (forSigning)
			{
				random = new SecureRandom();
			}
		}
		cipher.Init(forSigning, rsaKeyParameters);
		keyBits = rsaKeyParameters.Modulus.BitLength;
		block = new byte[(keyBits + 7) / 8];
		if (trailer == 188)
		{
			mBuf = new byte[block.Length - digest.GetDigestSize() - saltLength - 1 - 1];
		}
		else
		{
			mBuf = new byte[block.Length - digest.GetDigestSize() - saltLength - 1 - 2];
		}
		Reset();
	}

	private bool IsSameAs(byte[] a, byte[] b)
	{
		if (messageLength != b.Length)
		{
			return false;
		}
		bool result = true;
		for (int i = 0; i != b.Length; i++)
		{
			if (a[i] != b[i])
			{
				result = false;
			}
		}
		return result;
	}

	private void ClearBlock(byte[] block)
	{
		Array.Clear(block, 0, block.Length);
	}

	public virtual void UpdateWithRecoveredMessage(byte[] signature)
	{
		byte[] array = cipher.ProcessBlock(signature, 0, signature.Length);
		if (array.Length < (keyBits + 7) / 8)
		{
			byte[] array2 = new byte[(keyBits + 7) / 8];
			Array.Copy(array, 0, array2, array2.Length - array.Length, array.Length);
			ClearBlock(array);
			array = array2;
		}
		int num;
		if (((array[array.Length - 1] & 0xFF) ^ 0xBC) == 0)
		{
			num = 1;
		}
		else
		{
			int num2 = ((array[array.Length - 2] & 0xFF) << 8) | (array[array.Length - 1] & 0xFF);
			if (IsoTrailers.NoTrailerAvailable(digest))
			{
				throw new ArgumentException("unrecognised hash in signature");
			}
			if (num2 != IsoTrailers.GetTrailer(digest))
			{
				throw new InvalidOperationException("signer initialised with wrong digest for trailer " + num2);
			}
			num = 2;
		}
		byte[] output = new byte[hLen];
		digest.DoFinal(output, 0);
		byte[] array3 = MaskGeneratorFunction1(array, array.Length - hLen - num, hLen, array.Length - hLen - num);
		byte[] array4;
		for (int i = 0; i != array3.Length; i++)
		{
			byte[] array5 = (array4 = array);
			int num3 = i;
			nint num4 = num3;
			array5[num3] = (byte)(array4[num4] ^ array3[i]);
		}
		(array4 = array)[0] = (byte)(array4[0] & 0x7Fu);
		int num5 = 0;
		while (num5 < array.Length && array[num5++] != 1)
		{
		}
		if (num5 >= array.Length)
		{
			ClearBlock(array);
		}
		fullMessage = num5 > 1;
		recoveredMessage = new byte[array3.Length - num5 - saltLength];
		Array.Copy(array, num5, recoveredMessage, 0, recoveredMessage.Length);
		recoveredMessage.CopyTo(mBuf, 0);
		preSig = signature;
		preBlock = array;
		preMStart = num5;
		preTLength = num;
	}

	public virtual void Update(byte input)
	{
		if (preSig == null && messageLength < mBuf.Length)
		{
			mBuf[messageLength++] = input;
		}
		else
		{
			digest.Update(input);
		}
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int length)
	{
		if (preSig == null)
		{
			while (length > 0 && messageLength < mBuf.Length)
			{
				Update(input[inOff]);
				inOff++;
				length--;
			}
		}
		if (length > 0)
		{
			digest.BlockUpdate(input, inOff, length);
		}
	}

	public virtual void Reset()
	{
		digest.Reset();
		messageLength = 0;
		if (mBuf != null)
		{
			ClearBlock(mBuf);
		}
		if (recoveredMessage != null)
		{
			ClearBlock(recoveredMessage);
			recoveredMessage = null;
		}
		fullMessage = false;
		if (preSig != null)
		{
			preSig = null;
			ClearBlock(preBlock);
			preBlock = null;
		}
	}

	public virtual byte[] GenerateSignature()
	{
		int digestSize = digest.GetDigestSize();
		byte[] array = new byte[digestSize];
		digest.DoFinal(array, 0);
		byte[] array2 = new byte[8];
		LtoOSP(messageLength * 8, array2);
		digest.BlockUpdate(array2, 0, array2.Length);
		digest.BlockUpdate(mBuf, 0, messageLength);
		digest.BlockUpdate(array, 0, array.Length);
		byte[] array3;
		if (standardSalt != null)
		{
			array3 = standardSalt;
		}
		else
		{
			array3 = new byte[saltLength];
			random.NextBytes(array3);
		}
		digest.BlockUpdate(array3, 0, array3.Length);
		byte[] array4 = new byte[digest.GetDigestSize()];
		digest.DoFinal(array4, 0);
		int num = 2;
		if (trailer == 188)
		{
			num = 1;
		}
		int num2 = block.Length - messageLength - array3.Length - hLen - num - 1;
		block[num2] = 1;
		Array.Copy(mBuf, 0, block, num2 + 1, messageLength);
		Array.Copy(array3, 0, block, num2 + 1 + messageLength, array3.Length);
		byte[] array5 = MaskGeneratorFunction1(array4, 0, array4.Length, block.Length - hLen - num);
		byte[] array6;
		for (int i = 0; i != array5.Length; i++)
		{
			byte[] array7 = (array6 = block);
			int num3 = i;
			nint num4 = num3;
			array7[num3] = (byte)(array6[num4] ^ array5[i]);
		}
		Array.Copy(array4, 0, block, block.Length - hLen - num, hLen);
		if (trailer == 188)
		{
			block[block.Length - 1] = 188;
		}
		else
		{
			block[block.Length - 2] = (byte)((uint)trailer >> 8);
			block[block.Length - 1] = (byte)trailer;
		}
		(array6 = block)[0] = (byte)(array6[0] & 0x7Fu);
		byte[] result = cipher.ProcessBlock(block, 0, block.Length);
		ClearBlock(mBuf);
		ClearBlock(block);
		messageLength = 0;
		return result;
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		byte[] array = new byte[hLen];
		digest.DoFinal(array, 0);
		int num = 0;
		if (preSig == null)
		{
			try
			{
				UpdateWithRecoveredMessage(signature);
			}
			catch (Exception)
			{
				return false;
			}
		}
		else if (!Arrays.AreEqual(preSig, signature))
		{
			throw new InvalidOperationException("UpdateWithRecoveredMessage called on different signature");
		}
		byte[] array2 = preBlock;
		num = preMStart;
		int num2 = preTLength;
		preSig = null;
		preBlock = null;
		byte[] array3 = new byte[8];
		LtoOSP(recoveredMessage.Length * 8, array3);
		digest.BlockUpdate(array3, 0, array3.Length);
		if (recoveredMessage.Length != 0)
		{
			digest.BlockUpdate(recoveredMessage, 0, recoveredMessage.Length);
		}
		digest.BlockUpdate(array, 0, array.Length);
		if (standardSalt != null)
		{
			digest.BlockUpdate(standardSalt, 0, standardSalt.Length);
		}
		else
		{
			digest.BlockUpdate(array2, num + recoveredMessage.Length, saltLength);
		}
		byte[] array4 = new byte[digest.GetDigestSize()];
		digest.DoFinal(array4, 0);
		int num3 = array2.Length - num2 - array4.Length;
		bool flag = true;
		for (int i = 0; i != array4.Length; i++)
		{
			if (array4[i] != array2[num3 + i])
			{
				flag = false;
			}
		}
		ClearBlock(array2);
		ClearBlock(array4);
		if (!flag)
		{
			fullMessage = false;
			messageLength = 0;
			ClearBlock(recoveredMessage);
			return false;
		}
		if (messageLength != 0 && !IsSameAs(mBuf, recoveredMessage))
		{
			messageLength = 0;
			ClearBlock(mBuf);
			return false;
		}
		messageLength = 0;
		ClearBlock(mBuf);
		return true;
	}

	public virtual bool HasFullMessage()
	{
		return fullMessage;
	}

	private void ItoOSP(int i, byte[] sp)
	{
		sp[0] = (byte)((uint)i >> 24);
		sp[1] = (byte)((uint)i >> 16);
		sp[2] = (byte)((uint)i >> 8);
		sp[3] = (byte)i;
	}

	private void LtoOSP(long l, byte[] sp)
	{
		sp[0] = (byte)((ulong)l >> 56);
		sp[1] = (byte)((ulong)l >> 48);
		sp[2] = (byte)((ulong)l >> 40);
		sp[3] = (byte)((ulong)l >> 32);
		sp[4] = (byte)((ulong)l >> 24);
		sp[5] = (byte)((ulong)l >> 16);
		sp[6] = (byte)((ulong)l >> 8);
		sp[7] = (byte)l;
	}

	private byte[] MaskGeneratorFunction1(byte[] Z, int zOff, int zLen, int length)
	{
		byte[] array = new byte[length];
		byte[] array2 = new byte[hLen];
		byte[] array3 = new byte[4];
		int num = 0;
		digest.Reset();
		do
		{
			ItoOSP(num, array3);
			digest.BlockUpdate(Z, zOff, zLen);
			digest.BlockUpdate(array3, 0, array3.Length);
			digest.DoFinal(array2, 0);
			Array.Copy(array2, 0, array, num * hLen, hLen);
		}
		while (++num < length / hLen);
		if (num * hLen < length)
		{
			ItoOSP(num, array3);
			digest.BlockUpdate(Z, zOff, zLen);
			digest.BlockUpdate(array3, 0, array3.Length);
			digest.DoFinal(array2, 0);
			Array.Copy(array2, 0, array, num * hLen, array.Length - num * hLen);
		}
		return array;
	}
}
