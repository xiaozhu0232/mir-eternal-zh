using System;
using System.Collections;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509;

public class NoticeReference : Asn1Encodable
{
	private readonly DisplayText organization;

	private readonly Asn1Sequence noticeNumbers;

	public virtual DisplayText Organization => organization;

	private static Asn1EncodableVector ConvertVector(IList numbers)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (object number in numbers)
		{
			DerInteger element;
			if (number is BigInteger)
			{
				element = new DerInteger((BigInteger)number);
			}
			else
			{
				if (!(number is int))
				{
					throw new ArgumentException();
				}
				element = new DerInteger((int)number);
			}
			asn1EncodableVector.Add(element);
		}
		return asn1EncodableVector;
	}

	public NoticeReference(string organization, IList numbers)
		: this(organization, ConvertVector(numbers))
	{
	}

	public NoticeReference(string organization, Asn1EncodableVector noticeNumbers)
		: this(new DisplayText(organization), noticeNumbers)
	{
	}

	public NoticeReference(DisplayText organization, Asn1EncodableVector noticeNumbers)
	{
		this.organization = organization;
		this.noticeNumbers = new DerSequence(noticeNumbers);
	}

	private NoticeReference(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		organization = DisplayText.GetInstance(seq[0]);
		noticeNumbers = Asn1Sequence.GetInstance(seq[1]);
	}

	public static NoticeReference GetInstance(object obj)
	{
		if (obj is NoticeReference)
		{
			return (NoticeReference)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new NoticeReference(Asn1Sequence.GetInstance(obj));
	}

	public virtual DerInteger[] GetNoticeNumbers()
	{
		DerInteger[] array = new DerInteger[noticeNumbers.Count];
		for (int i = 0; i != noticeNumbers.Count; i++)
		{
			array[i] = DerInteger.GetInstance(noticeNumbers[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(organization, noticeNumbers);
	}
}
