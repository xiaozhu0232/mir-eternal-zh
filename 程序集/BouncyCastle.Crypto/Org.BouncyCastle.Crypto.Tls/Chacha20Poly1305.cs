using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class Chacha20Poly1305 : TlsCipher
{
	private static readonly byte[] Zeroes = new byte[15];

	protected readonly TlsContext context;

	protected readonly ChaCha7539Engine encryptCipher;

	protected readonly ChaCha7539Engine decryptCipher;

	protected readonly byte[] encryptIV;

	protected readonly byte[] decryptIV;

	public Chacha20Poly1305(TlsContext context)
	{
		if (!TlsUtilities.IsTlsV12(context))
		{
			throw new TlsFatalAlert(80);
		}
		this.context = context;
		int num = 32;
		int num2 = 12;
		int num3 = 2 * num + 2 * num2;
		byte[] array = TlsUtilities.CalculateKeyBlock(context, num3);
		int num4 = 0;
		KeyParameter keyParameter = new KeyParameter(array, num4, num);
		num4 += num;
		KeyParameter keyParameter2 = new KeyParameter(array, num4, num);
		num4 += num;
		byte[] array2 = Arrays.CopyOfRange(array, num4, num4 + num2);
		num4 += num2;
		byte[] array3 = Arrays.CopyOfRange(array, num4, num4 + num2);
		num4 += num2;
		if (num4 != num3)
		{
			throw new TlsFatalAlert(80);
		}
		encryptCipher = new ChaCha7539Engine();
		decryptCipher = new ChaCha7539Engine();
		KeyParameter parameters;
		KeyParameter parameters2;
		if (context.IsServer)
		{
			parameters = keyParameter2;
			parameters2 = keyParameter;
			encryptIV = array3;
			decryptIV = array2;
		}
		else
		{
			parameters = keyParameter;
			parameters2 = keyParameter2;
			encryptIV = array2;
			decryptIV = array3;
		}
		encryptCipher.Init(forEncryption: true, new ParametersWithIV(parameters, encryptIV));
		decryptCipher.Init(forEncryption: false, new ParametersWithIV(parameters2, decryptIV));
	}

	public virtual int GetPlaintextLimit(int ciphertextLimit)
	{
		return ciphertextLimit - 16;
	}

	public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
	{
		KeyParameter macKey = InitRecord(encryptCipher, forEncryption: true, seqNo, encryptIV);
		byte[] array = new byte[len + 16];
		encryptCipher.ProcessBytes(plaintext, offset, len, array, 0);
		byte[] additionalData = GetAdditionalData(seqNo, type, len);
		byte[] array2 = CalculateRecordMac(macKey, additionalData, array, 0, len);
		Array.Copy(array2, 0, array, len, array2.Length);
		return array;
	}

	public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
	{
		if (GetPlaintextLimit(len) < 0)
		{
			throw new TlsFatalAlert(50);
		}
		KeyParameter macKey = InitRecord(decryptCipher, forEncryption: false, seqNo, decryptIV);
		int num = len - 16;
		byte[] additionalData = GetAdditionalData(seqNo, type, num);
		byte[] a = CalculateRecordMac(macKey, additionalData, ciphertext, offset, num);
		byte[] b = Arrays.CopyOfRange(ciphertext, offset + num, offset + len);
		if (!Arrays.ConstantTimeAreEqual(a, b))
		{
			throw new TlsFatalAlert(20);
		}
		byte[] array = new byte[num];
		decryptCipher.ProcessBytes(ciphertext, offset, num, array, 0);
		return array;
	}

	protected virtual KeyParameter InitRecord(IStreamCipher cipher, bool forEncryption, long seqNo, byte[] iv)
	{
		byte[] iv2 = CalculateNonce(seqNo, iv);
		cipher.Init(forEncryption, new ParametersWithIV(null, iv2));
		return GenerateRecordMacKey(cipher);
	}

	protected virtual byte[] CalculateNonce(long seqNo, byte[] iv)
	{
		byte[] array = new byte[12];
		TlsUtilities.WriteUint64(seqNo, array, 4);
		for (int i = 0; i < 12; i++)
		{
			byte[] array2;
			byte[] array3 = (array2 = array);
			int num = i;
			nint num2 = num;
			array3[num] = (byte)(array2[num2] ^ iv[i]);
		}
		return array;
	}

	protected virtual KeyParameter GenerateRecordMacKey(IStreamCipher cipher)
	{
		byte[] array = new byte[64];
		cipher.ProcessBytes(array, 0, array.Length, array, 0);
		KeyParameter result = new KeyParameter(array, 0, 32);
		Arrays.Fill(array, 0);
		return result;
	}

	protected virtual byte[] CalculateRecordMac(KeyParameter macKey, byte[] additionalData, byte[] buf, int off, int len)
	{
		IMac mac = new Poly1305();
		mac.Init(macKey);
		UpdateRecordMacText(mac, additionalData, 0, additionalData.Length);
		UpdateRecordMacText(mac, buf, off, len);
		UpdateRecordMacLength(mac, additionalData.Length);
		UpdateRecordMacLength(mac, len);
		return MacUtilities.DoFinal(mac);
	}

	protected virtual void UpdateRecordMacLength(IMac mac, int len)
	{
		byte[] array = Pack.UInt64_To_LE((ulong)len);
		mac.BlockUpdate(array, 0, array.Length);
	}

	protected virtual void UpdateRecordMacText(IMac mac, byte[] buf, int off, int len)
	{
		mac.BlockUpdate(buf, off, len);
		int num = len % 16;
		if (num != 0)
		{
			mac.BlockUpdate(Zeroes, 0, 16 - num);
		}
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
