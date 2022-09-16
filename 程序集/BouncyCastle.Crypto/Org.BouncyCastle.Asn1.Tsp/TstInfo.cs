using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Tsp;

public class TstInfo : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly DerObjectIdentifier tsaPolicyId;

	private readonly MessageImprint messageImprint;

	private readonly DerInteger serialNumber;

	private readonly DerGeneralizedTime genTime;

	private readonly Accuracy accuracy;

	private readonly DerBoolean ordering;

	private readonly DerInteger nonce;

	private readonly GeneralName tsa;

	private readonly X509Extensions extensions;

	public DerInteger Version => version;

	public MessageImprint MessageImprint => messageImprint;

	public DerObjectIdentifier Policy => tsaPolicyId;

	public DerInteger SerialNumber => serialNumber;

	public Accuracy Accuracy => accuracy;

	public DerGeneralizedTime GenTime => genTime;

	public DerBoolean Ordering => ordering;

	public DerInteger Nonce => nonce;

	public GeneralName Tsa => tsa;

	public X509Extensions Extensions => extensions;

	public static TstInfo GetInstance(object obj)
	{
		if (obj is TstInfo)
		{
			return (TstInfo)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new TstInfo(Asn1Sequence.GetInstance(obj));
	}

	private TstInfo(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		version = DerInteger.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		tsaPolicyId = DerObjectIdentifier.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		messageImprint = MessageImprint.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		serialNumber = DerInteger.GetInstance(enumerator.Current);
		enumerator.MoveNext();
		genTime = DerGeneralizedTime.GetInstance(enumerator.Current);
		ordering = DerBoolean.False;
		while (enumerator.MoveNext())
		{
			Asn1Object asn1Object = (Asn1Object)enumerator.Current;
			if (asn1Object is Asn1TaggedObject)
			{
				DerTaggedObject derTaggedObject = (DerTaggedObject)asn1Object;
				switch (derTaggedObject.TagNo)
				{
				case 0:
					tsa = GeneralName.GetInstance(derTaggedObject, explicitly: true);
					break;
				case 1:
					extensions = X509Extensions.GetInstance(derTaggedObject, explicitly: false);
					break;
				default:
					throw new ArgumentException("Unknown tag value " + derTaggedObject.TagNo);
				}
			}
			if (asn1Object is DerSequence)
			{
				accuracy = Accuracy.GetInstance(asn1Object);
			}
			if (asn1Object is DerBoolean)
			{
				ordering = DerBoolean.GetInstance(asn1Object);
			}
			if (asn1Object is DerInteger)
			{
				nonce = DerInteger.GetInstance(asn1Object);
			}
		}
	}

	public TstInfo(DerObjectIdentifier tsaPolicyId, MessageImprint messageImprint, DerInteger serialNumber, DerGeneralizedTime genTime, Accuracy accuracy, DerBoolean ordering, DerInteger nonce, GeneralName tsa, X509Extensions extensions)
	{
		version = new DerInteger(1);
		this.tsaPolicyId = tsaPolicyId;
		this.messageImprint = messageImprint;
		this.serialNumber = serialNumber;
		this.genTime = genTime;
		this.accuracy = accuracy;
		this.ordering = ordering;
		this.nonce = nonce;
		this.tsa = tsa;
		this.extensions = extensions;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, tsaPolicyId, messageImprint, serialNumber, genTime);
		asn1EncodableVector.AddOptional(accuracy);
		if (ordering != null && ordering.IsTrue)
		{
			asn1EncodableVector.Add(ordering);
		}
		asn1EncodableVector.AddOptional(nonce);
		asn1EncodableVector.AddOptionalTagged(isExplicit: true, 0, tsa);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, extensions);
		return new DerSequence(asn1EncodableVector);
	}
}
