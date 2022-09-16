using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Crmf;

public class PkiPublicationInfo : Asn1Encodable
{
	private readonly DerInteger action;

	private readonly Asn1Sequence pubInfos;

	public virtual DerInteger Action => action;

	private PkiPublicationInfo(Asn1Sequence seq)
	{
		action = DerInteger.GetInstance(seq[0]);
		pubInfos = Asn1Sequence.GetInstance(seq[1]);
	}

	public static PkiPublicationInfo GetInstance(object obj)
	{
		if (obj is PkiPublicationInfo)
		{
			return (PkiPublicationInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PkiPublicationInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual SinglePubInfo[] GetPubInfos()
	{
		if (pubInfos == null)
		{
			return null;
		}
		SinglePubInfo[] array = new SinglePubInfo[pubInfos.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = SinglePubInfo.GetInstance(pubInfos[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(action, pubInfos);
	}
}
