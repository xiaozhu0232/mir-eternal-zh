using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Rosstandart;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Tsp;

public class TspUtil
{
	private static ISet EmptySet;

	private static IList EmptyList;

	private static readonly IDictionary digestLengths;

	private static readonly IDictionary digestNames;

	static TspUtil()
	{
		EmptySet = CollectionUtilities.ReadOnly(new HashSet());
		EmptyList = CollectionUtilities.ReadOnly(Platform.CreateArrayList());
		digestLengths = Platform.CreateHashtable();
		digestNames = Platform.CreateHashtable();
		digestLengths.Add(PkcsObjectIdentifiers.MD5.Id, 16);
		digestLengths.Add(OiwObjectIdentifiers.IdSha1.Id, 20);
		digestLengths.Add(NistObjectIdentifiers.IdSha224.Id, 28);
		digestLengths.Add(NistObjectIdentifiers.IdSha256.Id, 32);
		digestLengths.Add(NistObjectIdentifiers.IdSha384.Id, 48);
		digestLengths.Add(NistObjectIdentifiers.IdSha512.Id, 64);
		digestLengths.Add(TeleTrusTObjectIdentifiers.RipeMD128.Id, 16);
		digestLengths.Add(TeleTrusTObjectIdentifiers.RipeMD160.Id, 20);
		digestLengths.Add(TeleTrusTObjectIdentifiers.RipeMD256.Id, 32);
		digestLengths.Add(CryptoProObjectIdentifiers.GostR3411.Id, 32);
		digestLengths.Add(RosstandartObjectIdentifiers.id_tc26_gost_3411_12_256.Id, 32);
		digestLengths.Add(RosstandartObjectIdentifiers.id_tc26_gost_3411_12_512.Id, 64);
		digestLengths.Add(GMObjectIdentifiers.sm3.Id, 32);
		digestNames.Add(PkcsObjectIdentifiers.MD5.Id, "MD5");
		digestNames.Add(OiwObjectIdentifiers.IdSha1.Id, "SHA1");
		digestNames.Add(NistObjectIdentifiers.IdSha224.Id, "SHA224");
		digestNames.Add(NistObjectIdentifiers.IdSha256.Id, "SHA256");
		digestNames.Add(NistObjectIdentifiers.IdSha384.Id, "SHA384");
		digestNames.Add(NistObjectIdentifiers.IdSha512.Id, "SHA512");
		digestNames.Add(PkcsObjectIdentifiers.MD5WithRsaEncryption.Id, "MD5");
		digestNames.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption.Id, "SHA1");
		digestNames.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption.Id, "SHA224");
		digestNames.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id, "SHA256");
		digestNames.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption.Id, "SHA384");
		digestNames.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id, "SHA512");
		digestNames.Add(TeleTrusTObjectIdentifiers.RipeMD128.Id, "RIPEMD128");
		digestNames.Add(TeleTrusTObjectIdentifiers.RipeMD160.Id, "RIPEMD160");
		digestNames.Add(TeleTrusTObjectIdentifiers.RipeMD256.Id, "RIPEMD256");
		digestNames.Add(CryptoProObjectIdentifiers.GostR3411.Id, "GOST3411");
		digestNames.Add(OiwObjectIdentifiers.DsaWithSha1.Id, "SHA1");
		digestNames.Add(OiwObjectIdentifiers.Sha1WithRsa.Id, "SHA1");
		digestNames.Add(OiwObjectIdentifiers.MD5WithRsa.Id, "MD5");
		digestNames.Add(RosstandartObjectIdentifiers.id_tc26_gost_3411_12_256.Id, "GOST3411-2012-256");
		digestNames.Add(RosstandartObjectIdentifiers.id_tc26_gost_3411_12_512.Id, "GOST3411-2012-512");
		digestNames.Add(GMObjectIdentifiers.sm3.Id, "SM3");
	}

	public static ICollection GetSignatureTimestamps(SignerInformation signerInfo)
	{
		IList list = Platform.CreateArrayList();
		Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttributes = signerInfo.UnsignedAttributes;
		if (unsignedAttributes != null)
		{
			foreach (Org.BouncyCastle.Asn1.Cms.Attribute item in unsignedAttributes.GetAll(PkcsObjectIdentifiers.IdAASignatureTimeStampToken))
			{
				foreach (Asn1Encodable attrValue in item.AttrValues)
				{
					try
					{
						Org.BouncyCastle.Asn1.Cms.ContentInfo instance = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(attrValue.ToAsn1Object());
						TimeStampToken timeStampToken = new TimeStampToken(instance);
						TimeStampTokenInfo timeStampInfo = timeStampToken.TimeStampInfo;
						byte[] a = DigestUtilities.CalculateDigest(GetDigestAlgName(timeStampInfo.MessageImprintAlgOid), signerInfo.GetSignature());
						if (!Arrays.ConstantTimeAreEqual(a, timeStampInfo.GetMessageImprintDigest()))
						{
							throw new TspValidationException("Incorrect digest in message imprint");
						}
						list.Add(timeStampToken);
					}
					catch (SecurityUtilityException)
					{
						throw new TspValidationException("Unknown hash algorithm specified in timestamp");
					}
					catch (Exception)
					{
						throw new TspValidationException("Timestamp could not be parsed");
					}
				}
			}
			return list;
		}
		return list;
	}

	public static void ValidateCertificate(X509Certificate cert)
	{
		if (cert.Version != 3)
		{
			throw new ArgumentException("Certificate must have an ExtendedKeyUsage extension.");
		}
		Asn1OctetString extensionValue = cert.GetExtensionValue(X509Extensions.ExtendedKeyUsage);
		if (extensionValue == null)
		{
			throw new TspValidationException("Certificate must have an ExtendedKeyUsage extension.");
		}
		if (!cert.GetCriticalExtensionOids().Contains(X509Extensions.ExtendedKeyUsage.Id))
		{
			throw new TspValidationException("Certificate must have an ExtendedKeyUsage extension marked as critical.");
		}
		try
		{
			ExtendedKeyUsage instance = ExtendedKeyUsage.GetInstance(Asn1Object.FromByteArray(extensionValue.GetOctets()));
			if (!instance.HasKeyPurposeId(KeyPurposeID.IdKPTimeStamping) || instance.Count != 1)
			{
				throw new TspValidationException("ExtendedKeyUsage not solely time stamping.");
			}
		}
		catch (IOException)
		{
			throw new TspValidationException("cannot process ExtendedKeyUsage extension");
		}
	}

	internal static string GetDigestAlgName(string digestAlgOID)
	{
		string text = (string)digestNames[digestAlgOID];
		if (text == null)
		{
			return digestAlgOID;
		}
		return text;
	}

	internal static int GetDigestLength(string digestAlgOID)
	{
		if (!digestLengths.Contains(digestAlgOID))
		{
			throw new TspException("digest algorithm cannot be found.");
		}
		return (int)digestLengths[digestAlgOID];
	}

	internal static IDigest CreateDigestInstance(string digestAlgOID)
	{
		string digestAlgName = GetDigestAlgName(digestAlgOID);
		return DigestUtilities.GetDigest(digestAlgName);
	}

	internal static ISet GetCriticalExtensionOids(X509Extensions extensions)
	{
		if (extensions == null)
		{
			return EmptySet;
		}
		return CollectionUtilities.ReadOnly(new HashSet(extensions.GetCriticalExtensionOids()));
	}

	internal static ISet GetNonCriticalExtensionOids(X509Extensions extensions)
	{
		if (extensions == null)
		{
			return EmptySet;
		}
		return CollectionUtilities.ReadOnly(new HashSet(extensions.GetNonCriticalExtensionOids()));
	}

	internal static IList GetExtensionOids(X509Extensions extensions)
	{
		if (extensions == null)
		{
			return EmptyList;
		}
		return CollectionUtilities.ReadOnly(Platform.CreateArrayList(extensions.GetExtensionOids()));
	}
}
