using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security;

public sealed class PbeUtilities
{
	private const string Pkcs5S1 = "Pkcs5S1";

	private const string Pkcs5S2 = "Pkcs5S2";

	private const string Pkcs12 = "Pkcs12";

	private const string OpenSsl = "OpenSsl";

	private static readonly IDictionary algorithms;

	private static readonly IDictionary algorithmType;

	private static readonly IDictionary oids;

	public static ICollection Algorithms => oids.Keys;

	private PbeUtilities()
	{
	}

	static PbeUtilities()
	{
		algorithms = Platform.CreateHashtable();
		algorithmType = Platform.CreateHashtable();
		oids = Platform.CreateHashtable();
		algorithms["PKCS5SCHEME1"] = "Pkcs5scheme1";
		algorithms["PKCS5SCHEME2"] = "Pkcs5scheme2";
		algorithms["PBKDF2"] = "Pkcs5scheme2";
		algorithms[PkcsObjectIdentifiers.IdPbeS2.Id] = "Pkcs5scheme2";
		algorithms["PBEWITHMD2ANDDES-CBC"] = "PBEwithMD2andDES-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithMD2AndDesCbc.Id] = "PBEwithMD2andDES-CBC";
		algorithms["PBEWITHMD2ANDRC2-CBC"] = "PBEwithMD2andRC2-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithMD2AndRC2Cbc.Id] = "PBEwithMD2andRC2-CBC";
		algorithms["PBEWITHMD5ANDDES-CBC"] = "PBEwithMD5andDES-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithMD5AndDesCbc.Id] = "PBEwithMD5andDES-CBC";
		algorithms["PBEWITHMD5ANDRC2-CBC"] = "PBEwithMD5andRC2-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithMD5AndRC2Cbc.Id] = "PBEwithMD5andRC2-CBC";
		algorithms["PBEWITHSHA1ANDDES"] = "PBEwithSHA-1andDES-CBC";
		algorithms["PBEWITHSHA-1ANDDES"] = "PBEwithSHA-1andDES-CBC";
		algorithms["PBEWITHSHA1ANDDES-CBC"] = "PBEwithSHA-1andDES-CBC";
		algorithms["PBEWITHSHA-1ANDDES-CBC"] = "PBEwithSHA-1andDES-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithSha1AndDesCbc.Id] = "PBEwithSHA-1andDES-CBC";
		algorithms["PBEWITHSHA1ANDRC2"] = "PBEwithSHA-1andRC2-CBC";
		algorithms["PBEWITHSHA-1ANDRC2"] = "PBEwithSHA-1andRC2-CBC";
		algorithms["PBEWITHSHA1ANDRC2-CBC"] = "PBEwithSHA-1andRC2-CBC";
		algorithms["PBEWITHSHA-1ANDRC2-CBC"] = "PBEwithSHA-1andRC2-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithSha1AndRC2Cbc.Id] = "PBEwithSHA-1andRC2-CBC";
		algorithms["PKCS12"] = "Pkcs12";
		algorithms[BCObjectIdentifiers.bc_pbe_sha1_pkcs12_aes128_cbc.Id] = "PBEwithSHA-1and128bitAES-CBC-BC";
		algorithms[BCObjectIdentifiers.bc_pbe_sha1_pkcs12_aes192_cbc.Id] = "PBEwithSHA-1and192bitAES-CBC-BC";
		algorithms[BCObjectIdentifiers.bc_pbe_sha1_pkcs12_aes256_cbc.Id] = "PBEwithSHA-1and256bitAES-CBC-BC";
		algorithms[BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes128_cbc.Id] = "PBEwithSHA-256and128bitAES-CBC-BC";
		algorithms[BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes192_cbc.Id] = "PBEwithSHA-256and192bitAES-CBC-BC";
		algorithms[BCObjectIdentifiers.bc_pbe_sha256_pkcs12_aes256_cbc.Id] = "PBEwithSHA-256and256bitAES-CBC-BC";
		algorithms["PBEWITHSHAAND128BITRC4"] = "PBEwithSHA-1and128bitRC4";
		algorithms["PBEWITHSHA1AND128BITRC4"] = "PBEwithSHA-1and128bitRC4";
		algorithms["PBEWITHSHA-1AND128BITRC4"] = "PBEwithSHA-1and128bitRC4";
		algorithms[PkcsObjectIdentifiers.PbeWithShaAnd128BitRC4.Id] = "PBEwithSHA-1and128bitRC4";
		algorithms["PBEWITHSHAAND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		algorithms["PBEWITHSHA1AND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		algorithms["PBEWITHSHA-1AND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		algorithms[PkcsObjectIdentifiers.PbeWithShaAnd40BitRC4.Id] = "PBEwithSHA-1and40bitRC4";
		algorithms["PBEWITHSHAAND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHAAND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHA1AND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHA1AND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHA-1AND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHA-1AND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc.Id] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		algorithms["PBEWITHSHAAND2-KEYDESEDE-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHAAND2-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHA1AND2-KEYDESEDE-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHA1AND2-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHA-1AND2-KEYDESEDE-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHA-1AND2-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithShaAnd2KeyTripleDesCbc.Id] = "PBEwithSHA-1and2-keyDESEDE-CBC";
		algorithms["PBEWITHSHAAND128BITRC2-CBC"] = "PBEwithSHA-1and128bitRC2-CBC";
		algorithms["PBEWITHSHA1AND128BITRC2-CBC"] = "PBEwithSHA-1and128bitRC2-CBC";
		algorithms["PBEWITHSHA-1AND128BITRC2-CBC"] = "PBEwithSHA-1and128bitRC2-CBC";
		algorithms[PkcsObjectIdentifiers.PbeWithShaAnd128BitRC2Cbc.Id] = "PBEwithSHA-1and128bitRC2-CBC";
		algorithms["PBEWITHSHAAND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		algorithms["PBEWITHSHA1AND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		algorithms["PBEWITHSHA-1AND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		algorithms[PkcsObjectIdentifiers.PbewithShaAnd40BitRC2Cbc.Id] = "PBEwithSHA-1and40bitRC2-CBC";
		algorithms["PBEWITHSHAAND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		algorithms["PBEWITHSHA1AND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		algorithms["PBEWITHSHA-1AND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		algorithms["PBEWITHSHAAND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		algorithms["PBEWITHSHA1AND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		algorithms["PBEWITHSHA-1AND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		algorithms["PBEWITHSHAAND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		algorithms["PBEWITHSHA1AND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		algorithms["PBEWITHSHA-1AND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		algorithms["PBEWITHSHA256AND128BITAES-CBC-BC"] = "PBEwithSHA-256and128bitAES-CBC-BC";
		algorithms["PBEWITHSHA-256AND128BITAES-CBC-BC"] = "PBEwithSHA-256and128bitAES-CBC-BC";
		algorithms["PBEWITHSHA256AND192BITAES-CBC-BC"] = "PBEwithSHA-256and192bitAES-CBC-BC";
		algorithms["PBEWITHSHA-256AND192BITAES-CBC-BC"] = "PBEwithSHA-256and192bitAES-CBC-BC";
		algorithms["PBEWITHSHA256AND256BITAES-CBC-BC"] = "PBEwithSHA-256and256bitAES-CBC-BC";
		algorithms["PBEWITHSHA-256AND256BITAES-CBC-BC"] = "PBEwithSHA-256and256bitAES-CBC-BC";
		algorithms["PBEWITHSHAANDIDEA"] = "PBEwithSHA-1andIDEA-CBC";
		algorithms["PBEWITHSHAANDIDEA-CBC"] = "PBEwithSHA-1andIDEA-CBC";
		algorithms["PBEWITHSHAANDTWOFISH"] = "PBEwithSHA-1andTWOFISH-CBC";
		algorithms["PBEWITHSHAANDTWOFISH-CBC"] = "PBEwithSHA-1andTWOFISH-CBC";
		algorithms["PBEWITHHMACSHA1"] = "PBEwithHmacSHA-1";
		algorithms["PBEWITHHMACSHA-1"] = "PBEwithHmacSHA-1";
		algorithms[OiwObjectIdentifiers.IdSha1.Id] = "PBEwithHmacSHA-1";
		algorithms["PBEWITHHMACSHA224"] = "PBEwithHmacSHA-224";
		algorithms["PBEWITHHMACSHA-224"] = "PBEwithHmacSHA-224";
		algorithms[NistObjectIdentifiers.IdSha224.Id] = "PBEwithHmacSHA-224";
		algorithms["PBEWITHHMACSHA256"] = "PBEwithHmacSHA-256";
		algorithms["PBEWITHHMACSHA-256"] = "PBEwithHmacSHA-256";
		algorithms[NistObjectIdentifiers.IdSha256.Id] = "PBEwithHmacSHA-256";
		algorithms["PBEWITHHMACRIPEMD128"] = "PBEwithHmacRipeMD128";
		algorithms[TeleTrusTObjectIdentifiers.RipeMD128.Id] = "PBEwithHmacRipeMD128";
		algorithms["PBEWITHHMACRIPEMD160"] = "PBEwithHmacRipeMD160";
		algorithms[TeleTrusTObjectIdentifiers.RipeMD160.Id] = "PBEwithHmacRipeMD160";
		algorithms["PBEWITHHMACRIPEMD256"] = "PBEwithHmacRipeMD256";
		algorithms[TeleTrusTObjectIdentifiers.RipeMD256.Id] = "PBEwithHmacRipeMD256";
		algorithms["PBEWITHHMACTIGER"] = "PBEwithHmacTiger";
		algorithms["PBEWITHMD5AND128BITAES-CBC-OPENSSL"] = "PBEwithMD5and128bitAES-CBC-OpenSSL";
		algorithms["PBEWITHMD5AND192BITAES-CBC-OPENSSL"] = "PBEwithMD5and192bitAES-CBC-OpenSSL";
		algorithms["PBEWITHMD5AND256BITAES-CBC-OPENSSL"] = "PBEwithMD5and256bitAES-CBC-OpenSSL";
		algorithmType["Pkcs5scheme1"] = "Pkcs5S1";
		algorithmType["Pkcs5scheme2"] = "Pkcs5S2";
		algorithmType["PBEwithMD2andDES-CBC"] = "Pkcs5S1";
		algorithmType["PBEwithMD2andRC2-CBC"] = "Pkcs5S1";
		algorithmType["PBEwithMD5andDES-CBC"] = "Pkcs5S1";
		algorithmType["PBEwithMD5andRC2-CBC"] = "Pkcs5S1";
		algorithmType["PBEwithSHA-1andDES-CBC"] = "Pkcs5S1";
		algorithmType["PBEwithSHA-1andRC2-CBC"] = "Pkcs5S1";
		algorithmType["Pkcs12"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and128bitRC4"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and40bitRC4"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and3-keyDESEDE-CBC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and2-keyDESEDE-CBC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and128bitRC2-CBC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and40bitRC2-CBC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and128bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and192bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1and256bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-256and128bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-256and192bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-256and256bitAES-CBC-BC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1andIDEA-CBC"] = "Pkcs12";
		algorithmType["PBEwithSHA-1andTWOFISH-CBC"] = "Pkcs12";
		algorithmType["PBEwithHmacSHA-1"] = "Pkcs12";
		algorithmType["PBEwithHmacSHA-224"] = "Pkcs12";
		algorithmType["PBEwithHmacSHA-256"] = "Pkcs12";
		algorithmType["PBEwithHmacRipeMD128"] = "Pkcs12";
		algorithmType["PBEwithHmacRipeMD160"] = "Pkcs12";
		algorithmType["PBEwithHmacRipeMD256"] = "Pkcs12";
		algorithmType["PBEwithHmacTiger"] = "Pkcs12";
		algorithmType["PBEwithMD5and128bitAES-CBC-OpenSSL"] = "OpenSsl";
		algorithmType["PBEwithMD5and192bitAES-CBC-OpenSSL"] = "OpenSsl";
		algorithmType["PBEwithMD5and256bitAES-CBC-OpenSSL"] = "OpenSsl";
		oids["PBEwithMD2andDES-CBC"] = PkcsObjectIdentifiers.PbeWithMD2AndDesCbc;
		oids["PBEwithMD2andRC2-CBC"] = PkcsObjectIdentifiers.PbeWithMD2AndRC2Cbc;
		oids["PBEwithMD5andDES-CBC"] = PkcsObjectIdentifiers.PbeWithMD5AndDesCbc;
		oids["PBEwithMD5andRC2-CBC"] = PkcsObjectIdentifiers.PbeWithMD5AndRC2Cbc;
		oids["PBEwithSHA-1andDES-CBC"] = PkcsObjectIdentifiers.PbeWithSha1AndDesCbc;
		oids["PBEwithSHA-1andRC2-CBC"] = PkcsObjectIdentifiers.PbeWithSha1AndRC2Cbc;
		oids["PBEwithSHA-1and128bitRC4"] = PkcsObjectIdentifiers.PbeWithShaAnd128BitRC4;
		oids["PBEwithSHA-1and40bitRC4"] = PkcsObjectIdentifiers.PbeWithShaAnd40BitRC4;
		oids["PBEwithSHA-1and3-keyDESEDE-CBC"] = PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc;
		oids["PBEwithSHA-1and2-keyDESEDE-CBC"] = PkcsObjectIdentifiers.PbeWithShaAnd2KeyTripleDesCbc;
		oids["PBEwithSHA-1and128bitRC2-CBC"] = PkcsObjectIdentifiers.PbeWithShaAnd128BitRC2Cbc;
		oids["PBEwithSHA-1and40bitRC2-CBC"] = PkcsObjectIdentifiers.PbewithShaAnd40BitRC2Cbc;
		oids["PBEwithHmacSHA-1"] = OiwObjectIdentifiers.IdSha1;
		oids["PBEwithHmacSHA-224"] = NistObjectIdentifiers.IdSha224;
		oids["PBEwithHmacSHA-256"] = NistObjectIdentifiers.IdSha256;
		oids["PBEwithHmacRipeMD128"] = TeleTrusTObjectIdentifiers.RipeMD128;
		oids["PBEwithHmacRipeMD160"] = TeleTrusTObjectIdentifiers.RipeMD160;
		oids["PBEwithHmacRipeMD256"] = TeleTrusTObjectIdentifiers.RipeMD256;
		oids["Pkcs5scheme2"] = PkcsObjectIdentifiers.IdPbeS2;
	}

	private static PbeParametersGenerator MakePbeGenerator(string type, IDigest digest, byte[] key, byte[] salt, int iterationCount)
	{
		PbeParametersGenerator pbeParametersGenerator;
		if (type.Equals("Pkcs5S1"))
		{
			pbeParametersGenerator = new Pkcs5S1ParametersGenerator(digest);
		}
		else if (type.Equals("Pkcs5S2"))
		{
			pbeParametersGenerator = new Pkcs5S2ParametersGenerator(digest);
		}
		else if (type.Equals("Pkcs12"))
		{
			pbeParametersGenerator = new Pkcs12ParametersGenerator(digest);
		}
		else
		{
			if (!type.Equals("OpenSsl"))
			{
				throw new ArgumentException("Unknown PBE type: " + type, "type");
			}
			pbeParametersGenerator = new OpenSslPbeParametersGenerator();
		}
		pbeParametersGenerator.Init(key, salt, iterationCount);
		return pbeParametersGenerator;
	}

	public static DerObjectIdentifier GetObjectIdentifier(string mechanism)
	{
		mechanism = (string)algorithms[Platform.ToUpperInvariant(mechanism)];
		if (mechanism != null)
		{
			return (DerObjectIdentifier)oids[mechanism];
		}
		return null;
	}

	public static bool IsPkcs12(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (text != null)
		{
			return "Pkcs12".Equals(algorithmType[text]);
		}
		return false;
	}

	public static bool IsPkcs5Scheme1(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (text != null)
		{
			return "Pkcs5S1".Equals(algorithmType[text]);
		}
		return false;
	}

	public static bool IsPkcs5Scheme2(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (text != null)
		{
			return "Pkcs5S2".Equals(algorithmType[text]);
		}
		return false;
	}

	public static bool IsOpenSsl(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (text != null)
		{
			return "OpenSsl".Equals(algorithmType[text]);
		}
		return false;
	}

	public static bool IsPbeAlgorithm(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (text != null)
		{
			return algorithmType[text] != null;
		}
		return false;
	}

	public static Asn1Encodable GenerateAlgorithmParameters(DerObjectIdentifier algorithmOid, byte[] salt, int iterationCount)
	{
		return GenerateAlgorithmParameters(algorithmOid.Id, salt, iterationCount);
	}

	public static Asn1Encodable GenerateAlgorithmParameters(string algorithm, byte[] salt, int iterationCount)
	{
		if (IsPkcs12(algorithm))
		{
			return new Pkcs12PbeParams(salt, iterationCount);
		}
		if (IsPkcs5Scheme2(algorithm))
		{
			return new Pbkdf2Params(salt, iterationCount);
		}
		return new PbeParameter(salt, iterationCount);
	}

	public static Asn1Encodable GenerateAlgorithmParameters(DerObjectIdentifier cipherAlgorithm, DerObjectIdentifier hashAlgorithm, byte[] salt, int iterationCount, SecureRandom secureRandom)
	{
		if (NistObjectIdentifiers.IdAes128Cbc.Equals(cipherAlgorithm) || NistObjectIdentifiers.IdAes192Cbc.Equals(cipherAlgorithm) || NistObjectIdentifiers.IdAes256Cbc.Equals(cipherAlgorithm) || NistObjectIdentifiers.IdAes128Cfb.Equals(cipherAlgorithm) || NistObjectIdentifiers.IdAes192Cfb.Equals(cipherAlgorithm) || NistObjectIdentifiers.IdAes256Cfb.Equals(cipherAlgorithm))
		{
			byte[] array = new byte[16];
			secureRandom.NextBytes(array);
			EncryptionScheme encScheme = new EncryptionScheme(cipherAlgorithm, new DerOctetString(array));
			KeyDerivationFunc keyDevFunc = new KeyDerivationFunc(PkcsObjectIdentifiers.IdPbkdf2, new Pbkdf2Params(salt, iterationCount, new AlgorithmIdentifier(hashAlgorithm, DerNull.Instance)));
			return new PbeS2Parameters(keyDevFunc, encScheme);
		}
		throw new ArgumentException("unknown cipher: " + cipherAlgorithm);
	}

	public static ICipherParameters GenerateCipherParameters(DerObjectIdentifier algorithmOid, char[] password, Asn1Encodable pbeParameters)
	{
		return GenerateCipherParameters(algorithmOid.Id, password, wrongPkcs12Zero: false, pbeParameters);
	}

	public static ICipherParameters GenerateCipherParameters(DerObjectIdentifier algorithmOid, char[] password, bool wrongPkcs12Zero, Asn1Encodable pbeParameters)
	{
		return GenerateCipherParameters(algorithmOid.Id, password, wrongPkcs12Zero, pbeParameters);
	}

	public static ICipherParameters GenerateCipherParameters(AlgorithmIdentifier algID, char[] password)
	{
		return GenerateCipherParameters(algID.Algorithm.Id, password, wrongPkcs12Zero: false, algID.Parameters);
	}

	public static ICipherParameters GenerateCipherParameters(AlgorithmIdentifier algID, char[] password, bool wrongPkcs12Zero)
	{
		return GenerateCipherParameters(algID.Algorithm.Id, password, wrongPkcs12Zero, algID.Parameters);
	}

	public static ICipherParameters GenerateCipherParameters(string algorithm, char[] password, Asn1Encodable pbeParameters)
	{
		return GenerateCipherParameters(algorithm, password, wrongPkcs12Zero: false, pbeParameters);
	}

	public static ICipherParameters GenerateCipherParameters(string algorithm, char[] password, bool wrongPkcs12Zero, Asn1Encodable pbeParameters)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		byte[] array = null;
		byte[] salt = null;
		int iterationCount = 0;
		if (IsPkcs12(text))
		{
			Pkcs12PbeParams instance = Pkcs12PbeParams.GetInstance(pbeParameters);
			salt = instance.GetIV();
			iterationCount = instance.Iterations.IntValue;
			array = PbeParametersGenerator.Pkcs12PasswordToBytes(password, wrongPkcs12Zero);
		}
		else if (!IsPkcs5Scheme2(text))
		{
			PbeParameter instance2 = PbeParameter.GetInstance(pbeParameters);
			salt = instance2.GetSalt();
			iterationCount = instance2.IterationCount.IntValue;
			array = PbeParametersGenerator.Pkcs5PasswordToBytes(password);
		}
		ICipherParameters parameters = null;
		if (IsPkcs5Scheme2(text))
		{
			PbeS2Parameters instance3 = PbeS2Parameters.GetInstance(pbeParameters.ToAsn1Object());
			AlgorithmIdentifier encryptionScheme = instance3.EncryptionScheme;
			DerObjectIdentifier algorithm2 = encryptionScheme.Algorithm;
			Asn1Object obj = encryptionScheme.Parameters.ToAsn1Object();
			Pbkdf2Params instance4 = Pbkdf2Params.GetInstance(instance3.KeyDerivationFunc.Parameters.ToAsn1Object());
			IDigest digest = DigestUtilities.GetDigest(instance4.Prf.Algorithm);
			byte[] array2;
			if (algorithm2.Equals(PkcsObjectIdentifiers.RC2Cbc))
			{
				RC2CbcParameter instance5 = RC2CbcParameter.GetInstance(obj);
				array2 = instance5.GetIV();
			}
			else
			{
				array2 = Asn1OctetString.GetInstance(obj).GetOctets();
			}
			salt = instance4.GetSalt();
			iterationCount = instance4.IterationCount.IntValue;
			array = PbeParametersGenerator.Pkcs5PasswordToBytes(password);
			int keySize = ((instance4.KeyLength != null) ? (instance4.KeyLength.IntValue * 8) : GeneratorUtilities.GetDefaultKeySize(algorithm2));
			PbeParametersGenerator pbeParametersGenerator = MakePbeGenerator((string)algorithmType[text], digest, array, salt, iterationCount);
			parameters = pbeParametersGenerator.GenerateDerivedParameters(algorithm2.Id, keySize);
			if (array2 != null && !Arrays.AreEqual(array2, new byte[array2.Length]))
			{
				parameters = new ParametersWithIV(parameters, array2);
			}
		}
		else if (Platform.StartsWith(text, "PBEwithSHA-1"))
		{
			PbeParametersGenerator pbeParametersGenerator2 = MakePbeGenerator((string)algorithmType[text], new Sha1Digest(), array, salt, iterationCount);
			if (text.Equals("PBEwithSHA-1and128bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("AES", 128, 128);
			}
			else if (text.Equals("PBEwithSHA-1and192bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("AES", 192, 128);
			}
			else if (text.Equals("PBEwithSHA-1and256bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("AES", 256, 128);
			}
			else if (text.Equals("PBEwithSHA-1and128bitRC4"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("RC4", 128);
			}
			else if (text.Equals("PBEwithSHA-1and40bitRC4"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("RC4", 40);
			}
			else if (text.Equals("PBEwithSHA-1and3-keyDESEDE-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("DESEDE", 192, 64);
			}
			else if (text.Equals("PBEwithSHA-1and2-keyDESEDE-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("DESEDE", 128, 64);
			}
			else if (text.Equals("PBEwithSHA-1and128bitRC2-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("RC2", 128, 64);
			}
			else if (text.Equals("PBEwithSHA-1and40bitRC2-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("RC2", 40, 64);
			}
			else if (text.Equals("PBEwithSHA-1andDES-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("DES", 64, 64);
			}
			else if (text.Equals("PBEwithSHA-1andRC2-CBC"))
			{
				parameters = pbeParametersGenerator2.GenerateDerivedParameters("RC2", 64, 64);
			}
		}
		else if (Platform.StartsWith(text, "PBEwithSHA-256"))
		{
			PbeParametersGenerator pbeParametersGenerator3 = MakePbeGenerator((string)algorithmType[text], new Sha256Digest(), array, salt, iterationCount);
			if (text.Equals("PBEwithSHA-256and128bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator3.GenerateDerivedParameters("AES", 128, 128);
			}
			else if (text.Equals("PBEwithSHA-256and192bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator3.GenerateDerivedParameters("AES", 192, 128);
			}
			else if (text.Equals("PBEwithSHA-256and256bitAES-CBC-BC"))
			{
				parameters = pbeParametersGenerator3.GenerateDerivedParameters("AES", 256, 128);
			}
		}
		else if (Platform.StartsWith(text, "PBEwithMD5"))
		{
			PbeParametersGenerator pbeParametersGenerator4 = MakePbeGenerator((string)algorithmType[text], new MD5Digest(), array, salt, iterationCount);
			if (text.Equals("PBEwithMD5andDES-CBC"))
			{
				parameters = pbeParametersGenerator4.GenerateDerivedParameters("DES", 64, 64);
			}
			else if (text.Equals("PBEwithMD5andRC2-CBC"))
			{
				parameters = pbeParametersGenerator4.GenerateDerivedParameters("RC2", 64, 64);
			}
			else if (text.Equals("PBEwithMD5and128bitAES-CBC-OpenSSL"))
			{
				parameters = pbeParametersGenerator4.GenerateDerivedParameters("AES", 128, 128);
			}
			else if (text.Equals("PBEwithMD5and192bitAES-CBC-OpenSSL"))
			{
				parameters = pbeParametersGenerator4.GenerateDerivedParameters("AES", 192, 128);
			}
			else if (text.Equals("PBEwithMD5and256bitAES-CBC-OpenSSL"))
			{
				parameters = pbeParametersGenerator4.GenerateDerivedParameters("AES", 256, 128);
			}
		}
		else if (Platform.StartsWith(text, "PBEwithMD2"))
		{
			PbeParametersGenerator pbeParametersGenerator5 = MakePbeGenerator((string)algorithmType[text], new MD2Digest(), array, salt, iterationCount);
			if (text.Equals("PBEwithMD2andDES-CBC"))
			{
				parameters = pbeParametersGenerator5.GenerateDerivedParameters("DES", 64, 64);
			}
			else if (text.Equals("PBEwithMD2andRC2-CBC"))
			{
				parameters = pbeParametersGenerator5.GenerateDerivedParameters("RC2", 64, 64);
			}
		}
		else if (Platform.StartsWith(text, "PBEwithHmac"))
		{
			string algorithm3 = text.Substring("PBEwithHmac".Length);
			IDigest digest2 = DigestUtilities.GetDigest(algorithm3);
			PbeParametersGenerator pbeParametersGenerator6 = MakePbeGenerator((string)algorithmType[text], digest2, array, salt, iterationCount);
			int keySize2 = digest2.GetDigestSize() * 8;
			parameters = pbeParametersGenerator6.GenerateDerivedMacParameters(keySize2);
		}
		Array.Clear(array, 0, array.Length);
		return FixDesParity(text, parameters);
	}

	public static object CreateEngine(DerObjectIdentifier algorithmOid)
	{
		return CreateEngine(algorithmOid.Id);
	}

	public static object CreateEngine(AlgorithmIdentifier algID)
	{
		string id = algID.Algorithm.Id;
		if (IsPkcs5Scheme2(id))
		{
			PbeS2Parameters instance = PbeS2Parameters.GetInstance(algID.Parameters.ToAsn1Object());
			AlgorithmIdentifier encryptionScheme = instance.EncryptionScheme;
			return CipherUtilities.GetCipher(encryptionScheme.Algorithm);
		}
		return CreateEngine(id);
	}

	public static object CreateEngine(string algorithm)
	{
		string text = (string)algorithms[Platform.ToUpperInvariant(algorithm)];
		if (Platform.StartsWith(text, "PBEwithHmac"))
		{
			string text2 = text.Substring("PBEwithHmac".Length);
			return MacUtilities.GetMac("HMAC/" + text2);
		}
		if (Platform.StartsWith(text, "PBEwithMD2") || Platform.StartsWith(text, "PBEwithMD5") || Platform.StartsWith(text, "PBEwithSHA-1") || Platform.StartsWith(text, "PBEwithSHA-256"))
		{
			if (Platform.EndsWith(text, "AES-CBC-BC") || Platform.EndsWith(text, "AES-CBC-OPENSSL"))
			{
				return CipherUtilities.GetCipher("AES/CBC");
			}
			if (Platform.EndsWith(text, "DES-CBC"))
			{
				return CipherUtilities.GetCipher("DES/CBC");
			}
			if (Platform.EndsWith(text, "DESEDE-CBC"))
			{
				return CipherUtilities.GetCipher("DESEDE/CBC");
			}
			if (Platform.EndsWith(text, "RC2-CBC"))
			{
				return CipherUtilities.GetCipher("RC2/CBC");
			}
			if (Platform.EndsWith(text, "RC4"))
			{
				return CipherUtilities.GetCipher("RC4");
			}
		}
		return null;
	}

	public static string GetEncodingName(DerObjectIdentifier oid)
	{
		return (string)algorithms[oid.Id];
	}

	private static ICipherParameters FixDesParity(string mechanism, ICipherParameters parameters)
	{
		if (!Platform.EndsWith(mechanism, "DES-CBC") && !Platform.EndsWith(mechanism, "DESEDE-CBC"))
		{
			return parameters;
		}
		if (parameters is ParametersWithIV)
		{
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			return new ParametersWithIV(FixDesParity(mechanism, parametersWithIV.Parameters), parametersWithIV.GetIV());
		}
		KeyParameter keyParameter = (KeyParameter)parameters;
		byte[] key = keyParameter.GetKey();
		DesParameters.SetOddParity(key);
		return new KeyParameter(key);
	}
}
