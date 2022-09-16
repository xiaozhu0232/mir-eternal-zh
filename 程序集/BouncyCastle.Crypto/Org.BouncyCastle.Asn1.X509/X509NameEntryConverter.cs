using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.X509;

public abstract class X509NameEntryConverter
{
	protected Asn1Object ConvertHexEncoded(string hexString, int offset)
	{
		return Asn1Object.FromByteArray(Hex.DecodeStrict(hexString, offset, hexString.Length - offset));
	}

	protected bool CanBePrintable(string str)
	{
		return DerPrintableString.IsPrintableString(str);
	}

	public abstract Asn1Object GetConvertedValue(DerObjectIdentifier oid, string value);
}
