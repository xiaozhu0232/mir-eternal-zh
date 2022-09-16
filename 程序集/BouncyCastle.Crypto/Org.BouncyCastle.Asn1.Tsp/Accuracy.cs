using System;

namespace Org.BouncyCastle.Asn1.Tsp;

public class Accuracy : Asn1Encodable
{
	protected const int MinMillis = 1;

	protected const int MaxMillis = 999;

	protected const int MinMicros = 1;

	protected const int MaxMicros = 999;

	private readonly DerInteger seconds;

	private readonly DerInteger millis;

	private readonly DerInteger micros;

	public DerInteger Seconds => seconds;

	public DerInteger Millis => millis;

	public DerInteger Micros => micros;

	public Accuracy(DerInteger seconds, DerInteger millis, DerInteger micros)
	{
		if (millis != null)
		{
			int intValueExact = millis.IntValueExact;
			if (intValueExact < 1 || intValueExact > 999)
			{
				throw new ArgumentException("Invalid millis field : not in (1..999)");
			}
		}
		if (micros != null)
		{
			int intValueExact2 = micros.IntValueExact;
			if (intValueExact2 < 1 || intValueExact2 > 999)
			{
				throw new ArgumentException("Invalid micros field : not in (1..999)");
			}
		}
		this.seconds = seconds;
		this.millis = millis;
		this.micros = micros;
	}

	private Accuracy(Asn1Sequence seq)
	{
		for (int i = 0; i < seq.Count; i++)
		{
			if (seq[i] is DerInteger)
			{
				seconds = (DerInteger)seq[i];
			}
			else
			{
				if (!(seq[i] is Asn1TaggedObject))
				{
					continue;
				}
				Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)seq[i];
				switch (asn1TaggedObject.TagNo)
				{
				case 0:
				{
					millis = DerInteger.GetInstance(asn1TaggedObject, isExplicit: false);
					int intValueExact2 = millis.IntValueExact;
					if (intValueExact2 < 1 || intValueExact2 > 999)
					{
						throw new ArgumentException("Invalid millis field : not in (1..999)");
					}
					break;
				}
				case 1:
				{
					micros = DerInteger.GetInstance(asn1TaggedObject, isExplicit: false);
					int intValueExact = micros.IntValueExact;
					if (intValueExact < 1 || intValueExact > 999)
					{
						throw new ArgumentException("Invalid micros field : not in (1..999)");
					}
					break;
				}
				default:
					throw new ArgumentException("Invalid tag number");
				}
			}
		}
	}

	public static Accuracy GetInstance(object obj)
	{
		if (obj is Accuracy)
		{
			return (Accuracy)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new Accuracy(Asn1Sequence.GetInstance(obj));
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptional(seconds);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, millis);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, micros);
		return new DerSequence(asn1EncodableVector);
	}
}
