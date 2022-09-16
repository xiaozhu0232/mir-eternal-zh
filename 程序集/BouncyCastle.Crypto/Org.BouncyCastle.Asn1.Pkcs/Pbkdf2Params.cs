using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class Pbkdf2Params : Asn1Encodable
{
	private static AlgorithmIdentifier algid_hmacWithSHA1 = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdHmacWithSha1, DerNull.Instance);

	private readonly Asn1OctetString octStr;

	private readonly DerInteger iterationCount;

	private readonly DerInteger keyLength;

	private readonly AlgorithmIdentifier prf;

	public BigInteger IterationCount => iterationCount.Value;

	public BigInteger KeyLength
	{
		get
		{
			if (keyLength != null)
			{
				return keyLength.Value;
			}
			return null;
		}
	}

	public bool IsDefaultPrf
	{
		get
		{
			if (prf != null)
			{
				return prf.Equals(algid_hmacWithSHA1);
			}
			return true;
		}
	}

	public AlgorithmIdentifier Prf
	{
		get
		{
			if (prf == null)
			{
				return algid_hmacWithSHA1;
			}
			return prf;
		}
	}

	public static Pbkdf2Params GetInstance(object obj)
	{
		if (obj == null || obj is Pbkdf2Params)
		{
			return (Pbkdf2Params)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new Pbkdf2Params((Asn1Sequence)obj);
		}
		throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	public Pbkdf2Params(Asn1Sequence seq)
	{
		if (seq.Count < 2 || seq.Count > 4)
		{
			throw new ArgumentException("Wrong number of elements in sequence", "seq");
		}
		octStr = (Asn1OctetString)seq[0];
		iterationCount = (DerInteger)seq[1];
		Asn1Encodable asn1Encodable = null;
		Asn1Encodable asn1Encodable2 = null;
		if (seq.Count > 3)
		{
			asn1Encodable = seq[2];
			asn1Encodable2 = seq[3];
		}
		else if (seq.Count > 2)
		{
			if (seq[2] is DerInteger)
			{
				asn1Encodable = seq[2];
			}
			else
			{
				asn1Encodable2 = seq[2];
			}
		}
		if (asn1Encodable != null)
		{
			keyLength = (DerInteger)asn1Encodable;
		}
		if (asn1Encodable2 != null)
		{
			prf = AlgorithmIdentifier.GetInstance(asn1Encodable2);
		}
	}

	public Pbkdf2Params(byte[] salt, int iterationCount)
	{
		octStr = new DerOctetString(salt);
		this.iterationCount = new DerInteger(iterationCount);
	}

	public Pbkdf2Params(byte[] salt, int iterationCount, int keyLength)
		: this(salt, iterationCount)
	{
		this.keyLength = new DerInteger(keyLength);
	}

	public Pbkdf2Params(byte[] salt, int iterationCount, int keyLength, AlgorithmIdentifier prf)
		: this(salt, iterationCount, keyLength)
	{
		this.prf = prf;
	}

	public Pbkdf2Params(byte[] salt, int iterationCount, AlgorithmIdentifier prf)
		: this(salt, iterationCount)
	{
		this.prf = prf;
	}

	public byte[] GetSalt()
	{
		return octStr.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(octStr, iterationCount);
		asn1EncodableVector.AddOptional(keyLength);
		if (!IsDefaultPrf)
		{
			asn1EncodableVector.Add(prf);
		}
		return new DerSequence(asn1EncodableVector);
	}
}
