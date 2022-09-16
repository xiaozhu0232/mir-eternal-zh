using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsBlockCipher : TlsCipher
{
	protected readonly TlsContext context;

	protected readonly byte[] randomData;

	protected readonly bool useExplicitIV;

	protected readonly bool encryptThenMac;

	protected readonly IBlockCipher encryptCipher;

	protected readonly IBlockCipher decryptCipher;

	protected readonly TlsMac mWriteMac;

	protected readonly TlsMac mReadMac;

	public virtual TlsMac WriteMac => mWriteMac;

	public virtual TlsMac ReadMac => mReadMac;

	public TlsBlockCipher(TlsContext context, IBlockCipher clientWriteCipher, IBlockCipher serverWriteCipher, IDigest clientWriteDigest, IDigest serverWriteDigest, int cipherKeySize)
	{
		this.context = context;
		randomData = new byte[256];
		context.NonceRandomGenerator.NextBytes(randomData);
		useExplicitIV = TlsUtilities.IsTlsV11(context);
		encryptThenMac = context.SecurityParameters.encryptThenMac;
		int num = 2 * cipherKeySize + clientWriteDigest.GetDigestSize() + serverWriteDigest.GetDigestSize();
		if (!useExplicitIV)
		{
			num += clientWriteCipher.GetBlockSize() + serverWriteCipher.GetBlockSize();
		}
		byte[] array = TlsUtilities.CalculateKeyBlock(context, num);
		int num2 = 0;
		TlsMac tlsMac = new TlsMac(context, clientWriteDigest, array, num2, clientWriteDigest.GetDigestSize());
		num2 += clientWriteDigest.GetDigestSize();
		TlsMac tlsMac2 = new TlsMac(context, serverWriteDigest, array, num2, serverWriteDigest.GetDigestSize());
		num2 += serverWriteDigest.GetDigestSize();
		KeyParameter parameters = new KeyParameter(array, num2, cipherKeySize);
		num2 += cipherKeySize;
		KeyParameter parameters2 = new KeyParameter(array, num2, cipherKeySize);
		num2 += cipherKeySize;
		byte[] iv;
		byte[] iv2;
		if (useExplicitIV)
		{
			iv = new byte[clientWriteCipher.GetBlockSize()];
			iv2 = new byte[serverWriteCipher.GetBlockSize()];
		}
		else
		{
			iv = Arrays.CopyOfRange(array, num2, num2 + clientWriteCipher.GetBlockSize());
			num2 += clientWriteCipher.GetBlockSize();
			iv2 = Arrays.CopyOfRange(array, num2, num2 + serverWriteCipher.GetBlockSize());
			num2 += serverWriteCipher.GetBlockSize();
		}
		if (num2 != num)
		{
			throw new TlsFatalAlert(80);
		}
		ICipherParameters parameters3;
		ICipherParameters parameters4;
		if (context.IsServer)
		{
			mWriteMac = tlsMac2;
			mReadMac = tlsMac;
			encryptCipher = serverWriteCipher;
			decryptCipher = clientWriteCipher;
			parameters3 = new ParametersWithIV(parameters2, iv2);
			parameters4 = new ParametersWithIV(parameters, iv);
		}
		else
		{
			mWriteMac = tlsMac;
			mReadMac = tlsMac2;
			encryptCipher = clientWriteCipher;
			decryptCipher = serverWriteCipher;
			parameters3 = new ParametersWithIV(parameters, iv);
			parameters4 = new ParametersWithIV(parameters2, iv2);
		}
		encryptCipher.Init(forEncryption: true, parameters3);
		decryptCipher.Init(forEncryption: false, parameters4);
	}

	public virtual int GetPlaintextLimit(int ciphertextLimit)
	{
		int blockSize = encryptCipher.GetBlockSize();
		int size = mWriteMac.Size;
		int num = ciphertextLimit;
		if (useExplicitIV)
		{
			num -= blockSize;
		}
		if (encryptThenMac)
		{
			num -= size;
			num -= num % blockSize;
		}
		else
		{
			num -= num % blockSize;
			num -= size;
		}
		return num - 1;
	}

	public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
	{
		int blockSize = encryptCipher.GetBlockSize();
		int size = mWriteMac.Size;
		ProtocolVersion serverVersion = context.ServerVersion;
		int num = len;
		if (!encryptThenMac)
		{
			num += size;
		}
		int num2 = blockSize - 1 - num % blockSize;
		if ((encryptThenMac || !context.SecurityParameters.truncatedHMac) && !serverVersion.IsDtls && !serverVersion.IsSsl)
		{
			int max = (255 - num2) / blockSize;
			int num3 = ChooseExtraPadBlocks(context.SecureRandom, max);
			num2 += num3 * blockSize;
		}
		int num4 = len + size + num2 + 1;
		if (useExplicitIV)
		{
			num4 += blockSize;
		}
		byte[] array = new byte[num4];
		int num5 = 0;
		if (useExplicitIV)
		{
			byte[] array2 = new byte[blockSize];
			context.NonceRandomGenerator.NextBytes(array2);
			encryptCipher.Init(forEncryption: true, new ParametersWithIV(null, array2));
			Array.Copy(array2, 0, array, num5, blockSize);
			num5 += blockSize;
		}
		int num6 = num5;
		Array.Copy(plaintext, offset, array, num5, len);
		num5 += len;
		if (!encryptThenMac)
		{
			byte[] array3 = mWriteMac.CalculateMac(seqNo, type, plaintext, offset, len);
			Array.Copy(array3, 0, array, num5, array3.Length);
			num5 += array3.Length;
		}
		for (int i = 0; i <= num2; i++)
		{
			array[num5++] = (byte)num2;
		}
		for (int j = num6; j < num5; j += blockSize)
		{
			encryptCipher.ProcessBlock(array, j, array, j);
		}
		if (encryptThenMac)
		{
			byte[] array4 = mWriteMac.CalculateMac(seqNo, type, array, 0, num5);
			Array.Copy(array4, 0, array, num5, array4.Length);
			num5 += array4.Length;
		}
		return array;
	}

	public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
	{
		int blockSize = decryptCipher.GetBlockSize();
		int size = mReadMac.Size;
		int num = blockSize;
		num = ((!encryptThenMac) ? System.Math.Max(num, size + 1) : (num + size));
		if (useExplicitIV)
		{
			num += blockSize;
		}
		if (len < num)
		{
			throw new TlsFatalAlert(50);
		}
		int num2 = len;
		if (encryptThenMac)
		{
			num2 -= size;
		}
		if (num2 % blockSize != 0)
		{
			throw new TlsFatalAlert(21);
		}
		if (encryptThenMac)
		{
			int num3 = offset + len;
			byte[] b = Arrays.CopyOfRange(ciphertext, num3 - size, num3);
			byte[] a = mReadMac.CalculateMac(seqNo, type, ciphertext, offset, len - size);
			if (!Arrays.ConstantTimeAreEqual(a, b))
			{
				throw new TlsFatalAlert(20);
			}
		}
		if (useExplicitIV)
		{
			decryptCipher.Init(forEncryption: false, new ParametersWithIV(null, ciphertext, offset, blockSize));
			offset += blockSize;
			num2 -= blockSize;
		}
		for (int i = 0; i < num2; i += blockSize)
		{
			decryptCipher.ProcessBlock(ciphertext, offset + i, ciphertext, offset + i);
		}
		int num4 = CheckPaddingConstantTime(ciphertext, offset, num2, blockSize, (!encryptThenMac) ? size : 0);
		bool flag = num4 == 0;
		int num5 = num2 - num4;
		if (!encryptThenMac)
		{
			num5 -= size;
			int num6 = num5;
			int num7 = offset + num6;
			byte[] b2 = Arrays.CopyOfRange(ciphertext, num7, num7 + size);
			byte[] a2 = mReadMac.CalculateMacConstantTime(seqNo, type, ciphertext, offset, num6, num2 - size, randomData);
			flag |= !Arrays.ConstantTimeAreEqual(a2, b2);
		}
		if (flag)
		{
			throw new TlsFatalAlert(20);
		}
		return Arrays.CopyOfRange(ciphertext, offset, offset + num5);
	}

	protected virtual int CheckPaddingConstantTime(byte[] buf, int off, int len, int blockSize, int macSize)
	{
		int num = off + len;
		byte b = buf[num - 1];
		int num2 = b & 0xFF;
		int num3 = num2 + 1;
		int num4 = 0;
		byte b2 = 0;
		if ((TlsUtilities.IsSsl(context) && num3 > blockSize) || macSize + num3 > len)
		{
			num3 = 0;
		}
		else
		{
			int num5 = num - num3;
			do
			{
				b2 = (byte)(b2 | (byte)(buf[num5++] ^ b));
			}
			while (num5 < num);
			num4 = num3;
			if (b2 != 0)
			{
				num3 = 0;
			}
		}
		byte[] array = randomData;
		while (num4 < 256)
		{
			b2 = (byte)(b2 | (byte)(array[num4++] ^ b));
		}
		byte[] array2;
		(array2 = array)[0] = (byte)(array2[0] ^ b2);
		return num3;
	}

	protected virtual int ChooseExtraPadBlocks(SecureRandom r, int max)
	{
		int x = r.NextInt();
		int val = LowestBitSet(x);
		return System.Math.Min(val, max);
	}

	protected virtual int LowestBitSet(int x)
	{
		if (x == 0)
		{
			return 32;
		}
		uint num = (uint)x;
		int num2 = 0;
		while ((num & 1) == 0)
		{
			num2++;
			num >>= 1;
		}
		return num2;
	}
}
