using System;

namespace Org.BouncyCastle.Asn1.X509;

public class X509Extension
{
	internal bool critical;

	internal Asn1OctetString value;

	public bool IsCritical => critical;

	public Asn1OctetString Value => value;

	public X509Extension(DerBoolean critical, Asn1OctetString value)
	{
		if (critical == null)
		{
			throw new ArgumentNullException("critical");
		}
		this.critical = critical.IsTrue;
		this.value = value;
	}

	public X509Extension(bool critical, Asn1OctetString value)
	{
		this.critical = critical;
		this.value = value;
	}

	public Asn1Encodable GetParsedValue()
	{
		return ConvertValueToObject(this);
	}

	public override int GetHashCode()
	{
		int hashCode = Value.GetHashCode();
		if (!IsCritical)
		{
			return ~hashCode;
		}
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is X509Extension x509Extension))
		{
			return false;
		}
		if (Value.Equals(x509Extension.Value))
		{
			return IsCritical == x509Extension.IsCritical;
		}
		return false;
	}

	public static Asn1Object ConvertValueToObject(X509Extension ext)
	{
		try
		{
			return Asn1Object.FromByteArray(ext.Value.GetOctets());
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("can't convert extension", innerException);
		}
	}
}
