using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CertConfirmContent : Asn1Encodable
{
	private readonly Asn1Sequence content;

	private CertConfirmContent(Asn1Sequence seq)
	{
		content = seq;
	}

	public static CertConfirmContent GetInstance(object obj)
	{
		if (obj is CertConfirmContent)
		{
			return (CertConfirmContent)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertConfirmContent((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual CertStatus[] ToCertStatusArray()
	{
		CertStatus[] array = new CertStatus[content.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertStatus.GetInstance(content[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return content;
	}
}
