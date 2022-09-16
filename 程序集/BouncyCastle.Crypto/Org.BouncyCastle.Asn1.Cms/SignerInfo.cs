using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class SignerInfo : Asn1Encodable
{
	private DerInteger version;

	private SignerIdentifier sid;

	private AlgorithmIdentifier digAlgorithm;

	private Asn1Set authenticatedAttributes;

	private AlgorithmIdentifier digEncryptionAlgorithm;

	private Asn1OctetString encryptedDigest;

	private Asn1Set unauthenticatedAttributes;

	public DerInteger Version => version;

	public SignerIdentifier SignerID => sid;

	public Asn1Set AuthenticatedAttributes => authenticatedAttributes;

	public AlgorithmIdentifier DigestAlgorithm => digAlgorithm;

	public Asn1OctetString EncryptedDigest => encryptedDigest;

	public AlgorithmIdentifier DigestEncryptionAlgorithm => digEncryptionAlgorithm;

	public Asn1Set UnauthenticatedAttributes => unauthenticatedAttributes;

	public static SignerInfo GetInstance(object obj)
	{
		if (obj == null || obj is SignerInfo)
		{
			return (SignerInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new SignerInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public SignerInfo(SignerIdentifier sid, AlgorithmIdentifier digAlgorithm, Asn1Set authenticatedAttributes, AlgorithmIdentifier digEncryptionAlgorithm, Asn1OctetString encryptedDigest, Asn1Set unauthenticatedAttributes)
	{
		version = new DerInteger((!sid.IsTagged) ? 1 : 3);
		this.sid = sid;
		this.digAlgorithm = digAlgorithm;
		this.authenticatedAttributes = authenticatedAttributes;
		this.digEncryptionAlgorithm = digEncryptionAlgorithm;
		this.encryptedDigest = encryptedDigest;
		this.unauthenticatedAttributes = unauthenticatedAttributes;
	}

	public SignerInfo(SignerIdentifier sid, AlgorithmIdentifier digAlgorithm, Attributes authenticatedAttributes, AlgorithmIdentifier digEncryptionAlgorithm, Asn1OctetString encryptedDigest, Attributes unauthenticatedAttributes)
	{
		version = new DerInteger((!sid.IsTagged) ? 1 : 3);
		this.sid = sid;
		this.digAlgorithm = digAlgorithm;
		this.authenticatedAttributes = Asn1Set.GetInstance(authenticatedAttributes);
		this.digEncryptionAlgorithm = digEncryptionAlgorithm;
		this.encryptedDigest = encryptedDigest;
		this.unauthenticatedAttributes = Asn1Set.GetInstance(unauthenticatedAttributes);
	}

	[Obsolete("Use 'GetInstance' instead")]
	public SignerInfo(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		version = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		sid = SignerIdentifier.GetInstance(enumerator.Current);
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
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, sid, digAlgorithm);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, authenticatedAttributes);
		asn1EncodableVector.Add(digEncryptionAlgorithm, encryptedDigest);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, unauthenticatedAttributes);
		return new DerSequence(asn1EncodableVector);
	}
}
