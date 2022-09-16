using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Rosstandart;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509;

public sealed class SubjectPublicKeyInfoFactory
{
	private SubjectPublicKeyInfoFactory()
	{
	}

	public static SubjectPublicKeyInfo CreateSubjectPublicKeyInfo(AsymmetricKeyParameter publicKey)
	{
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (publicKey.IsPrivate)
		{
			throw new ArgumentException("Private key passed - public key expected.", "publicKey");
		}
		if (publicKey is ElGamalPublicKeyParameters)
		{
			ElGamalPublicKeyParameters elGamalPublicKeyParameters = (ElGamalPublicKeyParameters)publicKey;
			ElGamalParameters parameters = elGamalPublicKeyParameters.Parameters;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(OiwObjectIdentifiers.ElGamalAlgorithm, new ElGamalParameter(parameters.P, parameters.G).ToAsn1Object()), new DerInteger(elGamalPublicKeyParameters.Y));
		}
		if (publicKey is DsaPublicKeyParameters)
		{
			DsaPublicKeyParameters dsaPublicKeyParameters = (DsaPublicKeyParameters)publicKey;
			DsaParameters parameters2 = dsaPublicKeyParameters.Parameters;
			Asn1Encodable parameters3 = ((parameters2 == null) ? null : new DsaParameter(parameters2.P, parameters2.Q, parameters2.G).ToAsn1Object());
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(X9ObjectIdentifiers.IdDsa, parameters3), new DerInteger(dsaPublicKeyParameters.Y));
		}
		if (publicKey is DHPublicKeyParameters)
		{
			DHPublicKeyParameters dHPublicKeyParameters = (DHPublicKeyParameters)publicKey;
			DHParameters parameters4 = dHPublicKeyParameters.Parameters;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(dHPublicKeyParameters.AlgorithmOid, new DHParameter(parameters4.P, parameters4.G, parameters4.L).ToAsn1Object()), new DerInteger(dHPublicKeyParameters.Y));
		}
		if (publicKey is RsaKeyParameters)
		{
			RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)publicKey;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance), new RsaPublicKeyStructure(rsaKeyParameters.Modulus, rsaKeyParameters.Exponent).ToAsn1Object());
		}
		if (publicKey is ECPublicKeyParameters)
		{
			ECPublicKeyParameters eCPublicKeyParameters = (ECPublicKeyParameters)publicKey;
			if (eCPublicKeyParameters.Parameters is ECGost3410Parameters)
			{
				ECGost3410Parameters eCGost3410Parameters = (ECGost3410Parameters)eCPublicKeyParameters.Parameters;
				BigInteger bigInteger = eCPublicKeyParameters.Q.AffineXCoord.ToBigInteger();
				BigInteger bI = eCPublicKeyParameters.Q.AffineYCoord.ToBigInteger();
				bool flag = bigInteger.BitLength > 256;
				Gost3410PublicKeyAlgParameters parameters5 = new Gost3410PublicKeyAlgParameters(eCGost3410Parameters.PublicKeyParamSet, eCGost3410Parameters.DigestParamSet, eCGost3410Parameters.EncryptionParamSet);
				int num;
				int offSet;
				DerObjectIdentifier algorithm;
				if (flag)
				{
					num = 128;
					offSet = 64;
					algorithm = RosstandartObjectIdentifiers.id_tc26_gost_3410_12_512;
				}
				else
				{
					num = 64;
					offSet = 32;
					algorithm = RosstandartObjectIdentifiers.id_tc26_gost_3410_12_256;
				}
				byte[] array = new byte[num];
				ExtractBytes(array, num / 2, 0, bigInteger);
				ExtractBytes(array, num / 2, offSet, bI);
				return new SubjectPublicKeyInfo(new AlgorithmIdentifier(algorithm, parameters5), new DerOctetString(array));
			}
			if (eCPublicKeyParameters.AlgorithmName == "ECGOST3410")
			{
				if (eCPublicKeyParameters.PublicKeyParamSet == null)
				{
					throw Platform.CreateNotImplementedException("Not a CryptoPro parameter set");
				}
				ECPoint eCPoint = eCPublicKeyParameters.Q.Normalize();
				BigInteger bI2 = eCPoint.AffineXCoord.ToBigInteger();
				BigInteger bI3 = eCPoint.AffineYCoord.ToBigInteger();
				byte[] array2 = new byte[64];
				ExtractBytes(array2, 0, bI2);
				ExtractBytes(array2, 32, bI3);
				Gost3410PublicKeyAlgParameters gost3410PublicKeyAlgParameters = new Gost3410PublicKeyAlgParameters(eCPublicKeyParameters.PublicKeyParamSet, CryptoProObjectIdentifiers.GostR3411x94CryptoProParamSet);
				AlgorithmIdentifier algID = new AlgorithmIdentifier(CryptoProObjectIdentifiers.GostR3410x2001, gost3410PublicKeyAlgParameters.ToAsn1Object());
				return new SubjectPublicKeyInfo(algID, new DerOctetString(array2));
			}
			X962Parameters x962Parameters;
			if (eCPublicKeyParameters.PublicKeyParamSet == null)
			{
				ECDomainParameters parameters6 = eCPublicKeyParameters.Parameters;
				X9ECParameters ecParameters = new X9ECParameters(parameters6.Curve, parameters6.G, parameters6.N, parameters6.H, parameters6.GetSeed());
				x962Parameters = new X962Parameters(ecParameters);
			}
			else
			{
				x962Parameters = new X962Parameters(eCPublicKeyParameters.PublicKeyParamSet);
			}
			byte[] encoded = eCPublicKeyParameters.Q.GetEncoded(compressed: false);
			AlgorithmIdentifier algID2 = new AlgorithmIdentifier(X9ObjectIdentifiers.IdECPublicKey, x962Parameters.ToAsn1Object());
			return new SubjectPublicKeyInfo(algID2, encoded);
		}
		if (publicKey is Gost3410PublicKeyParameters)
		{
			Gost3410PublicKeyParameters gost3410PublicKeyParameters = (Gost3410PublicKeyParameters)publicKey;
			if (gost3410PublicKeyParameters.PublicKeyParamSet == null)
			{
				throw Platform.CreateNotImplementedException("Not a CryptoPro parameter set");
			}
			byte[] array3 = gost3410PublicKeyParameters.Y.ToByteArrayUnsigned();
			byte[] array4 = new byte[array3.Length];
			for (int i = 0; i != array4.Length; i++)
			{
				array4[i] = array3[array3.Length - 1 - i];
			}
			Gost3410PublicKeyAlgParameters gost3410PublicKeyAlgParameters2 = new Gost3410PublicKeyAlgParameters(gost3410PublicKeyParameters.PublicKeyParamSet, CryptoProObjectIdentifiers.GostR3411x94CryptoProParamSet);
			AlgorithmIdentifier algID3 = new AlgorithmIdentifier(CryptoProObjectIdentifiers.GostR3410x94, gost3410PublicKeyAlgParameters2.ToAsn1Object());
			return new SubjectPublicKeyInfo(algID3, new DerOctetString(array4));
		}
		if (publicKey is X448PublicKeyParameters)
		{
			X448PublicKeyParameters x448PublicKeyParameters = (X448PublicKeyParameters)publicKey;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(EdECObjectIdentifiers.id_X448), x448PublicKeyParameters.GetEncoded());
		}
		if (publicKey is X25519PublicKeyParameters)
		{
			X25519PublicKeyParameters x25519PublicKeyParameters = (X25519PublicKeyParameters)publicKey;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(EdECObjectIdentifiers.id_X25519), x25519PublicKeyParameters.GetEncoded());
		}
		if (publicKey is Ed448PublicKeyParameters)
		{
			Ed448PublicKeyParameters ed448PublicKeyParameters = (Ed448PublicKeyParameters)publicKey;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed448), ed448PublicKeyParameters.GetEncoded());
		}
		if (publicKey is Ed25519PublicKeyParameters)
		{
			Ed25519PublicKeyParameters ed25519PublicKeyParameters = (Ed25519PublicKeyParameters)publicKey;
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed25519), ed25519PublicKeyParameters.GetEncoded());
		}
		throw new ArgumentException("Class provided no convertible: " + Platform.GetTypeName(publicKey));
	}

	private static void ExtractBytes(byte[] encKey, int offset, BigInteger bI)
	{
		byte[] array = bI.ToByteArray();
		int num = (bI.BitLength + 7) / 8;
		for (int i = 0; i < num; i++)
		{
			encKey[offset + i] = array[array.Length - 1 - i];
		}
	}

	private static void ExtractBytes(byte[] encKey, int size, int offSet, BigInteger bI)
	{
		byte[] array = bI.ToByteArray();
		if (array.Length < size)
		{
			byte[] array2 = new byte[size];
			Array.Copy(array, 0, array2, array2.Length - array.Length, array.Length);
			array = array2;
		}
		for (int i = 0; i != size; i++)
		{
			encKey[offSet + i] = array[array.Length - 1 - i];
		}
	}
}
