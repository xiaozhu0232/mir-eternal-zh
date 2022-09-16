using System;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class RsaesOaepParameters : Asn1Encodable
{
	private AlgorithmIdentifier hashAlgorithm;

	private AlgorithmIdentifier maskGenAlgorithm;

	private AlgorithmIdentifier pSourceAlgorithm;

	public static readonly AlgorithmIdentifier DefaultHashAlgorithm = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);

	public static readonly AlgorithmIdentifier DefaultMaskGenFunction = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, DefaultHashAlgorithm);

	public static readonly AlgorithmIdentifier DefaultPSourceAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdPSpecified, new DerOctetString(new byte[0]));

	public AlgorithmIdentifier HashAlgorithm => hashAlgorithm;

	public AlgorithmIdentifier MaskGenAlgorithm => maskGenAlgorithm;

	public AlgorithmIdentifier PSourceAlgorithm => pSourceAlgorithm;

	public static RsaesOaepParameters GetInstance(object obj)
	{
		if (obj is RsaesOaepParameters)
		{
			return (RsaesOaepParameters)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RsaesOaepParameters((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public RsaesOaepParameters()
	{
		hashAlgorithm = DefaultHashAlgorithm;
		maskGenAlgorithm = DefaultMaskGenFunction;
		pSourceAlgorithm = DefaultPSourceAlgorithm;
	}

	public RsaesOaepParameters(AlgorithmIdentifier hashAlgorithm, AlgorithmIdentifier maskGenAlgorithm, AlgorithmIdentifier pSourceAlgorithm)
	{
		this.hashAlgorithm = hashAlgorithm;
		this.maskGenAlgorithm = maskGenAlgorithm;
		this.pSourceAlgorithm = pSourceAlgorithm;
	}

	public RsaesOaepParameters(Asn1Sequence seq)
	{
		hashAlgorithm = DefaultHashAlgorithm;
		maskGenAlgorithm = DefaultMaskGenFunction;
		pSourceAlgorithm = DefaultPSourceAlgorithm;
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
				pSourceAlgorithm = AlgorithmIdentifier.GetInstance(asn1TaggedObject, explicitly: true);
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
		if (!pSourceAlgorithm.Equals(DefaultPSourceAlgorithm))
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 2, pSourceAlgorithm));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
