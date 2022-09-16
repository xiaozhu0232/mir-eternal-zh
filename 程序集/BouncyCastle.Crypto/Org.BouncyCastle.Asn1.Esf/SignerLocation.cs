using System;
using Org.BouncyCastle.Asn1.X500;

namespace Org.BouncyCastle.Asn1.Esf;

public class SignerLocation : Asn1Encodable
{
	private DirectoryString countryName;

	private DirectoryString localityName;

	private Asn1Sequence postalAddress;

	public DirectoryString Country => countryName;

	public DirectoryString Locality => localityName;

	[Obsolete("Use 'Country' property instead")]
	public DerUtf8String CountryName
	{
		get
		{
			if (countryName != null)
			{
				return new DerUtf8String(countryName.GetString());
			}
			return null;
		}
	}

	[Obsolete("Use 'Locality' property instead")]
	public DerUtf8String LocalityName
	{
		get
		{
			if (localityName != null)
			{
				return new DerUtf8String(localityName.GetString());
			}
			return null;
		}
	}

	public Asn1Sequence PostalAddress => postalAddress;

	public SignerLocation(Asn1Sequence seq)
	{
		foreach (Asn1TaggedObject item in seq)
		{
			switch (item.TagNo)
			{
			case 0:
				countryName = DirectoryString.GetInstance(item, isExplicit: true);
				break;
			case 1:
				localityName = DirectoryString.GetInstance(item, isExplicit: true);
				break;
			case 2:
			{
				bool explicitly = item.IsExplicit();
				postalAddress = Asn1Sequence.GetInstance(item, explicitly);
				if (postalAddress != null && postalAddress.Count > 6)
				{
					throw new ArgumentException("postal address must contain less than 6 strings");
				}
				break;
			}
			default:
				throw new ArgumentException("illegal tag");
			}
		}
	}

	private SignerLocation(DirectoryString countryName, DirectoryString localityName, Asn1Sequence postalAddress)
	{
		if (postalAddress != null && postalAddress.Count > 6)
		{
			throw new ArgumentException("postal address must contain less than 6 strings");
		}
		this.countryName = countryName;
		this.localityName = localityName;
		this.postalAddress = postalAddress;
	}

	public SignerLocation(DirectoryString countryName, DirectoryString localityName, DirectoryString[] postalAddress)
		: this(countryName, localityName, new DerSequence(postalAddress))
	{
	}

	public SignerLocation(DerUtf8String countryName, DerUtf8String localityName, Asn1Sequence postalAddress)
		: this(DirectoryString.GetInstance(countryName), DirectoryString.GetInstance(localityName), postalAddress)
	{
	}

	public static SignerLocation GetInstance(object obj)
	{
		if (obj == null || obj is SignerLocation)
		{
			return (SignerLocation)obj;
		}
		return new SignerLocation(Asn1Sequence.GetInstance(obj));
	}

	public DirectoryString[] GetPostal()
	{
		if (postalAddress == null)
		{
			return null;
		}
		DirectoryString[] array = new DirectoryString[postalAddress.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = DirectoryString.GetInstance(postalAddress[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, countryName);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, localityName);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 2, postalAddress);
		return new DerSequence(asn1EncodableVector);
	}
}
