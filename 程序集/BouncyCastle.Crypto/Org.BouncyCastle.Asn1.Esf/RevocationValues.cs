using System;
using System.Collections;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf;

public class RevocationValues : Asn1Encodable
{
	private readonly Asn1Sequence crlVals;

	private readonly Asn1Sequence ocspVals;

	private readonly OtherRevVals otherRevVals;

	public OtherRevVals OtherRevVals => otherRevVals;

	public static RevocationValues GetInstance(object obj)
	{
		if (obj == null || obj is RevocationValues)
		{
			return (RevocationValues)obj;
		}
		return new RevocationValues(Asn1Sequence.GetInstance(obj));
	}

	private RevocationValues(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		foreach (Asn1TaggedObject item in seq)
		{
			Asn1Object @object = item.GetObject();
			switch (item.TagNo)
			{
			case 0:
			{
				Asn1Sequence asn1Sequence2 = (Asn1Sequence)@object;
				foreach (Asn1Encodable item2 in asn1Sequence2)
				{
					CertificateList.GetInstance(item2.ToAsn1Object());
				}
				crlVals = asn1Sequence2;
				break;
			}
			case 1:
			{
				Asn1Sequence asn1Sequence = (Asn1Sequence)@object;
				foreach (Asn1Encodable item3 in asn1Sequence)
				{
					BasicOcspResponse.GetInstance(item3.ToAsn1Object());
				}
				ocspVals = asn1Sequence;
				break;
			}
			case 2:
				otherRevVals = OtherRevVals.GetInstance(@object);
				break;
			default:
				throw new ArgumentException("Illegal tag in RevocationValues", "seq");
			}
		}
	}

	public RevocationValues(CertificateList[] crlVals, BasicOcspResponse[] ocspVals, OtherRevVals otherRevVals)
	{
		if (crlVals != null)
		{
			this.crlVals = new DerSequence(crlVals);
		}
		if (ocspVals != null)
		{
			this.ocspVals = new DerSequence(ocspVals);
		}
		this.otherRevVals = otherRevVals;
	}

	public RevocationValues(IEnumerable crlVals, IEnumerable ocspVals, OtherRevVals otherRevVals)
	{
		if (crlVals != null)
		{
			if (!CollectionUtilities.CheckElementsAreOfType(crlVals, typeof(CertificateList)))
			{
				throw new ArgumentException("Must contain only 'CertificateList' objects", "crlVals");
			}
			this.crlVals = new DerSequence(Asn1EncodableVector.FromEnumerable(crlVals));
		}
		if (ocspVals != null)
		{
			if (!CollectionUtilities.CheckElementsAreOfType(ocspVals, typeof(BasicOcspResponse)))
			{
				throw new ArgumentException("Must contain only 'BasicOcspResponse' objects", "ocspVals");
			}
			this.ocspVals = new DerSequence(Asn1EncodableVector.FromEnumerable(ocspVals));
		}
		this.otherRevVals = otherRevVals;
	}

	public CertificateList[] GetCrlVals()
	{
		CertificateList[] array = new CertificateList[crlVals.Count];
		for (int i = 0; i < crlVals.Count; i++)
		{
			array[i] = CertificateList.GetInstance(crlVals[i].ToAsn1Object());
		}
		return array;
	}

	public BasicOcspResponse[] GetOcspVals()
	{
		BasicOcspResponse[] array = new BasicOcspResponse[ocspVals.Count];
		for (int i = 0; i < ocspVals.Count; i++)
		{
			array[i] = BasicOcspResponse.GetInstance(ocspVals[i].ToAsn1Object());
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, crlVals);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, ocspVals);
		if (otherRevVals != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 2, otherRevVals.ToAsn1Object()));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
