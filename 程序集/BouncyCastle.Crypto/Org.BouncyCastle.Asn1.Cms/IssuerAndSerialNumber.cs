using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Cms;

public class IssuerAndSerialNumber : Asn1Encodable
{
	private X509Name name;

	private DerInteger serialNumber;

	public X509Name Name => name;

	public DerInteger SerialNumber => serialNumber;

	public static IssuerAndSerialNumber GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is IssuerAndSerialNumber result)
		{
			return result;
		}
		return new IssuerAndSerialNumber(Asn1Sequence.GetInstance(obj));
	}

	[Obsolete("Use GetInstance() instead")]
	public IssuerAndSerialNumber(Asn1Sequence seq)
	{
		name = X509Name.GetInstance(seq[0]);
		serialNumber = (DerInteger)seq[1];
	}

	public IssuerAndSerialNumber(X509Name name, BigInteger serialNumber)
	{
		this.name = name;
		this.serialNumber = new DerInteger(serialNumber);
	}

	public IssuerAndSerialNumber(X509Name name, DerInteger serialNumber)
	{
		this.name = name;
		this.serialNumber = serialNumber;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(name, serialNumber);
	}
}
