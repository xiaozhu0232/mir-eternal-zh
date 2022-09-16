using System;
using System.IO;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class TlsRsaUtilities
{
	public static byte[] GenerateEncryptedPreMasterSecret(TlsContext context, RsaKeyParameters rsaServerPublicKey, Stream output)
	{
		byte[] array = new byte[48];
		context.SecureRandom.NextBytes(array);
		TlsUtilities.WriteVersion(context.ClientVersion, array, 0);
		Pkcs1Encoding pkcs1Encoding = new Pkcs1Encoding(new RsaBlindedEngine());
		pkcs1Encoding.Init(forEncryption: true, new ParametersWithRandom(rsaServerPublicKey, context.SecureRandom));
		try
		{
			byte[] array2 = pkcs1Encoding.ProcessBlock(array, 0, array.Length);
			if (TlsUtilities.IsSsl(context))
			{
				output.Write(array2, 0, array2.Length);
				return array;
			}
			TlsUtilities.WriteOpaque16(array2, output);
			return array;
		}
		catch (InvalidCipherTextException alertCause)
		{
			throw new TlsFatalAlert(80, alertCause);
		}
	}

	public static byte[] SafeDecryptPreMasterSecret(TlsContext context, RsaKeyParameters rsaServerPrivateKey, byte[] encryptedPreMasterSecret)
	{
		ProtocolVersion clientVersion = context.ClientVersion;
		bool flag = false;
		byte[] array = new byte[48];
		context.SecureRandom.NextBytes(array);
		byte[] array2 = Arrays.Clone(array);
		try
		{
			Pkcs1Encoding pkcs1Encoding = new Pkcs1Encoding(new RsaBlindedEngine(), array);
			pkcs1Encoding.Init(forEncryption: false, new ParametersWithRandom(rsaServerPrivateKey, context.SecureRandom));
			array2 = pkcs1Encoding.ProcessBlock(encryptedPreMasterSecret, 0, encryptedPreMasterSecret.Length);
		}
		catch (Exception)
		{
		}
		if (!flag || !clientVersion.IsEqualOrEarlierVersionOf(ProtocolVersion.TLSv10))
		{
			int num = (clientVersion.MajorVersion ^ (array2[0] & 0xFF)) | (clientVersion.MinorVersion ^ (array2[1] & 0xFF));
			num |= num >> 1;
			num |= num >> 2;
			num |= num >> 4;
			int num2 = ~((num & 1) - 1);
			for (int i = 0; i < 48; i++)
			{
				array2[i] = (byte)((array2[i] & ~num2) | (array[i] & num2));
			}
		}
		return array2;
	}
}
