using System;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace Org.BouncyCastle.OpenSsl;

public class Pkcs8Generator : PemObjectGenerator
{
	public static readonly string PbeSha1_RC4_128 = PkcsObjectIdentifiers.PbeWithShaAnd128BitRC4.Id;

	public static readonly string PbeSha1_RC4_40 = PkcsObjectIdentifiers.PbeWithShaAnd40BitRC4.Id;

	public static readonly string PbeSha1_3DES = PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc.Id;

	public static readonly string PbeSha1_2DES = PkcsObjectIdentifiers.PbeWithShaAnd2KeyTripleDesCbc.Id;

	public static readonly string PbeSha1_RC2_128 = PkcsObjectIdentifiers.PbeWithShaAnd128BitRC2Cbc.Id;

	public static readonly string PbeSha1_RC2_40 = PkcsObjectIdentifiers.PbewithShaAnd40BitRC2Cbc.Id;

	private char[] password;

	private string algorithm;

	private int iterationCount;

	private AsymmetricKeyParameter privKey;

	private SecureRandom random;

	public SecureRandom SecureRandom
	{
		set
		{
			random = value;
		}
	}

	public char[] Password
	{
		set
		{
			password = value;
		}
	}

	public int IterationCount
	{
		set
		{
			iterationCount = value;
		}
	}

	public Pkcs8Generator(AsymmetricKeyParameter privKey)
	{
		this.privKey = privKey;
	}

	public Pkcs8Generator(AsymmetricKeyParameter privKey, string algorithm)
	{
		this.privKey = privKey;
		this.algorithm = algorithm;
		iterationCount = 2048;
	}

	public PemObject Generate()
	{
		if (algorithm == null)
		{
			PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privKey);
			return new PemObject("PRIVATE KEY", privateKeyInfo.GetEncoded());
		}
		byte[] array = new byte[20];
		if (random == null)
		{
			random = new SecureRandom();
		}
		random.NextBytes(array);
		try
		{
			EncryptedPrivateKeyInfo encryptedPrivateKeyInfo = EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(algorithm, password, array, iterationCount, privKey);
			return new PemObject("ENCRYPTED PRIVATE KEY", encryptedPrivateKeyInfo.GetEncoded());
		}
		catch (Exception exception)
		{
			throw new PemGenerationException("Couldn't encrypt private key", exception);
		}
	}
}
