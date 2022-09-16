using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.OpenSsl;

public class MiscPemGenerator : PemObjectGenerator
{
	private object obj;

	private string algorithm;

	private char[] password;

	private SecureRandom random;

	public MiscPemGenerator(object obj)
	{
		this.obj = obj;
	}

	public MiscPemGenerator(object obj, string algorithm, char[] password, SecureRandom random)
	{
		this.obj = obj;
		this.algorithm = algorithm;
		this.password = password;
		this.random = random;
	}

	private static PemObject CreatePemObject(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj is AsymmetricCipherKeyPair)
		{
			return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private);
		}
		if (obj is PemObject)
		{
			return (PemObject)obj;
		}
		if (obj is PemObjectGenerator)
		{
			return ((PemObjectGenerator)obj).Generate();
		}
		string keyType;
		byte[] content;
		if (obj is X509Certificate)
		{
			keyType = "CERTIFICATE";
			try
			{
				content = ((X509Certificate)obj).GetEncoded();
			}
			catch (CertificateEncodingException ex)
			{
				throw new IOException("Cannot Encode object: " + ex.ToString());
			}
		}
		else if (obj is X509Crl)
		{
			keyType = "X509 CRL";
			try
			{
				content = ((X509Crl)obj).GetEncoded();
			}
			catch (CrlException ex2)
			{
				throw new IOException("Cannot Encode object: " + ex2.ToString());
			}
		}
		else if (obj is AsymmetricKeyParameter)
		{
			AsymmetricKeyParameter asymmetricKeyParameter = (AsymmetricKeyParameter)obj;
			if (asymmetricKeyParameter.IsPrivate)
			{
				content = EncodePrivateKey(asymmetricKeyParameter, out keyType);
			}
			else
			{
				keyType = "PUBLIC KEY";
				content = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(asymmetricKeyParameter).GetDerEncoded();
			}
		}
		else if (obj is IX509AttributeCertificate)
		{
			keyType = "ATTRIBUTE CERTIFICATE";
			content = ((X509V2AttributeCertificate)obj).GetEncoded();
		}
		else if (obj is Pkcs10CertificationRequest)
		{
			keyType = "CERTIFICATE REQUEST";
			content = ((Pkcs10CertificationRequest)obj).GetEncoded();
		}
		else
		{
			if (!(obj is Org.BouncyCastle.Asn1.Cms.ContentInfo))
			{
				throw new PemGenerationException("Object type not supported: " + Platform.GetTypeName(obj));
			}
			keyType = "PKCS7";
			content = ((Org.BouncyCastle.Asn1.Cms.ContentInfo)obj).GetEncoded();
		}
		return new PemObject(keyType, content);
	}

	private static PemObject CreatePemObject(object obj, string algorithm, char[] password, SecureRandom random)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		if (obj is AsymmetricCipherKeyPair)
		{
			return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private, algorithm, password, random);
		}
		string keyType = null;
		byte[] array = null;
		if (obj is AsymmetricKeyParameter)
		{
			AsymmetricKeyParameter asymmetricKeyParameter = (AsymmetricKeyParameter)obj;
			if (asymmetricKeyParameter.IsPrivate)
			{
				array = EncodePrivateKey(asymmetricKeyParameter, out keyType);
			}
		}
		if (keyType == null || array == null)
		{
			throw new PemGenerationException("Object type not supported: " + Platform.GetTypeName(obj));
		}
		string text = Platform.ToUpperInvariant(algorithm);
		if (text == "DESEDE")
		{
			text = "DES-EDE3-CBC";
		}
		int num = (Platform.StartsWith(text, "AES-") ? 16 : 8);
		byte[] array2 = new byte[num];
		random.NextBytes(array2);
		byte[] content = PemUtilities.Crypt(encrypt: true, array, password, text, array2);
		IList list = Platform.CreateArrayList(2);
		list.Add(new PemHeader("Proc-Type", "4,ENCRYPTED"));
		list.Add(new PemHeader("DEK-Info", text + "," + Hex.ToHexString(array2)));
		return new PemObject(keyType, list, content);
	}

	private static byte[] EncodePrivateKey(AsymmetricKeyParameter akp, out string keyType)
	{
		PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp);
		AlgorithmIdentifier privateKeyAlgorithm = privateKeyInfo.PrivateKeyAlgorithm;
		DerObjectIdentifier derObjectIdentifier = privateKeyAlgorithm.Algorithm;
		if (derObjectIdentifier.Equals(X9ObjectIdentifiers.IdDsa))
		{
			keyType = "DSA PRIVATE KEY";
			DsaParameter instance = DsaParameter.GetInstance(privateKeyAlgorithm.Parameters);
			BigInteger x = ((DsaPrivateKeyParameters)akp).X;
			BigInteger value = instance.G.ModPow(x, instance.P);
			return new DerSequence(new DerInteger(0), new DerInteger(instance.P), new DerInteger(instance.Q), new DerInteger(instance.G), new DerInteger(value), new DerInteger(x)).GetEncoded();
		}
		if (derObjectIdentifier.Equals(PkcsObjectIdentifiers.RsaEncryption))
		{
			keyType = "RSA PRIVATE KEY";
			return privateKeyInfo.ParsePrivateKey().GetEncoded();
		}
		if (derObjectIdentifier.Equals(CryptoProObjectIdentifiers.GostR3410x2001) || derObjectIdentifier.Equals(X9ObjectIdentifiers.IdECPublicKey))
		{
			keyType = "EC PRIVATE KEY";
			return privateKeyInfo.ParsePrivateKey().GetEncoded();
		}
		keyType = "PRIVATE KEY";
		return privateKeyInfo.GetEncoded();
	}

	public PemObject Generate()
	{
		try
		{
			if (algorithm != null)
			{
				return CreatePemObject(obj, algorithm, password, random);
			}
			return CreatePemObject(obj);
		}
		catch (IOException exception)
		{
			throw new PemGenerationException("encoding exception", exception);
		}
	}
}
