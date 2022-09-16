using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ess;

public class EssCertID : Asn1Encodable
{
	private Asn1OctetString certHash;

	private IssuerSerial issuerSerial;

	public IssuerSerial IssuerSerial => issuerSerial;

	public static EssCertID GetInstance(object o)
	{
		if (o == null || o is EssCertID)
		{
			return (EssCertID)o;
		}
		if (o is Asn1Sequence)
		{
			return new EssCertID((Asn1Sequence)o);
		}
		throw new ArgumentException("unknown object in 'EssCertID' factory : " + Platform.GetTypeName(o) + ".");
	}

	public EssCertID(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		certHash = Asn1OctetString.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			issuerSerial = IssuerSerial.GetInstance(seq[1]);
		}
	}

	public EssCertID(byte[] hash)
	{
		certHash = new DerOctetString(hash);
	}

	public EssCertID(byte[] hash, IssuerSerial issuerSerial)
	{
		certHash = new DerOctetString(hash);
		this.issuerSerial = issuerSerial;
	}

	public byte[] GetCertHash()
	{
		return certHash.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(certHash);
		asn1EncodableVector.AddOptional(issuerSerial);
		return new DerSequence(asn1EncodableVector);
	}
}
