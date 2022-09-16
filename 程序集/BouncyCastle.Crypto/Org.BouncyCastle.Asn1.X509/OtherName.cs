namespace Org.BouncyCastle.Asn1.X509;

public class OtherName : Asn1Encodable
{
	private readonly DerObjectIdentifier typeID;

	private readonly Asn1Encodable value;

	public virtual DerObjectIdentifier TypeID => typeID;

	public Asn1Encodable Value => value;

	public static OtherName GetInstance(object obj)
	{
		if (obj is OtherName)
		{
			return (OtherName)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new OtherName(Asn1Sequence.GetInstance(obj));
	}

	public OtherName(DerObjectIdentifier typeID, Asn1Encodable value)
	{
		this.typeID = typeID;
		this.value = value;
	}

	private OtherName(Asn1Sequence seq)
	{
		typeID = DerObjectIdentifier.GetInstance(seq[0]);
		value = Asn1TaggedObject.GetInstance(seq[1]).GetObject();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(typeID, new DerTaggedObject(explicitly: true, 0, value));
	}
}
