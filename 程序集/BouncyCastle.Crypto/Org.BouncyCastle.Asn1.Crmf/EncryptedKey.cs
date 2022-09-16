using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Asn1.Crmf;

public class EncryptedKey : Asn1Encodable, IAsn1Choice
{
	private readonly EnvelopedData envelopedData;

	private readonly EncryptedValue encryptedValue;

	public virtual bool IsEncryptedValue => encryptedValue != null;

	public virtual Asn1Encodable Value
	{
		get
		{
			if (encryptedValue != null)
			{
				return encryptedValue;
			}
			return envelopedData;
		}
	}

	public static EncryptedKey GetInstance(object o)
	{
		if (o is EncryptedKey)
		{
			return (EncryptedKey)o;
		}
		if (o is Asn1TaggedObject)
		{
			return new EncryptedKey(EnvelopedData.GetInstance((Asn1TaggedObject)o, explicitly: false));
		}
		if (o is EncryptedValue)
		{
			return new EncryptedKey((EncryptedValue)o);
		}
		return new EncryptedKey(EncryptedValue.GetInstance(o));
	}

	public EncryptedKey(EnvelopedData envelopedData)
	{
		this.envelopedData = envelopedData;
	}

	public EncryptedKey(EncryptedValue encryptedValue)
	{
		this.encryptedValue = encryptedValue;
	}

	public override Asn1Object ToAsn1Object()
	{
		if (encryptedValue != null)
		{
			return encryptedValue.ToAsn1Object();
		}
		return new DerTaggedObject(explicitly: false, 0, envelopedData);
	}
}
