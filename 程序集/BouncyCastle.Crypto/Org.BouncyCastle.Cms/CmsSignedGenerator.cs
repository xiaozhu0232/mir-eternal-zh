using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms;

public class CmsSignedGenerator
{
	public static readonly string Data = CmsObjectIdentifiers.Data.Id;

	public static readonly string DigestSha1 = OiwObjectIdentifiers.IdSha1.Id;

	public static readonly string DigestSha224 = NistObjectIdentifiers.IdSha224.Id;

	public static readonly string DigestSha256 = NistObjectIdentifiers.IdSha256.Id;

	public static readonly string DigestSha384 = NistObjectIdentifiers.IdSha384.Id;

	public static readonly string DigestSha512 = NistObjectIdentifiers.IdSha512.Id;

	public static readonly string DigestMD5 = PkcsObjectIdentifiers.MD5.Id;

	public static readonly string DigestGost3411 = CryptoProObjectIdentifiers.GostR3411.Id;

	public static readonly string DigestRipeMD128 = TeleTrusTObjectIdentifiers.RipeMD128.Id;

	public static readonly string DigestRipeMD160 = TeleTrusTObjectIdentifiers.RipeMD160.Id;

	public static readonly string DigestRipeMD256 = TeleTrusTObjectIdentifiers.RipeMD256.Id;

	public static readonly string EncryptionRsa = PkcsObjectIdentifiers.RsaEncryption.Id;

	public static readonly string EncryptionDsa = X9ObjectIdentifiers.IdDsaWithSha1.Id;

	public static readonly string EncryptionECDsa = X9ObjectIdentifiers.ECDsaWithSha1.Id;

	public static readonly string EncryptionRsaPss = PkcsObjectIdentifiers.IdRsassaPss.Id;

	public static readonly string EncryptionGost3410 = CryptoProObjectIdentifiers.GostR3410x94.Id;

	public static readonly string EncryptionECGost3410 = CryptoProObjectIdentifiers.GostR3410x2001.Id;

	internal IList _certs = Platform.CreateArrayList();

	internal IList _crls = Platform.CreateArrayList();

	internal IList _signers = Platform.CreateArrayList();

	internal IDictionary _digests = Platform.CreateHashtable();

	internal bool _useDerForCerts = false;

	internal bool _useDerForCrls = false;

	protected readonly SecureRandom rand;

	public bool UseDerForCerts
	{
		get
		{
			return _useDerForCerts;
		}
		set
		{
			_useDerForCerts = value;
		}
	}

	public bool UseDerForCrls
	{
		get
		{
			return _useDerForCrls;
		}
		set
		{
			_useDerForCrls = value;
		}
	}

	protected CmsSignedGenerator()
		: this(new SecureRandom())
	{
	}

	protected CmsSignedGenerator(SecureRandom rand)
	{
		this.rand = rand;
	}

	protected internal virtual IDictionary GetBaseParameters(DerObjectIdentifier contentType, AlgorithmIdentifier digAlgId, byte[] hash)
	{
		IDictionary dictionary = Platform.CreateHashtable();
		if (contentType != null)
		{
			dictionary[CmsAttributeTableParameter.ContentType] = contentType;
		}
		dictionary[CmsAttributeTableParameter.DigestAlgorithmIdentifier] = digAlgId;
		dictionary[CmsAttributeTableParameter.Digest] = hash.Clone();
		return dictionary;
	}

	protected internal virtual Asn1Set GetAttributeSet(Org.BouncyCastle.Asn1.Cms.AttributeTable attr)
	{
		if (attr != null)
		{
			return new DerSet(attr.ToAsn1EncodableVector());
		}
		return null;
	}

	public void AddCertificates(IX509Store certStore)
	{
		CollectionUtilities.AddRange(_certs, CmsUtilities.GetCertificatesFromStore(certStore));
	}

	public void AddCrls(IX509Store crlStore)
	{
		CollectionUtilities.AddRange(_crls, CmsUtilities.GetCrlsFromStore(crlStore));
	}

	public void AddAttributeCertificates(IX509Store store)
	{
		try
		{
			foreach (IX509AttributeCertificate match in store.GetMatches(null))
			{
				_certs.Add(new DerTaggedObject(explicitly: false, 2, AttributeCertificate.GetInstance(Asn1Object.FromByteArray(match.GetEncoded()))));
			}
		}
		catch (Exception e)
		{
			throw new CmsException("error processing attribute certs", e);
		}
	}

	public void AddSigners(SignerInformationStore signerStore)
	{
		foreach (SignerInformation signer in signerStore.GetSigners())
		{
			_signers.Add(signer);
			AddSignerCallback(signer);
		}
	}

	public IDictionary GetGeneratedDigests()
	{
		return Platform.CreateHashtable(_digests);
	}

	internal virtual void AddSignerCallback(SignerInformation si)
	{
	}

	internal static SignerIdentifier GetSignerIdentifier(X509Certificate cert)
	{
		return new SignerIdentifier(CmsUtilities.GetIssuerAndSerialNumber(cert));
	}

	internal static SignerIdentifier GetSignerIdentifier(byte[] subjectKeyIdentifier)
	{
		return new SignerIdentifier(new DerOctetString(subjectKeyIdentifier));
	}
}
