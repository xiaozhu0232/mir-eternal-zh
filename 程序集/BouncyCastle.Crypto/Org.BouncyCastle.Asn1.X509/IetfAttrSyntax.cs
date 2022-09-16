using System;

namespace Org.BouncyCastle.Asn1.X509;

public class IetfAttrSyntax : Asn1Encodable
{
	public const int ValueOctets = 1;

	public const int ValueOid = 2;

	public const int ValueUtf8 = 3;

	internal readonly GeneralNames policyAuthority;

	internal readonly Asn1EncodableVector values = new Asn1EncodableVector();

	internal int valueChoice = -1;

	public GeneralNames PolicyAuthority => policyAuthority;

	public int ValueType => valueChoice;

	public IetfAttrSyntax(Asn1Sequence seq)
	{
		int num = 0;
		if (seq[0] is Asn1TaggedObject)
		{
			policyAuthority = GeneralNames.GetInstance((Asn1TaggedObject)seq[0], explicitly: false);
			num++;
		}
		else if (seq.Count == 2)
		{
			policyAuthority = GeneralNames.GetInstance(seq[0]);
			num++;
		}
		if (!(seq[num] is Asn1Sequence))
		{
			throw new ArgumentException("Non-IetfAttrSyntax encoding");
		}
		seq = (Asn1Sequence)seq[num];
		foreach (Asn1Object item in seq)
		{
			int num2;
			if (item is DerObjectIdentifier)
			{
				num2 = 2;
			}
			else if (item is DerUtf8String)
			{
				num2 = 3;
			}
			else
			{
				if (!(item is DerOctetString))
				{
					throw new ArgumentException("Bad value type encoding IetfAttrSyntax");
				}
				num2 = 1;
			}
			if (valueChoice < 0)
			{
				valueChoice = num2;
			}
			if (num2 != valueChoice)
			{
				throw new ArgumentException("Mix of value types in IetfAttrSyntax");
			}
			values.Add(item);
		}
	}

	public object[] GetValues()
	{
		if (ValueType == 1)
		{
			Asn1OctetString[] array = new Asn1OctetString[values.Count];
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = (Asn1OctetString)values[i];
			}
			return array;
		}
		if (ValueType == 2)
		{
			DerObjectIdentifier[] array2 = new DerObjectIdentifier[values.Count];
			for (int j = 0; j != array2.Length; j++)
			{
				array2[j] = (DerObjectIdentifier)values[j];
			}
			return array2;
		}
		DerUtf8String[] array3 = new DerUtf8String[values.Count];
		for (int k = 0; k != array3.Length; k++)
		{
			array3[k] = (DerUtf8String)values[k];
		}
		return array3;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, policyAuthority);
		asn1EncodableVector.Add(new DerSequence(values));
		return new DerSequence(asn1EncodableVector);
	}
}
