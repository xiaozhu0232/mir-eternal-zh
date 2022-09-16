using System;

namespace Org.BouncyCastle.Asn1.Pkcs;

public abstract class PkcsObjectIdentifiers
{
	public const string Pkcs1 = "1.2.840.113549.1.1";

	public const string Pkcs3 = "1.2.840.113549.1.3";

	public const string Pkcs5 = "1.2.840.113549.1.5";

	public const string EncryptionAlgorithm = "1.2.840.113549.3";

	public const string DigestAlgorithm = "1.2.840.113549.2";

	public const string Pkcs7 = "1.2.840.113549.1.7";

	public const string Pkcs9 = "1.2.840.113549.1.9";

	public const string CertTypes = "1.2.840.113549.1.9.22";

	public const string CrlTypes = "1.2.840.113549.1.9.23";

	public const string IdCT = "1.2.840.113549.1.9.16.1";

	public const string IdCti = "1.2.840.113549.1.9.16.6";

	public const string IdAA = "1.2.840.113549.1.9.16.2";

	public const string IdSpq = "1.2.840.113549.1.9.16.5";

	public const string Pkcs12 = "1.2.840.113549.1.12";

	public const string BagTypes = "1.2.840.113549.1.12.10.1";

	public const string Pkcs12PbeIds = "1.2.840.113549.1.12.1";

	internal static readonly DerObjectIdentifier Pkcs1Oid = new DerObjectIdentifier("1.2.840.113549.1.1");

	public static readonly DerObjectIdentifier RsaEncryption = Pkcs1Oid.Branch("1");

	public static readonly DerObjectIdentifier MD2WithRsaEncryption = Pkcs1Oid.Branch("2");

	public static readonly DerObjectIdentifier MD4WithRsaEncryption = Pkcs1Oid.Branch("3");

	public static readonly DerObjectIdentifier MD5WithRsaEncryption = Pkcs1Oid.Branch("4");

	public static readonly DerObjectIdentifier Sha1WithRsaEncryption = Pkcs1Oid.Branch("5");

	public static readonly DerObjectIdentifier SrsaOaepEncryptionSet = Pkcs1Oid.Branch("6");

	public static readonly DerObjectIdentifier IdRsaesOaep = Pkcs1Oid.Branch("7");

	public static readonly DerObjectIdentifier IdMgf1 = Pkcs1Oid.Branch("8");

	public static readonly DerObjectIdentifier IdPSpecified = Pkcs1Oid.Branch("9");

	public static readonly DerObjectIdentifier IdRsassaPss = Pkcs1Oid.Branch("10");

	public static readonly DerObjectIdentifier Sha256WithRsaEncryption = Pkcs1Oid.Branch("11");

	public static readonly DerObjectIdentifier Sha384WithRsaEncryption = Pkcs1Oid.Branch("12");

	public static readonly DerObjectIdentifier Sha512WithRsaEncryption = Pkcs1Oid.Branch("13");

	public static readonly DerObjectIdentifier Sha224WithRsaEncryption = Pkcs1Oid.Branch("14");

	public static readonly DerObjectIdentifier Sha512_224WithRSAEncryption = Pkcs1Oid.Branch("15");

	public static readonly DerObjectIdentifier Sha512_256WithRSAEncryption = Pkcs1Oid.Branch("16");

	public static readonly DerObjectIdentifier DhKeyAgreement = new DerObjectIdentifier("1.2.840.113549.1.3.1");

	public static readonly DerObjectIdentifier PbeWithMD2AndDesCbc = new DerObjectIdentifier("1.2.840.113549.1.5.1");

	public static readonly DerObjectIdentifier PbeWithMD2AndRC2Cbc = new DerObjectIdentifier("1.2.840.113549.1.5.4");

	public static readonly DerObjectIdentifier PbeWithMD5AndDesCbc = new DerObjectIdentifier("1.2.840.113549.1.5.3");

	public static readonly DerObjectIdentifier PbeWithMD5AndRC2Cbc = new DerObjectIdentifier("1.2.840.113549.1.5.6");

	public static readonly DerObjectIdentifier PbeWithSha1AndDesCbc = new DerObjectIdentifier("1.2.840.113549.1.5.10");

	public static readonly DerObjectIdentifier PbeWithSha1AndRC2Cbc = new DerObjectIdentifier("1.2.840.113549.1.5.11");

	public static readonly DerObjectIdentifier IdPbeS2 = new DerObjectIdentifier("1.2.840.113549.1.5.13");

	public static readonly DerObjectIdentifier IdPbkdf2 = new DerObjectIdentifier("1.2.840.113549.1.5.12");

	public static readonly DerObjectIdentifier DesEde3Cbc = new DerObjectIdentifier("1.2.840.113549.3.7");

	public static readonly DerObjectIdentifier RC2Cbc = new DerObjectIdentifier("1.2.840.113549.3.2");

	public static readonly DerObjectIdentifier rc4 = new DerObjectIdentifier("1.2.840.113549.3.4");

	public static readonly DerObjectIdentifier MD2 = new DerObjectIdentifier("1.2.840.113549.2.2");

	public static readonly DerObjectIdentifier MD4 = new DerObjectIdentifier("1.2.840.113549.2.4");

	public static readonly DerObjectIdentifier MD5 = new DerObjectIdentifier("1.2.840.113549.2.5");

	public static readonly DerObjectIdentifier IdHmacWithSha1 = new DerObjectIdentifier("1.2.840.113549.2.7");

	public static readonly DerObjectIdentifier IdHmacWithSha224 = new DerObjectIdentifier("1.2.840.113549.2.8");

	public static readonly DerObjectIdentifier IdHmacWithSha256 = new DerObjectIdentifier("1.2.840.113549.2.9");

	public static readonly DerObjectIdentifier IdHmacWithSha384 = new DerObjectIdentifier("1.2.840.113549.2.10");

	public static readonly DerObjectIdentifier IdHmacWithSha512 = new DerObjectIdentifier("1.2.840.113549.2.11");

	public static readonly DerObjectIdentifier Data = new DerObjectIdentifier("1.2.840.113549.1.7.1");

	public static readonly DerObjectIdentifier SignedData = new DerObjectIdentifier("1.2.840.113549.1.7.2");

	public static readonly DerObjectIdentifier EnvelopedData = new DerObjectIdentifier("1.2.840.113549.1.7.3");

	public static readonly DerObjectIdentifier SignedAndEnvelopedData = new DerObjectIdentifier("1.2.840.113549.1.7.4");

	public static readonly DerObjectIdentifier DigestedData = new DerObjectIdentifier("1.2.840.113549.1.7.5");

	public static readonly DerObjectIdentifier EncryptedData = new DerObjectIdentifier("1.2.840.113549.1.7.6");

	public static readonly DerObjectIdentifier Pkcs9AtEmailAddress = new DerObjectIdentifier("1.2.840.113549.1.9.1");

	public static readonly DerObjectIdentifier Pkcs9AtUnstructuredName = new DerObjectIdentifier("1.2.840.113549.1.9.2");

	public static readonly DerObjectIdentifier Pkcs9AtContentType = new DerObjectIdentifier("1.2.840.113549.1.9.3");

	public static readonly DerObjectIdentifier Pkcs9AtMessageDigest = new DerObjectIdentifier("1.2.840.113549.1.9.4");

	public static readonly DerObjectIdentifier Pkcs9AtSigningTime = new DerObjectIdentifier("1.2.840.113549.1.9.5");

	public static readonly DerObjectIdentifier Pkcs9AtCounterSignature = new DerObjectIdentifier("1.2.840.113549.1.9.6");

	public static readonly DerObjectIdentifier Pkcs9AtChallengePassword = new DerObjectIdentifier("1.2.840.113549.1.9.7");

	public static readonly DerObjectIdentifier Pkcs9AtUnstructuredAddress = new DerObjectIdentifier("1.2.840.113549.1.9.8");

	public static readonly DerObjectIdentifier Pkcs9AtExtendedCertificateAttributes = new DerObjectIdentifier("1.2.840.113549.1.9.9");

	public static readonly DerObjectIdentifier Pkcs9AtSigningDescription = new DerObjectIdentifier("1.2.840.113549.1.9.13");

	public static readonly DerObjectIdentifier Pkcs9AtExtensionRequest = new DerObjectIdentifier("1.2.840.113549.1.9.14");

	public static readonly DerObjectIdentifier Pkcs9AtSmimeCapabilities = new DerObjectIdentifier("1.2.840.113549.1.9.15");

	public static readonly DerObjectIdentifier IdSmime = new DerObjectIdentifier("1.2.840.113549.1.9.16");

	public static readonly DerObjectIdentifier Pkcs9AtFriendlyName = new DerObjectIdentifier("1.2.840.113549.1.9.20");

	public static readonly DerObjectIdentifier Pkcs9AtLocalKeyID = new DerObjectIdentifier("1.2.840.113549.1.9.21");

	[Obsolete("Use X509Certificate instead")]
	public static readonly DerObjectIdentifier X509CertType = new DerObjectIdentifier("1.2.840.113549.1.9.22.1");

	public static readonly DerObjectIdentifier X509Certificate = new DerObjectIdentifier("1.2.840.113549.1.9.22.1");

	public static readonly DerObjectIdentifier SdsiCertificate = new DerObjectIdentifier("1.2.840.113549.1.9.22.2");

	public static readonly DerObjectIdentifier X509Crl = new DerObjectIdentifier("1.2.840.113549.1.9.23.1");

	public static readonly DerObjectIdentifier IdAlg = IdSmime.Branch("3");

	public static readonly DerObjectIdentifier IdAlgEsdh = IdAlg.Branch("5");

	public static readonly DerObjectIdentifier IdAlgCms3DesWrap = IdAlg.Branch("6");

	public static readonly DerObjectIdentifier IdAlgCmsRC2Wrap = IdAlg.Branch("7");

	public static readonly DerObjectIdentifier IdAlgPwriKek = IdAlg.Branch("9");

	public static readonly DerObjectIdentifier IdAlgSsdh = IdAlg.Branch("10");

	public static readonly DerObjectIdentifier IdRsaKem = IdAlg.Branch("14");

	public static readonly DerObjectIdentifier IdAlgAeadChaCha20Poly1305 = IdAlg.Branch("18");

	public static readonly DerObjectIdentifier PreferSignedData = Pkcs9AtSmimeCapabilities.Branch("1");

	public static readonly DerObjectIdentifier CannotDecryptAny = Pkcs9AtSmimeCapabilities.Branch("2");

	public static readonly DerObjectIdentifier SmimeCapabilitiesVersions = Pkcs9AtSmimeCapabilities.Branch("3");

	public static readonly DerObjectIdentifier IdAAReceiptRequest = IdSmime.Branch("2.1");

	public static readonly DerObjectIdentifier IdCTAuthData = new DerObjectIdentifier("1.2.840.113549.1.9.16.1.2");

	public static readonly DerObjectIdentifier IdCTTstInfo = new DerObjectIdentifier("1.2.840.113549.1.9.16.1.4");

	public static readonly DerObjectIdentifier IdCTCompressedData = new DerObjectIdentifier("1.2.840.113549.1.9.16.1.9");

	public static readonly DerObjectIdentifier IdCTAuthEnvelopedData = new DerObjectIdentifier("1.2.840.113549.1.9.16.1.23");

	public static readonly DerObjectIdentifier IdCTTimestampedData = new DerObjectIdentifier("1.2.840.113549.1.9.16.1.31");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfOrigin = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.1");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfReceipt = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.2");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfDelivery = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.3");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfSender = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.4");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfApproval = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.5");

	public static readonly DerObjectIdentifier IdCtiEtsProofOfCreation = new DerObjectIdentifier("1.2.840.113549.1.9.16.6.6");

	public static readonly DerObjectIdentifier IdAAOid = new DerObjectIdentifier("1.2.840.113549.1.9.16.2");

	public static readonly DerObjectIdentifier IdAAContentHint = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.4");

	public static readonly DerObjectIdentifier IdAAMsgSigDigest = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.5");

	public static readonly DerObjectIdentifier IdAAContentReference = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.10");

	public static readonly DerObjectIdentifier IdAAEncrypKeyPref = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.11");

	public static readonly DerObjectIdentifier IdAASigningCertificate = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.12");

	public static readonly DerObjectIdentifier IdAASigningCertificateV2 = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.47");

	public static readonly DerObjectIdentifier IdAAContentIdentifier = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.7");

	public static readonly DerObjectIdentifier IdAASignatureTimeStampToken = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.14");

	public static readonly DerObjectIdentifier IdAAEtsSigPolicyID = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.15");

	public static readonly DerObjectIdentifier IdAAEtsCommitmentType = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.16");

	public static readonly DerObjectIdentifier IdAAEtsSignerLocation = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.17");

	public static readonly DerObjectIdentifier IdAAEtsSignerAttr = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.18");

	public static readonly DerObjectIdentifier IdAAEtsOtherSigCert = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.19");

	public static readonly DerObjectIdentifier IdAAEtsContentTimestamp = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.20");

	public static readonly DerObjectIdentifier IdAAEtsCertificateRefs = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.21");

	public static readonly DerObjectIdentifier IdAAEtsRevocationRefs = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.22");

	public static readonly DerObjectIdentifier IdAAEtsCertValues = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.23");

	public static readonly DerObjectIdentifier IdAAEtsRevocationValues = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.24");

	public static readonly DerObjectIdentifier IdAAEtsEscTimeStamp = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.25");

	public static readonly DerObjectIdentifier IdAAEtsCertCrlTimestamp = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.26");

	public static readonly DerObjectIdentifier IdAAEtsArchiveTimestamp = new DerObjectIdentifier("1.2.840.113549.1.9.16.2.27");

	public static readonly DerObjectIdentifier IdAADecryptKeyID = IdAAOid.Branch("37");

	public static readonly DerObjectIdentifier IdAAImplCryptoAlgs = IdAAOid.Branch("38");

	public static readonly DerObjectIdentifier IdAAAsymmDecryptKeyID = IdAAOid.Branch("54");

	public static readonly DerObjectIdentifier IdAAImplCompressAlgs = IdAAOid.Branch("43");

	public static readonly DerObjectIdentifier IdAACommunityIdentifiers = IdAAOid.Branch("40");

	[Obsolete("Use 'IdAAEtsSigPolicyID' instead")]
	public static readonly DerObjectIdentifier IdAASigPolicyID = IdAAEtsSigPolicyID;

	[Obsolete("Use 'IdAAEtsCommitmentType' instead")]
	public static readonly DerObjectIdentifier IdAACommitmentType = IdAAEtsCommitmentType;

	[Obsolete("Use 'IdAAEtsSignerLocation' instead")]
	public static readonly DerObjectIdentifier IdAASignerLocation = IdAAEtsSignerLocation;

	[Obsolete("Use 'IdAAEtsOtherSigCert' instead")]
	public static readonly DerObjectIdentifier IdAAOtherSigCert = IdAAEtsOtherSigCert;

	public static readonly DerObjectIdentifier IdSpqEtsUri = new DerObjectIdentifier("1.2.840.113549.1.9.16.5.1");

	public static readonly DerObjectIdentifier IdSpqEtsUNotice = new DerObjectIdentifier("1.2.840.113549.1.9.16.5.2");

	public static readonly DerObjectIdentifier KeyBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.1");

	public static readonly DerObjectIdentifier Pkcs8ShroudedKeyBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.2");

	public static readonly DerObjectIdentifier CertBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.3");

	public static readonly DerObjectIdentifier CrlBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.4");

	public static readonly DerObjectIdentifier SecretBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.5");

	public static readonly DerObjectIdentifier SafeContentsBag = new DerObjectIdentifier("1.2.840.113549.1.12.10.1.6");

	public static readonly DerObjectIdentifier PbeWithShaAnd128BitRC4 = new DerObjectIdentifier("1.2.840.113549.1.12.1.1");

	public static readonly DerObjectIdentifier PbeWithShaAnd40BitRC4 = new DerObjectIdentifier("1.2.840.113549.1.12.1.2");

	public static readonly DerObjectIdentifier PbeWithShaAnd3KeyTripleDesCbc = new DerObjectIdentifier("1.2.840.113549.1.12.1.3");

	public static readonly DerObjectIdentifier PbeWithShaAnd2KeyTripleDesCbc = new DerObjectIdentifier("1.2.840.113549.1.12.1.4");

	public static readonly DerObjectIdentifier PbeWithShaAnd128BitRC2Cbc = new DerObjectIdentifier("1.2.840.113549.1.12.1.5");

	public static readonly DerObjectIdentifier PbewithShaAnd40BitRC2Cbc = new DerObjectIdentifier("1.2.840.113549.1.12.1.6");
}
