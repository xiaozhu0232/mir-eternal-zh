using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class Evidence : Asn1Encodable, IAsn1Choice
{
	private TimeStampTokenEvidence tstEvidence;

	private Asn1Sequence otherEvidence;

	public virtual TimeStampTokenEvidence TstEvidence => tstEvidence;

	public Evidence(TimeStampTokenEvidence tstEvidence)
	{
		this.tstEvidence = tstEvidence;
	}

	private Evidence(Asn1TaggedObject tagged)
	{
		if (tagged.TagNo == 0)
		{
			tstEvidence = TimeStampTokenEvidence.GetInstance(tagged, isExplicit: false);
			return;
		}
		if (tagged.TagNo == 2)
		{
			otherEvidence = Asn1Sequence.GetInstance(tagged, explicitly: false);
			return;
		}
		throw new ArgumentException("unknown tag in Evidence", "tagged");
	}

	public static Evidence GetInstance(object obj)
	{
		if (obj is Evidence)
		{
			return (Evidence)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new Evidence(Asn1TaggedObject.GetInstance(obj));
		}
		throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	public static Evidence GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(obj.GetObject());
	}

	public override Asn1Object ToAsn1Object()
	{
		if (tstEvidence != null)
		{
			return new DerTaggedObject(explicitly: false, 0, tstEvidence);
		}
		return new DerTaggedObject(explicitly: false, 2, otherEvidence);
	}
}
