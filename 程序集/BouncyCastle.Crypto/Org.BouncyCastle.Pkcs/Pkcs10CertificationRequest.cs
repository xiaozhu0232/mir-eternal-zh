using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkcs;

public class Pkcs10CertificationRequest : CertificationRequest
{
	protected static readonly IDictionary algorithms;

	protected static readonly IDictionary exParams;

	protected static readonly IDictionary keyAlgorithms;

	protected static readonly IDictionary oids;

	protected static readonly ISet noParams;

	static Pkcs10CertificationRequest()
	{
		algorithms = Platform.CreateHashtable();
		exParams = Platform.CreateHashtable();
		keyAlgorithms = Platform.CreateHashtable();
		oids = Platform.CreateHashtable();
		noParams = new HashSet();
		algorithms.Add("MD2WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD2WithRsaEncryption);
		algorithms.Add("MD2WITHRSA", PkcsObjectIdentifiers.MD2WithRsaEncryption);
		algorithms.Add("MD5WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD5WithRsaEncryption);
		algorithms.Add("MD5WITHRSA", PkcsObjectIdentifiers.MD5WithRsaEncryption);
		algorithms.Add("RSAWITHMD5", PkcsObjectIdentifiers.MD5WithRsaEncryption);
		algorithms.Add("SHA1WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
		algorithms.Add("SHA1WITHRSA", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
		algorithms.Add("SHA224WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
		algorithms.Add("SHA224WITHRSA", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
		algorithms.Add("SHA256WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
		algorithms.Add("SHA256WITHRSA", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
		algorithms.Add("SHA384WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
		algorithms.Add("SHA384WITHRSA", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
		algorithms.Add("SHA512WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
		algorithms.Add("SHA512WITHRSA", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
		algorithms.Add("SHA1WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
		algorithms.Add("SHA224WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
		algorithms.Add("SHA256WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
		algorithms.Add("SHA384WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
		algorithms.Add("SHA512WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
		algorithms.Add("RSAWITHSHA1", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
		algorithms.Add("RIPEMD128WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
		algorithms.Add("RIPEMD128WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
		algorithms.Add("RIPEMD160WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
		algorithms.Add("RIPEMD160WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
		algorithms.Add("RIPEMD256WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
		algorithms.Add("RIPEMD256WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
		algorithms.Add("SHA1WITHDSA", X9ObjectIdentifiers.IdDsaWithSha1);
		algorithms.Add("DSAWITHSHA1", X9ObjectIdentifiers.IdDsaWithSha1);
		algorithms.Add("SHA224WITHDSA", NistObjectIdentifiers.DsaWithSha224);
		algorithms.Add("SHA256WITHDSA", NistObjectIdentifiers.DsaWithSha256);
		algorithms.Add("SHA384WITHDSA", NistObjectIdentifiers.DsaWithSha384);
		algorithms.Add("SHA512WITHDSA", NistObjectIdentifiers.DsaWithSha512);
		algorithms.Add("SHA1WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha1);
		algorithms.Add("SHA224WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha224);
		algorithms.Add("SHA256WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha256);
		algorithms.Add("SHA384WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha384);
		algorithms.Add("SHA512WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha512);
		algorithms.Add("ECDSAWITHSHA1", X9ObjectIdentifiers.ECDsaWithSha1);
		algorithms.Add("GOST3411WITHGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
		algorithms.Add("GOST3410WITHGOST3411", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
		algorithms.Add("GOST3411WITHECGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
		algorithms.Add("GOST3411WITHECGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
		algorithms.Add("GOST3411WITHGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
		oids.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption, "SHA1WITHRSA");
		oids.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption, "SHA224WITHRSA");
		oids.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption, "SHA256WITHRSA");
		oids.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption, "SHA384WITHRSA");
		oids.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption, "SHA512WITHRSA");
		oids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94, "GOST3411WITHGOST3410");
		oids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001, "GOST3411WITHECGOST3410");
		oids.Add(PkcsObjectIdentifiers.MD5WithRsaEncryption, "MD5WITHRSA");
		oids.Add(PkcsObjectIdentifiers.MD2WithRsaEncryption, "MD2WITHRSA");
		oids.Add(X9ObjectIdentifiers.IdDsaWithSha1, "SHA1WITHDSA");
		oids.Add(X9ObjectIdentifiers.ECDsaWithSha1, "SHA1WITHECDSA");
		oids.Add(X9ObjectIdentifiers.ECDsaWithSha224, "SHA224WITHECDSA");
		oids.Add(X9ObjectIdentifiers.ECDsaWithSha256, "SHA256WITHECDSA");
		oids.Add(X9ObjectIdentifiers.ECDsaWithSha384, "SHA384WITHECDSA");
		oids.Add(X9ObjectIdentifiers.ECDsaWithSha512, "SHA512WITHECDSA");
		oids.Add(OiwObjectIdentifiers.MD5WithRsa, "MD5WITHRSA");
		oids.Add(OiwObjectIdentifiers.Sha1WithRsa, "SHA1WITHRSA");
		oids.Add(OiwObjectIdentifiers.DsaWithSha1, "SHA1WITHDSA");
		oids.Add(NistObjectIdentifiers.DsaWithSha224, "SHA224WITHDSA");
		oids.Add(NistObjectIdentifiers.DsaWithSha256, "SHA256WITHDSA");
		keyAlgorithms.Add(PkcsObjectIdentifiers.RsaEncryption, "RSA");
		keyAlgorithms.Add(X9ObjectIdentifiers.IdDsa, "DSA");
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha1);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha224);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha256);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha384);
		noParams.Add(X9ObjectIdentifiers.ECDsaWithSha512);
		noParams.Add(X9ObjectIdentifiers.IdDsaWithSha1);
		noParams.Add(OiwObjectIdentifiers.DsaWithSha1);
		noParams.Add(NistObjectIdentifiers.DsaWithSha224);
		noParams.Add(NistObjectIdentifiers.DsaWithSha256);
		noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
		noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
		AlgorithmIdentifier hashAlgId = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
		exParams.Add("SHA1WITHRSAANDMGF1", CreatePssParams(hashAlgId, 20));
		AlgorithmIdentifier hashAlgId2 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha224, DerNull.Instance);
		exParams.Add("SHA224WITHRSAANDMGF1", CreatePssParams(hashAlgId2, 28));
		AlgorithmIdentifier hashAlgId3 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha256, DerNull.Instance);
		exParams.Add("SHA256WITHRSAANDMGF1", CreatePssParams(hashAlgId3, 32));
		AlgorithmIdentifier hashAlgId4 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha384, DerNull.Instance);
		exParams.Add("SHA384WITHRSAANDMGF1", CreatePssParams(hashAlgId4, 48));
		AlgorithmIdentifier hashAlgId5 = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha512, DerNull.Instance);
		exParams.Add("SHA512WITHRSAANDMGF1", CreatePssParams(hashAlgId5, 64));
	}

	private static RsassaPssParameters CreatePssParams(AlgorithmIdentifier hashAlgId, int saltSize)
	{
		return new RsassaPssParameters(hashAlgId, new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, hashAlgId), new DerInteger(saltSize), new DerInteger(1));
	}

	protected Pkcs10CertificationRequest()
	{
	}

	public Pkcs10CertificationRequest(byte[] encoded)
		: base((Asn1Sequence)Asn1Object.FromByteArray(encoded))
	{
	}

	public Pkcs10CertificationRequest(Asn1Sequence seq)
		: base(seq)
	{
	}

	public Pkcs10CertificationRequest(Stream input)
		: base((Asn1Sequence)Asn1Object.FromStream(input))
	{
	}

	public Pkcs10CertificationRequest(string signatureAlgorithm, X509Name subject, AsymmetricKeyParameter publicKey, Asn1Set attributes, AsymmetricKeyParameter signingKey)
		: this(new Asn1SignatureFactory(signatureAlgorithm, signingKey), subject, publicKey, attributes)
	{
	}

	[Obsolete("Use constructor without 'signingKey' parameter (ignored here)")]
	public Pkcs10CertificationRequest(ISignatureFactory signatureFactory, X509Name subject, AsymmetricKeyParameter publicKey, Asn1Set attributes, AsymmetricKeyParameter signingKey)
		: this(signatureFactory, subject, publicKey, attributes)
	{
	}

	public Pkcs10CertificationRequest(ISignatureFactory signatureFactory, X509Name subject, AsymmetricKeyParameter publicKey, Asn1Set attributes)
	{
		if (signatureFactory == null)
		{
			throw new ArgumentNullException("signatureFactory");
		}
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (publicKey.IsPrivate)
		{
			throw new ArgumentException("expected public key", "publicKey");
		}
		Init(signatureFactory, subject, publicKey, attributes);
	}

	private void Init(ISignatureFactory signatureFactory, X509Name subject, AsymmetricKeyParameter publicKey, Asn1Set attributes)
	{
		sigAlgId = (AlgorithmIdentifier)signatureFactory.AlgorithmDetails;
		SubjectPublicKeyInfo pkInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
		reqInfo = new CertificationRequestInfo(subject, pkInfo, attributes);
		IStreamCalculator streamCalculator = signatureFactory.CreateCalculator();
		byte[] derEncoded = reqInfo.GetDerEncoded();
		streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
		Platform.Dispose(streamCalculator.Stream);
		sigBits = new DerBitString(((IBlockResult)streamCalculator.GetResult()).Collect());
	}

	public AsymmetricKeyParameter GetPublicKey()
	{
		return PublicKeyFactory.CreateKey(reqInfo.SubjectPublicKeyInfo);
	}

	public bool Verify()
	{
		return Verify(GetPublicKey());
	}

	public bool Verify(AsymmetricKeyParameter publicKey)
	{
		return Verify(new Asn1VerifierFactoryProvider(publicKey));
	}

	public bool Verify(IVerifierFactoryProvider verifierProvider)
	{
		return Verify(verifierProvider.CreateVerifierFactory(sigAlgId));
	}

	public bool Verify(IVerifierFactory verifier)
	{
		try
		{
			byte[] derEncoded = reqInfo.GetDerEncoded();
			IStreamCalculator streamCalculator = verifier.CreateCalculator();
			streamCalculator.Stream.Write(derEncoded, 0, derEncoded.Length);
			Platform.Dispose(streamCalculator.Stream);
			return ((IVerifier)streamCalculator.GetResult()).IsVerified(sigBits.GetOctets());
		}
		catch (Exception exception)
		{
			throw new SignatureException("exception encoding TBS cert request", exception);
		}
	}

	private void SetSignatureParameters(ISigner signature, Asn1Encodable asn1Params)
	{
		if (asn1Params != null && !(asn1Params is Asn1Null) && Platform.EndsWith(signature.AlgorithmName, "MGF1"))
		{
			throw Platform.CreateNotImplementedException("signature algorithm with MGF1");
		}
	}

	internal static string GetSignatureName(AlgorithmIdentifier sigAlgId)
	{
		Asn1Encodable parameters = sigAlgId.Parameters;
		if (parameters != null && !(parameters is Asn1Null) && sigAlgId.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss))
		{
			RsassaPssParameters instance = RsassaPssParameters.GetInstance(parameters);
			return GetDigestAlgName(instance.HashAlgorithm.Algorithm) + "withRSAandMGF1";
		}
		return sigAlgId.Algorithm.Id;
	}

	private static string GetDigestAlgName(DerObjectIdentifier digestAlgOID)
	{
		if (PkcsObjectIdentifiers.MD5.Equals(digestAlgOID))
		{
			return "MD5";
		}
		if (OiwObjectIdentifiers.IdSha1.Equals(digestAlgOID))
		{
			return "SHA1";
		}
		if (NistObjectIdentifiers.IdSha224.Equals(digestAlgOID))
		{
			return "SHA224";
		}
		if (NistObjectIdentifiers.IdSha256.Equals(digestAlgOID))
		{
			return "SHA256";
		}
		if (NistObjectIdentifiers.IdSha384.Equals(digestAlgOID))
		{
			return "SHA384";
		}
		if (NistObjectIdentifiers.IdSha512.Equals(digestAlgOID))
		{
			return "SHA512";
		}
		if (TeleTrusTObjectIdentifiers.RipeMD128.Equals(digestAlgOID))
		{
			return "RIPEMD128";
		}
		if (TeleTrusTObjectIdentifiers.RipeMD160.Equals(digestAlgOID))
		{
			return "RIPEMD160";
		}
		if (TeleTrusTObjectIdentifiers.RipeMD256.Equals(digestAlgOID))
		{
			return "RIPEMD256";
		}
		if (CryptoProObjectIdentifiers.GostR3411.Equals(digestAlgOID))
		{
			return "GOST3411";
		}
		return digestAlgOID.Id;
	}
}
