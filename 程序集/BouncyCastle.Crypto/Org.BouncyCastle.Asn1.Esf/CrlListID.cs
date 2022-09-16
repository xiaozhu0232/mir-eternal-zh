using System;
using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf;

public class CrlListID : Asn1Encodable
{
	private readonly Asn1Sequence crls;

	public static CrlListID GetInstance(object obj)
	{
		if (obj == null || obj is CrlListID)
		{
			return (CrlListID)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CrlListID((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'CrlListID' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CrlListID(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count != 1)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		crls = (Asn1Sequence)seq[0].ToAsn1Object();
		foreach (Asn1Encodable crl in crls)
		{
			CrlValidatedID.GetInstance(crl.ToAsn1Object());
		}
	}

	public CrlListID(params CrlValidatedID[] crls)
	{
		if (crls == null)
		{
			throw new ArgumentNullException("crls");
		}
		this.crls = new DerSequence(crls);
	}

	public CrlListID(IEnumerable crls)
	{
		if (crls == null)
		{
			throw new ArgumentNullException("crls");
		}
		if (!CollectionUtilities.CheckElementsAreOfType(crls, typeof(CrlValidatedID)))
		{
			throw new ArgumentException("Must contain only 'CrlValidatedID' objects", "crls");
		}
		this.crls = new DerSequence(Asn1EncodableVector.FromEnumerable(crls));
	}

	public CrlValidatedID[] GetCrls()
	{
		CrlValidatedID[] array = new CrlValidatedID[crls.Count];
		for (int i = 0; i < crls.Count; i++)
		{
			array[i] = CrlValidatedID.GetInstance(crls[i].ToAsn1Object());
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(crls);
	}
}
