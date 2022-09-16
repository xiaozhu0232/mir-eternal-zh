using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms;

public class RecipientID : X509CertStoreSelector
{
	private byte[] keyIdentifier;

	public byte[] KeyIdentifier
	{
		get
		{
			return Arrays.Clone(keyIdentifier);
		}
		set
		{
			keyIdentifier = Arrays.Clone(value);
		}
	}

	public override int GetHashCode()
	{
		int num = Arrays.GetHashCode(keyIdentifier) ^ Arrays.GetHashCode(base.SubjectKeyIdentifier);
		BigInteger bigInteger = base.SerialNumber;
		if (bigInteger != null)
		{
			num ^= bigInteger.GetHashCode();
		}
		X509Name x509Name = base.Issuer;
		if (x509Name != null)
		{
			num ^= x509Name.GetHashCode();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is RecipientID recipientID))
		{
			return false;
		}
		if (Arrays.AreEqual(keyIdentifier, recipientID.keyIdentifier) && Arrays.AreEqual(base.SubjectKeyIdentifier, recipientID.SubjectKeyIdentifier) && object.Equals(base.SerialNumber, recipientID.SerialNumber))
		{
			return X509CertStoreSelector.IssuersMatch(base.Issuer, recipientID.Issuer);
		}
		return false;
	}
}
