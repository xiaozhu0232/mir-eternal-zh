using System.Collections;

namespace Org.BouncyCastle.Asn1.X9;

public class KeySpecificInfo : Asn1Encodable
{
	private DerObjectIdentifier algorithm;

	private Asn1OctetString counter;

	public DerObjectIdentifier Algorithm => algorithm;

	public Asn1OctetString Counter => counter;

	public KeySpecificInfo(DerObjectIdentifier algorithm, Asn1OctetString counter)
	{
		this.algorithm = algorithm;
		this.counter = counter;
	}

	public KeySpecificInfo(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		algorithm = (DerObjectIdentifier)enumerator.Current;
		enumerator.MoveNext();
		counter = (Asn1OctetString)enumerator.Current;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(algorithm, counter);
	}
}
