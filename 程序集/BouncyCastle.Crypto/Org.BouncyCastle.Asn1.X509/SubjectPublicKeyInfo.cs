using System;

namespace Org.BouncyCastle.Asn1.X509;

public class SubjectPublicKeyInfo : Asn1Encodable
{
	private readonly AlgorithmIdentifier algID;

	private readonly DerBitString keyData;

	public AlgorithmIdentifier AlgorithmID => algID;

	public DerBitString PublicKeyData => keyData;

	public static SubjectPublicKeyInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static SubjectPublicKeyInfo GetInstance(object obj)
	{
		if (obj is SubjectPublicKeyInfo)
		{
			return (SubjectPublicKeyInfo)obj;
		}
		if (obj != null)
		{
			return new SubjectPublicKeyInfo(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	public SubjectPublicKeyInfo(AlgorithmIdentifier algID, Asn1Encodable publicKey)
	{
		keyData = new DerBitString(publicKey);
		this.algID = algID;
	}

	public SubjectPublicKeyInfo(AlgorithmIdentifier algID, byte[] publicKey)
	{
		keyData = new DerBitString(publicKey);
		this.algID = algID;
	}

	private SubjectPublicKeyInfo(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
		}
		algID = AlgorithmIdentifier.GetInstance(seq[0]);
		keyData = DerBitString.GetInstance(seq[1]);
	}

	public Asn1Object ParsePublicKey()
	{
		return Asn1Object.FromByteArray(keyData.GetOctets());
	}

	[Obsolete("Use 'ParsePublicKey' instead")]
	public Asn1Object GetPublicKey()
	{
		return Asn1Object.FromByteArray(keyData.GetOctets());
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(algID, keyData);
	}
}
