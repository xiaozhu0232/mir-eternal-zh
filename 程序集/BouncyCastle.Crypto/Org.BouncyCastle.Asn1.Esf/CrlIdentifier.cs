using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class CrlIdentifier : Asn1Encodable
{
	private readonly X509Name crlIssuer;

	private readonly DerUtcTime crlIssuedTime;

	private readonly DerInteger crlNumber;

	public X509Name CrlIssuer => crlIssuer;

	public DateTime CrlIssuedTime => crlIssuedTime.ToAdjustedDateTime();

	public BigInteger CrlNumber
	{
		get
		{
			if (crlNumber != null)
			{
				return crlNumber.Value;
			}
			return null;
		}
	}

	public static CrlIdentifier GetInstance(object obj)
	{
		if (obj == null || obj is CrlIdentifier)
		{
			return (CrlIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CrlIdentifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'CrlIdentifier' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CrlIdentifier(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count < 2 || seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		crlIssuer = X509Name.GetInstance(seq[0]);
		crlIssuedTime = DerUtcTime.GetInstance(seq[1]);
		if (seq.Count > 2)
		{
			crlNumber = DerInteger.GetInstance(seq[2]);
		}
	}

	public CrlIdentifier(X509Name crlIssuer, DateTime crlIssuedTime)
		: this(crlIssuer, crlIssuedTime, null)
	{
	}

	public CrlIdentifier(X509Name crlIssuer, DateTime crlIssuedTime, BigInteger crlNumber)
	{
		if (crlIssuer == null)
		{
			throw new ArgumentNullException("crlIssuer");
		}
		this.crlIssuer = crlIssuer;
		this.crlIssuedTime = new DerUtcTime(crlIssuedTime);
		if (crlNumber != null)
		{
			this.crlNumber = new DerInteger(crlNumber);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(crlIssuer.ToAsn1Object(), crlIssuedTime);
		asn1EncodableVector.AddOptional(crlNumber);
		return new DerSequence(asn1EncodableVector);
	}
}
