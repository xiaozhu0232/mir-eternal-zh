using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class OtherHashAlgAndValue : Asn1Encodable
{
	private readonly AlgorithmIdentifier hashAlgorithm;

	private readonly Asn1OctetString hashValue;

	public AlgorithmIdentifier HashAlgorithm => hashAlgorithm;

	public static OtherHashAlgAndValue GetInstance(object obj)
	{
		if (obj == null || obj is OtherHashAlgAndValue)
		{
			return (OtherHashAlgAndValue)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OtherHashAlgAndValue((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'OtherHashAlgAndValue' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private OtherHashAlgAndValue(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[0].ToAsn1Object());
		hashValue = (Asn1OctetString)seq[1].ToAsn1Object();
	}

	public OtherHashAlgAndValue(AlgorithmIdentifier hashAlgorithm, byte[] hashValue)
	{
		if (hashAlgorithm == null)
		{
			throw new ArgumentNullException("hashAlgorithm");
		}
		if (hashValue == null)
		{
			throw new ArgumentNullException("hashValue");
		}
		this.hashAlgorithm = hashAlgorithm;
		this.hashValue = new DerOctetString(hashValue);
	}

	public OtherHashAlgAndValue(AlgorithmIdentifier hashAlgorithm, Asn1OctetString hashValue)
	{
		if (hashAlgorithm == null)
		{
			throw new ArgumentNullException("hashAlgorithm");
		}
		if (hashValue == null)
		{
			throw new ArgumentNullException("hashValue");
		}
		this.hashAlgorithm = hashAlgorithm;
		this.hashValue = hashValue;
	}

	public byte[] GetHashValue()
	{
		return hashValue.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(hashAlgorithm, hashValue);
	}
}
