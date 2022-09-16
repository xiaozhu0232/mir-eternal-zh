using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsStreamCipher : TlsCipher
{
	protected readonly TlsContext context;

	protected readonly IStreamCipher encryptCipher;

	protected readonly IStreamCipher decryptCipher;

	protected readonly TlsMac writeMac;

	protected readonly TlsMac readMac;

	protected readonly bool usesNonce;

	public TlsStreamCipher(TlsContext context, IStreamCipher clientWriteCipher, IStreamCipher serverWriteCipher, IDigest clientWriteDigest, IDigest serverWriteDigest, int cipherKeySize, bool usesNonce)
	{
		bool isServer = context.IsServer;
		this.context = context;
		this.usesNonce = usesNonce;
		encryptCipher = clientWriteCipher;
		decryptCipher = serverWriteCipher;
		int num = 2 * cipherKeySize + clientWriteDigest.GetDigestSize() + serverWriteDigest.GetDigestSize();
		byte[] key = TlsUtilities.CalculateKeyBlock(context, num);
		int num2 = 0;
		TlsMac tlsMac = new TlsMac(context, clientWriteDigest, key, num2, clientWriteDigest.GetDigestSize());
		num2 += clientWriteDigest.GetDigestSize();
		TlsMac tlsMac2 = new TlsMac(context, serverWriteDigest, key, num2, serverWriteDigest.GetDigestSize());
		num2 += serverWriteDigest.GetDigestSize();
		KeyParameter keyParameter = new KeyParameter(key, num2, cipherKeySize);
		num2 += cipherKeySize;
		KeyParameter keyParameter2 = new KeyParameter(key, num2, cipherKeySize);
		num2 += cipherKeySize;
		if (num2 != num)
		{
			throw new TlsFatalAlert(80);
		}
		ICipherParameters parameters;
		ICipherParameters parameters2;
		if (isServer)
		{
			writeMac = tlsMac2;
			readMac = tlsMac;
			encryptCipher = serverWriteCipher;
			decryptCipher = clientWriteCipher;
			parameters = keyParameter2;
			parameters2 = keyParameter;
		}
		else
		{
			writeMac = tlsMac;
			readMac = tlsMac2;
			encryptCipher = clientWriteCipher;
			decryptCipher = serverWriteCipher;
			parameters = keyParameter;
			parameters2 = keyParameter2;
		}
		if (usesNonce)
		{
			byte[] iv = new byte[8];
			parameters = new ParametersWithIV(parameters, iv);
			parameters2 = new ParametersWithIV(parameters2, iv);
		}
		encryptCipher.Init(forEncryption: true, parameters);
		decryptCipher.Init(forEncryption: false, parameters2);
	}

	public virtual int GetPlaintextLimit(int ciphertextLimit)
	{
		return ciphertextLimit - writeMac.Size;
	}

	public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
	{
		if (usesNonce)
		{
			UpdateIV(encryptCipher, forEncryption: true, seqNo);
		}
		byte[] array = new byte[len + writeMac.Size];
		encryptCipher.ProcessBytes(plaintext, offset, len, array, 0);
		byte[] array2 = writeMac.CalculateMac(seqNo, type, plaintext, offset, len);
		encryptCipher.ProcessBytes(array2, 0, array2.Length, array, len);
		return array;
	}

	public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
	{
		if (usesNonce)
		{
			UpdateIV(decryptCipher, forEncryption: false, seqNo);
		}
		int size = readMac.Size;
		if (len < size)
		{
			throw new TlsFatalAlert(50);
		}
		int num = len - size;
		byte[] array = new byte[len];
		decryptCipher.ProcessBytes(ciphertext, offset, len, array, 0);
		CheckMac(seqNo, type, array, num, len, array, 0, num);
		return Arrays.CopyOfRange(array, 0, num);
	}

	protected virtual void CheckMac(long seqNo, byte type, byte[] recBuf, int recStart, int recEnd, byte[] calcBuf, int calcOff, int calcLen)
	{
		byte[] a = Arrays.CopyOfRange(recBuf, recStart, recEnd);
		byte[] b = readMac.CalculateMac(seqNo, type, calcBuf, calcOff, calcLen);
		if (!Arrays.ConstantTimeAreEqual(a, b))
		{
			throw new TlsFatalAlert(20);
		}
	}

	protected virtual void UpdateIV(IStreamCipher cipher, bool forEncryption, long seqNo)
	{
		byte[] array = new byte[8];
		TlsUtilities.WriteUint64(seqNo, array, 0);
		cipher.Init(forEncryption, new ParametersWithIV(null, array));
	}
}
