using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiStatusInfo : Asn1Encodable
{
	private DerInteger status;

	private PkiFreeText statusString;

	private DerBitString failInfo;

	public BigInteger Status => status.Value;

	public PkiFreeText StatusString => statusString;

	public DerBitString FailInfo => failInfo;

	public static PkiStatusInfo GetInstance(Asn1TaggedObject obj, bool isExplicit)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
	}

	public static PkiStatusInfo GetInstance(object obj)
	{
		if (obj is PkiStatusInfo)
		{
			return (PkiStatusInfo)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PkiStatusInfo((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public PkiStatusInfo(Asn1Sequence seq)
	{
		status = DerInteger.GetInstance(seq[0]);
		statusString = null;
		failInfo = null;
		if (seq.Count > 2)
		{
			statusString = PkiFreeText.GetInstance(seq[1]);
			failInfo = DerBitString.GetInstance(seq[2]);
		}
		else if (seq.Count > 1)
		{
			object obj = seq[1];
			if (obj is DerBitString)
			{
				failInfo = DerBitString.GetInstance(obj);
			}
			else
			{
				statusString = PkiFreeText.GetInstance(obj);
			}
		}
	}

	public PkiStatusInfo(int status)
	{
		this.status = new DerInteger(status);
	}

	public PkiStatusInfo(int status, PkiFreeText statusString)
	{
		this.status = new DerInteger(status);
		this.statusString = statusString;
	}

	public PkiStatusInfo(int status, PkiFreeText statusString, PkiFailureInfo failInfo)
	{
		this.status = new DerInteger(status);
		this.statusString = statusString;
		this.failInfo = failInfo;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(status);
		asn1EncodableVector.AddOptional(statusString, failInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
