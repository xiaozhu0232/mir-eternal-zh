using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.OpenSsl;

public class PemReader : Org.BouncyCastle.Utilities.IO.Pem.PemReader
{
	private readonly IPasswordFinder pFinder;

	static PemReader()
	{
	}

	public PemReader(TextReader reader)
		: this(reader, null)
	{
	}

	public PemReader(TextReader reader, IPasswordFinder pFinder)
		: base(reader)
	{
		this.pFinder = pFinder;
	}

	public object ReadObject()
	{
		PemObject pemObject = ReadPemObject();
		if (pemObject == null)
		{
			return null;
		}
		if (Platform.EndsWith(pemObject.Type, "PRIVATE KEY"))
		{
			return ReadPrivateKey(pemObject);
		}
		switch (pemObject.Type)
		{
		case "PUBLIC KEY":
			return ReadPublicKey(pemObject);
		case "RSA PUBLIC KEY":
			return ReadRsaPublicKey(pemObject);
		case "CERTIFICATE REQUEST":
		case "NEW CERTIFICATE REQUEST":
			return ReadCertificateRequest(pemObject);
		case "CERTIFICATE":
		case "X509 CERTIFICATE":
			return ReadCertificate(pemObject);
		case "PKCS7":
		case "CMS":
			return ReadPkcs7(pemObject);
		case "X509 CRL":
			return ReadCrl(pemObject);
		case "ATTRIBUTE CERTIFICATE":
			return ReadAttributeCertificate(pemObject);
		default:
			throw new IOException("unrecognised object: " + pemObject.Type);
		}
	}

	private AsymmetricKeyParameter ReadRsaPublicKey(PemObject pemObject)
	{
		RsaPublicKeyStructure instance = RsaPublicKeyStructure.GetInstance(Asn1Object.FromByteArray(pemObject.Content));
		return new RsaKeyParameters(isPrivate: false, instance.Modulus, instance.PublicExponent);
	}

	private AsymmetricKeyParameter ReadPublicKey(PemObject pemObject)
	{
		return PublicKeyFactory.CreateKey(pemObject.Content);
	}

	private X509Certificate ReadCertificate(PemObject pemObject)
	{
		try
		{
			return new X509CertificateParser().ReadCertificate(pemObject.Content);
		}
		catch (Exception ex)
		{
			throw new PemException("problem parsing cert: " + ex.ToString());
		}
	}

	private X509Crl ReadCrl(PemObject pemObject)
	{
		try
		{
			return new X509CrlParser().ReadCrl(pemObject.Content);
		}
		catch (Exception ex)
		{
			throw new PemException("problem parsing cert: " + ex.ToString());
		}
	}

	private Pkcs10CertificationRequest ReadCertificateRequest(PemObject pemObject)
	{
		try
		{
			return new Pkcs10CertificationRequest(pemObject.Content);
		}
		catch (Exception ex)
		{
			throw new PemException("problem parsing cert: " + ex.ToString());
		}
	}

	private IX509AttributeCertificate ReadAttributeCertificate(PemObject pemObject)
	{
		return new X509V2AttributeCertificate(pemObject.Content);
	}

	private Org.BouncyCastle.Asn1.Cms.ContentInfo ReadPkcs7(PemObject pemObject)
	{
		try
		{
			return Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(Asn1Object.FromByteArray(pemObject.Content));
		}
		catch (Exception ex)
		{
			throw new PemException("problem parsing PKCS7 object: " + ex.ToString());
		}
	}

	private object ReadPrivateKey(PemObject pemObject)
	{
		string text = pemObject.Type.Substring(0, pemObject.Type.Length - "PRIVATE KEY".Length).Trim();
		byte[] array = pemObject.Content;
		IDictionary dictionary = Platform.CreateHashtable();
		foreach (PemHeader header in pemObject.Headers)
		{
			dictionary[header.Name] = header.Value;
		}
		string text2 = (string)dictionary["Proc-Type"];
		if (text2 == "4,ENCRYPTED")
		{
			if (pFinder == null)
			{
				throw new PasswordException("No password finder specified, but a password is required");
			}
			char[] password = pFinder.GetPassword();
			if (password == null)
			{
				throw new PasswordException("Password is null, but a password is required");
			}
			string text3 = (string)dictionary["DEK-Info"];
			string[] array2 = text3.Split(',');
			string dekAlgName = array2[0].Trim();
			byte[] iv = Hex.Decode(array2[1].Trim());
			array = PemUtilities.Crypt(encrypt: false, array, password, dekAlgName, iv);
		}
		try
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(array);
			AsymmetricKeyParameter publicParameter;
			AsymmetricKeyParameter asymmetricKeyParameter;
			switch (text)
			{
			case "RSA":
			{
				if (instance.Count != 9)
				{
					throw new PemException("malformed sequence in RSA private key");
				}
				RsaPrivateKeyStructure instance2 = RsaPrivateKeyStructure.GetInstance(instance);
				publicParameter = new RsaKeyParameters(isPrivate: false, instance2.Modulus, instance2.PublicExponent);
				asymmetricKeyParameter = new RsaPrivateCrtKeyParameters(instance2.Modulus, instance2.PublicExponent, instance2.PrivateExponent, instance2.Prime1, instance2.Prime2, instance2.Exponent1, instance2.Exponent2, instance2.Coefficient);
				break;
			}
			case "DSA":
			{
				if (instance.Count != 6)
				{
					throw new PemException("malformed sequence in DSA private key");
				}
				DerInteger derInteger = (DerInteger)instance[1];
				DerInteger derInteger2 = (DerInteger)instance[2];
				DerInteger derInteger3 = (DerInteger)instance[3];
				DerInteger derInteger4 = (DerInteger)instance[4];
				DerInteger derInteger5 = (DerInteger)instance[5];
				DsaParameters parameters = new DsaParameters(derInteger.Value, derInteger2.Value, derInteger3.Value);
				asymmetricKeyParameter = new DsaPrivateKeyParameters(derInteger5.Value, parameters);
				publicParameter = new DsaPublicKeyParameters(derInteger4.Value, parameters);
				break;
			}
			case "EC":
			{
				ECPrivateKeyStructure instance3 = ECPrivateKeyStructure.GetInstance(instance);
				AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(X9ObjectIdentifiers.IdECPublicKey, instance3.GetParameters());
				PrivateKeyInfo keyInfo = new PrivateKeyInfo(algorithmIdentifier, instance3.ToAsn1Object());
				asymmetricKeyParameter = PrivateKeyFactory.CreateKey(keyInfo);
				DerBitString publicKey = instance3.GetPublicKey();
				if (publicKey != null)
				{
					SubjectPublicKeyInfo keyInfo2 = new SubjectPublicKeyInfo(algorithmIdentifier, publicKey.GetBytes());
					publicParameter = PublicKeyFactory.CreateKey(keyInfo2);
				}
				else
				{
					publicParameter = ECKeyPairGenerator.GetCorrespondingPublicKey((ECPrivateKeyParameters)asymmetricKeyParameter);
				}
				break;
			}
			case "ENCRYPTED":
			{
				char[] password2 = pFinder.GetPassword();
				if (password2 == null)
				{
					throw new PasswordException("Password is null, but a password is required");
				}
				return PrivateKeyFactory.DecryptKey(password2, EncryptedPrivateKeyInfo.GetInstance(instance));
			}
			case "":
				return PrivateKeyFactory.CreateKey(PrivateKeyInfo.GetInstance(instance));
			default:
				throw new ArgumentException("Unknown key type: " + text, "type");
			}
			return new AsymmetricCipherKeyPair(publicParameter, asymmetricKeyParameter);
		}
		catch (IOException ex)
		{
			throw ex;
		}
		catch (Exception ex2)
		{
			throw new PemException("problem creating " + text + " private key: " + ex2.ToString());
		}
	}

	private static X9ECParameters GetCurveParameters(string name)
	{
		X9ECParameters byName = CustomNamedCurves.GetByName(name);
		if (byName == null)
		{
			byName = ECNamedCurveTable.GetByName(name);
		}
		if (byName == null)
		{
			throw new Exception("unknown curve name: " + name);
		}
		return byName;
	}
}
