using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Esf;

public class CommitmentTypeQualifier : Asn1Encodable
{
	private readonly DerObjectIdentifier commitmentTypeIdentifier;

	private readonly Asn1Object qualifier;

	public DerObjectIdentifier CommitmentTypeIdentifier => commitmentTypeIdentifier;

	public Asn1Object Qualifier => qualifier;

	public CommitmentTypeQualifier(DerObjectIdentifier commitmentTypeIdentifier)
		: this(commitmentTypeIdentifier, null)
	{
	}

	public CommitmentTypeQualifier(DerObjectIdentifier commitmentTypeIdentifier, Asn1Encodable qualifier)
	{
		if (commitmentTypeIdentifier == null)
		{
			throw new ArgumentNullException("commitmentTypeIdentifier");
		}
		this.commitmentTypeIdentifier = commitmentTypeIdentifier;
		if (qualifier != null)
		{
			this.qualifier = qualifier.ToAsn1Object();
		}
	}

	public CommitmentTypeQualifier(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		commitmentTypeIdentifier = (DerObjectIdentifier)seq[0].ToAsn1Object();
		if (seq.Count > 1)
		{
			qualifier = seq[1].ToAsn1Object();
		}
	}

	public static CommitmentTypeQualifier GetInstance(object obj)
	{
		if (obj == null || obj is CommitmentTypeQualifier)
		{
			return (CommitmentTypeQualifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CommitmentTypeQualifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'CommitmentTypeQualifier' factory: " + Platform.GetTypeName(obj), "obj");
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(commitmentTypeIdentifier);
		asn1EncodableVector.AddOptional(qualifier);
		return new DerSequence(asn1EncodableVector);
	}
}
