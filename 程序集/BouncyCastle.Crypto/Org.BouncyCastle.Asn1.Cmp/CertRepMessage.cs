using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CertRepMessage : Asn1Encodable
{
	private readonly Asn1Sequence caPubs;

	private readonly Asn1Sequence response;

	private CertRepMessage(Asn1Sequence seq)
	{
		int index = 0;
		if (seq.Count > 1)
		{
			caPubs = Asn1Sequence.GetInstance((Asn1TaggedObject)seq[index++], explicitly: true);
		}
		response = Asn1Sequence.GetInstance(seq[index]);
	}

	public static CertRepMessage GetInstance(object obj)
	{
		if (obj is CertRepMessage)
		{
			return (CertRepMessage)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertRepMessage((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public CertRepMessage(CmpCertificate[] caPubs, CertResponse[] response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (caPubs != null)
		{
			this.caPubs = new DerSequence(caPubs);
		}
		this.response = new DerSequence(response);
	}

	public virtual CmpCertificate[] GetCAPubs()
	{
		if (caPubs == null)
		{
			return null;
		}
		CmpCertificate[] array = new CmpCertificate[caPubs.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CmpCertificate.GetInstance(caPubs[i]);
		}
		return array;
	}

	public virtual CertResponse[] GetResponse()
	{
		CertResponse[] array = new CertResponse[response.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertResponse.GetInstance(response[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, caPubs);
		asn1EncodableVector.Add(response);
		return new DerSequence(asn1EncodableVector);
	}
}
