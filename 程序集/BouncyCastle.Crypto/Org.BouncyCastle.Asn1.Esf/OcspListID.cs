using System;
using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf;

public class OcspListID : Asn1Encodable
{
	private readonly Asn1Sequence ocspResponses;

	public static OcspListID GetInstance(object obj)
	{
		if (obj == null || obj is OcspListID)
		{
			return (OcspListID)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OcspListID((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in 'OcspListID' factory: " + Platform.GetTypeName(obj), "obj");
	}

	private OcspListID(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count != 1)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		ocspResponses = (Asn1Sequence)seq[0].ToAsn1Object();
		foreach (Asn1Encodable ocspResponse in ocspResponses)
		{
			OcspResponsesID.GetInstance(ocspResponse.ToAsn1Object());
		}
	}

	public OcspListID(params OcspResponsesID[] ocspResponses)
	{
		if (ocspResponses == null)
		{
			throw new ArgumentNullException("ocspResponses");
		}
		this.ocspResponses = new DerSequence(ocspResponses);
	}

	public OcspListID(IEnumerable ocspResponses)
	{
		if (ocspResponses == null)
		{
			throw new ArgumentNullException("ocspResponses");
		}
		if (!CollectionUtilities.CheckElementsAreOfType(ocspResponses, typeof(OcspResponsesID)))
		{
			throw new ArgumentException("Must contain only 'OcspResponsesID' objects", "ocspResponses");
		}
		this.ocspResponses = new DerSequence(Asn1EncodableVector.FromEnumerable(ocspResponses));
	}

	public OcspResponsesID[] GetOcspResponses()
	{
		OcspResponsesID[] array = new OcspResponsesID[ocspResponses.Count];
		for (int i = 0; i < ocspResponses.Count; i++)
		{
			array[i] = OcspResponsesID.GetInstance(ocspResponses[i].ToAsn1Object());
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(ocspResponses);
	}
}
