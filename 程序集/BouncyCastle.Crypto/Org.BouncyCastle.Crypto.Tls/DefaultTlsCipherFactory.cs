using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace Org.BouncyCastle.Crypto.Tls;

public class DefaultTlsCipherFactory : AbstractTlsCipherFactory
{
	public override TlsCipher CreateCipher(TlsContext context, int encryptionAlgorithm, int macAlgorithm)
	{
		return encryptionAlgorithm switch
		{
			7 => CreateDesEdeCipher(context, macAlgorithm), 
			8 => CreateAESCipher(context, 16, macAlgorithm), 
			15 => CreateCipher_Aes_Ccm(context, 16, 16), 
			16 => CreateCipher_Aes_Ccm(context, 16, 8), 
			10 => CreateCipher_Aes_Gcm(context, 16, 16), 
			103 => CreateCipher_Aes_Ocb(context, 16, 12), 
			9 => CreateAESCipher(context, 32, macAlgorithm), 
			17 => CreateCipher_Aes_Ccm(context, 32, 16), 
			18 => CreateCipher_Aes_Ccm(context, 32, 8), 
			11 => CreateCipher_Aes_Gcm(context, 32, 16), 
			104 => CreateCipher_Aes_Ocb(context, 32, 12), 
			12 => CreateCamelliaCipher(context, 16, macAlgorithm), 
			19 => CreateCipher_Camellia_Gcm(context, 16, 16), 
			13 => CreateCamelliaCipher(context, 32, macAlgorithm), 
			20 => CreateCipher_Camellia_Gcm(context, 32, 16), 
			21 => CreateChaCha20Poly1305(context), 
			0 => CreateNullCipher(context, macAlgorithm), 
			2 => CreateRC4Cipher(context, 16, macAlgorithm), 
			14 => CreateSeedCipher(context, macAlgorithm), 
			_ => throw new TlsFatalAlert(80), 
		};
	}

	protected virtual TlsBlockCipher CreateAESCipher(TlsContext context, int cipherKeySize, int macAlgorithm)
	{
		return new TlsBlockCipher(context, CreateAesBlockCipher(), CreateAesBlockCipher(), CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm), cipherKeySize);
	}

	protected virtual TlsBlockCipher CreateCamelliaCipher(TlsContext context, int cipherKeySize, int macAlgorithm)
	{
		return new TlsBlockCipher(context, CreateCamelliaBlockCipher(), CreateCamelliaBlockCipher(), CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm), cipherKeySize);
	}

	protected virtual TlsCipher CreateChaCha20Poly1305(TlsContext context)
	{
		return new Chacha20Poly1305(context);
	}

	protected virtual TlsAeadCipher CreateCipher_Aes_Ccm(TlsContext context, int cipherKeySize, int macSize)
	{
		return new TlsAeadCipher(context, CreateAeadBlockCipher_Aes_Ccm(), CreateAeadBlockCipher_Aes_Ccm(), cipherKeySize, macSize);
	}

	protected virtual TlsAeadCipher CreateCipher_Aes_Gcm(TlsContext context, int cipherKeySize, int macSize)
	{
		return new TlsAeadCipher(context, CreateAeadBlockCipher_Aes_Gcm(), CreateAeadBlockCipher_Aes_Gcm(), cipherKeySize, macSize);
	}

	protected virtual TlsAeadCipher CreateCipher_Aes_Ocb(TlsContext context, int cipherKeySize, int macSize)
	{
		return new TlsAeadCipher(context, CreateAeadBlockCipher_Aes_Ocb(), CreateAeadBlockCipher_Aes_Ocb(), cipherKeySize, macSize, 2);
	}

	protected virtual TlsAeadCipher CreateCipher_Camellia_Gcm(TlsContext context, int cipherKeySize, int macSize)
	{
		return new TlsAeadCipher(context, CreateAeadBlockCipher_Camellia_Gcm(), CreateAeadBlockCipher_Camellia_Gcm(), cipherKeySize, macSize);
	}

	protected virtual TlsBlockCipher CreateDesEdeCipher(TlsContext context, int macAlgorithm)
	{
		return new TlsBlockCipher(context, CreateDesEdeBlockCipher(), CreateDesEdeBlockCipher(), CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm), 24);
	}

	protected virtual TlsNullCipher CreateNullCipher(TlsContext context, int macAlgorithm)
	{
		return new TlsNullCipher(context, CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm));
	}

	protected virtual TlsStreamCipher CreateRC4Cipher(TlsContext context, int cipherKeySize, int macAlgorithm)
	{
		return new TlsStreamCipher(context, CreateRC4StreamCipher(), CreateRC4StreamCipher(), CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm), cipherKeySize, usesNonce: false);
	}

	protected virtual TlsBlockCipher CreateSeedCipher(TlsContext context, int macAlgorithm)
	{
		return new TlsBlockCipher(context, CreateSeedBlockCipher(), CreateSeedBlockCipher(), CreateHMacDigest(macAlgorithm), CreateHMacDigest(macAlgorithm), 16);
	}

	protected virtual IBlockCipher CreateAesEngine()
	{
		return new AesEngine();
	}

	protected virtual IBlockCipher CreateCamelliaEngine()
	{
		return new CamelliaEngine();
	}

	protected virtual IBlockCipher CreateAesBlockCipher()
	{
		return new CbcBlockCipher(CreateAesEngine());
	}

	protected virtual IAeadBlockCipher CreateAeadBlockCipher_Aes_Ccm()
	{
		return new CcmBlockCipher(CreateAesEngine());
	}

	protected virtual IAeadBlockCipher CreateAeadBlockCipher_Aes_Gcm()
	{
		return new GcmBlockCipher(CreateAesEngine());
	}

	protected virtual IAeadBlockCipher CreateAeadBlockCipher_Aes_Ocb()
	{
		return new OcbBlockCipher(CreateAesEngine(), CreateAesEngine());
	}

	protected virtual IAeadBlockCipher CreateAeadBlockCipher_Camellia_Gcm()
	{
		return new GcmBlockCipher(CreateCamelliaEngine());
	}

	protected virtual IBlockCipher CreateCamelliaBlockCipher()
	{
		return new CbcBlockCipher(CreateCamelliaEngine());
	}

	protected virtual IBlockCipher CreateDesEdeBlockCipher()
	{
		return new CbcBlockCipher(new DesEdeEngine());
	}

	protected virtual IStreamCipher CreateRC4StreamCipher()
	{
		return new RC4Engine();
	}

	protected virtual IBlockCipher CreateSeedBlockCipher()
	{
		return new CbcBlockCipher(new SeedEngine());
	}

	protected virtual IDigest CreateHMacDigest(int macAlgorithm)
	{
		return macAlgorithm switch
		{
			0 => null, 
			1 => TlsUtilities.CreateHash(1), 
			2 => TlsUtilities.CreateHash(2), 
			3 => TlsUtilities.CreateHash(4), 
			4 => TlsUtilities.CreateHash(5), 
			5 => TlsUtilities.CreateHash(6), 
			_ => throw new TlsFatalAlert(80), 
		};
	}
}
