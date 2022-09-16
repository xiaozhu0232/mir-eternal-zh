using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsNullCipher : TlsCipher
{
	protected readonly TlsContext context;

	protected readonly TlsMac writeMac;

	protected readonly TlsMac readMac;

	public TlsNullCipher(TlsContext context)
	{
		this.context = context;
		writeMac = null;
		readMac = null;
	}

	public TlsNullCipher(TlsContext context, IDigest clientWriteDigest, IDigest serverWriteDigest)
	{
		if (clientWriteDigest == null != (serverWriteDigest == null))
		{
			throw new TlsFatalAlert(80);
		}
		this.context = context;
		TlsMac tlsMac = null;
		TlsMac tlsMac2 = null;
		if (clientWriteDigest != null)
		{
			int num = clientWriteDigest.GetDigestSize() + serverWriteDigest.GetDigestSize();
			byte[] key = TlsUtilities.CalculateKeyBlock(context, num);
			int num2 = 0;
			tlsMac = new TlsMac(context, clientWriteDigest, key, num2, clientWriteDigest.GetDigestSize());
			num2 += clientWriteDigest.GetDigestSize();
			tlsMac2 = new TlsMac(context, serverWriteDigest, key, num2, serverWriteDigest.GetDigestSize());
			num2 += serverWriteDigest.GetDigestSize();
			if (num2 != num)
			{
				throw new TlsFatalAlert(80);
			}
		}
		if (context.IsServer)
		{
			writeMac = tlsMac2;
			readMac = tlsMac;
		}
		else
		{
			writeMac = tlsMac;
			readMac = tlsMac2;
		}
	}

	public virtual int GetPlaintextLimit(int ciphertextLimit)
	{
		int num = ciphertextLimit;
		if (writeMac != null)
		{
			num -= writeMac.Size;
		}
		return num;
	}

	public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
	{
		if (writeMac == null)
		{
			return Arrays.CopyOfRange(plaintext, offset, offset + len);
		}
		byte[] array = writeMac.CalculateMac(seqNo, type, plaintext, offset, len);
		byte[] array2 = new byte[len + array.Length];
		Array.Copy(plaintext, offset, array2, 0, len);
		Array.Copy(array, 0, array2, len, array.Length);
		return array2;
	}

	public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
	{
		if (readMac == null)
		{
			return Arrays.CopyOfRange(ciphertext, offset, offset + len);
		}
		int size = readMac.Size;
		if (len < size)
		{
			throw new TlsFatalAlert(50);
		}
		int num = len - size;
		byte[] a = Arrays.CopyOfRange(ciphertext, offset + num, offset + len);
		byte[] b = readMac.CalculateMac(seqNo, type, ciphertext, offset, num);
		if (!Arrays.ConstantTimeAreEqual(a, b))
		{
			throw new TlsFatalAlert(20);
		}
		return Arrays.CopyOfRange(ciphertext, offset, offset + num);
	}
}
