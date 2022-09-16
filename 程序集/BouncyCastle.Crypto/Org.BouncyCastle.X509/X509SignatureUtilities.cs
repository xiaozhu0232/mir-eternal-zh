using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.X509;

internal class X509SignatureUtilities
{
	private static readonly Asn1Null derNull = DerNull.Instance;

	internal static void SetSignatureParameters(ISigner signature, Asn1Encodable parameters)
	{
		if (parameters != null)
		{
			derNull.Equals(parameters);
		}
	}

	internal static string GetSignatureName(AlgorithmIdentifier sigAlgId)
	{
		Asn1Encodable parameters = sigAlgId.Parameters;
		if (parameters != null && !derNull.Equals(parameters))
		{
			if (sigAlgId.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss))
			{
				RsassaPssParameters instance = RsassaPssParameters.GetInstance(parameters);
				return GetDigestAlgName(instance.HashAlgorithm.Algorithm) + "withRSAandMGF1";
			}
			if (sigAlgId.Algorithm.Equals(X9ObjectIdentifiers.ECDsaWithSha2))
			{
				Asn1Sequence instance2 = Asn1Sequence.GetInstance(parameters);
				return GetDigestAlgName((DerObjectIdentifier)instance2[0]) + "withECDSA";
			}
		}
		string encodingName = SignerUtilities.GetEncodingName(sigAlgId.Algorithm);
		if (encodingName != null)
		{
			return encodingName;
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
