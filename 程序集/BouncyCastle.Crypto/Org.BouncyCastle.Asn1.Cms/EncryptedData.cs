using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class EncryptedData : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly EncryptedContentInfo encryptedContentInfo;

	private readonly Asn1Set unprotectedAttrs;

	public virtual DerInteger Version => version;

	public virtual EncryptedContentInfo EncryptedContentInfo => encryptedContentInfo;

	public virtual Asn1Set UnprotectedAttrs => unprotectedAttrs;

	public static EncryptedData GetInstance(object obj)
	{
		if (obj is EncryptedData)
		{
			return (EncryptedData)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new EncryptedData((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid EncryptedData: " + Platform.GetTypeName(obj));
	}

	public EncryptedData(EncryptedContentInfo encInfo)
		: this(encInfo, null)
	{
	}

	public EncryptedData(EncryptedContentInfo encInfo, Asn1Set unprotectedAttrs)
	{
		if (encInfo == null)
		{
			throw new ArgumentNullException("encInfo");
		}
		version = new DerInteger((unprotectedAttrs != null) ? 2 : 0);
		encryptedContentInfo = encInfo;
		this.unprotectedAttrs = unprotectedAttrs;
	}

	private EncryptedData(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		if (seq.Count < 2 || seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		version = DerInteger.GetInstance(seq[0]);
		encryptedContentInfo = EncryptedContentInfo.GetInstance(seq[1]);
		if (seq.Count > 2)
		{
			unprotectedAttrs = Asn1Set.GetInstance((Asn1TaggedObject)seq[2], explicitly: false);
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, encryptedContentInfo);
		if (unprotectedAttrs != null)
		{
			asn1EncodableVector.Add(new BerTaggedObject(explicitly: false, 1, unprotectedAttrs));
		}
		return new BerSequence(asn1EncodableVector);
	}
}
