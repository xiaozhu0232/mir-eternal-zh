using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class CrlAnnContent : Asn1Encodable
{
	private readonly Asn1Sequence content;

	private CrlAnnContent(Asn1Sequence seq)
	{
		content = seq;
	}

	public static CrlAnnContent GetInstance(object obj)
	{
		if (obj is CrlAnnContent)
		{
			return (CrlAnnContent)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CrlAnnContent((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual CertificateList[] ToCertificateListArray()
	{
		CertificateList[] array = new CertificateList[content.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertificateList.GetInstance(content[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		return content;
	}
}
