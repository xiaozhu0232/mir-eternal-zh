using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.Ocsp;

public class CertHash : Asn1Encodable
{
	private readonly AlgorithmIdentifier hashAlgorithm;

	private readonly byte[] certificateHash;

	public AlgorithmIdentifier HashAlgorithm => hashAlgorithm;

	public byte[] CertificateHash => (byte[])certificateHash.Clone();

	public static CertHash GetInstance(object obj)
	{
		if (obj == null || obj is CertHash)
		{
			return (CertHash)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertHash((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CertHash(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[0]);
		certificateHash = Asn1OctetString.GetInstance(seq[1]).GetOctets();
	}

	public CertHash(AlgorithmIdentifier hashAlgorithm, byte[] certificateHash)
	{
		if (hashAlgorithm == null)
		{
			throw new ArgumentNullException("hashAlgorithm");
		}
		if (certificateHash == null)
		{
			throw new ArgumentNullException("certificateHash");
		}
		this.hashAlgorithm = hashAlgorithm;
		this.certificateHash = (byte[])certificateHash.Clone();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(hashAlgorithm, new DerOctetString(certificateHash));
	}
}
