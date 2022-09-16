namespace Org.BouncyCastle.Asn1.X500;

public class Rdn : Asn1Encodable
{
	private readonly Asn1Set values;

	public virtual bool IsMultiValued => values.Count > 1;

	public virtual int Count => values.Count;

	private Rdn(Asn1Set values)
	{
		this.values = values;
	}

	public static Rdn GetInstance(object obj)
	{
		if (obj is Rdn)
		{
			return (Rdn)obj;
		}
		if (obj != null)
		{
			return new Rdn(Asn1Set.GetInstance(obj));
		}
		return null;
	}

	public Rdn(DerObjectIdentifier oid, Asn1Encodable value)
	{
		values = new DerSet(new DerSequence(oid, value));
	}

	public Rdn(AttributeTypeAndValue attrTAndV)
	{
		values = new DerSet(attrTAndV);
	}

	public Rdn(AttributeTypeAndValue[] aAndVs)
	{
		values = new DerSet(aAndVs);
	}

	public virtual AttributeTypeAndValue GetFirst()
	{
		if (values.Count == 0)
		{
			return null;
		}
		return AttributeTypeAndValue.GetInstance(values[0]);
	}

	public virtual AttributeTypeAndValue[] GetTypesAndValues()
	{
		AttributeTypeAndValue[] array = new AttributeTypeAndValue[values.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = AttributeTypeAndValue.GetInstance(values[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return values;
	}
}
