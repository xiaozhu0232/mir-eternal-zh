using System;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Esf;

public class OtherHash : Asn1Encodable, IAsn1Choice
{
	private readonly Asn1OctetString sha1Hash;

	private readonly OtherHashAlgAndValue otherHash;

	public AlgorithmIdentifier HashAlgorithm
	{
		get
		{
			if (otherHash != null)
			{
				return otherHash.HashAlgorithm;
			}
			return new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1);
		}
	}

	public static OtherHash GetInstance(object obj)
	{
		if (obj == null || obj is OtherHash)
		{
			return (OtherHash)obj;
		}
		if (obj is Asn1OctetString)
		{
			return new OtherHash((Asn1OctetString)obj);
		}
		return new OtherHash(OtherHashAlgAndValue.GetInstance(obj));
	}

	public OtherHash(byte[] sha1Hash)
	{
		if (sha1Hash == null)
		{
			throw new ArgumentNullException("sha1Hash");
		}
		this.sha1Hash = new DerOctetString(sha1Hash);
	}

	public OtherHash(Asn1OctetString sha1Hash)
	{
		if (sha1Hash == null)
		{
			throw new ArgumentNullException("sha1Hash");
		}
		this.sha1Hash = sha1Hash;
	}

	public OtherHash(OtherHashAlgAndValue otherHash)
	{
		if (otherHash == null)
		{
			throw new ArgumentNullException("otherHash");
		}
		this.otherHash = otherHash;
	}

	public byte[] GetHashValue()
	{
		if (otherHash != null)
		{
			return otherHash.GetHashValue();
		}
		return sha1Hash.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		if (otherHash != null)
		{
			return otherHash.ToAsn1Object();
		}
		return sha1Hash;
	}
}
