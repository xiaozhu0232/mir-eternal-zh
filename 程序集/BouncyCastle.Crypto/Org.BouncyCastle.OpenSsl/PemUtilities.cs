using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.OpenSsl;

internal sealed class PemUtilities
{
	private enum PemBaseAlg
	{
		AES_128,
		AES_192,
		AES_256,
		BF,
		DES,
		DES_EDE,
		DES_EDE3,
		RC2,
		RC2_40,
		RC2_64
	}

	private enum PemMode
	{
		CBC,
		CFB,
		ECB,
		OFB
	}

	static PemUtilities()
	{
		((PemBaseAlg)Enums.GetArbitraryValue(typeof(PemBaseAlg))).ToString();
		((PemMode)Enums.GetArbitraryValue(typeof(PemMode))).ToString();
	}

	private static void ParseDekAlgName(string dekAlgName, out PemBaseAlg baseAlg, out PemMode mode)
	{
		try
		{
			mode = PemMode.ECB;
			if (dekAlgName == "DES-EDE" || dekAlgName == "DES-EDE3")
			{
				baseAlg = (PemBaseAlg)Enums.GetEnumValue(typeof(PemBaseAlg), dekAlgName);
				return;
			}
			int num = dekAlgName.LastIndexOf('-');
			if (num >= 0)
			{
				baseAlg = (PemBaseAlg)Enums.GetEnumValue(typeof(PemBaseAlg), dekAlgName.Substring(0, num));
				mode = (PemMode)Enums.GetEnumValue(typeof(PemMode), dekAlgName.Substring(num + 1));
				return;
			}
		}
		catch (ArgumentException)
		{
		}
		throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
	}

	internal static byte[] Crypt(bool encrypt, byte[] bytes, char[] password, string dekAlgName, byte[] iv)
	{
		ParseDekAlgName(dekAlgName, out var baseAlg, out var mode);
		string text;
		switch (mode)
		{
		case PemMode.CBC:
		case PemMode.ECB:
			text = "PKCS5Padding";
			break;
		case PemMode.CFB:
		case PemMode.OFB:
			text = "NoPadding";
			break;
		default:
			throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
		}
		byte[] array = iv;
		string text2;
		switch (baseAlg)
		{
		case PemBaseAlg.AES_128:
		case PemBaseAlg.AES_192:
		case PemBaseAlg.AES_256:
			text2 = "AES";
			if (array.Length > 8)
			{
				array = new byte[8];
				Array.Copy(iv, 0, array, 0, array.Length);
			}
			break;
		case PemBaseAlg.BF:
			text2 = "BLOWFISH";
			break;
		case PemBaseAlg.DES:
			text2 = "DES";
			break;
		case PemBaseAlg.DES_EDE:
		case PemBaseAlg.DES_EDE3:
			text2 = "DESede";
			break;
		case PemBaseAlg.RC2:
		case PemBaseAlg.RC2_40:
		case PemBaseAlg.RC2_64:
			text2 = "RC2";
			break;
		default:
			throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
		}
		string algorithm = string.Concat(text2, "/", mode, "/", text);
		IBufferedCipher cipher = CipherUtilities.GetCipher(algorithm);
		ICipherParameters parameters = GetCipherParameters(password, baseAlg, array);
		if (mode != PemMode.ECB)
		{
			parameters = new ParametersWithIV(parameters, iv);
		}
		cipher.Init(encrypt, parameters);
		return cipher.DoFinal(bytes);
	}

	private static ICipherParameters GetCipherParameters(char[] password, PemBaseAlg baseAlg, byte[] salt)
	{
		int keySize;
		string algorithm;
		switch (baseAlg)
		{
		case PemBaseAlg.AES_128:
			keySize = 128;
			algorithm = "AES128";
			break;
		case PemBaseAlg.AES_192:
			keySize = 192;
			algorithm = "AES192";
			break;
		case PemBaseAlg.AES_256:
			keySize = 256;
			algorithm = "AES256";
			break;
		case PemBaseAlg.BF:
			keySize = 128;
			algorithm = "BLOWFISH";
			break;
		case PemBaseAlg.DES:
			keySize = 64;
			algorithm = "DES";
			break;
		case PemBaseAlg.DES_EDE:
			keySize = 128;
			algorithm = "DESEDE";
			break;
		case PemBaseAlg.DES_EDE3:
			keySize = 192;
			algorithm = "DESEDE3";
			break;
		case PemBaseAlg.RC2:
			keySize = 128;
			algorithm = "RC2";
			break;
		case PemBaseAlg.RC2_40:
			keySize = 40;
			algorithm = "RC2";
			break;
		case PemBaseAlg.RC2_64:
			keySize = 64;
			algorithm = "RC2";
			break;
		default:
			return null;
		}
		OpenSslPbeParametersGenerator openSslPbeParametersGenerator = new OpenSslPbeParametersGenerator();
		openSslPbeParametersGenerator.Init(PbeParametersGenerator.Pkcs5PasswordToBytes(password), salt);
		return openSslPbeParametersGenerator.GenerateDerivedParameters(algorithm, keySize);
	}
}
