using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf;

public class EncryptedValue : Asn1Encodable
{
	private readonly AlgorithmIdentifier intendedAlg;

	private readonly AlgorithmIdentifier symmAlg;

	private readonly DerBitString encSymmKey;

	private readonly AlgorithmIdentifier keyAlg;

	private readonly Asn1OctetString valueHint;

	private readonly DerBitString encValue;

	public virtual AlgorithmIdentifier IntendedAlg => intendedAlg;

	public virtual AlgorithmIdentifier SymmAlg => symmAlg;

	public virtual DerBitString EncSymmKey => encSymmKey;

	public virtual AlgorithmIdentifier KeyAlg => keyAlg;

	public virtual Asn1OctetString ValueHint => valueHint;

	public virtual DerBitString EncValue => encValue;

	private EncryptedValue(Asn1Sequence seq)
	{
		int i;
		for (i = 0; seq[i] is Asn1TaggedObject; i++)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i];
			switch (asn1TaggedObject.TagNo)
			{
			case 0:
				intendedAlg = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			case 1:
				symmAlg = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			case 2:
				encSymmKey = DerBitString.GetInstance(asn1TaggedObject, isExplicit: false);
				break;
			case 3:
				keyAlg = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			case 4:
				valueHint = Asn1OctetString.GetInstance(asn1TaggedObject, isExplicit: false);
				break;
			}
		}
		encValue = DerBitString.GetInstance(seq[i]);
	}

	public static EncryptedValue GetInstance(object obj)
	{
		if (obj is EncryptedValue)
		{
			return (EncryptedValue)obj;
		}
		if (obj != null)
		{
			return new EncryptedValue(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public EncryptedValue(AlgorithmIdentifier intendedAlg, AlgorithmIdentifier symmAlg, DerBitString encSymmKey, AlgorithmIdentifier keyAlg, Asn1OctetString valueHint, DerBitString encValue)
	{
		if (encValue == null)
		{
			throw new ArgumentNullException("encValue");
		}
		this.intendedAlg = intendedAlg;
		this.symmAlg = symmAlg;
		this.encSymmKey = encSymmKey;
		this.keyAlg = keyAlg;
		this.valueHint = valueHint;
		this.encValue = encValue;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, intendedAlg);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, symmAlg);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 2, encSymmKey);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 3, keyAlg);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 4, valueHint);
		asn1EncodableVector.Add(encValue);
		return new DerSequence(asn1EncodableVector);
	}
}
