using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Pkcs;

public class PrivateKeyInfo : Asn1Encodable
{
	private readonly DerInteger version;

	private readonly AlgorithmIdentifier privateKeyAlgorithm;

	private readonly Asn1OctetString privateKey;

	private readonly Asn1Set attributes;

	private readonly DerBitString publicKey;

	public virtual DerInteger Version => version;

	public virtual Asn1Set Attributes => attributes;

	public virtual bool HasPublicKey => publicKey != null;

	public virtual AlgorithmIdentifier PrivateKeyAlgorithm => privateKeyAlgorithm;

	public virtual Asn1OctetString PrivateKeyData => privateKey;

	public virtual DerBitString PublicKeyData => publicKey;

	public static PrivateKeyInfo GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static PrivateKeyInfo GetInstance(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is PrivateKeyInfo)
		{
			return (PrivateKeyInfo)obj;
		}
		return new PrivateKeyInfo(Asn1Sequence.GetInstance(obj));
	}

	private static int GetVersionValue(DerInteger version)
	{
		BigInteger value = version.Value;
		if (value.CompareTo(BigInteger.Zero) < 0 || value.CompareTo(BigInteger.One) > 0)
		{
			throw new ArgumentException("invalid version for private key info", "version");
		}
		return value.IntValue;
	}

	public PrivateKeyInfo(AlgorithmIdentifier privateKeyAlgorithm, Asn1Encodable privateKey)
		: this(privateKeyAlgorithm, privateKey, null, null)
	{
	}

	public PrivateKeyInfo(AlgorithmIdentifier privateKeyAlgorithm, Asn1Encodable privateKey, Asn1Set attributes)
		: this(privateKeyAlgorithm, privateKey, attributes, null)
	{
	}

	public PrivateKeyInfo(AlgorithmIdentifier privateKeyAlgorithm, Asn1Encodable privateKey, Asn1Set attributes, byte[] publicKey)
	{
		version = new DerInteger((publicKey != null) ? BigInteger.One : BigInteger.Zero);
		this.privateKeyAlgorithm = privateKeyAlgorithm;
		this.privateKey = new DerOctetString(privateKey);
		this.attributes = attributes;
		this.publicKey = ((publicKey == null) ? null : new DerBitString(publicKey));
	}

	private PrivateKeyInfo(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		version = DerInteger.GetInstance(CollectionUtilities.RequireNext(enumerator));
		int versionValue = GetVersionValue(version);
		privateKeyAlgorithm = AlgorithmIdentifier.GetInstance(CollectionUtilities.RequireNext(enumerator));
		privateKey = Asn1OctetString.GetInstance(CollectionUtilities.RequireNext(enumerator));
		int num = -1;
		while (enumerator.MoveNext())
		{
			Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)enumerator.Current;
			int tagNo = asn1TaggedObject.TagNo;
			if (tagNo <= num)
			{
				throw new ArgumentException("invalid optional field in private key info", "seq");
			}
			num = tagNo;
			switch (tagNo)
			{
			case 0:
				attributes = Asn1Set.GetInstance(asn1TaggedObject, explicitly: false);
				break;
			case 1:
				if (versionValue < 1)
				{
					throw new ArgumentException("'publicKey' requires version v2(1) or later", "seq");
				}
				publicKey = DerBitString.GetInstance(asn1TaggedObject, isExplicit: false);
				break;
			default:
				throw new ArgumentException("unknown optional field in private key info", "seq");
			}
		}
	}

	public virtual Asn1Object ParsePrivateKey()
	{
		return Asn1Object.FromByteArray(privateKey.GetOctets());
	}

	public virtual Asn1Object ParsePublicKey()
	{
		if (publicKey != null)
		{
			return Asn1Object.FromByteArray(publicKey.GetOctets());
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(version, privateKeyAlgorithm, privateKey);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, attributes);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, publicKey);
		return new DerSequence(asn1EncodableVector);
	}
}
