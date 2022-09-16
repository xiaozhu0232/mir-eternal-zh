using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Sec;

public class ECPrivateKeyStructure : Asn1Encodable
{
	private readonly Asn1Sequence seq;

	public static ECPrivateKeyStructure GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is ECPrivateKeyStructure)
		{
			return (ECPrivateKeyStructure)obj;
		}
		return new ECPrivateKeyStructure(Asn1Sequence.GetInstance(obj));
	}

	[Obsolete("Use 'GetInstance' instead")]
	public ECPrivateKeyStructure(Asn1Sequence seq)
	{
		if (seq == null)
		{
			throw new ArgumentNullException("seq");
		}
		this.seq = seq;
	}

	[Obsolete("Use constructor which takes 'orderBitLength' instead, to guarantee correct encoding")]
	public ECPrivateKeyStructure(BigInteger key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		seq = new DerSequence(new DerInteger(1), new DerOctetString(key.ToByteArrayUnsigned()));
	}

	public ECPrivateKeyStructure(int orderBitLength, BigInteger key)
		: this(orderBitLength, key, null)
	{
	}

	[Obsolete("Use constructor which takes 'orderBitLength' instead, to guarantee correct encoding")]
	public ECPrivateKeyStructure(BigInteger key, Asn1Encodable parameters)
		: this(key, null, parameters)
	{
	}

	[Obsolete("Use constructor which takes 'orderBitLength' instead, to guarantee correct encoding")]
	public ECPrivateKeyStructure(BigInteger key, DerBitString publicKey, Asn1Encodable parameters)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(1), new DerOctetString(key.ToByteArrayUnsigned()));
		if (parameters != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, parameters));
		}
		if (publicKey != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 1, publicKey));
		}
		seq = new DerSequence(asn1EncodableVector);
	}

	public ECPrivateKeyStructure(int orderBitLength, BigInteger key, Asn1Encodable parameters)
		: this(orderBitLength, key, null, parameters)
	{
	}

	public ECPrivateKeyStructure(int orderBitLength, BigInteger key, DerBitString publicKey, Asn1Encodable parameters)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (orderBitLength < key.BitLength)
		{
			throw new ArgumentException("must be >= key bitlength", "orderBitLength");
		}
		byte[] str = BigIntegers.AsUnsignedByteArray((orderBitLength + 7) / 8, key);
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(1), new DerOctetString(str));
		if (parameters != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, parameters));
		}
		if (publicKey != null)
		{
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 1, publicKey));
		}
		seq = new DerSequence(asn1EncodableVector);
	}

	public virtual BigInteger GetKey()
	{
		Asn1OctetString asn1OctetString = (Asn1OctetString)seq[1];
		return new BigInteger(1, asn1OctetString.GetOctets());
	}

	public virtual DerBitString GetPublicKey()
	{
		return (DerBitString)GetObjectInTag(1);
	}

	public virtual Asn1Object GetParameters()
	{
		return GetObjectInTag(0);
	}

	private Asn1Object GetObjectInTag(int tagNo)
	{
		foreach (Asn1Encodable item in seq)
		{
			Asn1Object asn1Object = item.ToAsn1Object();
			if (asn1Object is Asn1TaggedObject)
			{
				Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)asn1Object;
				if (asn1TaggedObject.TagNo == tagNo)
				{
					return asn1TaggedObject.GetObject();
				}
			}
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}
}
