using System;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ess;

public class EssCertIDv2 : Asn1Encodable
{
	private readonly AlgorithmIdentifier hashAlgorithm;

	private readonly byte[] certHash;

	private readonly IssuerSerial issuerSerial;

	private static readonly AlgorithmIdentifier DefaultAlgID = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha256);

	public AlgorithmIdentifier HashAlgorithm => hashAlgorithm;

	public IssuerSerial IssuerSerial => issuerSerial;

	public static EssCertIDv2 GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is EssCertIDv2 result)
		{
			return result;
		}
		return new EssCertIDv2(Asn1Sequence.GetInstance(obj));
	}

	private EssCertIDv2(Asn1Sequence seq)
	{
		if (seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		int num = 0;
		if (seq[0] is Asn1OctetString)
		{
			hashAlgorithm = DefaultAlgID;
		}
		else
		{
			hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[num++].ToAsn1Object());
		}
		certHash = Asn1OctetString.GetInstance(seq[num++].ToAsn1Object()).GetOctets();
		if (seq.Count > num)
		{
			issuerSerial = IssuerSerial.GetInstance(Asn1Sequence.GetInstance(seq[num].ToAsn1Object()));
		}
	}

	public EssCertIDv2(byte[] certHash)
		: this(null, certHash, null)
	{
	}

	public EssCertIDv2(AlgorithmIdentifier algId, byte[] certHash)
		: this(algId, certHash, null)
	{
	}

	public EssCertIDv2(byte[] certHash, IssuerSerial issuerSerial)
		: this(null, certHash, issuerSerial)
	{
	}

	public EssCertIDv2(AlgorithmIdentifier algId, byte[] certHash, IssuerSerial issuerSerial)
	{
		if (algId == null)
		{
			hashAlgorithm = DefaultAlgID;
		}
		else
		{
			hashAlgorithm = algId;
		}
		this.certHash = certHash;
		this.issuerSerial = issuerSerial;
	}

	public byte[] GetCertHash()
	{
		return Arrays.Clone(certHash);
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (!hashAlgorithm.Equals(DefaultAlgID))
		{
			asn1EncodableVector.Add(hashAlgorithm);
		}
		asn1EncodableVector.Add(new DerOctetString(certHash).ToAsn1Object());
		asn1EncodableVector.AddOptional(issuerSerial);
		return new DerSequence(asn1EncodableVector);
	}
}
