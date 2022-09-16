using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms;

public class OriginatorPublicKey : Asn1Encodable
{
	private readonly AlgorithmIdentifier mAlgorithm;

	private readonly DerBitString mPublicKey;

	public AlgorithmIdentifier Algorithm => mAlgorithm;

	public DerBitString PublicKey => mPublicKey;

	public OriginatorPublicKey(AlgorithmIdentifier algorithm, byte[] publicKey)
	{
		mAlgorithm = algorithm;
		mPublicKey = new DerBitString(publicKey);
	}

	[Obsolete("Use 'GetInstance' instead")]
	public OriginatorPublicKey(Asn1Sequence seq)
	{
		mAlgorithm = AlgorithmIdentifier.GetInstance(seq[0]);
		mPublicKey = DerBitString.GetInstance(seq[1]);
	}

	public static OriginatorPublicKey GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static OriginatorPublicKey GetInstance(object obj)
	{
		if (obj == null || obj is OriginatorPublicKey)
		{
			return (OriginatorPublicKey)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OriginatorPublicKey(Asn1Sequence.GetInstance(obj));
		}
		throw new ArgumentException("Invalid OriginatorPublicKey: " + Platform.GetTypeName(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(mAlgorithm, mPublicKey);
	}
}
