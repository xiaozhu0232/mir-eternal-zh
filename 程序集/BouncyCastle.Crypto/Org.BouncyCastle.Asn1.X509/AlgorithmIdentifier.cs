using System;

namespace Org.BouncyCastle.Asn1.X509;

public class AlgorithmIdentifier : Asn1Encodable
{
	private readonly DerObjectIdentifier algorithm;

	private readonly Asn1Encodable parameters;

	public virtual DerObjectIdentifier Algorithm => algorithm;

	[Obsolete("Use 'Algorithm' property instead")]
	public virtual DerObjectIdentifier ObjectID => algorithm;

	public virtual Asn1Encodable Parameters => parameters;

	public static AlgorithmIdentifier GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static AlgorithmIdentifier GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is AlgorithmIdentifier)
		{
			return (AlgorithmIdentifier)obj;
		}
		return new AlgorithmIdentifier(Asn1Sequence.GetInstance(obj));
	}

	public AlgorithmIdentifier(DerObjectIdentifier algorithm)
	{
		this.algorithm = algorithm;
	}

	[Obsolete("Use version taking a DerObjectIdentifier")]
	public AlgorithmIdentifier(string algorithm)
	{
		this.algorithm = new DerObjectIdentifier(algorithm);
	}

	public AlgorithmIdentifier(DerObjectIdentifier algorithm, Asn1Encodable parameters)
	{
		this.algorithm = algorithm;
		this.parameters = parameters;
	}

	internal AlgorithmIdentifier(Asn1Sequence seq)
	{
		if (seq.Count < 1 || seq.Count > 2)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		algorithm = DerObjectIdentifier.GetInstance(seq[0]);
		parameters = ((seq.Count < 2) ? null : seq[1]);
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(algorithm);
		asn1EncodableVector.AddOptional(parameters);
		return new DerSequence(asn1EncodableVector);
	}
}
