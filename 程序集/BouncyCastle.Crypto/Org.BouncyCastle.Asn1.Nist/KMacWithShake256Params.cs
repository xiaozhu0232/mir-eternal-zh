using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Nist;

public class KMacWithShake256Params : Asn1Encodable
{
	private static readonly byte[] EMPTY_STRING = new byte[0];

	private static readonly int DEF_LENGTH = 512;

	private readonly int outputLength;

	private readonly byte[] customizationString;

	public int OutputLength => outputLength;

	public byte[] CustomizationString => Arrays.Clone(customizationString);

	public KMacWithShake256Params(int outputLength)
	{
		this.outputLength = outputLength;
		customizationString = EMPTY_STRING;
	}

	public KMacWithShake256Params(int outputLength, byte[] customizationString)
	{
		this.outputLength = outputLength;
		this.customizationString = Arrays.Clone(customizationString);
	}

	public static KMacWithShake256Params GetInstance(object o)
	{
		if (o is KMacWithShake256Params)
		{
			return (KMacWithShake256Params)o;
		}
		if (o != null)
		{
			return new KMacWithShake256Params(Asn1Sequence.GetInstance(o));
		}
		return null;
	}

	private KMacWithShake256Params(Asn1Sequence seq)
	{
		if (seq.Count > 2)
		{
			throw new InvalidOperationException("sequence size greater than 2");
		}
		if (seq.Count == 2)
		{
			outputLength = DerInteger.GetInstance(seq[0]).IntValueExact;
			customizationString = Arrays.Clone(Asn1OctetString.GetInstance(seq[1]).GetOctets());
		}
		else if (seq.Count == 1)
		{
			if (seq[0] is DerInteger)
			{
				outputLength = DerInteger.GetInstance(seq[0]).IntValueExact;
				customizationString = EMPTY_STRING;
			}
			else
			{
				outputLength = DEF_LENGTH;
				customizationString = Arrays.Clone(Asn1OctetString.GetInstance(seq[0]).GetOctets());
			}
		}
		else
		{
			outputLength = DEF_LENGTH;
			customizationString = EMPTY_STRING;
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (outputLength != DEF_LENGTH)
		{
			asn1EncodableVector.Add(new DerInteger(outputLength));
		}
		if (customizationString.Length != 0)
		{
			asn1EncodableVector.Add(new DerOctetString(CustomizationString));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
