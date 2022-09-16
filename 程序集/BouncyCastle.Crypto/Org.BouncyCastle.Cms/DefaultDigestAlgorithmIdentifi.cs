using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms;

public class DefaultDigestAlgorithmIdentifierFinder
{
	private static readonly IDictionary digestOids;

	private static readonly IDictionary digestNameToOids;

	static DefaultDigestAlgorithmIdentifierFinder()
	{
		digestOids = Platform.CreateHashtable();
		digestNameToOids = Platform.CreateHashtable();
		digestOids.Add(OiwObjectIdentifiers.MD4WithRsaEncryption, PkcsObjectIdentifiers.MD4);
		digestOids.Add(OiwObjectIdentifiers.MD4WithRsa, PkcsObjectIdentifiers.MD4);
		digestOids.Add(OiwObjectIdentifiers.MD5WithRsa, PkcsObjectIdentifiers.MD5);
		digestOids.Add(OiwObjectIdentifiers.Sha1WithRsa, OiwObjectIdentifiers.IdSha1);
		digestOids.Add(OiwObjectIdentifiers.DsaWithSha1, OiwObjectIdentifiers.IdSha1);
		digestOids.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption, NistObjectIdentifiers.IdSha224);
		digestOids.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption, NistObjectIdentifiers.IdSha256);
		digestOids.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption, NistObjectIdentifiers.IdSha384);
		digestOids.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption, NistObjectIdentifiers.IdSha512);
		digestOids.Add(PkcsObjectIdentifiers.MD2WithRsaEncryption, PkcsObjectIdentifiers.MD2);
		digestOids.Add(PkcsObjectIdentifiers.MD4WithRsaEncryption, PkcsObjectIdentifiers.MD4);
		digestOids.Add(PkcsObjectIdentifiers.MD5WithRsaEncryption, PkcsObjectIdentifiers.MD5);
		digestOids.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption, OiwObjectIdentifiers.IdSha1);
		digestOids.Add(X9ObjectIdentifiers.ECDsaWithSha1, OiwObjectIdentifiers.IdSha1);
		digestOids.Add(X9ObjectIdentifiers.ECDsaWithSha224, NistObjectIdentifiers.IdSha224);
		digestOids.Add(X9ObjectIdentifiers.ECDsaWithSha256, NistObjectIdentifiers.IdSha256);
		digestOids.Add(X9ObjectIdentifiers.ECDsaWithSha384, NistObjectIdentifiers.IdSha384);
		digestOids.Add(X9ObjectIdentifiers.ECDsaWithSha512, NistObjectIdentifiers.IdSha512);
		digestOids.Add(X9ObjectIdentifiers.IdDsaWithSha1, OiwObjectIdentifiers.IdSha1);
		digestOids.Add(NistObjectIdentifiers.DsaWithSha224, NistObjectIdentifiers.IdSha224);
		digestOids.Add(NistObjectIdentifiers.DsaWithSha256, NistObjectIdentifiers.IdSha256);
		digestOids.Add(NistObjectIdentifiers.DsaWithSha384, NistObjectIdentifiers.IdSha384);
		digestOids.Add(NistObjectIdentifiers.DsaWithSha512, NistObjectIdentifiers.IdSha512);
		digestOids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128, TeleTrusTObjectIdentifiers.RipeMD128);
		digestOids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160, TeleTrusTObjectIdentifiers.RipeMD160);
		digestOids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256, TeleTrusTObjectIdentifiers.RipeMD256);
		digestOids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94, CryptoProObjectIdentifiers.GostR3411);
		digestOids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001, CryptoProObjectIdentifiers.GostR3411);
		digestNameToOids.Add("SHA-1", OiwObjectIdentifiers.IdSha1);
		digestNameToOids.Add("SHA-224", NistObjectIdentifiers.IdSha224);
		digestNameToOids.Add("SHA-256", NistObjectIdentifiers.IdSha256);
		digestNameToOids.Add("SHA-384", NistObjectIdentifiers.IdSha384);
		digestNameToOids.Add("SHA-512", NistObjectIdentifiers.IdSha512);
		digestNameToOids.Add("SHA1", OiwObjectIdentifiers.IdSha1);
		digestNameToOids.Add("SHA224", NistObjectIdentifiers.IdSha224);
		digestNameToOids.Add("SHA256", NistObjectIdentifiers.IdSha256);
		digestNameToOids.Add("SHA384", NistObjectIdentifiers.IdSha384);
		digestNameToOids.Add("SHA512", NistObjectIdentifiers.IdSha512);
		digestNameToOids.Add("SHA3-224", NistObjectIdentifiers.IdSha3_224);
		digestNameToOids.Add("SHA3-256", NistObjectIdentifiers.IdSha3_256);
		digestNameToOids.Add("SHA3-384", NistObjectIdentifiers.IdSha3_384);
		digestNameToOids.Add("SHA3-512", NistObjectIdentifiers.IdSha3_512);
		digestNameToOids.Add("SHAKE-128", NistObjectIdentifiers.IdShake128);
		digestNameToOids.Add("SHAKE-256", NistObjectIdentifiers.IdShake256);
		digestNameToOids.Add("GOST3411", CryptoProObjectIdentifiers.GostR3411);
		digestNameToOids.Add("MD2", PkcsObjectIdentifiers.MD2);
		digestNameToOids.Add("MD4", PkcsObjectIdentifiers.MD4);
		digestNameToOids.Add("MD5", PkcsObjectIdentifiers.MD5);
		digestNameToOids.Add("RIPEMD128", TeleTrusTObjectIdentifiers.RipeMD128);
		digestNameToOids.Add("RIPEMD160", TeleTrusTObjectIdentifiers.RipeMD160);
		digestNameToOids.Add("RIPEMD256", TeleTrusTObjectIdentifiers.RipeMD256);
	}

	public AlgorithmIdentifier find(AlgorithmIdentifier sigAlgId)
	{
		if (sigAlgId.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss))
		{
			return RsassaPssParameters.GetInstance(sigAlgId.Parameters).HashAlgorithm;
		}
		return new AlgorithmIdentifier((DerObjectIdentifier)digestOids[sigAlgId.Algorithm], DerNull.Instance);
	}

	public AlgorithmIdentifier find(string digAlgName)
	{
		return new AlgorithmIdentifier((DerObjectIdentifier)digestNameToOids[digAlgName], DerNull.Instance);
	}
}
