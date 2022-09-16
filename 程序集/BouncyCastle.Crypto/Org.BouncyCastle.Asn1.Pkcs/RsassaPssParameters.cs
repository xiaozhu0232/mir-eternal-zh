using System;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class RsassaPssParameters : Asn1Encodable
{
	private AlgorithmIdentifier hashAlgorithm;

	private AlgorithmIdentifier maskGenAlgorithm;

	private DerInteger saltLength;

	private DerInteger trailerField;

	public static readonly AlgorithmIdentifier DefaultHashAlgorithm = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);

	public static readonly AlgorithmIdentifier DefaultMaskGenFunction = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, DefaultHashAlgorithm);

	public static readonly DerInteger DefaultSaltLength = new DerInteger(20);

	public static readonly DerInteger DefaultTrailerField = new DerInteger(1);

	public AlgorithmIdentifier HashAlgorithm => hashAlgorithm;

	public AlgorithmIdentifier MaskGenAlgorithm => maskGenAlgorithm;

	public DerInteger SaltLength => saltLength;

	public DerInteger TrailerField => trailerField;

	public static RsassaPssParameters GetInstance(object obj)
	{
		if (obj == null || obj is RsassaPssParameters)
		{
			return (RsassaPssParameters)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RsassaPssParameters((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public RsassaPssParameters()
	{
		hashAlgorithm = DefaultHashAlgorithm;
		maskGenAlgorithm = DefaultMaskGenFunction;
		saltLength = DefaultSaltLength;
		trailerField = DefaultTrailerField;
	}

	public RsassaPssParameters(AlgorithmIdentifier hashAlgorithm, AlgorithmIdentifier maskGenAlgorithm, DerInteger saltLength, DerInteger trailerField)
	{
		this.hashAlgorithm = hashAlgorithm;
		this.maskGenAlgorithm = maskGenAlgorithm;
		this.saltLength = saltLength;
		this.trailerField = trailerField;
	}

	public RsassaPssParameters(Asn1Sequence seq)
	{
		hashAlgorithm = DefaultHashAlgorithm;
		maskGenAlgorithm = DefaultMaskGenFunction;
		saltLength = DefaultSaltLength;
		trailerField = DefaultTrailerField;
		for (int i = 0; i != seq.Count; i++)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i];
			switch (asn1TaggedObject.TagNo)
			{
			case 0:
				hashAlgorithm = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: true);
				break;
			case 1:
				maskGenAlgorithm = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: true);
				break;
			case 2:
				saltLength = DerInteger.GetInstance(asn1TaggedObject, isExplicit: true);
				break;
			case 3:
				trailerField = DerInteger.GetInstance(asn1TaggedObject, isExplicit: true);
				break;
			default:
				throw new ArgumentException("unknown tag");
			}
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (!hashAlgorithm.Equals(DefaultHashAlgorithm))
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, hashAlgorithm));
		}
		if (!maskGenAlgorithm.Equals(DefaultMaskGenFunction))
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 1, maskGenAlgorithm));
		}
		if (!saltLength.Equals(DefaultSaltLength))
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 2, saltLength));
		}
		if (!trailerField.Equals(DefaultTrailerField))
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 3, trailerField));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
