using System;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiBody : Asn1Encodable, IAsn1Choice
{
	public const int TYPE_INIT_REQ = 0;

	public const int TYPE_INIT_REP = 1;

	public const int TYPE_CERT_REQ = 2;

	public const int TYPE_CERT_REP = 3;

	public const int TYPE_P10_CERT_REQ = 4;

	public const int TYPE_POPO_CHALL = 5;

	public const int TYPE_POPO_REP = 6;

	public const int TYPE_KEY_UPDATE_REQ = 7;

	public const int TYPE_KEY_UPDATE_REP = 8;

	public const int TYPE_KEY_RECOVERY_REQ = 9;

	public const int TYPE_KEY_RECOVERY_REP = 10;

	public const int TYPE_REVOCATION_REQ = 11;

	public const int TYPE_REVOCATION_REP = 12;

	public const int TYPE_CROSS_CERT_REQ = 13;

	public const int TYPE_CROSS_CERT_REP = 14;

	public const int TYPE_CA_KEY_UPDATE_ANN = 15;

	public const int TYPE_CERT_ANN = 16;

	public const int TYPE_REVOCATION_ANN = 17;

	public const int TYPE_CRL_ANN = 18;

	public const int TYPE_CONFIRM = 19;

	public const int TYPE_NESTED = 20;

	public const int TYPE_GEN_MSG = 21;

	public const int TYPE_GEN_REP = 22;

	public const int TYPE_ERROR = 23;

	public const int TYPE_CERT_CONFIRM = 24;

	public const int TYPE_POLL_REQ = 25;

	public const int TYPE_POLL_REP = 26;

	private int tagNo;

	private Asn1Encodable body;

	public virtual int Type => tagNo;

	public virtual Asn1Encodable Content => body;

	public static PkiBody GetInstance(object obj)
	{
		if (obj is PkiBody)
		{
			return (PkiBody)obj;
		}
		if (obj is Asn1TaggedObject)
		{
			return new PkiBody((Asn1TaggedObject)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	private PkiBody(Asn1TaggedObject tagged)
	{
		tagNo = tagged.TagNo;
		body = GetBodyForType(tagNo, tagged.GetObject());
	}

	public PkiBody(int type, Asn1Encodable content)
	{
		tagNo = type;
		body = GetBodyForType(type, content);
	}

	private static Asn1Encodable GetBodyForType(int type, Asn1Encodable o)
	{
		return type switch
		{
			0 => CertReqMessages.GetInstance(o), 
			1 => CertRepMessage.GetInstance(o), 
			2 => CertReqMessages.GetInstance(o), 
			3 => CertRepMessage.GetInstance(o), 
			4 => CertificationRequest.GetInstance(o), 
			5 => PopoDecKeyChallContent.GetInstance(o), 
			6 => PopoDecKeyRespContent.GetInstance(o), 
			7 => CertReqMessages.GetInstance(o), 
			8 => CertRepMessage.GetInstance(o), 
			9 => CertReqMessages.GetInstance(o), 
			10 => KeyRecRepContent.GetInstance(o), 
			11 => RevReqContent.GetInstance(o), 
			12 => RevRepContent.GetInstance(o), 
			13 => CertReqMessages.GetInstance(o), 
			14 => CertRepMessage.GetInstance(o), 
			15 => CAKeyUpdAnnContent.GetInstance(o), 
			16 => CmpCertificate.GetInstance(o), 
			17 => RevAnnContent.GetInstance(o), 
			18 => CrlAnnContent.GetInstance(o), 
			19 => PkiConfirmContent.GetInstance(o), 
			20 => PkiMessages.GetInstance(o), 
			21 => GenMsgContent.GetInstance(o), 
			22 => GenRepContent.GetInstance(o), 
			23 => ErrorMsgContent.GetInstance(o), 
			24 => CertConfirmContent.GetInstance(o), 
			25 => PollReqContent.GetInstance(o), 
			26 => PollRepContent.GetInstance(o), 
			_ => throw new ArgumentException("unknown tag number: " + type, "type"), 
		};
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerTaggedObject(explicitly: true, tagNo, body);
	}
}
