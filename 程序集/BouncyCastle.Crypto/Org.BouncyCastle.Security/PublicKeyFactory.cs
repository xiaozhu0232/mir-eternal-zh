using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Rosstandart;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security;

public sealed class PublicKeyFactory
{
	private PublicKeyFactory()
	{
	}

	public static AsymmetricKeyParameter CreateKey(byte[] keyInfoData)
	{
		return CreateKey(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromByteArray(keyInfoData)));
	}

	public static AsymmetricKeyParameter CreateKey(Stream inStr)
	{
		return CreateKey(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromStream(inStr)));
	}

	public static AsymmetricKeyParameter CreateKey(SubjectPublicKeyInfo keyInfo)
	{
		AlgorithmIdentifier algorithmID = keyInfo.AlgorithmID;
		DerObjectIdentifier algorithm = algorithmID.Algorithm;
		if (algorithm.Equals(PkcsObjectIdentifiers.RsaEncryption) || algorithm.Equals(X509ObjectIdentifiers.IdEARsa) || algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss) || algorithm.Equals(PkcsObjectIdentifiers.IdRsaesOaep))
		{
			RsaPublicKeyStructure instance = RsaPublicKeyStructure.GetInstance(keyInfo.ParsePublicKey());
			return new RsaKeyParameters(isPrivate: false, instance.Modulus, instance.PublicExponent);
		}
		if (algorithm.Equals(X9ObjectIdentifiers.DHPublicNumber))
		{
			Asn1Sequence instance2 = Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object());
			DHPublicKey instance3 = DHPublicKey.GetInstance(keyInfo.ParsePublicKey());
			BigInteger value = instance3.Y.Value;
			if (IsPkcsDHParam(instance2))
			{
				return ReadPkcsDHParam(algorithm, value, instance2);
			}
			DHDomainParameters instance4 = DHDomainParameters.GetInstance(instance2);
			BigInteger value2 = instance4.P.Value;
			BigInteger value3 = instance4.G.Value;
			BigInteger value4 = instance4.Q.Value;
			BigInteger j = null;
			if (instance4.J != null)
			{
				j = instance4.J.Value;
			}
			DHValidationParameters validation = null;
			DHValidationParms validationParms = instance4.ValidationParms;
			if (validationParms != null)
			{
				byte[] bytes = validationParms.Seed.GetBytes();
				BigInteger value5 = validationParms.PgenCounter.Value;
				validation = new DHValidationParameters(bytes, value5.IntValue);
			}
			return new DHPublicKeyParameters(value, new DHParameters(value2, value3, value4, j, validation));
		}
		if (algorithm.Equals(PkcsObjectIdentifiers.DhKeyAgreement))
		{
			Asn1Sequence instance5 = Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object());
			DerInteger derInteger = (DerInteger)keyInfo.ParsePublicKey();
			return ReadPkcsDHParam(algorithm, derInteger.Value, instance5);
		}
		if (algorithm.Equals(OiwObjectIdentifiers.ElGamalAlgorithm))
		{
			ElGamalParameter elGamalParameter = new ElGamalParameter(Asn1Sequence.GetInstance(algorithmID.Parameters.ToAsn1Object()));
			DerInteger derInteger2 = (DerInteger)keyInfo.ParsePublicKey();
			return new ElGamalPublicKeyParameters(derInteger2.Value, new ElGamalParameters(elGamalParameter.P, elGamalParameter.G));
		}
		if (algorithm.Equals(X9ObjectIdentifiers.IdDsa) || algorithm.Equals(OiwObjectIdentifiers.DsaWithSha1))
		{
			DerInteger derInteger3 = (DerInteger)keyInfo.ParsePublicKey();
			Asn1Encodable parameters = algorithmID.Parameters;
			DsaParameters parameters2 = null;
			if (parameters != null)
			{
				DsaParameter instance6 = DsaParameter.GetInstance(parameters.ToAsn1Object());
				parameters2 = new DsaParameters(instance6.P, instance6.Q, instance6.G);
			}
			return new DsaPublicKeyParameters(derInteger3.Value, parameters2);
		}
		if (algorithm.Equals(X9ObjectIdentifiers.IdECPublicKey))
		{
			X962Parameters instance7 = X962Parameters.GetInstance(algorithmID.Parameters.ToAsn1Object());
			X9ECParameters x9ECParameters = ((!instance7.IsNamedCurve) ? new X9ECParameters((Asn1Sequence)instance7.Parameters) : ECKeyPairGenerator.FindECCurveByOid((DerObjectIdentifier)instance7.Parameters));
			Asn1OctetString s = new DerOctetString(keyInfo.PublicKeyData.GetBytes());
			X9ECPoint x9ECPoint = new X9ECPoint(x9ECParameters.Curve, s);
			ECPoint point = x9ECPoint.Point;
			if (instance7.IsNamedCurve)
			{
				return new ECPublicKeyParameters("EC", point, (DerObjectIdentifier)instance7.Parameters);
			}
			ECDomainParameters parameters3 = new ECDomainParameters(x9ECParameters);
			return new ECPublicKeyParameters(point, parameters3);
		}
		if (algorithm.Equals(CryptoProObjectIdentifiers.GostR3410x2001))
		{
			Gost3410PublicKeyAlgParameters instance8 = Gost3410PublicKeyAlgParameters.GetInstance(algorithmID.Parameters);
			DerObjectIdentifier publicKeyParamSet = instance8.PublicKeyParamSet;
			X9ECParameters byOidX = ECGost3410NamedCurves.GetByOidX9(publicKeyParamSet);
			if (byOidX == null)
			{
				return null;
			}
			Asn1OctetString asn1OctetString;
			try
			{
				asn1OctetString = (Asn1OctetString)keyInfo.ParsePublicKey();
			}
			catch (IOException innerException)
			{
				throw new ArgumentException("error recovering GOST3410_2001 public key", innerException);
			}
			int num = 32;
			int num2 = 2 * num;
			byte[] octets = asn1OctetString.GetOctets();
			if (octets.Length != num2)
			{
				throw new ArgumentException("invalid length for GOST3410_2001 public key");
			}
			byte[] array = new byte[1 + num2];
			array[0] = 4;
			for (int i = 1; i <= num; i++)
			{
				array[i] = octets[num - i];
				array[i + num] = octets[num2 - i];
			}
			ECPoint q = byOidX.Curve.DecodePoint(array);
			return new ECPublicKeyParameters("ECGOST3410", q, publicKeyParamSet);
		}
		if (algorithm.Equals(CryptoProObjectIdentifiers.GostR3410x94))
		{
			Gost3410PublicKeyAlgParameters instance9 = Gost3410PublicKeyAlgParameters.GetInstance(algorithmID.Parameters);
			Asn1OctetString asn1OctetString2;
			try
			{
				asn1OctetString2 = (Asn1OctetString)keyInfo.ParsePublicKey();
			}
			catch (IOException innerException2)
			{
				throw new ArgumentException("error recovering GOST3410_94 public key", innerException2);
			}
			byte[] bytes2 = Arrays.Reverse(asn1OctetString2.GetOctets());
			BigInteger y = new BigInteger(1, bytes2);
			return new Gost3410PublicKeyParameters(y, instance9.PublicKeyParamSet);
		}
		if (algorithm.Equals(EdECObjectIdentifiers.id_X25519))
		{
			return new X25519PublicKeyParameters(GetRawKey(keyInfo, X25519PublicKeyParameters.KeySize), 0);
		}
		if (algorithm.Equals(EdECObjectIdentifiers.id_X448))
		{
			return new X448PublicKeyParameters(GetRawKey(keyInfo, X448PublicKeyParameters.KeySize), 0);
		}
		if (algorithm.Equals(EdECObjectIdentifiers.id_Ed25519))
		{
			return new Ed25519PublicKeyParameters(GetRawKey(keyInfo, Ed25519PublicKeyParameters.KeySize), 0);
		}
		if (algorithm.Equals(EdECObjectIdentifiers.id_Ed448))
		{
			return new Ed448PublicKeyParameters(GetRawKey(keyInfo, Ed448PublicKeyParameters.KeySize), 0);
		}
		if (algorithm.Equals(RosstandartObjectIdentifiers.id_tc26_gost_3410_12_256) || algorithm.Equals(RosstandartObjectIdentifiers.id_tc26_gost_3410_12_512))
		{
			Gost3410PublicKeyAlgParameters instance10 = Gost3410PublicKeyAlgParameters.GetInstance(algorithmID.Parameters);
			DerObjectIdentifier publicKeyParamSet2 = instance10.PublicKeyParamSet;
			ECGost3410Parameters eCGost3410Parameters = new ECGost3410Parameters(new ECNamedDomainParameters(publicKeyParamSet2, ECGost3410NamedCurves.GetByOidX9(publicKeyParamSet2)), publicKeyParamSet2, instance10.DigestParamSet, instance10.EncryptionParamSet);
			Asn1OctetString asn1OctetString3;
			try
			{
				asn1OctetString3 = (Asn1OctetString)keyInfo.ParsePublicKey();
			}
			catch (IOException innerException3)
			{
				throw new ArgumentException("error recovering GOST3410_2012 public key", innerException3);
			}
			int num3 = 32;
			if (algorithm.Equals(RosstandartObjectIdentifiers.id_tc26_gost_3410_12_512))
			{
				num3 = 64;
			}
			int num4 = 2 * num3;
			byte[] octets2 = asn1OctetString3.GetOctets();
			if (octets2.Length != num4)
			{
				throw new ArgumentException("invalid length for GOST3410_2012 public key");
			}
			byte[] array2 = new byte[1 + num4];
			array2[0] = 4;
			for (int k = 1; k <= num3; k++)
			{
				array2[k] = octets2[num3 - k];
				array2[k + num3] = octets2[num4 - k];
			}
			ECPoint q2 = eCGost3410Parameters.Curve.DecodePoint(array2);
			return new ECPublicKeyParameters(q2, eCGost3410Parameters);
		}
		throw new SecurityUtilityException("algorithm identifier in public key not recognised: " + algorithm);
	}

	private static byte[] GetRawKey(SubjectPublicKeyInfo keyInfo, int expectedSize)
	{
		byte[] octets = keyInfo.PublicKeyData.GetOctets();
		if (expectedSize != octets.Length)
		{
			throw new SecurityUtilityException("public key encoding has incorrect length");
		}
		return octets;
	}

	private static bool IsPkcsDHParam(Asn1Sequence seq)
	{
		if (seq.Count == 2)
		{
			return true;
		}
		if (seq.Count > 3)
		{
			return false;
		}
		DerInteger instance = DerInteger.GetInstance(seq[2]);
		DerInteger instance2 = DerInteger.GetInstance(seq[0]);
		return instance.Value.CompareTo(BigInteger.ValueOf(instance2.Value.BitLength)) <= 0;
	}

	private static DHPublicKeyParameters ReadPkcsDHParam(DerObjectIdentifier algOid, BigInteger y, Asn1Sequence seq)
	{
		DHParameter dHParameter = new DHParameter(seq);
		int l = dHParameter.L?.IntValue ?? 0;
		DHParameters parameters = new DHParameters(dHParameter.P, dHParameter.G, null, l);
		return new DHPublicKeyParameters(y, parameters, algOid);
	}
}
