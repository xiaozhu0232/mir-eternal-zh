using System;

namespace Org.BouncyCastle.Asn1.Icao;

public class DataGroupHash : Asn1Encodable
{
	private readonly DerInteger dataGroupNumber;

	private readonly Asn1OctetString dataGroupHashValue;

	public int DataGroupNumber => dataGroupNumber.IntValueExact;

	public Asn1OctetString DataGroupHashValue => dataGroupHashValue;

	public static DataGroupHash GetInstance(object obj)
	{
		if (obj is DataGroupHash)
		{
			return (DataGroupHash)obj;
		}
		if (obj != null)
		{
			return new DataGroupHash(Asn1Sequence.GetInstance(obj));
		}
		return null;
	}

	private DataGroupHash(Asn1Sequence seq)
	{
		if (seq.Count != 2)
		{
			throw new ArgumentException("Wrong number of elements in sequence", "seq");
		}
		dataGroupNumber = DerInteger.GetInstance(seq[0]);
		dataGroupHashValue = Asn1OctetString.GetInstance(seq[1]);
	}

	public DataGroupHash(int dataGroupNumber, Asn1OctetString dataGroupHashValue)
	{
		this.dataGroupNumber = new DerInteger(dataGroupNumber);
		this.dataGroupHashValue = dataGroupHashValue;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(dataGroupNumber, dataGroupHashValue);
	}
}
