using System;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsAeadCipher : TlsCipher
{
	public const int NONCE_RFC5288 = 1;

	internal const int NONCE_DRAFT_CHACHA20_POLY1305 = 2;

	protected readonly TlsContext context;

	protected readonly int macSize;

	protected readonly int record_iv_length;

	protected readonly IAeadBlockCipher encryptCipher;

	protected readonly IAeadBlockCipher decryptCipher;

	protected readonly byte[] encryptImplicitNonce;

	protected readonly byte[] decryptImplicitNonce;

	protected readonly int nonceMode;

	public TlsAeadCipher(TlsContext context, IAeadBlockCipher clientWriteCipher, IAeadBlockCipher serverWriteCipher, int cipherKeySize, int macSize)
		: this(context, clientWriteCipher, serverWriteCipher, cipherKeySize, macSize, 1)
	{
	}

	internal TlsAeadCipher(TlsContext context, IAeadBlockCipher clientWriteCipher, IAeadBlockCipher serverWriteCipher, int cipherKeySize, int macSize, int nonceMode)
	{
		if (!TlsUtilities.IsTlsV12(context))
		{
			throw new TlsFatalAlert(80);
		}
		this.nonceMode = nonceMode;
		int num;
		switch (nonceMode)
		{
		case 1:
			num = 4;
			record_iv_length = 8;
			break;
		case 2:
			num = 12;
			record_iv_length = 0;
			break;
		default:
			throw new TlsFatalAlert(80);
		}
		this.context = context;
		this.macSize = macSize;
		int num2 = 2 * cipherKeySize + 2 * num;
		byte[] array = TlsUtilities.CalculateKeyBlock(context, num2);
		int num3 = 0;
		KeyParameter keyParameter = new KeyParameter(array, num3, cipherKeySize);
		num3 += cipherKeySize;
		KeyParameter keyParameter2 = new KeyParameter(array, num3, cipherKeySize);
		num3 += cipherKeySize;
		byte[] array2 = Arrays.CopyOfRange(array, num3, num3 + num);
		num3 += num;
		byte[] array3 = Arrays.CopyOfRange(array, num3, num3 + num);
		num3 += num;
		if (num3 != num2)
		{
			throw new TlsFatalAlert(80);
		}
		KeyParameter key;
		KeyParameter key2;
		if (context.IsServer)
		{
			encryptCipher = serverWriteCipher;
			decryptCipher = clientWriteCipher;
			encryptImplicitNonce = array3;
			decryptImplicitNonce = array2;
			key = keyParameter2;
			key2 = keyParameter;
		}
		else
		{
			encryptCipher = clientWriteCipher;
			decryptCipher = serverWriteCipher;
			encryptImplicitNonce = array2;
			decryptImplicitNonce = array3;
			key = keyParameter;
			key2 = keyParameter2;
		}
		byte[] array4 = new byte[num + record_iv_length];
		array4[0] = (byte)(~encryptImplicitNonce[0]);
		array4[1] = (byte)(~decryptImplicitNonce[1]);
		encryptCipher.Init(forEncryption: true, new AeadParameters(key, 8 * macSize, array4));
		decryptCipher.Init(forEncryption: false, new AeadParameters(key2, 8 * macSize, array4));
	}

	public virtual int GetPlaintextLimit(int ciphertextLimit)
	{
		return ciphertextLimit - macSize - record_iv_length;
	}

	public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
	{
		byte[] array = new byte[encryptImplicitNonce.Length + record_iv_length];
		switch (nonceMode)
		{
		case 1:
			Array.Copy(encryptImplicitNonce, 0, array, 0, encryptImplicitNonce.Length);
			TlsUtilities.WriteUint64(seqNo, array, encryptImplicitNonce.Length);
			break;
		case 2:
		{
			TlsUtilities.WriteUint64(seqNo, array, array.Length - 8);
			for (int i = 0; i < encryptImplicitNonce.Length; i++)
			{
				byte[] array2;
				byte[] array3 = (array2 = array);
				int num = i;
				nint num2 = num;
				array3[num] = (byte)(array2[num2] ^ encryptImplicitNonce[i]);
			}
			break;
		}
		default:
			throw new TlsFatalAlert(80);
		}
		int outputSize = encryptCipher.GetOutputSize(len);
		byte[] array4 = new byte[record_iv_length + outputSize];
		if (record_iv_length != 0)
		{
			Array.Copy(array, array.Length - record_iv_length, array4, 0, record_iv_length);
		}
		int num3 = record_iv_length;
		byte[] additionalData = GetAdditionalData(seqNo, type, len);
		AeadParameters parameters = new AeadParameters(null, 8 * macSize, array, additionalData);
		try
		{
			encryptCipher.Init(forEncryption: true, parameters);
			num3 += encryptCipher.ProcessBytes(plaintext, offset, len, array4, num3);
			num3 += encryptCipher.DoFinal(array4, num3);
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(80, alertCause);
		}
		if (num3 != array4.Length)
		{
			throw new TlsFatalAlert(80);
		}
		return array4;
	}

	public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
	{
		if (GetPlaintextLimit(len) < 0)
		{
			throw new TlsFatalAlert(50);
		}
		byte[] array = new byte[decryptImplicitNonce.Length + record_iv_length];
		switch (nonceMode)
		{
		case 1:
			Array.Copy(decryptImplicitNonce, 0, array, 0, decryptImplicitNonce.Length);
			Array.Copy(ciphertext, offset, array, array.Length - record_iv_length, record_iv_length);
			break;
		case 2:
		{
			TlsUtilities.WriteUint64(seqNo, array, array.Length - 8);
			for (int i = 0; i < decryptImplicitNonce.Length; i++)
			{
				byte[] array2;
				byte[] array3 = (array2 = array);
				int num = i;
				nint num2 = num;
				array3[num] = (byte)(array2[num2] ^ decryptImplicitNonce[i]);
			}
			break;
		}
		default:
			throw new TlsFatalAlert(80);
		}
		int inOff = offset + record_iv_length;
		int len2 = len - record_iv_length;
		int outputSize = decryptCipher.GetOutputSize(len2);
		byte[] array4 = new byte[outputSize];
		int num3 = 0;
		byte[] additionalData = GetAdditionalData(seqNo, type, outputSize);
		AeadParameters parameters = new AeadParameters(null, 8 * macSize, array, additionalData);
		try
		{
			decryptCipher.Init(forEncryption: false, parameters);
			num3 += decryptCipher.ProcessBytes(ciphertext, inOff, len2, array4, num3);
			num3 += decryptCipher.DoFinal(array4, num3);
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(20, alertCause);
		}
		if (num3 != array4.Length)
		{
			throw new TlsFatalAlert(80);
		}
		return array4;
	}

	protected virtual byte[] GetAdditionalData(long seqNo, byte type, int len)
	{
		byte[] array = new byte[13];
		TlsUtilities.WriteUint64(seqNo, array, 0);
		TlsUtilities.WriteUint8(type, array, 8);
		TlsUtilities.WriteVersion(context.ServerVersion, array, 9);
		TlsUtilities.WriteUint16(len, array, 11);
		return array;
	}
}
