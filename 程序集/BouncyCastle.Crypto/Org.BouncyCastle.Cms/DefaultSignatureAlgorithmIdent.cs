using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Asn1.Bsi;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Eac;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Rosstandart;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Cms;

public class DefaultSignatureAlgorithmIdentifierFinder
{
	private static readonly IDictionary algorithms;

	private static readonly ISet noParams;

	private static readonly IDictionary _params;

	private static readonly ISet pkcs15RsaEncryption;

	private static readonly IDictionary digestOids;

	private static readonly IDictionary digestBuilders;

	private static readonly DerObjectIdentifier ENCRYPTION_RSA;

	private static readonly DerObjectIdentifier ENCRYPTION_DSA;

	private static readonly DerObjectIdentifier ENCRYPTION_ECDSA;

	private static readonly DerObjectIdentifier ENCRYPTION_RSA_PSS;

	private static readonly DerObjectIdentifier ENCRYPTION_GOST3410;

	private static readonly DerObjectIdentifier ENCRYPTION_ECGOST3410;

	private static readonly DerObjectIdentifier ENCRYPTION_ECGOST3410_2012_256;

	private static readonly DerObjectIdentifier ENCRYPTION_ECGOST3410_2012_512;

	static DefaultSignatureAlgorithmIdentifierFinder()
	{
		algorithms = Platform.CreateHashtable();
		noParams = new HashSet();
		_params = Platform.CreateHashtable();
		pkcs15RsaEncryption = new HashSet();
		digestOids = Platform.CreateHashtable();
		digestBuilders = Platform.CreateHashtable();
		ENCRYPTION_RSA = PkcsObjectIdentifiers.RsaEncryption;
		ENCRYPTION_DSA = X9ObjectIdentifiers.IdDsaWithSha1;
		ENCRYPTION_ECDSA = X9ObjectIdentifiers.ECDsaWithSha1;
		ENCRYPTION_RSA_PSS = PkcsObjectIdentifiers.IdRsassaPss;
		ENCRYPTION_GOST3410 = CryptoProObjectIdentifiers.GostR3410x94;
		ENCRYPTION_ECGOST3410 = CryptoProObjectIdentifiers.GostR3410x2001;
		ENCRYPTION_ECGOST3410_2012_256 = RosstandartObjectIdentifiers.id_tc26_gost_3410_12_256;
		ENCRYPTION_ECGOST3410_2012_512 = RosstandartObjectIdentifiers.id_tc26_gost_3410_12_512;
		algorithms["MD2WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.MD2WithRsaEncryption;
		algorithms["MD2WITHRSA"] = PkcsObjectIdentifiers.MD2WithRsaEncryption;
		algorithms["MD5WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.MD5WithRsaEncryption;
		algorithms["MD5WITHRSA"] = PkcsObjectIdentifiers.MD5WithRsaEncryption;
		algorithms["SHA1WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.Sha1WithRsaEncryption;
		algorithms["SHA1WITHRSA"] = PkcsObjectIdentifiers.Sha1WithRsaEncryption;
		algorithms["SHA-1WITHRSA"] = PkcsObjectIdentifiers.Sha1WithRsaEncryption;
		algorithms["SHA224WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.Sha224WithRsaEncryption;
		algorithms["SHA224WITHRSA"] = PkcsObjectIdentifiers.Sha224WithRsaEncryption;
		algorithms["SHA256WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.Sha256WithRsaEncryption;
		algorithms["SHA256WITHRSA"] = PkcsObjectIdentifiers.Sha256WithRsaEncryption;
		algorithms["SHA384WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.Sha384WithRsaEncryption;
		algorithms["SHA384WITHRSA"] = PkcsObjectIdentifiers.Sha384WithRsaEncryption;
		algorithms["SHA512WITHRSAENCRYPTION"] = PkcsObjectIdentifiers.Sha512WithRsaEncryption;
		algorithms["SHA512WITHRSA"] = PkcsObjectIdentifiers.Sha512WithRsaEncryption;
		algorithms["SHA1WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA224WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA256WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA384WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA512WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA3-224WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA3-256WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA3-384WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["SHA3-512WITHRSAANDMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		algorithms["RIPEMD160WITHRSAENCRYPTION"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160;
		algorithms["RIPEMD160WITHRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160;
		algorithms["RIPEMD128WITHRSAENCRYPTION"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128;
		algorithms["RIPEMD128WITHRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128;
		algorithms["RIPEMD256WITHRSAENCRYPTION"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256;
		algorithms["RIPEMD256WITHRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256;
		algorithms["SHA1WITHDSA"] = X9ObjectIdentifiers.IdDsaWithSha1;
		algorithms["SHA-1WITHDSA"] = X9ObjectIdentifiers.IdDsaWithSha1;
		algorithms["DSAWITHSHA1"] = X9ObjectIdentifiers.IdDsaWithSha1;
		algorithms["SHA224WITHDSA"] = NistObjectIdentifiers.DsaWithSha224;
		algorithms["SHA256WITHDSA"] = NistObjectIdentifiers.DsaWithSha256;
		algorithms["SHA384WITHDSA"] = NistObjectIdentifiers.DsaWithSha384;
		algorithms["SHA512WITHDSA"] = NistObjectIdentifiers.DsaWithSha512;
		algorithms["SHA3-224WITHDSA"] = NistObjectIdentifiers.IdDsaWithSha3_224;
		algorithms["SHA3-256WITHDSA"] = NistObjectIdentifiers.IdDsaWithSha3_256;
		algorithms["SHA3-384WITHDSA"] = NistObjectIdentifiers.IdDsaWithSha3_384;
		algorithms["SHA3-512WITHDSA"] = NistObjectIdentifiers.IdDsaWithSha3_512;
		algorithms["SHA3-224WITHECDSA"] = NistObjectIdentifiers.IdEcdsaWithSha3_224;
		algorithms["SHA3-256WITHECDSA"] = NistObjectIdentifiers.IdEcdsaWithSha3_256;
		algorithms["SHA3-384WITHECDSA"] = NistObjectIdentifiers.IdEcdsaWithSha3_384;
		algorithms["SHA3-512WITHECDSA"] = NistObjectIdentifiers.IdEcdsaWithSha3_512;
		algorithms["SHA3-224WITHRSA"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_224;
		algorithms["SHA3-256WITHRSA"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_256;
		algorithms["SHA3-384WITHRSA"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_384;
		algorithms["SHA3-512WITHRSA"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_512;
		algorithms["SHA3-224WITHRSAENCRYPTION"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_224;
		algorithms["SHA3-256WITHRSAENCRYPTION"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_256;
		algorithms["SHA3-384WITHRSAENCRYPTION"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_384;
		algorithms["SHA3-512WITHRSAENCRYPTION"] = NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_512;
		algorithms["SHA1WITHECDSA"] = X9ObjectIdentifiers.ECDsaWithSha1;
		algorithms["ECDSAWITHSHA1"] = X9ObjectIdentifiers.ECDsaWithSha1;
		algorithms["SHA224WITHECDSA"] = X9ObjectIdentifiers.ECDsaWithSha224;
		algorithms["SHA256WITHECDSA"] = X9ObjectIdentifiers.ECDsaWithSha224;
		algorithms["SHA384WITHECDSA"] = X9ObjectIdentifiers.ECDsaWithSha384;
		algorithms["SHA512WITHECDSA"] = X9ObjectIdentifiers.ECDsaWithSha256;
		algorithms["GOST3411WITHGOST3410"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94;
		algorithms["GOST3411WITHGOST3410-94"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94;
		algorithms["GOST3411WITHECGOST3410"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001;
		algorithms["GOST3411WITHECGOST3410-2001"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001;
		algorithms["GOST3411WITHGOST3410-2001"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001;
		algorithms["GOST3411WITHECGOST3410-2012-256"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256;
		algorithms["GOST3411WITHECGOST3410-2012-512"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512;
		algorithms["GOST3411WITHGOST3410-2012-256"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256;
		algorithms["GOST3411WITHGOST3410-2012-512"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512;
		algorithms["GOST3411-2012-256WITHECGOST3410-2012-256"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256;
		algorithms["GOST3411-2012-512WITHECGOST3410-2012-512"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512;
		algorithms["GOST3411-2012-256WITHGOST3410-2012-256"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256;
		algorithms["GOST3411-2012-512WITHGOST3410-2012-512"] = RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512;
		algorithms["SHA1WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA1;
		algorithms["SHA224WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA224;
		algorithms["SHA256WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA256;
		algorithms["SHA384WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA384;
		algorithms["SHA512WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA512;
		algorithms["RIPEMD160WITHPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_RIPEMD160;
		algorithms["SHA1WITHCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_1;
		algorithms["SHA224WITHCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_224;
		algorithms["SHA256WITHCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_256;
		algorithms["SHA384WITHCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_384;
		algorithms["SHA512WITHCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_512;
		algorithms["SHA3-512WITHSPHINCS256"] = BCObjectIdentifiers.sphincs256_with_SHA3_512;
		algorithms["SHA512WITHSPHINCS256"] = BCObjectIdentifiers.sphincs256_with_SHA512;
		algorithms["SHA256WITHSM2"] = GMObjectIdentifiers.sm2sign_with_sha256;
		algorithms["SM3WITHSM2"] = GMObjectIdentifiers.sm2sign_with_sm3;
		algorithms["SHA256WITHXMSS"] = BCObjectIdentifiers.xmss_with_SHA256;
		algorithms["SHA512WITHXMSS"] = BCObjectIdentifiers.xmss_with_SHA512;
		algorithms["SHAKE128WITHXMSS"] = BCObjectIdentifiers.xmss_with_SHAKE128;
		algorithms["SHAKE256WITHXMSS"] = BCObjectIdentifiers.xmss_with_SHAKE256;
		algorithms["SHA256WITHXMSSMT"] = BCObjectIdentifiers.xmss_mt_with_SHA256;
		algorithms["SHA512WITHXMSSMT"] = BCObjectIdentifiers.xmss_mt_with_SHA512;
		algorithms["SHAKE128WITHXMSSMT"] = BCObjectIdentifiers.xmss_mt_with_SHAKE128;
		algorithms["SHAKE256WITHXMSSMT"] = BCObjectIdentifiers.xmss_mt_with_SHAKE256;
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha1);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha224);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha256);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha384);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha512);
		noParams.Add(X9ObjectIdentifiers.IdDsaWithSha1);
		noParams.Add(NistObjectIdentifiers.DsaWithSha224);
		noParams.Add(NistObjectIdentifiers.DsaWithSha256);
		noParams.Add(NistObjectIdentifiers.DsaWithSha384);
		noParams.Add(NistObjectIdentifiers.DsaWithSha512);
		noParams.Add(NistObjectIdentifiers.IdDsaWithSha3_224);
		noParams.Add(NistObjectIdentifiers.IdDsaWithSha3_256);
		noParams.Add(NistObjectIdentifiers.IdDsaWithSha3_384);
		noParams.Add(NistObjectIdentifiers.IdDsaWithSha3_512);
		noParams.Add(NistObjectIdentifiers.IdEcdsaWithSha3_224);
		noParams.Add(NistObjectIdentifiers.IdEcdsaWithSha3_256);
		noParams.Add(NistObjectIdentifiers.IdEcdsaWithSha3_384);
		noParams.Add(NistObjectIdentifiers.IdEcdsaWithSha3_512);
		noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
		noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
		noParams.Add(RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256);
		noParams.Add(RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512);
		noParams.Add(BCObjectIdentifiers.sphincs256_with_SHA512);
		noParams.Add(BCObjectIdentifiers.sphincs256_with_SHA3_512);
		noParams.Add(BCObjectIdentifiers.xmss_with_SHA256);
		noParams.Add(BCObjectIdentifiers.xmss_with_SHA512);
		noParams.Add(BCObjectIdentifiers.xmss_with_SHAKE128);
		noParams.Add(BCObjectIdentifiers.xmss_with_SHAKE256);
		noParams.Add(BCObjectIdentifiers.xmss_mt_with_SHA256);
		noParams.Add(BCObjectIdentifiers.xmss_mt_with_SHA512);
		noParams.Add(BCObjectIdentifiers.xmss_mt_with_SHAKE128);
		noParams.Add(BCObjectIdentifiers.xmss_mt_with_SHAKE256);
		noParams.Add(GMObjectIdentifiers.sm2sign_with_sha256);
		noParams.Add(GMObjectIdentifiers.sm2sign_with_sm3);
		pkcs15RsaEncryption.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption);
		pkcs15RsaEncryption.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption);
		pkcs15RsaEncryption.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption);
		pkcs15RsaEncryption.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption);
		pkcs15RsaEncryption.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption);
		pkcs15RsaEncryption.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
		pkcs15RsaEncryption.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
		pkcs15RsaEncryption.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
		pkcs15RsaEncryption.Add(NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_224);
		pkcs15RsaEncryption.Add(NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_256);
		pkcs15RsaEncryption.Add(NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_384);
		pkcs15RsaEncryption.Add(NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_512);
		AlgorithmIdentifier hashAlgId = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
		_params["SHA1WITHRSAANDMGF1"] = CreatePssParams(hashAlgId, 20);
		AlgorithmIdentifier hashAlgId2 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha224, DerNull.Instance);
		_params["SHA224WITHRSAANDMGF1"] = CreatePssParams(hashAlgId2, 28);
		AlgorithmIdentifier hashAlgId3 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha256, DerNull.Instance);
		_params["SHA256WITHRSAANDMGF1"] = CreatePssParams(hashAlgId3, 32);
		AlgorithmIdentifier hashAlgId4 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha384, DerNull.Instance);
		_params["SHA384WITHRSAANDMGF1"] = CreatePssParams(hashAlgId4, 48);
		AlgorithmIdentifier hashAlgId5 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha512, DerNull.Instance);
		_params["SHA512WITHRSAANDMGF1"] = CreatePssParams(hashAlgId5, 64);
		AlgorithmIdentifier hashAlgId6 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha3_224, DerNull.Instance);
		_params["SHA3-224WITHRSAANDMGF1"] = CreatePssParams(hashAlgId6, 28);
		AlgorithmIdentifier hashAlgId7 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha3_256, DerNull.Instance);
		_params["SHA3-256WITHRSAANDMGF1"] = CreatePssParams(hashAlgId7, 32);
		AlgorithmIdentifier hashAlgId8 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha3_384, DerNull.Instance);
		_params["SHA3-384WITHRSAANDMGF1"] = CreatePssParams(hashAlgId8, 48);
		AlgorithmIdentifier hashAlgId9 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha3_512, DerNull.Instance);
		_params["SHA3-512WITHRSAANDMGF1"] = CreatePssParams(hashAlgId9, 64);
		digestOids[PkcsObjectIdentifiers.Sha224WithRsaEncryption] = NistObjectIdentifiers.IdSha224;
		digestOids[PkcsObjectIdentifiers.Sha256WithRsaEncryption] = NistObjectIdentifiers.IdSha256;
		digestOids[PkcsObjectIdentifiers.Sha384WithRsaEncryption] = NistObjectIdentifiers.IdSha384;
		digestOids[PkcsObjectIdentifiers.Sha512WithRsaEncryption] = NistObjectIdentifiers.IdSha512;
		digestOids[NistObjectIdentifiers.DsaWithSha224] = NistObjectIdentifiers.IdSha224;
		digestOids[NistObjectIdentifiers.DsaWithSha224] = NistObjectIdentifiers.IdSha256;
		digestOids[NistObjectIdentifiers.DsaWithSha224] = NistObjectIdentifiers.IdSha384;
		digestOids[NistObjectIdentifiers.DsaWithSha224] = NistObjectIdentifiers.IdSha512;
		digestOids[NistObjectIdentifiers.IdDsaWithSha3_224] = NistObjectIdentifiers.IdSha3_224;
		digestOids[NistObjectIdentifiers.IdDsaWithSha3_256] = NistObjectIdentifiers.IdSha3_256;
		digestOids[NistObjectIdentifiers.IdDsaWithSha3_384] = NistObjectIdentifiers.IdSha3_384;
		digestOids[NistObjectIdentifiers.IdDsaWithSha3_512] = NistObjectIdentifiers.IdSha3_512;
		digestOids[NistObjectIdentifiers.IdEcdsaWithSha3_224] = NistObjectIdentifiers.IdSha3_224;
		digestOids[NistObjectIdentifiers.IdEcdsaWithSha3_256] = NistObjectIdentifiers.IdSha3_256;
		digestOids[NistObjectIdentifiers.IdEcdsaWithSha3_384] = NistObjectIdentifiers.IdSha3_384;
		digestOids[NistObjectIdentifiers.IdEcdsaWithSha3_512] = NistObjectIdentifiers.IdSha3_512;
		digestOids[NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_224] = NistObjectIdentifiers.IdSha3_224;
		digestOids[NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_256] = NistObjectIdentifiers.IdSha3_256;
		digestOids[NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_384] = NistObjectIdentifiers.IdSha3_384;
		digestOids[NistObjectIdentifiers.IdRsassaPkcs1V15WithSha3_512] = NistObjectIdentifiers.IdSha3_512;
		digestOids[PkcsObjectIdentifiers.MD2WithRsaEncryption] = PkcsObjectIdentifiers.MD2;
		digestOids[PkcsObjectIdentifiers.MD4WithRsaEncryption] = PkcsObjectIdentifiers.MD4;
		digestOids[PkcsObjectIdentifiers.MD5WithRsaEncryption] = PkcsObjectIdentifiers.MD5;
		digestOids[PkcsObjectIdentifiers.Sha1WithRsaEncryption] = OiwObjectIdentifiers.IdSha1;
		digestOids[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128] = TeleTrusTObjectIdentifiers.RipeMD128;
		digestOids[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160] = TeleTrusTObjectIdentifiers.RipeMD160;
		digestOids[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256] = TeleTrusTObjectIdentifiers.RipeMD256;
		digestOids[CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94] = CryptoProObjectIdentifiers.GostR3411;
		digestOids[CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001] = CryptoProObjectIdentifiers.GostR3411;
		digestOids[RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_256] = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_256;
		digestOids[RosstandartObjectIdentifiers.id_tc26_signwithdigest_gost_3410_12_512] = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_512;
		digestOids[GMObjectIdentifiers.sm2sign_with_sha256] = NistObjectIdentifiers.IdSha256;
		digestOids[GMObjectIdentifiers.sm2sign_with_sm3] = GMObjectIdentifiers.sm3;
	}

	private static AlgorithmIdentifier Generate(string signatureAlgorithm)
	{
		string text = Strings.ToUpperCase(signatureAlgorithm);
		DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)algorithms[text];
		if (derObjectIdentifier == null)
		{
			throw new ArgumentException("Unknown signature type requested: " + text);
		}
		AlgorithmIdentifier algorithmIdentifier = (noParams.Contains(derObjectIdentifier) ? new AlgorithmIdentifier(derObjectIdentifier) : ((!_params.Contains(text)) ? new AlgorithmIdentifier(derObjectIdentifier, DerNull.Instance) : new AlgorithmIdentifier(derObjectIdentifier, (Asn1Encodable)_params[text])));
		if (pkcs15RsaEncryption.Contains(derObjectIdentifier))
		{
			new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);
		}
		if (algorithmIdentifier.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss))
		{
			_ = ((RsassaPssParameters)algorithmIdentifier.Parameters).HashAlgorithm;
		}
		else
		{
			new AlgorithmIdentifier((DerObjectIdentifier)digestOids[derObjectIdentifier], DerNull.Instance);
		}
		return algorithmIdentifier;
	}

	private static RsassaPssParameters CreatePssParams(AlgorithmIdentifier hashAlgId, int saltSize)
	{
		return new RsassaPssParameters(hashAlgId, new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, hashAlgId), new DerInteger(saltSize), new DerInteger(1));
	}

	public AlgorithmIdentifier Find(string sigAlgName)
	{
		return Generate(sigAlgName);
	}
}
