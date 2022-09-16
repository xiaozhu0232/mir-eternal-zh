using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509;

public class GeneralSubtree : Asn1Encodable
{
	private readonly GeneralName baseName;

	private readonly DerInteger minimum;

	private readonly DerInteger maximum;

	public GeneralName Base => baseName;

	public BigInteger Minimum
	{
		get
		{
			if (minimum != null)
			{
				return minimum.Value;
			}
			return BigInteger.Zero;
		}
	}

	public BigInteger Maximum
	{
		get
		{
			if (maximum != null)
			{
				return maximum.Value;
			}
			return null;
		}
	}

	private GeneralSubtree(Asn1Sequence seq)
	{
		baseName = GeneralName.GetInstance(seq[0]);
		switch (seq.Count)
		{
		case 2:
		{
			Asn1TaggedObject instance3 = Asn1TaggedObject.GetInstance(seq[1]);
			switch (instance3.TagNo)
			{
			case 0:
				minimum = DerInteger.GetInstance(instance3, isExplicit: false);
				break;
			case 1:
				maximum = DerInteger.GetInstance(instance3, isExplicit: false);
				break;
			default:
				throw new ArgumentException("Bad tag number: " + instance3.TagNo);
			}
			break;
		}
		case 3:
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[1]);
			if (instance.TagNo != 0)
			{
				throw new ArgumentException("Bad tag number for 'minimum': " + instance.TagNo);
			}
			minimum = DerInteger.GetInstance(instance, isExplicit: false);
			Asn1TaggedObject instance2 = Asn1TaggedObject.GetInstance(seq[2]);
			if (instance2.TagNo != 1)
			{
				throw new ArgumentException("Bad tag number for 'maximum': " + instance2.TagNo);
			}
			maximum = DerInteger.GetInstance(instance2, isExplicit: false);
			break;
		}
		default:
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		case 1:
			break;
		}
	}

	public GeneralSubtree(GeneralName baseName, BigInteger minimum, BigInteger maximum)
	{
		this.baseName = baseName;
		if (minimum != null)
		{
			this.minimum = new DerInteger(minimum);
		}
		if (maximum != null)
		{
			this.maximum = new DerInteger(maximum);
		}
	}

	public GeneralSubtree(GeneralName baseName)
		: this(baseName, null, null)
	{
	}

	public static GeneralSubtree GetInstance(Asn1TaggedObject o, bool isExplicit)
	{
		return new GeneralSubtree(Asn1Sequence.GetInstance(o, isExplicit));
	}

	public static GeneralSubtree GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is GeneralSubtree)
		{
			return (GeneralSubtree)obj;
		}
		return new GeneralSubtree(Asn1Sequence.GetInstance(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(baseName);
		if (minimum != null && minimum.Value.SignValue != 0)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 0, minimum));
		}
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, maximum);
		return new DerSequence(asn1EncodableVector);
	}
}
