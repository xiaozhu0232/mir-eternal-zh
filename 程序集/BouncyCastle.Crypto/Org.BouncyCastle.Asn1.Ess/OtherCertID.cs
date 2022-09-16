using System;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ess;

[Obsolete("Use version in Asn1.Esf instead")]
public class OtherCertID : Asn1Encodable
{
	private Asn1Encodable otherCertHash;

	private IssuerSerial issuerSerial;

	public AlgorithmIdentifier AlgorithmHash
	{
		get
		{
			if (otherCertHash.ToAsn1Object() is Asn1OctetString)
			{
				return new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1);
			}
			return DigestInfo.GetInstance(otherCertHash).AlgorithmID;
		}
	}

	public IssuerSerial IssuerSerial => issuerSerial;

	public static OtherCertID GetInstance(object o)
	{
		if (o == null || o is OtherCertID)
		{
			return (OtherCertID)o;
		}
		if (o is Asn1Sequence)
		{
			return new OtherCertID((Asn1Sequence)o);
		}
		throw new ArgumentException("unknown object in 'OtherCertID' factory : " + Platform.GetTypeName(o) + ".");
	}

	public OtherCertID(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		if (seq[0].ToAsn1Object() is Asn1OctetString)
		{
			otherCertHash = Asn1OctetString.GetInstance(seq[0]);
		}
		else
		{
			otherCertHash = DigestInfo.GetInstance(seq[0]);
		}
		if (seq.Count > 1)
		{
			issuerSerial = IssuerSerial.GetInstance(Asn1Sequence.GetInstance(seq[1]));
		}
	}

	public OtherCertID(AlgorithmIdentifier algId, byte[] digest)
	{
		otherCertHash = new DigestInfo(algId, digest);
	}

	public OtherCertID(AlgorithmIdentifier algId, byte[] digest, IssuerSerial issuerSerial)
	{
		otherCertHash = new DigestInfo(algId, digest);
		this.issuerSerial = issuerSerial;
	}

	public byte[] GetCertHash()
	{
		if (otherCertHash.ToAsn1Object() is Asn1OctetString)
		{
			return ((Asn1OctetString)otherCertHash.ToAsn1Object()).GetOctets();
		}
		return DigestInfo.GetInstance(otherCertHash).GetDigest();
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(otherCertHash);
		asn1EncodableVector.AddOptional(issuerSerial);
		return new DerSequence(asn1EncodableVector);
	}
}
