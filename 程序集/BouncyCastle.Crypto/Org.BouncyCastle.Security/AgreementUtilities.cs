using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security;

public sealed class AgreementUtilities
{
	private static readonly IDictionary algorithms;

	private AgreementUtilities()
	{
	}

	static AgreementUtilities()
	{
		algorithms = Platform.CreateHashtable();
		algorithms[X9ObjectIdentifiers.DHSinglePassCofactorDHSha1KdfScheme.Id] = "ECCDHWITHSHA1KDF";
		algorithms[X9ObjectIdentifiers.DHSinglePassStdDHSha1KdfScheme.Id] = "ECDHWITHSHA1KDF";
		algorithms[X9ObjectIdentifiers.MqvSinglePassSha1KdfScheme.Id] = "ECMQVWITHSHA1KDF";
		algorithms[EdECObjectIdentifiers.id_X25519.Id] = "X25519";
		algorithms[EdECObjectIdentifiers.id_X448.Id] = "X448";
	}

	public static IBasicAgreement GetBasicAgreement(DerObjectIdentifier oid)
	{
		return GetBasicAgreement(oid.Id);
	}

	public static IBasicAgreement GetBasicAgreement(string algorithm)
	{
		switch (GetMechanism(algorithm))
		{
		case "DH":
		case "DIFFIEHELLMAN":
			return new DHBasicAgreement();
		case "ECDH":
			return new ECDHBasicAgreement();
		case "ECDHC":
		case "ECCDH":
			return new ECDHCBasicAgreement();
		case "ECMQV":
			return new ECMqvBasicAgreement();
		default:
			throw new SecurityUtilityException("Basic Agreement " + algorithm + " not recognised.");
		}
	}

	public static IBasicAgreement GetBasicAgreementWithKdf(DerObjectIdentifier oid, string wrapAlgorithm)
	{
		return GetBasicAgreementWithKdf(oid.Id, wrapAlgorithm);
	}

	public static IBasicAgreement GetBasicAgreementWithKdf(string agreeAlgorithm, string wrapAlgorithm)
	{
		switch (GetMechanism(agreeAlgorithm))
		{
		case "DHWITHSHA1KDF":
		case "ECDHWITHSHA1KDF":
			return new ECDHWithKdfBasicAgreement(wrapAlgorithm, new ECDHKekGenerator(new Sha1Digest()));
		case "ECMQVWITHSHA1KDF":
			return new ECMqvWithKdfBasicAgreement(wrapAlgorithm, new ECDHKekGenerator(new Sha1Digest()));
		default:
			throw new SecurityUtilityException("Basic Agreement (with KDF) " + agreeAlgorithm + " not recognised.");
		}
	}

	public static IRawAgreement GetRawAgreement(DerObjectIdentifier oid)
	{
		return GetRawAgreement(oid.Id);
	}

	public static IRawAgreement GetRawAgreement(string algorithm)
	{
		string mechanism = GetMechanism(algorithm);
		if (mechanism == "X25519")
		{
			return new X25519Agreement();
		}
		if (mechanism == "X448")
		{
			return new X448Agreement();
		}
		throw new SecurityUtilityException("Raw Agreement " + algorithm + " not recognised.");
	}

	public static string GetAlgorithmName(DerObjectIdentifier oid)
	{
		return (string)algorithms[oid.Id];
	}

	private static string GetMechanism(string algorithm)
	{
		string text = Platform.ToUpperInvariant(algorithm);
		string text2 = (string)algorithms[text];
		if (text2 != null)
		{
			return text2;
		}
		return text;
	}
}
