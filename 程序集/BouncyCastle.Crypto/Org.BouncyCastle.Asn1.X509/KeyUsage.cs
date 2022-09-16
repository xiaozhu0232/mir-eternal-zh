namespace Org.BouncyCastle.Asn1.X509;

public class KeyUsage : DerBitString
{
	public const int DigitalSignature = 128;

	public const int NonRepudiation = 64;

	public const int KeyEncipherment = 32;

	public const int DataEncipherment = 16;

	public const int KeyAgreement = 8;

	public const int KeyCertSign = 4;

	public const int CrlSign = 2;

	public const int EncipherOnly = 1;

	public const int DecipherOnly = 32768;

	public new static KeyUsage GetInstance(object obj)
	{
		if (obj is KeyUsage)
		{
			return (KeyUsage)obj;
		}
		if (obj is X509Extension)
		{
			return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		if (obj == null)
		{
			return null;
		}
		return new KeyUsage(DerBitString.GetInstance(obj));
	}

	public static KeyUsage FromExtensions(X509Extensions extensions)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.KeyUsage));
	}

	public KeyUsage(int usage)
		: base(usage)
	{
	}

	private KeyUsage(DerBitString usage)
		: base(usage.GetBytes(), usage.PadBits)
	{
	}

	public override string ToString()
	{
		byte[] bytes = GetBytes();
		if (bytes.Length == 1)
		{
			return "KeyUsage: 0x" + (bytes[0] & 0xFF).ToString("X");
		}
		return "KeyUsage: 0x" + (((bytes[1] & 0xFF) << 8) | (bytes[0] & 0xFF)).ToString("X");
	}
}
