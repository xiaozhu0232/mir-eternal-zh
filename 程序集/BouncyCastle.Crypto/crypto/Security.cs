using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace crypto;

public class Security
{
	public static string ComputeHash(string text, string salt)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		Sha512Digest sha512Digest = new Sha512Digest();
		Pkcs5S2ParametersGenerator pkcs5S2ParametersGenerator = new Pkcs5S2ParametersGenerator(sha512Digest);
		pkcs5S2ParametersGenerator.Init(bytes, Base64.Decode(salt), 2048);
		return Base64.ToBase64String(((KeyParameter)pkcs5S2ParametersGenerator.GenerateDerivedParameters(sha512Digest.GetDigestSize() * 8)).GetKey());
	}

	public static string Decrypt(string cipherText, string key, string iv)
	{
		IBufferedCipher bufferedCipher = CreateCipher(isEncryption: false, key, iv);
		byte[] array = bufferedCipher.DoFinal(Base64.Decode(cipherText));
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	public static string Encrypt(string plainText, string key, string iv)
	{
		IBufferedCipher bufferedCipher = CreateCipher(isEncryption: true, key, iv);
		return Base64.ToBase64String(bufferedCipher.DoFinal(Encoding.UTF8.GetBytes(plainText)));
	}

	public static string GenerateText(int size)
	{
		byte[] array = new byte[size];
		SecureRandom instance = SecureRandom.GetInstance("SHA256PRNG", autoSeed: true);
		instance.NextBytes(array);
		return Base64.ToBase64String(array);
	}

	private static IBufferedCipher CreateCipher(bool isEncryption, string key, string iv)
	{
		IBufferedCipher bufferedCipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new RijndaelEngine()), new ISO10126d2Padding());
		KeyParameter keyParameter = new KeyParameter(Base64.Decode(key));
		ICipherParameters parameters = ((iv == null || iv.Length < 1) ? ((ICipherParameters)keyParameter) : ((ICipherParameters)new ParametersWithIV(keyParameter, Base64.Decode(iv))));
		bufferedCipher.Init(isEncryption, parameters);
		return bufferedCipher;
	}
}
