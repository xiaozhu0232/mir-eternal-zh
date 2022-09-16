using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Bsi;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Eac;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security;

public sealed class SignerUtilities
{
	internal static readonly IDictionary algorithms;

	internal static readonly IDictionary oids;

	public static ICollection Algorithms => oids.Keys;

	private SignerUtilities()
	{
	}

	static SignerUtilities()
	{
		algorithms = Platform.CreateHashtable();
		oids = Platform.CreateHashtable();
		algorithms["MD2WITHRSA"] = "MD2withRSA";
		algorithms["MD2WITHRSAENCRYPTION"] = "MD2withRSA";
		algorithms[PkcsObjectIdentifiers.MD2WithRsaEncryption.Id] = "MD2withRSA";
		algorithms["MD4WITHRSA"] = "MD4withRSA";
		algorithms["MD4WITHRSAENCRYPTION"] = "MD4withRSA";
		algorithms[PkcsObjectIdentifiers.MD4WithRsaEncryption.Id] = "MD4withRSA";
		algorithms[OiwObjectIdentifiers.MD4WithRsa.Id] = "MD4withRSA";
		algorithms[OiwObjectIdentifiers.MD4WithRsaEncryption.Id] = "MD4withRSA";
		algorithms["MD5WITHRSA"] = "MD5withRSA";
		algorithms["MD5WITHRSAENCRYPTION"] = "MD5withRSA";
		algorithms[PkcsObjectIdentifiers.MD5WithRsaEncryption.Id] = "MD5withRSA";
		algorithms[OiwObjectIdentifiers.MD5WithRsa.Id] = "MD5withRSA";
		algorithms["SHA1WITHRSA"] = "SHA-1withRSA";
		algorithms["SHA1WITHRSAENCRYPTION"] = "SHA-1withRSA";
		algorithms["SHA-1WITHRSA"] = "SHA-1withRSA";
		algorithms[PkcsObjectIdentifiers.Sha1WithRsaEncryption.Id] = "SHA-1withRSA";
		algorithms[OiwObjectIdentifiers.Sha1WithRsa.Id] = "SHA-1withRSA";
		algorithms["SHA224WITHRSA"] = "SHA-224withRSA";
		algorithms["SHA224WITHRSAENCRYPTION"] = "SHA-224withRSA";
		algorithms[PkcsObjectIdentifiers.Sha224WithRsaEncryption.Id] = "SHA-224withRSA";
		algorithms["SHA-224WITHRSA"] = "SHA-224withRSA";
		algorithms["SHA256WITHRSA"] = "SHA-256withRSA";
		algorithms["SHA256WITHRSAENCRYPTION"] = "SHA-256withRSA";
		algorithms[PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id] = "SHA-256withRSA";
		algorithms["SHA-256WITHRSA"] = "SHA-256withRSA";
		algorithms["SHA384WITHRSA"] = "SHA-384withRSA";
		algorithms["SHA384WITHRSAENCRYPTION"] = "SHA-384withRSA";
		algorithms[PkcsObjectIdentifiers.Sha384WithRsaEncryption.Id] = "SHA-384withRSA";
		algorithms["SHA-384WITHRSA"] = "SHA-384withRSA";
		algorithms["SHA512WITHRSA"] = "SHA-512withRSA";
		algorithms["SHA512WITHRSAENCRYPTION"] = "SHA-512withRSA";
		algorithms[PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id] = "SHA-512withRSA";
		algorithms["SHA-512WITHRSA"] = "SHA-512withRSA";
		algorithms["PSSWITHRSA"] = "PSSwithRSA";
		algorithms["RSASSA-PSS"] = "PSSwithRSA";
		algorithms[PkcsObjectIdentifiers.IdRsassaPss.Id] = "PSSwithRSA";
		algorithms["RSAPSS"] = "PSSwithRSA";
		algorithms["SHA1WITHRSAANDMGF1"] = "SHA-1withRSAandMGF1";
		algorithms["SHA-1WITHRSAANDMGF1"] = "SHA-1withRSAandMGF1";
		algorithms["SHA1WITHRSA/PSS"] = "SHA-1withRSAandMGF1";
		algorithms["SHA-1WITHRSA/PSS"] = "SHA-1withRSAandMGF1";
		algorithms["SHA224WITHRSAANDMGF1"] = "SHA-224withRSAandMGF1";
		algorithms["SHA-224WITHRSAANDMGF1"] = "SHA-224withRSAandMGF1";
		algorithms["SHA224WITHRSA/PSS"] = "SHA-224withRSAandMGF1";
		algorithms["SHA-224WITHRSA/PSS"] = "SHA-224withRSAandMGF1";
		algorithms["SHA256WITHRSAANDMGF1"] = "SHA-256withRSAandMGF1";
		algorithms["SHA-256WITHRSAANDMGF1"] = "SHA-256withRSAandMGF1";
		algorithms["SHA256WITHRSA/PSS"] = "SHA-256withRSAandMGF1";
		algorithms["SHA-256WITHRSA/PSS"] = "SHA-256withRSAandMGF1";
		algorithms["SHA384WITHRSAANDMGF1"] = "SHA-384withRSAandMGF1";
		algorithms["SHA-384WITHRSAANDMGF1"] = "SHA-384withRSAandMGF1";
		algorithms["SHA384WITHRSA/PSS"] = "SHA-384withRSAandMGF1";
		algorithms["SHA-384WITHRSA/PSS"] = "SHA-384withRSAandMGF1";
		algorithms["SHA512WITHRSAANDMGF1"] = "SHA-512withRSAandMGF1";
		algorithms["SHA-512WITHRSAANDMGF1"] = "SHA-512withRSAandMGF1";
		algorithms["SHA512WITHRSA/PSS"] = "SHA-512withRSAandMGF1";
		algorithms["SHA-512WITHRSA/PSS"] = "SHA-512withRSAandMGF1";
		algorithms["RIPEMD128WITHRSA"] = "RIPEMD128withRSA";
		algorithms["RIPEMD128WITHRSAENCRYPTION"] = "RIPEMD128withRSA";
		algorithms[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128.Id] = "RIPEMD128withRSA";
		algorithms["RIPEMD160WITHRSA"] = "RIPEMD160withRSA";
		algorithms["RIPEMD160WITHRSAENCRYPTION"] = "RIPEMD160withRSA";
		algorithms[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160.Id] = "RIPEMD160withRSA";
		algorithms["RIPEMD256WITHRSA"] = "RIPEMD256withRSA";
		algorithms["RIPEMD256WITHRSAENCRYPTION"] = "RIPEMD256withRSA";
		algorithms[TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256.Id] = "RIPEMD256withRSA";
		algorithms["NONEWITHRSA"] = "RSA";
		algorithms["RSAWITHNONE"] = "RSA";
		algorithms["RAWRSA"] = "RSA";
		algorithms["RAWRSAPSS"] = "RAWRSASSA-PSS";
		algorithms["NONEWITHRSAPSS"] = "RAWRSASSA-PSS";
		algorithms["NONEWITHRSASSA-PSS"] = "RAWRSASSA-PSS";
		algorithms["NONEWITHDSA"] = "NONEwithDSA";
		algorithms["DSAWITHNONE"] = "NONEwithDSA";
		algorithms["RAWDSA"] = "NONEwithDSA";
		algorithms["DSA"] = "SHA-1withDSA";
		algorithms["DSAWITHSHA1"] = "SHA-1withDSA";
		algorithms["DSAWITHSHA-1"] = "SHA-1withDSA";
		algorithms["SHA/DSA"] = "SHA-1withDSA";
		algorithms["SHA1/DSA"] = "SHA-1withDSA";
		algorithms["SHA-1/DSA"] = "SHA-1withDSA";
		algorithms["SHA1WITHDSA"] = "SHA-1withDSA";
		algorithms["SHA-1WITHDSA"] = "SHA-1withDSA";
		algorithms[X9ObjectIdentifiers.IdDsaWithSha1.Id] = "SHA-1withDSA";
		algorithms[OiwObjectIdentifiers.DsaWithSha1.Id] = "SHA-1withDSA";
		algorithms["DSAWITHSHA224"] = "SHA-224withDSA";
		algorithms["DSAWITHSHA-224"] = "SHA-224withDSA";
		algorithms["SHA224/DSA"] = "SHA-224withDSA";
		algorithms["SHA-224/DSA"] = "SHA-224withDSA";
		algorithms["SHA224WITHDSA"] = "SHA-224withDSA";
		algorithms["SHA-224WITHDSA"] = "SHA-224withDSA";
		algorithms[NistObjectIdentifiers.DsaWithSha224.Id] = "SHA-224withDSA";
		algorithms["DSAWITHSHA256"] = "SHA-256withDSA";
		algorithms["DSAWITHSHA-256"] = "SHA-256withDSA";
		algorithms["SHA256/DSA"] = "SHA-256withDSA";
		algorithms["SHA-256/DSA"] = "SHA-256withDSA";
		algorithms["SHA256WITHDSA"] = "SHA-256withDSA";
		algorithms["SHA-256WITHDSA"] = "SHA-256withDSA";
		algorithms[NistObjectIdentifiers.DsaWithSha256.Id] = "SHA-256withDSA";
		algorithms["DSAWITHSHA384"] = "SHA-384withDSA";
		algorithms["DSAWITHSHA-384"] = "SHA-384withDSA";
		algorithms["SHA384/DSA"] = "SHA-384withDSA";
		algorithms["SHA-384/DSA"] = "SHA-384withDSA";
		algorithms["SHA384WITHDSA"] = "SHA-384withDSA";
		algorithms["SHA-384WITHDSA"] = "SHA-384withDSA";
		algorithms[NistObjectIdentifiers.DsaWithSha384.Id] = "SHA-384withDSA";
		algorithms["DSAWITHSHA512"] = "SHA-512withDSA";
		algorithms["DSAWITHSHA-512"] = "SHA-512withDSA";
		algorithms["SHA512/DSA"] = "SHA-512withDSA";
		algorithms["SHA-512/DSA"] = "SHA-512withDSA";
		algorithms["SHA512WITHDSA"] = "SHA-512withDSA";
		algorithms["SHA-512WITHDSA"] = "SHA-512withDSA";
		algorithms[NistObjectIdentifiers.DsaWithSha512.Id] = "SHA-512withDSA";
		algorithms["NONEWITHECDSA"] = "NONEwithECDSA";
		algorithms["ECDSAWITHNONE"] = "NONEwithECDSA";
		algorithms["ECDSA"] = "SHA-1withECDSA";
		algorithms["SHA1/ECDSA"] = "SHA-1withECDSA";
		algorithms["SHA-1/ECDSA"] = "SHA-1withECDSA";
		algorithms["ECDSAWITHSHA1"] = "SHA-1withECDSA";
		algorithms["ECDSAWITHSHA-1"] = "SHA-1withECDSA";
		algorithms["SHA1WITHECDSA"] = "SHA-1withECDSA";
		algorithms["SHA-1WITHECDSA"] = "SHA-1withECDSA";
		algorithms[X9ObjectIdentifiers.ECDsaWithSha1.Id] = "SHA-1withECDSA";
		algorithms[TeleTrusTObjectIdentifiers.ECSignWithSha1.Id] = "SHA-1withECDSA";
		algorithms["SHA224/ECDSA"] = "SHA-224withECDSA";
		algorithms["SHA-224/ECDSA"] = "SHA-224withECDSA";
		algorithms["ECDSAWITHSHA224"] = "SHA-224withECDSA";
		algorithms["ECDSAWITHSHA-224"] = "SHA-224withECDSA";
		algorithms["SHA224WITHECDSA"] = "SHA-224withECDSA";
		algorithms["SHA-224WITHECDSA"] = "SHA-224withECDSA";
		algorithms[X9ObjectIdentifiers.ECDsaWithSha224.Id] = "SHA-224withECDSA";
		algorithms["SHA256/ECDSA"] = "SHA-256withECDSA";
		algorithms["SHA-256/ECDSA"] = "SHA-256withECDSA";
		algorithms["ECDSAWITHSHA256"] = "SHA-256withECDSA";
		algorithms["ECDSAWITHSHA-256"] = "SHA-256withECDSA";
		algorithms["SHA256WITHECDSA"] = "SHA-256withECDSA";
		algorithms["SHA-256WITHECDSA"] = "SHA-256withECDSA";
		algorithms[X9ObjectIdentifiers.ECDsaWithSha256.Id] = "SHA-256withECDSA";
		algorithms["SHA384/ECDSA"] = "SHA-384withECDSA";
		algorithms["SHA-384/ECDSA"] = "SHA-384withECDSA";
		algorithms["ECDSAWITHSHA384"] = "SHA-384withECDSA";
		algorithms["ECDSAWITHSHA-384"] = "SHA-384withECDSA";
		algorithms["SHA384WITHECDSA"] = "SHA-384withECDSA";
		algorithms["SHA-384WITHECDSA"] = "SHA-384withECDSA";
		algorithms[X9ObjectIdentifiers.ECDsaWithSha384.Id] = "SHA-384withECDSA";
		algorithms["SHA512/ECDSA"] = "SHA-512withECDSA";
		algorithms["SHA-512/ECDSA"] = "SHA-512withECDSA";
		algorithms["ECDSAWITHSHA512"] = "SHA-512withECDSA";
		algorithms["ECDSAWITHSHA-512"] = "SHA-512withECDSA";
		algorithms["SHA512WITHECDSA"] = "SHA-512withECDSA";
		algorithms["SHA-512WITHECDSA"] = "SHA-512withECDSA";
		algorithms[X9ObjectIdentifiers.ECDsaWithSha512.Id] = "SHA-512withECDSA";
		algorithms["RIPEMD160/ECDSA"] = "RIPEMD160withECDSA";
		algorithms["ECDSAWITHRIPEMD160"] = "RIPEMD160withECDSA";
		algorithms["RIPEMD160WITHECDSA"] = "RIPEMD160withECDSA";
		algorithms[TeleTrusTObjectIdentifiers.ECSignWithRipeMD160.Id] = "RIPEMD160withECDSA";
		algorithms["NONEWITHCVC-ECDSA"] = "NONEwithCVC-ECDSA";
		algorithms["CVC-ECDSAWITHNONE"] = "NONEwithCVC-ECDSA";
		algorithms["SHA1/CVC-ECDSA"] = "SHA-1withCVC-ECDSA";
		algorithms["SHA-1/CVC-ECDSA"] = "SHA-1withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA1"] = "SHA-1withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA-1"] = "SHA-1withCVC-ECDSA";
		algorithms["SHA1WITHCVC-ECDSA"] = "SHA-1withCVC-ECDSA";
		algorithms["SHA-1WITHCVC-ECDSA"] = "SHA-1withCVC-ECDSA";
		algorithms[EacObjectIdentifiers.id_TA_ECDSA_SHA_1.Id] = "SHA-1withCVC-ECDSA";
		algorithms["SHA224/CVC-ECDSA"] = "SHA-224withCVC-ECDSA";
		algorithms["SHA-224/CVC-ECDSA"] = "SHA-224withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA224"] = "SHA-224withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA-224"] = "SHA-224withCVC-ECDSA";
		algorithms["SHA224WITHCVC-ECDSA"] = "SHA-224withCVC-ECDSA";
		algorithms["SHA-224WITHCVC-ECDSA"] = "SHA-224withCVC-ECDSA";
		algorithms[EacObjectIdentifiers.id_TA_ECDSA_SHA_224.Id] = "SHA-224withCVC-ECDSA";
		algorithms["SHA256/CVC-ECDSA"] = "SHA-256withCVC-ECDSA";
		algorithms["SHA-256/CVC-ECDSA"] = "SHA-256withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA256"] = "SHA-256withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA-256"] = "SHA-256withCVC-ECDSA";
		algorithms["SHA256WITHCVC-ECDSA"] = "SHA-256withCVC-ECDSA";
		algorithms["SHA-256WITHCVC-ECDSA"] = "SHA-256withCVC-ECDSA";
		algorithms[EacObjectIdentifiers.id_TA_ECDSA_SHA_256.Id] = "SHA-256withCVC-ECDSA";
		algorithms["SHA384/CVC-ECDSA"] = "SHA-384withCVC-ECDSA";
		algorithms["SHA-384/CVC-ECDSA"] = "SHA-384withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA384"] = "SHA-384withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA-384"] = "SHA-384withCVC-ECDSA";
		algorithms["SHA384WITHCVC-ECDSA"] = "SHA-384withCVC-ECDSA";
		algorithms["SHA-384WITHCVC-ECDSA"] = "SHA-384withCVC-ECDSA";
		algorithms[EacObjectIdentifiers.id_TA_ECDSA_SHA_384.Id] = "SHA-384withCVC-ECDSA";
		algorithms["SHA512/CVC-ECDSA"] = "SHA-512withCVC-ECDSA";
		algorithms["SHA-512/CVC-ECDSA"] = "SHA-512withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA512"] = "SHA-512withCVC-ECDSA";
		algorithms["CVC-ECDSAWITHSHA-512"] = "SHA-512withCVC-ECDSA";
		algorithms["SHA512WITHCVC-ECDSA"] = "SHA-512withCVC-ECDSA";
		algorithms["SHA-512WITHCVC-ECDSA"] = "SHA-512withCVC-ECDSA";
		algorithms[EacObjectIdentifiers.id_TA_ECDSA_SHA_512.Id] = "SHA-512withCVC-ECDSA";
		algorithms["NONEWITHPLAIN-ECDSA"] = "NONEwithPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHNONE"] = "NONEwithPLAIN-ECDSA";
		algorithms["SHA1/PLAIN-ECDSA"] = "SHA-1withPLAIN-ECDSA";
		algorithms["SHA-1/PLAIN-ECDSA"] = "SHA-1withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA1"] = "SHA-1withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA-1"] = "SHA-1withPLAIN-ECDSA";
		algorithms["SHA1WITHPLAIN-ECDSA"] = "SHA-1withPLAIN-ECDSA";
		algorithms["SHA-1WITHPLAIN-ECDSA"] = "SHA-1withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_SHA1.Id] = "SHA-1withPLAIN-ECDSA";
		algorithms["SHA224/PLAIN-ECDSA"] = "SHA-224withPLAIN-ECDSA";
		algorithms["SHA-224/PLAIN-ECDSA"] = "SHA-224withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA224"] = "SHA-224withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA-224"] = "SHA-224withPLAIN-ECDSA";
		algorithms["SHA224WITHPLAIN-ECDSA"] = "SHA-224withPLAIN-ECDSA";
		algorithms["SHA-224WITHPLAIN-ECDSA"] = "SHA-224withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_SHA224.Id] = "SHA-224withPLAIN-ECDSA";
		algorithms["SHA256/PLAIN-ECDSA"] = "SHA-256withPLAIN-ECDSA";
		algorithms["SHA-256/PLAIN-ECDSA"] = "SHA-256withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA256"] = "SHA-256withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA-256"] = "SHA-256withPLAIN-ECDSA";
		algorithms["SHA256WITHPLAIN-ECDSA"] = "SHA-256withPLAIN-ECDSA";
		algorithms["SHA-256WITHPLAIN-ECDSA"] = "SHA-256withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_SHA256.Id] = "SHA-256withPLAIN-ECDSA";
		algorithms["SHA384/PLAIN-ECDSA"] = "SHA-384withPLAIN-ECDSA";
		algorithms["SHA-384/PLAIN-ECDSA"] = "SHA-384withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA384"] = "SHA-384withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA-384"] = "SHA-384withPLAIN-ECDSA";
		algorithms["SHA384WITHPLAIN-ECDSA"] = "SHA-384withPLAIN-ECDSA";
		algorithms["SHA-384WITHPLAIN-ECDSA"] = "SHA-384withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_SHA384.Id] = "SHA-384withPLAIN-ECDSA";
		algorithms["SHA512/PLAIN-ECDSA"] = "SHA-512withPLAIN-ECDSA";
		algorithms["SHA-512/PLAIN-ECDSA"] = "SHA-512withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA512"] = "SHA-512withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHSHA-512"] = "SHA-512withPLAIN-ECDSA";
		algorithms["SHA512WITHPLAIN-ECDSA"] = "SHA-512withPLAIN-ECDSA";
		algorithms["SHA-512WITHPLAIN-ECDSA"] = "SHA-512withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_SHA512.Id] = "SHA-512withPLAIN-ECDSA";
		algorithms["RIPEMD160/PLAIN-ECDSA"] = "RIPEMD160withPLAIN-ECDSA";
		algorithms["PLAIN-ECDSAWITHRIPEMD160"] = "RIPEMD160withPLAIN-ECDSA";
		algorithms["RIPEMD160WITHPLAIN-ECDSA"] = "RIPEMD160withPLAIN-ECDSA";
		algorithms[BsiObjectIdentifiers.ecdsa_plain_RIPEMD160.Id] = "RIPEMD160withPLAIN-ECDSA";
		algorithms["SHA1WITHECNR"] = "SHA-1withECNR";
		algorithms["SHA-1WITHECNR"] = "SHA-1withECNR";
		algorithms["SHA224WITHECNR"] = "SHA-224withECNR";
		algorithms["SHA-224WITHECNR"] = "SHA-224withECNR";
		algorithms["SHA256WITHECNR"] = "SHA-256withECNR";
		algorithms["SHA-256WITHECNR"] = "SHA-256withECNR";
		algorithms["SHA384WITHECNR"] = "SHA-384withECNR";
		algorithms["SHA-384WITHECNR"] = "SHA-384withECNR";
		algorithms["SHA512WITHECNR"] = "SHA-512withECNR";
		algorithms["SHA-512WITHECNR"] = "SHA-512withECNR";
		algorithms["GOST-3410"] = "GOST3410";
		algorithms["GOST-3410-94"] = "GOST3410";
		algorithms["GOST3411WITHGOST3410"] = "GOST3410";
		algorithms[CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94.Id] = "GOST3410";
		algorithms["ECGOST-3410"] = "ECGOST3410";
		algorithms["ECGOST-3410-2001"] = "ECGOST3410";
		algorithms["GOST3411WITHECGOST3410"] = "ECGOST3410";
		algorithms[CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001.Id] = "ECGOST3410";
		algorithms["ED25519"] = "Ed25519";
		algorithms[EdECObjectIdentifiers.id_Ed25519.Id] = "Ed25519";
		algorithms["ED25519CTX"] = "Ed25519ctx";
		algorithms["ED25519PH"] = "Ed25519ph";
		algorithms["ED448"] = "Ed448";
		algorithms[EdECObjectIdentifiers.id_Ed448.Id] = "Ed448";
		algorithms["ED448PH"] = "Ed448ph";
		algorithms["SHA256WITHSM2"] = "SHA256withSM2";
		algorithms[GMObjectIdentifiers.sm2sign_with_sha256.Id] = "SHA256withSM2";
		algorithms["SM3WITHSM2"] = "SM3withSM2";
		algorithms[GMObjectIdentifiers.sm2sign_with_sm3.Id] = "SM3withSM2";
		oids["MD2withRSA"] = PkcsObjectIdentifiers.MD2WithRsaEncryption;
		oids["MD4withRSA"] = PkcsObjectIdentifiers.MD4WithRsaEncryption;
		oids["MD5withRSA"] = PkcsObjectIdentifiers.MD5WithRsaEncryption;
		oids["SHA-1withRSA"] = PkcsObjectIdentifiers.Sha1WithRsaEncryption;
		oids["SHA-224withRSA"] = PkcsObjectIdentifiers.Sha224WithRsaEncryption;
		oids["SHA-256withRSA"] = PkcsObjectIdentifiers.Sha256WithRsaEncryption;
		oids["SHA-384withRSA"] = PkcsObjectIdentifiers.Sha384WithRsaEncryption;
		oids["SHA-512withRSA"] = PkcsObjectIdentifiers.Sha512WithRsaEncryption;
		oids["PSSwithRSA"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["SHA-1withRSAandMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["SHA-224withRSAandMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["SHA-256withRSAandMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["SHA-384withRSAandMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["SHA-512withRSAandMGF1"] = PkcsObjectIdentifiers.IdRsassaPss;
		oids["RIPEMD128withRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128;
		oids["RIPEMD160withRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160;
		oids["RIPEMD256withRSA"] = TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256;
		oids["SHA-1withDSA"] = X9ObjectIdentifiers.IdDsaWithSha1;
		oids["SHA-1withECDSA"] = X9ObjectIdentifiers.ECDsaWithSha1;
		oids["SHA-224withECDSA"] = X9ObjectIdentifiers.ECDsaWithSha224;
		oids["SHA-256withECDSA"] = X9ObjectIdentifiers.ECDsaWithSha256;
		oids["SHA-384withECDSA"] = X9ObjectIdentifiers.ECDsaWithSha384;
		oids["SHA-512withECDSA"] = X9ObjectIdentifiers.ECDsaWithSha512;
		oids["RIPEMD160withECDSA"] = TeleTrusTObjectIdentifiers.ECSignWithRipeMD160;
		oids["SHA-1withCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_1;
		oids["SHA-224withCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_224;
		oids["SHA-256withCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_256;
		oids["SHA-384withCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_384;
		oids["SHA-512withCVC-ECDSA"] = EacObjectIdentifiers.id_TA_ECDSA_SHA_512;
		oids["SHA-1withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA1;
		oids["SHA-224withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA224;
		oids["SHA-256withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA256;
		oids["SHA-384withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA384;
		oids["SHA-512withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_SHA512;
		oids["RIPEMD160withPLAIN-ECDSA"] = BsiObjectIdentifiers.ecdsa_plain_RIPEMD160;
		oids["GOST3410"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94;
		oids["ECGOST3410"] = CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001;
		oids["Ed25519"] = EdECObjectIdentifiers.id_Ed25519;
		oids["Ed448"] = EdECObjectIdentifiers.id_Ed448;
		oids["SHA256withSM2"] = GMObjectIdentifiers.sm2sign_with_sha256;
		oids["SM3withSM2"] = GMObjectIdentifiers.sm2sign_with_sm3;
	}

	public static DerObjectIdentifier GetObjectIdentifier(string mechanism)
	{
		if (mechanism == null)
		{
			throw new ArgumentNullException("mechanism");
		}
		mechanism = Platform.ToUpperInvariant(mechanism);
		string text = (string)algorithms[mechanism];
		if (text != null)
		{
			mechanism = text;
		}
		return (DerObjectIdentifier)oids[mechanism];
	}

	public static Asn1Encodable GetDefaultX509Parameters(DerObjectIdentifier id)
	{
		return GetDefaultX509Parameters(id.Id);
	}

	public static Asn1Encodable GetDefaultX509Parameters(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		algorithm = Platform.ToUpperInvariant(algorithm);
		string text = (string)algorithms[algorithm];
		if (text == null)
		{
			text = algorithm;
		}
		if (text == "PSSwithRSA")
		{
			return GetPssX509Parameters("SHA-1");
		}
		if (Platform.EndsWith(text, "withRSAandMGF1"))
		{
			string digestName = text.Substring(0, text.Length - "withRSAandMGF1".Length);
			return GetPssX509Parameters(digestName);
		}
		return DerNull.Instance;
	}

	private static Asn1Encodable GetPssX509Parameters(string digestName)
	{
		AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(DigestUtilities.GetObjectIdentifier(digestName), DerNull.Instance);
		AlgorithmIdentifier maskGenAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, algorithmIdentifier);
		int digestSize = DigestUtilities.GetDigest(digestName).GetDigestSize();
		return new RsassaPssParameters(algorithmIdentifier, maskGenAlgorithm, new DerInteger(digestSize), new DerInteger(1));
	}

	public static ISigner GetSigner(DerObjectIdentifier id)
	{
		return GetSigner(id.Id);
	}

	public static ISigner GetSigner(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		algorithm = Platform.ToUpperInvariant(algorithm);
		string text = (string)algorithms[algorithm];
		if (text == null)
		{
			text = algorithm;
		}
		if (Platform.StartsWith(text, "Ed"))
		{
			if (text.Equals("Ed25519"))
			{
				return new Ed25519Signer();
			}
			if (text.Equals("Ed25519ctx"))
			{
				return new Ed25519ctxSigner(Arrays.EmptyBytes);
			}
			if (text.Equals("Ed25519ph"))
			{
				return new Ed25519phSigner(Arrays.EmptyBytes);
			}
			if (text.Equals("Ed448"))
			{
				return new Ed448Signer(Arrays.EmptyBytes);
			}
			if (text.Equals("Ed448ph"))
			{
				return new Ed448phSigner(Arrays.EmptyBytes);
			}
		}
		if (text.Equals("RSA"))
		{
			return new RsaDigestSigner((IDigest)new NullDigest(), (AlgorithmIdentifier)null);
		}
		if (text.Equals("RAWRSASSA-PSS"))
		{
			return PssSigner.CreateRawSigner(new RsaBlindedEngine(), new Sha1Digest());
		}
		if (text.Equals("PSSwithRSA"))
		{
			return new PssSigner(new RsaBlindedEngine(), new Sha1Digest());
		}
		if (Platform.EndsWith(text, "withRSA"))
		{
			string algorithm2 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest = DigestUtilities.GetDigest(algorithm2);
			return new RsaDigestSigner(digest);
		}
		if (Platform.EndsWith(text, "withRSAandMGF1"))
		{
			string algorithm3 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest2 = DigestUtilities.GetDigest(algorithm3);
			return new PssSigner(new RsaBlindedEngine(), digest2);
		}
		if (Platform.EndsWith(text, "withDSA"))
		{
			string algorithm4 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest3 = DigestUtilities.GetDigest(algorithm4);
			return new DsaDigestSigner(new DsaSigner(), digest3);
		}
		if (Platform.EndsWith(text, "withECDSA"))
		{
			string algorithm5 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest4 = DigestUtilities.GetDigest(algorithm5);
			return new DsaDigestSigner(new ECDsaSigner(), digest4);
		}
		if (Platform.EndsWith(text, "withCVC-ECDSA") || Platform.EndsWith(text, "withPLAIN-ECDSA"))
		{
			string algorithm6 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest5 = DigestUtilities.GetDigest(algorithm6);
			return new DsaDigestSigner(new ECDsaSigner(), digest5, PlainDsaEncoding.Instance);
		}
		if (Platform.EndsWith(text, "withECNR"))
		{
			string algorithm7 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest6 = DigestUtilities.GetDigest(algorithm7);
			return new DsaDigestSigner(new ECNRSigner(), digest6);
		}
		if (Platform.EndsWith(text, "withSM2"))
		{
			string algorithm8 = text.Substring(0, text.LastIndexOf("with"));
			IDigest digest7 = DigestUtilities.GetDigest(algorithm8);
			return new SM2Signer(digest7);
		}
		if (text.Equals("GOST3410"))
		{
			return new Gost3410DigestSigner(new Gost3410Signer(), new Gost3411Digest());
		}
		if (text.Equals("ECGOST3410"))
		{
			return new Gost3410DigestSigner(new ECGost3410Signer(), new Gost3411Digest());
		}
		if (text.Equals("SHA1WITHRSA/ISO9796-2"))
		{
			return new Iso9796d2Signer(new RsaBlindedEngine(), new Sha1Digest(), isImplicit: true);
		}
		if (text.Equals("MD5WITHRSA/ISO9796-2"))
		{
			return new Iso9796d2Signer(new RsaBlindedEngine(), new MD5Digest(), isImplicit: true);
		}
		if (text.Equals("RIPEMD160WITHRSA/ISO9796-2"))
		{
			return new Iso9796d2Signer(new RsaBlindedEngine(), new RipeMD160Digest(), isImplicit: true);
		}
		if (Platform.EndsWith(text, "/X9.31"))
		{
			string text2 = text.Substring(0, text.Length - "/X9.31".Length);
			int num = Platform.IndexOf(text2, "WITH");
			if (num > 0)
			{
				int num2 = num + "WITH".Length;
				string algorithm9 = text2.Substring(0, num);
				IDigest digest8 = DigestUtilities.GetDigest(algorithm9);
				string text3 = text2.Substring(num2, text2.Length - num2);
				if (text3.Equals("RSA"))
				{
					IAsymmetricBlockCipher cipher = new RsaBlindedEngine();
					return new X931Signer(cipher, digest8);
				}
			}
		}
		throw new SecurityUtilityException("Signer " + algorithm + " not recognised.");
	}

	public static string GetEncodingName(DerObjectIdentifier oid)
	{
		return (string)algorithms[oid.Id];
	}

	public static ISigner InitSigner(DerObjectIdentifier algorithmOid, bool forSigning, AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		return InitSigner(algorithmOid.Id, forSigning, privateKey, random);
	}

	public static ISigner InitSigner(string algorithm, bool forSigning, AsymmetricKeyParameter privateKey, SecureRandom random)
	{
		ISigner signer = GetSigner(algorithm);
		signer.Init(forSigning, ParameterUtilities.WithRandom(privateKey, random));
		return signer;
	}
}
