using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp;

public class TbsRequest : Asn1Encodable
{
	private static readonly DerInteger V1 = new DerInteger(0);

	private readonly DerInteger version;

	private readonly GeneralName requestorName;

	private readonly Asn1Sequence requestList;

	private readonly X509Extensions requestExtensions;

	private bool versionSet;

	public DerInteger Version => version;

	public GeneralName RequestorName => requestorName;

	public Asn1Sequence RequestList => requestList;

	public X509Extensions RequestExtensions => requestExtensions;

	public static TbsRequest GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static TbsRequest GetInstance(object obj)
	{
		if (obj == null || obj is TbsRequest)
		{
			return (TbsRequest)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new TbsRequest((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public TbsRequest(GeneralName requestorName, Asn1Sequence requestList, X509Extensions requestExtensions)
	{
		version = V1;
		this.requestorName = requestorName;
		this.requestList = requestList;
		this.requestExtensions = requestExtensions;
	}

	private TbsRequest(Asn1Sequence seq)
	{
		int num = 0;
		Asn1Encodable asn1Encodable = seq[0];
		if (asn1Encodable is Asn1TaggedObject)
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Encodable;
			if (asn1TaggedObject.TagNo == 0)
			{
				versionSet = true;
				version = DerInteger.GetInstance(asn1TaggedObject, isExplicit: true);
				num++;
			}
			else
			{
				version = V1;
			}
		}
		else
		{
			version = V1;
		}
		if (seq[num] is Asn1TaggedObject)
		{
			requestorName = GeneralName.GetInstance((Asn1TaggedObject)seq[num++], explicitly: true);
		}
		requestList = (Asn1Sequence)seq[num++];
		if (seq.Count == num + 1)
		{
			requestExtensions = X509Extensions.GetInstance((Asn1TaggedObject)seq[num], explicitly: true);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (!version.Equals(V1) || versionSet)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, version));
		}
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, requestorName);
		asn1EncodableVector.Add(requestList);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 2, requestExtensions);
		return new DerSequence(asn1EncodableVector);
	}
}
