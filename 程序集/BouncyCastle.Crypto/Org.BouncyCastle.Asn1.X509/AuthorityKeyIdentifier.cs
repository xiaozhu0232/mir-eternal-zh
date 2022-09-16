using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.X509;

public class AuthorityKeyIdentifier : Asn1Encodable
{
	private readonly Asn1OctetString keyidentifier;

	private readonly GeneralNames certissuer;

	private readonly DerInteger certserno;

	public GeneralNames AuthorityCertIssuer => certissuer;

	public BigInteger AuthorityCertSerialNumber
	{
		get
		{
			if (certserno != null)
			{
				return certserno.Value;
			}
			return null;
		}
	}

	public static AuthorityKeyIdentifier GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static AuthorityKeyIdentifier GetInstance(object obj)
	{
		if (obj is AuthorityKeyIdentifier)
		{
			return (AuthorityKeyIdentifier)obj;
		}
		if (obj is X509Extension)
		{
			return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		if (obj == null)
		{
			return null;
		}
		return new AuthorityKeyIdentifier(Asn1Sequence.GetInstance(obj));
	}

	public static AuthorityKeyIdentifier FromExtensions(X509Extensions extensions)
	{
		return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.AuthorityKeyIdentifier));
	}

	protected internal AuthorityKeyIdentifier(Asn1Sequence seq)
	{
		foreach (Asn1Encodable item in seq)
		{
			Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(item);
			switch (instance.TagNo)
			{
			case 0:
				keyidentifier = Asn1OctetString.GetInstance(instance, isExplicit: false);
				break;
			case 1:
				certissuer = GeneralNames.GetInstance(instance, explicitly: false);
				break;
			case 2:
				certserno = DerInteger.GetInstance(instance, isExplicit: false);
				break;
			default:
				throw new ArgumentException("illegal tag");
			}
		}
	}

	public AuthorityKeyIdentifier(SubjectPublicKeyInfo spki)
		: this(spki, null, null)
	{
	}

	public AuthorityKeyIdentifier(SubjectPublicKeyInfo spki, GeneralNames name, BigInteger serialNumber)
	{
		IDigest digest = new Sha1Digest();
		byte[] array = new byte[digest.GetDigestSize()];
		byte[] bytes = spki.PublicKeyData.GetBytes();
		digest.BlockUpdate(bytes, 0, bytes.Length);
		digest.DoFinal(array, 0);
		keyidentifier = new DerOctetString(array);
		certissuer = name;
		certserno = ((serialNumber == null) ? null : new DerInteger(serialNumber));
	}

	public AuthorityKeyIdentifier(GeneralNames name, BigInteger serialNumber)
		: this((byte[])null, name, serialNumber)
	{
	}

	public AuthorityKeyIdentifier(byte[] keyIdentifier)
		: this(keyIdentifier, null, null)
	{
	}

	public AuthorityKeyIdentifier(byte[] keyIdentifier, GeneralNames name, BigInteger serialNumber)
	{
		keyidentifier = ((keyIdentifier == null) ? null : new DerOctetString(keyIdentifier));
		certissuer = name;
		certserno = ((serialNumber == null) ? null : new DerInteger(serialNumber));
	}

	public byte[] GetKeyIdentifier()
	{
		if (keyidentifier != null)
		{
			return keyidentifier.GetOctets();
		}
		return null;
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 0, keyidentifier);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 1, certissuer);
		asn1EncodableVector.AddOptionalTagged(isExplicit: false, 2, certserno);
		return new DerSequence(asn1EncodableVector);
	}

	public override string ToString()
	{
		string text = ((keyidentifier != null) ? Hex.ToHexString(keyidentifier.GetOctets()) : "null");
		return "AuthorityKeyIdentifier: KeyID(" + text + ")";
	}
}
