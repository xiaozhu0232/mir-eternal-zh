using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class Iso9796d2Signer : ISignerWithRecovery, ISigner
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

	private int trailer;

	private int keyBits;

	private byte[] block;

	private byte[] mBuf;

	private int messageLength;

	private bool fullMessage;

	private byte[] recoveredMessage;

	private byte[] preSig;

	private byte[] preBlock;

	public virtual string AlgorithmName => digest.AlgorithmName + "withISO9796-2S1";

	public byte[] GetRecoveredMessage()
	{
		return recoveredMessage;
	}

	public Iso9796d2Signer(IAsymmetricBlockCipher cipher, IDigest digest, bool isImplicit)
	{
		this.cipher = cipher;
		this.digest = digest;
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

	public Iso9796d2Signer(IAsymmetricBlockCipher cipher, IDigest digest)
		: this(cipher, digest, isImplicit: false)
	{
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)parameters;
		cipher.Init(forSigning, rsaKeyParameters);
		keyBits = rsaKeyParameters.Modulus.BitLength;
		block = new byte[(keyBits + 7) / 8];
		if (trailer == 188)
		{
			mBuf = new byte[block.Length - digest.GetDigestSize() - 2];
		}
		else
		{
			mBuf = new byte[block.Length - digest.GetDigestSize() - 3];
		}
		Reset();
	}

	private bool IsSameAs(byte[] a, byte[] b)
	{
		int num;
		if (messageLength > mBuf.Length)
		{
			if (mBuf.Length > b.Length)
			{
				return false;
			}
			num = mBuf.Length;
		}
		else
		{
			if (messageLength != b.Length)
			{
				return false;
			}
			num = b.Length;
		}
		bool result = true;
		for (int i = 0; i != num; i++)
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
		if (((array[0] & 0xC0u) ^ 0x40u) != 0)
		{
			throw new InvalidCipherTextException("malformed signature");
		}
		if (((array[array.Length - 1] & 0xFu) ^ 0xCu) != 0)
		{
			throw new InvalidCipherTextException("malformed signature");
		}
		int num = 0;
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
		int num3 = 0;
		for (num3 = 0; num3 != array.Length && ((array[num3] & 0xFu) ^ 0xAu) != 0; num3++)
		{
		}
		num3++;
		int num4 = array.Length - num - digest.GetDigestSize();
		if (num4 - num3 <= 0)
		{
			throw new InvalidCipherTextException("malformed block");
		}
		if ((array[0] & 0x20) == 0)
		{
			fullMessage = true;
			recoveredMessage = new byte[num4 - num3];
			Array.Copy(array, num3, recoveredMessage, 0, recoveredMessage.Length);
		}
		else
		{
			fullMessage = false;
			recoveredMessage = new byte[num4 - num3];
			Array.Copy(array, num3, recoveredMessage, 0, recoveredMessage.Length);
		}
		preSig = signature;
		preBlock = array;
		digest.BlockUpdate(recoveredMessage, 0, recoveredMessage.Length);
		messageLength = recoveredMessage.Length;
		recoveredMessage.CopyTo(mBuf, 0);
	}

	public virtual void Update(byte input)
	{
		digest.Update(input);
		if (messageLength < mBuf.Length)
		{
			mBuf[messageLength] = input;
		}
		messageLength++;
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int length)
	{
		while (length > 0 && messageLength < mBuf.Length)
		{
			Update(input[inOff]);
			inOff++;
			length--;
		}
		digest.BlockUpdate(input, inOff, length);
		messageLength += length;
	}

	public virtual void Reset()
	{
		digest.Reset();
		messageLength = 0;
		ClearBlock(mBuf);
		if (recoveredMessage != null)
		{
			ClearBlock(recoveredMessage);
		}
		recoveredMessage = null;
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
		int num = 0;
		int num2 = 0;
		if (trailer == 188)
		{
			num = 8;
			num2 = block.Length - digestSize - 1;
			digest.DoFinal(block, num2);
			block[block.Length - 1] = 188;
		}
		else
		{
			num = 16;
			num2 = block.Length - digestSize - 2;
			digest.DoFinal(block, num2);
			block[block.Length - 2] = (byte)((uint)trailer >> 8);
			block[block.Length - 1] = (byte)trailer;
		}
		byte b = 0;
		int num3 = (digestSize + messageLength) * 8 + num + 4 - keyBits;
		if (num3 > 0)
		{
			int num4 = messageLength - (num3 + 7) / 8;
			b = 96;
			num2 -= num4;
			Array.Copy(mBuf, 0, block, num2, num4);
		}
		else
		{
			b = 64;
			num2 -= messageLength;
			Array.Copy(mBuf, 0, block, num2, messageLength);
		}
		if (num2 - 1 > 0)
		{
			for (int num5 = num2 - 1; num5 != 0; num5--)
			{
				block[num5] = 187;
			}
			byte[] array;
			byte[] array2 = (array = block);
			int num6 = num2 - 1;
			nint num7 = num6;
			array2[num6] = (byte)(array[num7] ^ 1u);
			block[0] = 11;
			(array = block)[0] = (byte)(array[0] | b);
		}
		else
		{
			block[0] = 10;
			byte[] array;
			(array = block)[0] = (byte)(array[0] | b);
		}
		byte[] result = cipher.ProcessBlock(block, 0, block.Length);
		messageLength = 0;
		ClearBlock(mBuf);
		ClearBlock(block);
		return result;
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		byte[] array;
		if (preSig == null)
		{
			try
			{
				array = cipher.ProcessBlock(signature, 0, signature.Length);
			}
			catch (Exception)
			{
				return false;
			}
		}
		else
		{
			if (!Arrays.AreEqual(preSig, signature))
			{
				throw new InvalidOperationException("updateWithRecoveredMessage called on different signature");
			}
			array = preBlock;
			preSig = null;
			preBlock = null;
		}
		if (((array[0] & 0xC0u) ^ 0x40u) != 0)
		{
			return ReturnFalse(array);
		}
		if (((array[array.Length - 1] & 0xFu) ^ 0xCu) != 0)
		{
			return ReturnFalse(array);
		}
		int num = 0;
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
		int i;
		for (i = 0; i != array.Length && ((array[i] & 0xFu) ^ 0xAu) != 0; i++)
		{
		}
		i++;
		byte[] array2 = new byte[digest.GetDigestSize()];
		int num3 = array.Length - num - array2.Length;
		if (num3 - i <= 0)
		{
			return ReturnFalse(array);
		}
		if ((array[0] & 0x20) == 0)
		{
			fullMessage = true;
			if (messageLength > num3 - i)
			{
				return ReturnFalse(array);
			}
			digest.Reset();
			digest.BlockUpdate(array, i, num3 - i);
			digest.DoFinal(array2, 0);
			bool flag = true;
			for (int j = 0; j != array2.Length; j++)
			{
				byte[] array3;
				byte[] array4 = (array3 = array);
				int num4 = num3 + j;
				nint num5 = num4;
				array4[num4] = (byte)(array3[num5] ^ array2[j]);
				if (array[num3 + j] != 0)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				return ReturnFalse(array);
			}
			recoveredMessage = new byte[num3 - i];
			Array.Copy(array, i, recoveredMessage, 0, recoveredMessage.Length);
		}
		else
		{
			fullMessage = false;
			digest.DoFinal(array2, 0);
			bool flag2 = true;
			for (int k = 0; k != array2.Length; k++)
			{
				byte[] array3;
				byte[] array5 = (array3 = array);
				int num6 = num3 + k;
				nint num5 = num6;
				array5[num6] = (byte)(array3[num5] ^ array2[k]);
				if (array[num3 + k] != 0)
				{
					flag2 = false;
				}
			}
			if (!flag2)
			{
				return ReturnFalse(array);
			}
			recoveredMessage = new byte[num3 - i];
			Array.Copy(array, i, recoveredMessage, 0, recoveredMessage.Length);
		}
		if (messageLength != 0 && !IsSameAs(mBuf, recoveredMessage))
		{
			return ReturnFalse(array);
		}
		ClearBlock(mBuf);
		ClearBlock(array);
		messageLength = 0;
		return true;
	}

	private bool ReturnFalse(byte[] block)
	{
		messageLength = 0;
		ClearBlock(mBuf);
		ClearBlock(block);
		return false;
	}

	public virtual bool HasFullMessage()
	{
		return fullMessage;
	}
}
