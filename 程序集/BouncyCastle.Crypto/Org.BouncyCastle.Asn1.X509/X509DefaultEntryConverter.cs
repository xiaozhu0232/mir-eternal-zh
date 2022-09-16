using System;
using System.IO;

namespace Org.BouncyCastle.Asn1.X509;

public class X509DefaultEntryConverter : X509NameEntryConverter
{
	public override Asn1Object GetConvertedValue(DerObjectIdentifier oid, string value)
	{
		if (value.Length != 0 && value[0] == '#')
		{
			try
			{
				return ConvertHexEncoded(value, 1);
			}
			catch (IOException)
			{
				throw new Exception("can't recode value for oid " + oid.Id);
			}
		}
		if (value.Length != 0 && value[0] == '\\')
		{
			value = value.Substring(1);
		}
		if (oid.Equals(X509Name.EmailAddress) || oid.Equals(X509Name.DC))
		{
			return new DerIA5String(value);
		}
		if (oid.Equals(X509Name.DateOfBirth))
		{
			return new DerGeneralizedTime(value);
		}
		if (oid.Equals(X509Name.C) || oid.Equals(X509Name.SerialNumber) || oid.Equals(X509Name.DnQualifier) || oid.Equals(X509Name.TelephoneNumber))
		{
			return new DerPrintableString(value);
		}
		return new DerUtf8String(value);
	}
}
