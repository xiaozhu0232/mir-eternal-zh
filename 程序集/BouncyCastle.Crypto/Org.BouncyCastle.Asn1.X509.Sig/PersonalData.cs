using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509.SigI;

public class PersonalData : Asn1Encodable
{
	private readonly NameOrPseudonym nameOrPseudonym;

	private readonly BigInteger nameDistinguisher;

	private readonly DerGeneralizedTime dateOfBirth;

	private readonly DirectoryString placeOfBirth;

	private readonly string gender;

	private readonly DirectoryString postalAddress;

	public NameOrPseudonym NameOrPseudonym => nameOrPseudonym;

	public BigInteger NameDistinguisher => nameDistinguisher;

	public DerGeneralizedTime DateOfBirth => dateOfBirth;

	public DirectoryString PlaceOfBirth => placeOfBirth;

	public string Gender => gender;

	public DirectoryString PostalAddress => postalAddress;

	public static PersonalData GetInstance(object obj)
	{
		if (obj == null || obj is PersonalData)
		{
			return (PersonalData)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PersonalData((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private PersonalData(Asn1Sequence seq)
	{
		if (seq.Count < 1)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		nameOrPseudonym = NameOrPseudonym.GetInstance(enumerator.Current);
		while (enumerator.MoveNext())
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(enumerator.Current);
			switch (instance.TagNo)
			{
			case 0:
				nameDistinguisher = DerInteger.GetInstance(instance, isExplicit: false).Value;
				break;
			case 1:
				dateOfBirth = DerGeneralizedTime.GetInstance(instance, isExplicit: false);
				break;
			case 2:
				placeOfBirth = DirectoryString.GetInstance(instance, isExplicit: true);
				break;
			case 3:
				gender = DerPrintableString.GetInstance(instance, isExplicit: false).GetString();
				break;
			case 4:
				postalAddress = DirectoryString.GetInstance(instance, isExplicit: true);
				break;
			default:
				throw new ArgumentException("Bad tag number: " + instance.TagNo);
			}
		}
	}

	public PersonalData(NameOrPseudonym nameOrPseudonym, BigInteger nameDistinguisher, DerGeneralizedTime dateOfBirth, DirectoryString placeOfBirth, string gender, DirectoryString postalAddress)
	{
		this.nameOrPseudonym = nameOrPseudonym;
		this.dateOfBirth = dateOfBirth;
		this.gender = gender;
		this.nameDistinguisher = nameDistinguisher;
		this.postalAddress = postalAddress;
		this.placeOfBirth = placeOfBirth;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(nameOrPseudonym);
		if (nameDistinguisher != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 0, new DerInteger(nameDistinguisher)));
		}
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, dateOfBirth);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 2, placeOfBirth);
		if (gender != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 3, new DerPrintableString(gender, validate: true)));
		}
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 4, postalAddress);
		return new DerSequence(asn1EncodableVector);
	}
}
