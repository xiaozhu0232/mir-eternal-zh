using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509;

public class IssuingDistributionPoint : Asn1Encodable
{
	private readonly DistributionPointName _distributionPoint;

	private readonly bool _onlyContainsUserCerts;

	private readonly bool _onlyContainsCACerts;

	private readonly ReasonFlags _onlySomeReasons;

	private readonly bool _indirectCRL;

	private readonly bool _onlyContainsAttributeCerts;

	private readonly Asn1Sequence seq;

	public bool OnlyContainsUserCerts => _onlyContainsUserCerts;

	public bool OnlyContainsCACerts => _onlyContainsCACerts;

	public bool IsIndirectCrl => _indirectCRL;

	public bool OnlyContainsAttributeCerts => _onlyContainsAttributeCerts;

	public DistributionPointName DistributionPoint => _distributionPoint;

	public ReasonFlags OnlySomeReasons => _onlySomeReasons;

	public static IssuingDistributionPoint GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static IssuingDistributionPoint GetInstance(object obj)
	{
		if (obj == null || obj is IssuingDistributionPoint)
		{
			return (IssuingDistributionPoint)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new IssuingDistributionPoint((Asn1Sequence)obj);
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public IssuingDistributionPoint(DistributionPointName distributionPoint, bool onlyContainsUserCerts, bool onlyContainsCACerts, ReasonFlags onlySomeReasons, bool indirectCRL, bool onlyContainsAttributeCerts)
	{
		_distributionPoint = distributionPoint;
		_indirectCRL = indirectCRL;
		_onlyContainsAttributeCerts = onlyContainsAttributeCerts;
		_onlyContainsCACerts = onlyContainsCACerts;
		_onlyContainsUserCerts = onlyContainsUserCerts;
		_onlySomeReasons = onlySomeReasons;
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		if (distributionPoint != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, distributionPoint));
		}
		if (onlyContainsUserCerts)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 1, DerBoolean.True));
		}
		if (onlyContainsCACerts)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 2, DerBoolean.True));
		}
		if (onlySomeReasons != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 3, onlySomeReasons));
		}
		if (indirectCRL)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 4, DerBoolean.True));
		}
		if (onlyContainsAttributeCerts)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 5, DerBoolean.True));
		}
		seq = new DerSequence(asn1EncodableVector);
	}

	private IssuingDistributionPoint(Asn1Sequence seq)
	{
		this.seq = seq;
		for (int i = 0; i != seq.Count; i++)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(seq[i]);
			switch (instance.TagNo)
			{
			case 0:
				_distributionPoint = DistributionPointName.GetInstance(instance, explicitly: true);
				break;
			case 1:
				_onlyContainsUserCerts = DerBoolean.GetInstance(instance, isExplicit: false).IsTrue;
				break;
			case 2:
				_onlyContainsCACerts = DerBoolean.GetInstance(instance, isExplicit: false).IsTrue;
				break;
			case 3:
				_onlySomeReasons = new ReasonFlags(DerBitString.GetInstance(instance, isExplicit: false));
				break;
			case 4:
				_indirectCRL = DerBoolean.GetInstance(instance, isExplicit: false).IsTrue;
				break;
			case 5:
				_onlyContainsAttributeCerts = DerBoolean.GetInstance(instance, isExplicit: false).IsTrue;
				break;
			default:
				throw new ArgumentException("unknown tag in IssuingDistributionPoint");
			}
		}
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}

	public override string ToString()
	{
		string newLine = Platform.NewLine;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("IssuingDistributionPoint: [");
		stringBuilder.Append(newLine);
		if (_distributionPoint != null)
		{
			appendObject(stringBuilder, newLine, "distributionPoint", _distributionPoint.ToString());
		}
		if (_onlyContainsUserCerts)
		{
			appendObject(stringBuilder, newLine, "onlyContainsUserCerts", _onlyContainsUserCerts.ToString());
		}
		if (_onlyContainsCACerts)
		{
			appendObject(stringBuilder, newLine, "onlyContainsCACerts", _onlyContainsCACerts.ToString());
		}
		if (_onlySomeReasons != null)
		{
			appendObject(stringBuilder, newLine, "onlySomeReasons", _onlySomeReasons.ToString());
		}
		if (_onlyContainsAttributeCerts)
		{
			appendObject(stringBuilder, newLine, "onlyContainsAttributeCerts", _onlyContainsAttributeCerts.ToString());
		}
		if (_indirectCRL)
		{
			appendObject(stringBuilder, newLine, "indirectCRL", _indirectCRL.ToString());
		}
		stringBuilder.Append("]");
		stringBuilder.Append(newLine);
		return stringBuilder.ToString();
	}

	private void appendObject(StringBuilder buf, string sep, string name, string val)
	{
		string value = "    ";
		buf.Append(value);
		buf.Append(name);
		buf.Append(":");
		buf.Append(sep);
		buf.Append(value);
		buf.Append(value);
		buf.Append(val);
		buf.Append(sep);
	}
}
