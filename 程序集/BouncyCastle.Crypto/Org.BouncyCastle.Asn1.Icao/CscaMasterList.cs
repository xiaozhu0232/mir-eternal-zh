using System;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Icao;

public class CscaMasterList : Asn1Encodable
{
	private DerInteger version = new DerInteger(0);

	private X509CertificateStructure[] certList;

	public virtual int Version => version.IntValueExact;

	public static CscaMasterList GetInstance(object obj)
	{
		if (obj is CscaMasterList)
		{
			return (CscaMasterList)obj;
		}
		if (obj != null)
		{
			return new CscaMasterList(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	private CscaMasterList(Asn1Sequence seq)
	{
		if (seq == null || seq.Count == 0)
		{
			throw new ArgumentException("null or empty sequence passed.");
		}
		if (seq.Count != 2)
		{
			throw new ArgumentException("Incorrect sequence size: " + seq.Count);
		}
		version = DerInteger.GetInstance(seq[0]);
		Asn1Set instance = Asn1Set.GetInstance(seq[1]);
		certList = new X509CertificateStructure[instance.Count];
		for (int i = 0; i < certList.Length; i++)
		{
			certList[i] = X509CertificateStructure.GetInstance(instance[i]);
		}
	}

	public CscaMasterList(X509CertificateStructure[] certStructs)
	{
		certList = CopyCertList(certStructs);
	}

	public X509CertificateStructure[] GetCertStructs()
	{
		return CopyCertList(certList);
	}

	private static X509CertificateStructure[] CopyCertList(X509CertificateStructure[] orig)
	{
		return (X509CertificateStructure[])orig.Clone();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(version, new DerSet(certList));
	}
}
