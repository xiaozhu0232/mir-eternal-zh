using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class KeyRecRepContent : Asn1Encodable
{
	private readonly PkiStatusInfo status;

	private readonly CmpCertificate newSigCert;

	private readonly Asn1Sequence caCerts;

	private readonly Asn1Sequence keyPairHist;

	public virtual PkiStatusInfo Status => status;

	public virtual CmpCertificate NewSigCert => newSigCert;

	private KeyRecRepContent(Asn1Sequence seq)
	{
		status = PkiStatusInfo.GetInstance(seq[0]);
		for (int i = 1; i < seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			switch (instance.TagNo)
			{
			case 0:
				newSigCert = CmpCertificate.GetInstance(instance.GetObject());
				break;
			case 1:
				caCerts = Asn1Sequence.GetInstance(instance.GetObject());
				break;
			case 2:
				keyPairHist = Asn1Sequence.GetInstance(instance.GetObject());
				break;
			default:
				throw new ArgumentException("unknown tag number: " + instance.TagNo, "seq");
			}
		}
	}

	public static KeyRecRepContent GetInstance(object obj)
	{
		if (obj is KeyRecRepContent)
		{
			return (KeyRecRepContent)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new KeyRecRepContent((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public virtual CmpCertificate[] GetCACerts()
	{
		if (caCerts == null)
		{
			return null;
		}
		CmpCertificate[] array = new CmpCertificate[caCerts.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CmpCertificate.GetInstance(caCerts[i]);
		}
		return array;
	}

	public virtual CertifiedKeyPair[] GetKeyPairHist()
	{
		if (keyPairHist == null)
		{
			return null;
		}
		CertifiedKeyPair[] array = new CertifiedKeyPair[keyPairHist.Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = CertifiedKeyPair.GetInstance(keyPairHist[i]);
		}
		return array;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(status);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, newSigCert);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 1, caCerts);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 2, keyPairHist);
		return new DerSequence(asn1EncodableVector);
	}
}
