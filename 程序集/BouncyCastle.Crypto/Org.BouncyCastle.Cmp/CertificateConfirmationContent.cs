using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cmp;

public class CertificateConfirmationContent
{
	private readonly DefaultDigestAlgorithmIdentifierFinder digestAlgFinder;

	private readonly CertConfirmContent content;

	public CertificateConfirmationContent(CertConfirmContent content)
	{
		this.content = content;
	}

	public CertificateConfirmationContent(CertConfirmContent content, DefaultDigestAlgorithmIdentifierFinder digestAlgFinder)
	{
		this.content = content;
		this.digestAlgFinder = digestAlgFinder;
	}

	public CertConfirmContent ToAsn1Structure()
	{
		return content;
	}

	public CertificateStatus[] GetStatusMessages()
	{
		CertStatus[] array = content.ToCertStatusArray();
		CertificateStatus[] array2 = new CertificateStatus[array.Length];
		for (int i = 0; i != array2.Length; i++)
		{
			array2[i] = new CertificateStatus(digestAlgFinder, array[i]);
		}
		return array2;
	}
}
public class CertificateConfirmationContentBuilder
{
	private static readonly DefaultSignatureAlgorithmIdentifierFinder sigAlgFinder = new DefaultSignatureAlgorithmIdentifierFinder();

	private readonly DefaultDigestAlgorithmIdentifierFinder digestAlgFinder;

	private readonly IList acceptedCerts = Platform.CreateArrayList();

	private readonly IList acceptedReqIds = Platform.CreateArrayList();

	public CertificateConfirmationContentBuilder()
		: this(new DefaultDigestAlgorithmIdentifierFinder())
	{
	}

	public CertificateConfirmationContentBuilder(DefaultDigestAlgorithmIdentifierFinder digestAlgFinder)
	{
		this.digestAlgFinder = digestAlgFinder;
	}

	public CertificateConfirmationContentBuilder AddAcceptedCertificate(X509Certificate certHolder, BigInteger certReqId)
	{
		acceptedCerts.Add(certHolder);
		acceptedReqIds.Add(certReqId);
		return this;
	}

	public CertificateConfirmationContent Build()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		for (int i = 0; i != acceptedCerts.Count; i++)
		{
			X509Certificate x509Certificate = (X509Certificate)acceptedCerts[i];
			BigInteger certReqId = (BigInteger)acceptedReqIds[i];
			AlgorithmIdentifier sigAlgId = sigAlgFinder.Find(x509Certificate.SigAlgName);
			AlgorithmIdentifier algorithmIdentifier = digestAlgFinder.find(sigAlgId);
			if (algorithmIdentifier == null)
			{
				throw new CmpException("cannot find algorithm for digest from signature");
			}
			byte[] certHash = DigestUtilities.CalculateDigest(algorithmIdentifier.Algorithm, x509Certificate.GetEncoded());
			asn1EncodableVector.Add(new CertStatus(certHash, certReqId));
		}
		return new CertificateConfirmationContent(CertConfirmContent.GetInstance(new DerSequence(asn1EncodableVector)), digestAlgFinder);
	}
}
