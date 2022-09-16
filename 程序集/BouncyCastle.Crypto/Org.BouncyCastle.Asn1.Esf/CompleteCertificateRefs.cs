using System;
using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf;

public class CompleteCertificateRefs : Asn1Encodable
{
	private readonly Asn1Sequence otherCertIDs;

	public static CompleteCertificateRefs GetInstance(object obj)
	{
		if (obj == null || obj is CompleteCertificateRefs)
		{
			return (CompleteCertificateRefs)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CompleteCertificateRefs((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'CompleteCertificateRefs' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private CompleteCertificateRefs(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		foreach (Asn1Encodable item in seq)
		{
			OtherCertID.GetInstance(item.ToAsn1Object());
		}
		otherCertIDs = seq;
	}

	public CompleteCertificateRefs(params OtherCertID[] otherCertIDs)
	{
		if (otherCertIDs == null)
		{
			throw new ArgumentNullException("otherCertIDs");
		}
		this.otherCertIDs = new DerSequence(otherCertIDs);
	}

	public CompleteCertificateRefs(IEnumerable otherCertIDs)
	{
		if (otherCertIDs == null)
		{
			throw new ArgumentNullException("otherCertIDs");
		}
		if (!CollectionUtilities.CheckElementsAreOfType(otherCertIDs, typeof(OtherCertID)))
		{
			throw new ArgumentException("Must contain only 'OtherCertID' objects", "otherCertIDs");
		}
		this.otherCertIDs = new DerSequence(Asn1EncodableVector.FromEnumerable(otherCertIDs));
	}

	public OtherCertID[] GetOtherCertIDs()
	{
		OtherCertID[] array = new OtherCertID[otherCertIDs.Count];
		for (int i = 0; i < otherCertIDs.Count; i++)
		{
			array[i] = OtherCertID.GetInstance(otherCertIDs[i].ToAsn1Object());
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return otherCertIDs;
	}
}
