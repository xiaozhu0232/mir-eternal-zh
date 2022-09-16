using System;

namespace Org.BouncyCastle.Asn1.CryptoPro;

public class Gost3410PublicKeyAlgParameters : Asn1Encodable
{
	private DerObjectIdentifier publicKeyParamSet;

	private DerObjectIdentifier digestParamSet;

	private DerObjectIdentifier encryptionParamSet;

	public DerObjectIdentifier PublicKeyParamSet => publicKeyParamSet;

	public DerObjectIdentifier DigestParamSet => digestParamSet;

	public DerObjectIdentifier EncryptionParamSet => encryptionParamSet;

	public static Gost3410PublicKeyAlgParameters GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static Gost3410PublicKeyAlgParameters GetInstance(object obj)
	{
		if (obj == null || obj is Gost3410PublicKeyAlgParameters)
		{
			return (Gost3410PublicKeyAlgParameters)obj;
		}
		return new Gost3410PublicKeyAlgParameters(Asn1Sequence.GetInstance(obj));
	}

	public Gost3410PublicKeyAlgParameters(DerObjectIdentifier publicKeyParamSet, DerObjectIdentifier digestParamSet)
		: this(publicKeyParamSet, digestParamSet, null)
	{
	}

	public Gost3410PublicKeyAlgParameters(DerObjectIdentifier publicKeyParamSet, DerObjectIdentifier digestParamSet, DerObjectIdentifier encryptionParamSet)
	{
		if (publicKeyParamSet == null)
		{
			throw new ArgumentNullException("publicKeyParamSet");
		}
		if (digestParamSet == null)
		{
			throw new ArgumentNullException("digestParamSet");
		}
		this.publicKeyParamSet = publicKeyParamSet;
		this.digestParamSet = digestParamSet;
		this.encryptionParamSet = encryptionParamSet;
	}

	[Obsolete("Use 'GetInstance' instead")]
	public Gost3410PublicKeyAlgParameters(Asn1Sequence seq)
	{
		publicKeyParamSet = (DerObjectIdentifier)seq[0];
		digestParamSet = (DerObjectIdentifier)seq[1];
		if (seq.Count > 2)
		{
			encryptionParamSet = (DerObjectIdentifier)seq[2];
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(publicKeyParamSet, digestParamSet);
		asn1EncodableVector.AddOptional(encryptionParamSet);
		return new DerSequence(asn1EncodableVector);
	}
}
