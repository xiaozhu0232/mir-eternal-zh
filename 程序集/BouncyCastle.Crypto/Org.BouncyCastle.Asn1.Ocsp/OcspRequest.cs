using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class OcspRequest : Asn1Encodable
{
	private readonly TbsRequest tbsRequest;

	private readonly Signature optionalSignature;

	public TbsRequest TbsRequest => tbsRequest;

	public Signature OptionalSignature => optionalSignature;

	public static OcspRequest GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static OcspRequest GetInstance(object obj)
	{
		if (obj == null || obj is OcspRequest)
		{
			return (OcspRequest)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OcspRequest((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public OcspRequest(TbsRequest tbsRequest, Signature optionalSignature)
	{
		if (tbsRequest == null)
		{
			throw new ArgumentNullException("tbsRequest");
		}
		this.tbsRequest = tbsRequest;
		this.optionalSignature = optionalSignature;
	}

	private OcspRequest(Asn1Sequence seq)
	{
		tbsRequest = TbsRequest.GetInstance(seq[0]);
		if (seq.Count == 2)
		{
			optionalSignature = Signature.GetInstance((Asn1TaggedObject)seq[1], explicitly: true);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(tbsRequest);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, optionalSignature);
		return new DerSequence(asn1EncodableVector);
	}
}
