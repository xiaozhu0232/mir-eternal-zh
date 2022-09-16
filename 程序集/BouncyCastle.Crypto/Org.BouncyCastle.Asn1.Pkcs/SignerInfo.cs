using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class SignerInfo : Asn1Encodable
{
	private DerInteger version;

	private IssuerAndSerialNumber issuerAndSerialNumber;

	private AlgorithmIdentifier digAlgorithm;

	private Asn1Set authenticatedAttributes;

	private AlgorithmIdentifier digEncryptionAlgorithm;

	private Asn1OctetString encryptedDigest;

	private Asn1Set unauthenticatedAttributes;

	public DerInteger Version => version;

	public IssuerAndSerialNumber IssuerAndSerialNumber => issuerAndSerialNumber;

	public Asn1Set AuthenticatedAttributes => authenticatedAttributes;

	public AlgorithmIdentifier DigestAlgorithm => digAlgorithm;

	public Asn1OctetString EncryptedDigest => encryptedDigest;

	public AlgorithmIdentifier DigestEncryptionAlgorithm => digEncryptionAlgorithm;

	public Asn1Set UnauthenticatedAttributes => unauthenticatedAttributes;

	public static SignerInfo GetInstance(object obj)
	{
		if (obj is SignerInfo)
		{
			return (SignerInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SignerInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public SignerInfo(DerInteger version, IssuerAndSerialNumber issuerAndSerialNumber, AlgorithmIdentifier digAlgorithm, Asn1Set authenticatedAttributes, AlgorithmIdentifier digEncryptionAlgorithm, Asn1OctetString encryptedDigest, Asn1Set unauthenticatedAttributes)
	{
		this.version = version;
		this.issuerAndSerialNumber = issuerAndSerialNumber;
		this.digAlgorithm = digAlgorithm;
		this.authenticatedAttributes = authenticatedAttributes;
		this.digEncryptionAlgorithm = digEncryptionAlgorithm;
		this.encryptedDigest = encryptedDigest;
		this.unauthenticatedAttributes = unauthenticatedAttributes;
	}

	public SignerInfo(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		version = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		issuerAndSerialNumber = IssuerAndSerialNumber.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		digAlgorithm = AlgorithmIdentifier.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		object current = enumerator.Current;
		if (current is Asn1TaggedObject)
		{
			authenticatedAttributes = Asn1Set.GetInstance((Asn1TaggedObject)current, explicitly: false);
			enumerator.MoveNext();
			digEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(enumerator.Current);
		}
		else
		{
			authenticatedAttributes = null;
			digEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(current);
		}
		enumerator.MoveNext();
		encryptedDigest = Asn1OctetString.GetInstance(enumerator.Current);
		if (enumerator.MoveNext())
		{
			unauthenticatedAttributes = Asn1Set.GetInstance((Asn1TaggedObject)enumerator.Current, explicitly: false);
		}
		else
		{
			unauthenticatedAttributes = null;
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, issuerAndSerialNumber, digAlgorithm);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, authenticatedAttributes);
		asn1EncodableVector.Add(digEncryptionAlgorithm, encryptedDigest);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, unauthenticatedAttributes);
		return new DerSequence(asn1EncodableVector);
	}
}
