using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class ServiceLocator : Asn1Encodable
{
	private readonly X509Name issuer;

	private readonly Asn1Object locator;

	public X509Name Issuer => issuer;

	public Asn1Object Locator => locator;

	public static ServiceLocator GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static ServiceLocator GetInstance(object obj)
	{
		if (obj == null || obj is ServiceLocator)
		{
			return (ServiceLocator)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new ServiceLocator((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public ServiceLocator(X509Name issuer)
		: this(issuer, null)
	{
	}

	public ServiceLocator(X509Name issuer, Asn1Object locator)
	{
		if (issuer == null)
		{
			throw new ArgumentNullException("issuer");
		}
		this.issuer = issuer;
		this.locator = locator;
	}

	private ServiceLocator(Asn1Sequence seq)
	{
		issuer = X509Name.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			locator = seq[1].ToAsn1Object();
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(issuer);
		asn1EncodableVector.AddOptional(locator);
		return new DerSequence(asn1EncodableVector);
	}
}
