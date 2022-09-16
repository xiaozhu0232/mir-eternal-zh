using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Ocsp;

public class CertificateID
{
	public const string HashSha1 = "1.3.14.3.2.26";

	private readonly CertID id;

	public string HashAlgOid => id.HashAlgorithm.Algorithm.Id;

	public BigInteger SerialNumber => id.SerialNumber.Value;

	public CertificateID(CertID id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		this.id = id;
	}

	public CertificateID(string hashAlgorithm, X509Certificate issuerCert, BigInteger serialNumber)
	{
		AlgorithmIdentifier hashAlg = new AlgorithmIdentifier(new DerObjectIdentifier(hashAlgorithm), DerNull.Instance);
		id = CreateCertID(hashAlg, issuerCert, new DerInteger(serialNumber));
	}

	public byte[] GetIssuerNameHash()
	{
		return id.IssuerNameHash.GetOctets();
	}

	public byte[] GetIssuerKeyHash()
	{
		return id.IssuerKeyHash.GetOctets();
	}

	public bool MatchesIssuer(X509Certificate issuerCert)
	{
		return CreateCertID(id.HashAlgorithm, issuerCert, id.SerialNumber).Equals(id);
	}

	public CertID ToAsn1Object()
	{
		return id;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is CertificateID certificateID))
		{
			return false;
		}
		return id.ToAsn1Object().Equals(certificateID.id.ToAsn1Object());
	}

	public override int GetHashCode()
	{
		return id.ToAsn1Object().GetHashCode();
	}

	public static CertificateID DeriveCertificateID(CertificateID original, BigInteger newSerialNumber)
	{
		return new CertificateID(new CertID(original.id.HashAlgorithm, original.id.IssuerNameHash, original.id.IssuerKeyHash, new DerInteger(newSerialNumber)));
	}

	private static CertID CreateCertID(AlgorithmIdentifier hashAlg, X509Certificate issuerCert, DerInteger serialNumber)
	{
		try
		{
			string algorithm = hashAlg.Algorithm.Id;
			X509Name subjectX509Principal = PrincipalUtilities.GetSubjectX509Principal(issuerCert);
			byte[] str = DigestUtilities.CalculateDigest(algorithm, subjectX509Principal.GetEncoded());
			AsymmetricKeyParameter publicKey = issuerCert.GetPublicKey();
			SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
			byte[] str2 = DigestUtilities.CalculateDigest(algorithm, subjectPublicKeyInfo.PublicKeyData.GetBytes());
			return new CertID(hashAlg, new DerOctetString(str), new DerOctetString(str2), serialNumber);
		}
		catch (Exception ex)
		{
			throw new OcspException("problem creating ID: " + ex, ex);
		}
	}
}
