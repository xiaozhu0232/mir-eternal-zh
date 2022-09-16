using System;

namespace Org.BouncyCastle.Asn1.X509;

public class UserNotice : Asn1Encodable
{
	private readonly NoticeReference noticeRef;

	private readonly DisplayText explicitText;

	public virtual NoticeReference NoticeRef => noticeRef;

	public virtual DisplayText ExplicitText => explicitText;

	public UserNotice(NoticeReference noticeRef, DisplayText explicitText)
	{
		this.noticeRef = noticeRef;
		this.explicitText = explicitText;
	}

	public UserNotice(NoticeReference noticeRef, string str)
		: this(noticeRef, new DisplayText(str))
	{
	}

	[Obsolete("Use GetInstance() instead")]
	public UserNotice(Asn1Sequence seq)
	{
		if (seq.Count == 2)
		{
			noticeRef = NoticeReference.GetInstance(seq[0]);
			explicitText = DisplayText.GetInstance(seq[1]);
		}
		else if (seq.Count == 1)
		{
			if (seq[0].ToAsn1Object() is Asn1Sequence)
			{
				noticeRef = NoticeReference.GetInstance(seq[0]);
				explicitText = null;
			}
			else
			{
				noticeRef = null;
				explicitText = DisplayText.GetInstance(seq[0]);
			}
		}
		else
		{
			if (seq.Count != 0)
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}
			noticeRef = null;
			explicitText = null;
		}
	}

	public static UserNotice GetInstance(object obj)
	{
		if (obj is UserNotice)
		{
			return (UserNotice)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new UserNotice(Asn1Sequence.GetInstance(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(noticeRef, explicitText);
		return new DerSequence(asn1EncodableVector);
	}
}
