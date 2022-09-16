using System;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.IsisMtt.Ocsp;

public class RequestedCertificate : Asn1Encodable, IAsn1Choice
{
	public enum Choice
	{
		Certificate = -1,
		PublicKeyCertificate,
		AttributeCertificate
	}

	private readonly X509CertificateStructure cert;

	private readonly byte[] publicKeyCert;

	private readonly byte[] attributeCert;

	public Choice Type
	{
		get
		{
			if (cert != null)
			{
				return Choice.Certificate;
			}
			if (publicKeyCert != null)
			{
				return Choice.PublicKeyCertificate;
			}
			return Choice.AttributeCertificate;
		}
	}

	public static RequestedCertificate GetInstance(object obj)
	{
		if (obj == null || obj is RequestedCertificate)
		{
			return (RequestedCertificate)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RequestedCertificate(X509CertificateStructure.GetInstance(obj));
		}
		if (obj is Asn1TaggedObject)
		{
			return new RequestedCertificate((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public static RequestedCertificate GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		if (!isExplicit)
		{
			throw new ArgumentException("choice item must be explicitly tagged");
		}
		return GetInstance(obj.GetObject());
	}

	private RequestedCertificate(Asn1TaggedObject tagged)
	{
		switch (tagged.TagNo)
		{
		case 1:
			attributeCert = Asn1OctetString.GetInstance(tagged, isExplicit: true).GetOctets();
			break;
		case 0:
			publicKeyCert = Asn1OctetString.GetInstance(tagged, isExplicit: true).GetOctets();
			break;
		default:
			throw new ArgumentException("unknown tag number: " + tagged.TagNo);
		}
	}

	public RequestedCertificate(X509CertificateStructure certificate)
	{
		cert = certificate;
	}

	public RequestedCertificate(Choice type, byte[] certificateOctets)
		: this(new DerTaggedObject((int)type, new DerOctetString(certificateOctets)))
	{
	}

	public byte[] GetCertificateBytes()
	{
		if (cert != null)
		{
			try
			{
				return cert.GetEncoded();
			}
			catch (IOException ex)
			{
				throw new InvalidOperationException("can't decode certificate: " + ex);
			}
		}
		if (publicKeyCert != null)
		{
			return publicKeyCert;
		}
		return attributeCert;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (publicKeyCert != null)
		{
			return new DerTaggedObject(0, new DerOctetString(publicKeyCert));
		}
		if (attributeCert != null)
		{
			return new DerTaggedObject(1, new DerOctetString(attributeCert));
		}
		return cert.ToAsn1Object();
	}
}
