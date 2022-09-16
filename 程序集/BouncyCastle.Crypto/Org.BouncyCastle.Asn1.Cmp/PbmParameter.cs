using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cmp;

public class PbmParameter : Asn1Encodable
{
	private Asn1OctetString salt;

	private AlgorithmIdentifier owf;

	private DerInteger iterationCount;

	private AlgorithmIdentifier mac;

	public virtual Asn1OctetString Salt => salt;

	public virtual AlgorithmIdentifier Owf => owf;

	public virtual DerInteger IterationCount => iterationCount;

	public virtual AlgorithmIdentifier Mac => mac;

	private PbmParameter(Asn1Sequence seq)
	{
		salt = Asn1OctetString.GetInstance(seq[0]);
		owf = AlgorithmIdentifier.GetInstance(seq[1]);
		iterationCount = DerInteger.GetInstance(seq[2]);
		mac = AlgorithmIdentifier.GetInstance(seq[3]);
	}

	public static PbmParameter GetInstance(object obj)
	{
		if (obj is PbmParameter)
		{
			return (PbmParameter)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PbmParameter((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid object: " + Platform.GetTypeName(obj), "obj");
	}

	public PbmParameter(byte[] salt, AlgorithmIdentifier owf, int iterationCount, AlgorithmIdentifier mac)
		: this(new DerOctetString(salt), owf, new DerInteger(iterationCount), mac)
	{
	}

	public PbmParameter(Asn1OctetString salt, AlgorithmIdentifier owf, DerInteger iterationCount, AlgorithmIdentifier mac)
	{
		this.salt = salt;
		this.owf = owf;
		this.iterationCount = iterationCount;
		this.mac = mac;
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(salt, owf, iterationCount, mac);
	}
}
