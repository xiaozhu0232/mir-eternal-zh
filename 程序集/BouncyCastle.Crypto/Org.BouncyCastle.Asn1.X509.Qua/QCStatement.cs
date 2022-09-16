using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509.Qualified;

public class QCStatement : Asn1Encodable
{
	private readonly DerObjectIdentifier qcStatementId;

	private readonly Asn1Encodable qcStatementInfo;

	public DerObjectIdentifier StatementId => qcStatementId;

	public Asn1Encodable StatementInfo => qcStatementInfo;

	public static QCStatement GetInstance(object obj)
	{
		if (obj == null || obj is QCStatement)
		{
			return (QCStatement)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new QCStatement(Asn1Sequence.GetInstance(obj));
		}
		throw new ArgumentException("unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
	}

	private QCStatement(Asn1Sequence seq)
	{
		qcStatementId = DerObjectIdentifier.GetInstance(seq[0]);
		if (seq.Count > 1)
		{
			qcStatementInfo = seq[1];
		}
	}

	public QCStatement(DerObjectIdentifier qcStatementId)
	{
		this.qcStatementId = qcStatementId;
	}

	public QCStatement(DerObjectIdentifier qcStatementId, Asn1Encodable qcStatementInfo)
	{
		this.qcStatementId = qcStatementId;
		this.qcStatementInfo = qcStatementInfo;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(qcStatementId);
		asn1EncodableVector.AddOptional(qcStatementInfo);
		return new DerSequence(asn1EncodableVector);
	}
}
