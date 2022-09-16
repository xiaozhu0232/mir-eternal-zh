using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class NameConstraints : Asn1Encodable
{
	private Asn1Sequence permitted;

	private Asn1Sequence excluded;

	public Asn1Sequence PermittedSubtrees => permitted;

	public Asn1Sequence ExcludedSubtrees => excluded;

	public static NameConstraints GetInstance(object obj)
	{
		if (obj == null || obj is NameConstraints)
		{
			return (NameConstraints)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new NameConstraints((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public NameConstraints(Asn1Sequence seq)
	{
		foreach (Asn1TaggedObject item in seq)
		{
			switch (item.TagNo)
			{
			case 0:
				permitted = Asn1Sequence.GetInstance(item, explicitly: false);
				break;
			case 1:
				excluded = Asn1Sequence.GetInstance(item, explicitly: false);
				break;
			}
		}
	}

	public NameConstraints(ArrayList permitted, ArrayList excluded)
		: this((IList)permitted, (IList)excluded)
	{
	}

	public NameConstraints(IList permitted, IList excluded)
	{
		if (permitted != null)
		{
			this.permitted = CreateSequence(permitted);
		}
		if (excluded != null)
		{
			this.excluded = CreateSequence(excluded);
		}
	}

	private DerSequence CreateSequence(IList subtrees)
	{
		GeneralSubtree[] array = new GeneralSubtree[subtrees.Count];
		for (int i = 0; i < subtrees.Count; i++)
		{
			array[i] = (GeneralSubtree)subtrees[i];
		}
		return new DerSequence(array);
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, permitted);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, excluded);
		return new DerSequence(asn1EncodableVector);
	}
}
