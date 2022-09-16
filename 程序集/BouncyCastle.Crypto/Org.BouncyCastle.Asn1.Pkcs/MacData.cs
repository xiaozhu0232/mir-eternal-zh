using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class MacData : Asn1Encodable
{
	internal DigestInfo digInfo;

	internal byte[] salt;

	internal BigInteger iterationCount;

	public DigestInfo Mac => digInfo;

	public BigInteger IterationCount => iterationCount;

	public static MacData GetInstance(object obj)
	{
		if (obj is MacData)
		{
			return (MacData)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new MacData((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private MacData(Asn1Sequence seq)
	{
		digInfo = DigestInfo.GetInstance(seq[0]);
		salt = ((Asn1OctetString)seq[1]).GetOctets();
		if (seq.Count == 3)
		{
			iterationCount = ((DerInteger)seq[2]).Value;
		}
		else
		{
			iterationCount = BigInteger.One;
		}
	}

	public MacData(DigestInfo digInfo, byte[] salt, int iterationCount)
	{
		this.digInfo = digInfo;
		this.salt = (byte[])salt.Clone();
		this.iterationCount = BigInteger.ValueOf(iterationCount);
	}

	public byte[] GetSalt()
	{
		return (byte[])salt.Clone();
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(digInfo, new DerOctetString(salt));
		if (!iterationCount.Equals(BigInteger.One))
		{
			asn1EncodableVector.Add(new DerInteger(iterationCount));
		}
		return new DerSequence(asn1EncodableVector);
	}
}
