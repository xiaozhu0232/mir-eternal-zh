using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Security;

public sealed class DotNetUtilities
{
	private DotNetUtilities()
	{
	}

	public static System.Security.Cryptography.X509Certificates.X509Certificate ToX509Certificate(X509CertificateStructure x509Struct)
	{
		return new System.Security.Cryptography.X509Certificates.X509Certificate(x509Struct.GetDerEncoded());
	}

	public static System.Security.Cryptography.X509Certificates.X509Certificate ToX509Certificate(Org.BouncyCastle.X509.X509Certificate x509Cert)
	{
		return new System.Security.Cryptography.X509Certificates.X509Certificate(x509Cert.GetEncoded());
	}

	public static Org.BouncyCastle.X509.X509Certificate FromX509Certificate(System.Security.Cryptography.X509Certificates.X509Certificate x509Cert)
	{
		return new X509CertificateParser().ReadCertificate(x509Cert.GetRawCertData());
	}

	public static AsymmetricCipherKeyPair GetDsaKeyPair(DSA dsa)
	{
		return GetDsaKeyPair(dsa.ExportParameters(includePrivateParameters: true));
	}

	public static AsymmetricCipherKeyPair GetDsaKeyPair(DSAParameters dp)
	{
		DsaValidationParameters parameters = ((dp.Seed != null) ? new DsaValidationParameters(dp.Seed, dp.Counter) : null);
		DsaParameters parameters2 = new DsaParameters(new BigInteger(1, dp.P), new BigInteger(1, dp.Q), new BigInteger(1, dp.G), parameters);
		DsaPublicKeyParameters publicParameter = new DsaPublicKeyParameters(new BigInteger(1, dp.Y), parameters2);
		DsaPrivateKeyParameters privateParameter = new DsaPrivateKeyParameters(new BigInteger(1, dp.X), parameters2);
		return new AsymmetricCipherKeyPair(publicParameter, privateParameter);
	}

	public static DsaPublicKeyParameters GetDsaPublicKey(DSA dsa)
	{
		return GetDsaPublicKey(dsa.ExportParameters(includePrivateParameters: false));
	}

	public static DsaPublicKeyParameters GetDsaPublicKey(DSAParameters dp)
	{
		DsaValidationParameters parameters = ((dp.Seed != null) ? new DsaValidationParameters(dp.Seed, dp.Counter) : null);
		DsaParameters parameters2 = new DsaParameters(new BigInteger(1, dp.P), new BigInteger(1, dp.Q), new BigInteger(1, dp.G), parameters);
		return new DsaPublicKeyParameters(new BigInteger(1, dp.Y), parameters2);
	}

	public static AsymmetricCipherKeyPair GetRsaKeyPair(RSA rsa)
	{
		return GetRsaKeyPair(rsa.ExportParameters(includePrivateParameters: true));
	}

	public static AsymmetricCipherKeyPair GetRsaKeyPair(RSAParameters rp)
	{
		BigInteger modulus = new BigInteger(1, rp.Modulus);
		BigInteger bigInteger = new BigInteger(1, rp.Exponent);
		RsaKeyParameters publicParameter = new RsaKeyParameters(isPrivate: false, modulus, bigInteger);
		RsaPrivateCrtKeyParameters privateParameter = new RsaPrivateCrtKeyParameters(modulus, bigInteger, new BigInteger(1, rp.D), new BigInteger(1, rp.P), new BigInteger(1, rp.Q), new BigInteger(1, rp.DP), new BigInteger(1, rp.DQ), new BigInteger(1, rp.InverseQ));
		return new AsymmetricCipherKeyPair(publicParameter, privateParameter);
	}

	public static RsaKeyParameters GetRsaPublicKey(RSA rsa)
	{
		return GetRsaPublicKey(rsa.ExportParameters(includePrivateParameters: false));
	}

	public static RsaKeyParameters GetRsaPublicKey(RSAParameters rp)
	{
		return new RsaKeyParameters(isPrivate: false, new BigInteger(1, rp.Modulus), new BigInteger(1, rp.Exponent));
	}

	public static AsymmetricCipherKeyPair GetKeyPair(AsymmetricAlgorithm privateKey)
	{
		if (privateKey is DSA)
		{
			return GetDsaKeyPair((DSA)privateKey);
		}
		if (privateKey is RSA)
		{
			return GetRsaKeyPair((RSA)privateKey);
		}
		throw new ArgumentException("Unsupported algorithm specified", "privateKey");
	}

	public static RSA ToRSA(RsaKeyParameters rsaKey)
	{
		return CreateRSAProvider(ToRSAParameters(rsaKey));
	}

	public static RSA ToRSA(RsaKeyParameters rsaKey, CspParameters csp)
	{
		return CreateRSAProvider(ToRSAParameters(rsaKey), csp);
	}

	public static RSA ToRSA(RsaPrivateCrtKeyParameters privKey)
	{
		return CreateRSAProvider(ToRSAParameters(privKey));
	}

	public static RSA ToRSA(RsaPrivateCrtKeyParameters privKey, CspParameters csp)
	{
		return CreateRSAProvider(ToRSAParameters(privKey), csp);
	}

	public static RSA ToRSA(RsaPrivateKeyStructure privKey)
	{
		return CreateRSAProvider(ToRSAParameters(privKey));
	}

	public static RSA ToRSA(RsaPrivateKeyStructure privKey, CspParameters csp)
	{
		return CreateRSAProvider(ToRSAParameters(privKey), csp);
	}

	public static RSAParameters ToRSAParameters(RsaKeyParameters rsaKey)
	{
		RSAParameters result = default(RSAParameters);
		result.Modulus = rsaKey.Modulus.ToByteArrayUnsigned();
		if (rsaKey.IsPrivate)
		{
			result.D = ConvertRSAParametersField(rsaKey.Exponent, result.Modulus.Length);
		}
		else
		{
			result.Exponent = rsaKey.Exponent.ToByteArrayUnsigned();
		}
		return result;
	}

	public static RSAParameters ToRSAParameters(RsaPrivateCrtKeyParameters privKey)
	{
		RSAParameters result = default(RSAParameters);
		result.Modulus = privKey.Modulus.ToByteArrayUnsigned();
		result.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
		result.P = privKey.P.ToByteArrayUnsigned();
		result.Q = privKey.Q.ToByteArrayUnsigned();
		result.D = ConvertRSAParametersField(privKey.Exponent, result.Modulus.Length);
		result.DP = ConvertRSAParametersField(privKey.DP, result.P.Length);
		result.DQ = ConvertRSAParametersField(privKey.DQ, result.Q.Length);
		result.InverseQ = ConvertRSAParametersField(privKey.QInv, result.Q.Length);
		return result;
	}

	public static RSAParameters ToRSAParameters(RsaPrivateKeyStructure privKey)
	{
		RSAParameters result = default(RSAParameters);
		result.Modulus = privKey.Modulus.ToByteArrayUnsigned();
		result.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
		result.P = privKey.Prime1.ToByteArrayUnsigned();
		result.Q = privKey.Prime2.ToByteArrayUnsigned();
		result.D = ConvertRSAParametersField(privKey.PrivateExponent, result.Modulus.Length);
		result.DP = ConvertRSAParametersField(privKey.Exponent1, result.P.Length);
		result.DQ = ConvertRSAParametersField(privKey.Exponent2, result.Q.Length);
		result.InverseQ = ConvertRSAParametersField(privKey.Coefficient, result.Q.Length);
		return result;
	}

	private static byte[] ConvertRSAParametersField(BigInteger n, int size)
	{
		byte[] array = n.ToByteArrayUnsigned();
		if (array.Length == size)
		{
			return array;
		}
		if (array.Length > size)
		{
			throw new ArgumentException("Specified size too small", "size");
		}
		byte[] array2 = new byte[size];
		Array.Copy(array, 0, array2, size - array.Length, array.Length);
		return array2;
	}

	private static RSA CreateRSAProvider(RSAParameters rp)
	{
		CspParameters cspParameters = new CspParameters();
		cspParameters.KeyContainerName = $"BouncyCastle-{Guid.NewGuid()}";
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
		rSACryptoServiceProvider.ImportParameters(rp);
		return rSACryptoServiceProvider;
	}

	private static RSA CreateRSAProvider(RSAParameters rp, CspParameters csp)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(csp);
		rSACryptoServiceProvider.ImportParameters(rp);
		return rSACryptoServiceProvider;
	}
}
