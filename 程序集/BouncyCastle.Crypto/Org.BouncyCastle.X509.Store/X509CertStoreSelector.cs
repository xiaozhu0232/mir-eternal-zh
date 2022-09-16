using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509.Store;

public class X509CertStoreSelector : IX509Selector, ICloneable
{
	private byte[] authorityKeyIdentifier;

	private int basicConstraints = -1;

	private X509Certificate certificate;

	private DateTimeObject certificateValid;

	private ISet extendedKeyUsage;

	private bool ignoreX509NameOrdering;

	private X509Name issuer;

	private bool[] keyUsage;

	private ISet policy;

	private DateTimeObject privateKeyValid;

	private BigInteger serialNumber;

	private X509Name subject;

	private byte[] subjectKeyIdentifier;

	private SubjectPublicKeyInfo subjectPublicKey;

	private DerObjectIdentifier subjectPublicKeyAlgID;

	public byte[] AuthorityKeyIdentifier
	{
		get
		{
			return Arrays.Clone(authorityKeyIdentifier);
		}
		set
		{
			authorityKeyIdentifier = Arrays.Clone(value);
		}
	}

	public int BasicConstraints
	{
		get
		{
			return basicConstraints;
		}
		set
		{
			if (value < -2)
			{
				throw new ArgumentException("value can't be less than -2", "value");
			}
			basicConstraints = value;
		}
	}

	public X509Certificate Certificate
	{
		get
		{
			return certificate;
		}
		set
		{
			certificate = value;
		}
	}

	public DateTimeObject CertificateValid
	{
		get
		{
			return certificateValid;
		}
		set
		{
			certificateValid = value;
		}
	}

	public ISet ExtendedKeyUsage
	{
		get
		{
			return CopySet(extendedKeyUsage);
		}
		set
		{
			extendedKeyUsage = CopySet(value);
		}
	}

	public bool IgnoreX509NameOrdering
	{
		get
		{
			return ignoreX509NameOrdering;
		}
		set
		{
			ignoreX509NameOrdering = value;
		}
	}

	public X509Name Issuer
	{
		get
		{
			return issuer;
		}
		set
		{
			issuer = value;
		}
	}

	[Obsolete("Avoid working with X509Name objects in string form")]
	public string IssuerAsString
	{
		get
		{
			if (issuer == null)
			{
				return null;
			}
			return issuer.ToString();
		}
	}

	public bool[] KeyUsage
	{
		get
		{
			return CopyBoolArray(keyUsage);
		}
		set
		{
			keyUsage = CopyBoolArray(value);
		}
	}

	public ISet Policy
	{
		get
		{
			return CopySet(policy);
		}
		set
		{
			policy = CopySet(value);
		}
	}

	public DateTimeObject PrivateKeyValid
	{
		get
		{
			return privateKeyValid;
		}
		set
		{
			privateKeyValid = value;
		}
	}

	public BigInteger SerialNumber
	{
		get
		{
			return serialNumber;
		}
		set
		{
			serialNumber = value;
		}
	}

	public X509Name Subject
	{
		get
		{
			return subject;
		}
		set
		{
			subject = value;
		}
	}

	[Obsolete("Avoid working with X509Name objects in string form")]
	public string SubjectAsString
	{
		get
		{
			if (subject == null)
			{
				return null;
			}
			return subject.ToString();
		}
	}

	public byte[] SubjectKeyIdentifier
	{
		get
		{
			return Arrays.Clone(subjectKeyIdentifier);
		}
		set
		{
			subjectKeyIdentifier = Arrays.Clone(value);
		}
	}

	public SubjectPublicKeyInfo SubjectPublicKey
	{
		get
		{
			return subjectPublicKey;
		}
		set
		{
			subjectPublicKey = value;
		}
	}

	public DerObjectIdentifier SubjectPublicKeyAlgID
	{
		get
		{
			return subjectPublicKeyAlgID;
		}
		set
		{
			subjectPublicKeyAlgID = value;
		}
	}

	public X509CertStoreSelector()
	{
	}

	public X509CertStoreSelector(X509CertStoreSelector o)
	{
		authorityKeyIdentifier = o.AuthorityKeyIdentifier;
		basicConstraints = o.BasicConstraints;
		certificate = o.Certificate;
		certificateValid = o.CertificateValid;
		extendedKeyUsage = o.ExtendedKeyUsage;
		ignoreX509NameOrdering = o.IgnoreX509NameOrdering;
		issuer = o.Issuer;
		keyUsage = o.KeyUsage;
		policy = o.Policy;
		privateKeyValid = o.PrivateKeyValid;
		serialNumber = o.SerialNumber;
		subject = o.Subject;
		subjectKeyIdentifier = o.SubjectKeyIdentifier;
		subjectPublicKey = o.SubjectPublicKey;
		subjectPublicKeyAlgID = o.SubjectPublicKeyAlgID;
	}

	public virtual object Clone()
	{
		return new X509CertStoreSelector(this);
	}

	public virtual bool Match(object obj)
	{
		if (!(obj is X509Certificate x509Certificate))
		{
			return false;
		}
		if (!MatchExtension(authorityKeyIdentifier, x509Certificate, X509Extensions.AuthorityKeyIdentifier))
		{
			return false;
		}
		if (basicConstraints != -1)
		{
			int num = x509Certificate.GetBasicConstraints();
			if (basicConstraints == -2)
			{
				if (num != -1)
				{
					return false;
				}
			}
			else if (num < basicConstraints)
			{
				return false;
			}
		}
		if (certificate != null && !certificate.Equals(x509Certificate))
		{
			return false;
		}
		if (certificateValid != null && !x509Certificate.IsValid(certificateValid.Value))
		{
			return false;
		}
		if (extendedKeyUsage != null)
		{
			IList list = x509Certificate.GetExtendedKeyUsage();
			if (list != null)
			{
				foreach (DerObjectIdentifier item in extendedKeyUsage)
				{
					if (!list.Contains(item.Id))
					{
						return false;
					}
				}
			}
		}
		if (issuer != null && !issuer.Equivalent(x509Certificate.IssuerDN, !ignoreX509NameOrdering))
		{
			return false;
		}
		if (keyUsage != null)
		{
			bool[] array = x509Certificate.GetKeyUsage();
			if (array != null)
			{
				for (int i = 0; i < 9; i++)
				{
					if (keyUsage[i] && !array[i])
					{
						return false;
					}
				}
			}
		}
		if (policy != null)
		{
			Asn1OctetString extensionValue = x509Certificate.GetExtensionValue(X509Extensions.CertificatePolicies);
			if (extensionValue == null)
			{
				return false;
			}
			Asn1Sequence instance = Asn1Sequence.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue));
			if (policy.Count < 1 && instance.Count < 1)
			{
				return false;
			}
			bool flag = false;
			foreach (PolicyInformation item2 in instance)
			{
				if (policy.Contains(item2.PolicyIdentifier))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (privateKeyValid != null)
		{
			Asn1OctetString extensionValue2 = x509Certificate.GetExtensionValue(X509Extensions.PrivateKeyUsagePeriod);
			if (extensionValue2 == null)
			{
				return false;
			}
			PrivateKeyUsagePeriod instance2 = PrivateKeyUsagePeriod.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue2));
			DateTime value = privateKeyValid.Value;
			DateTime dateTime = instance2.NotAfter.ToDateTime();
			DateTime dateTime2 = instance2.NotBefore.ToDateTime();
			if (value.CompareTo((object)dateTime) > 0 || value.CompareTo((object)dateTime2) < 0)
			{
				return false;
			}
		}
		if (serialNumber != null && !serialNumber.Equals(x509Certificate.SerialNumber))
		{
			return false;
		}
		if (subject != null && !subject.Equivalent(x509Certificate.SubjectDN, !ignoreX509NameOrdering))
		{
			return false;
		}
		if (!MatchExtension(subjectKeyIdentifier, x509Certificate, X509Extensions.SubjectKeyIdentifier))
		{
			return false;
		}
		if (subjectPublicKey != null && !subjectPublicKey.Equals(GetSubjectPublicKey(x509Certificate)))
		{
			return false;
		}
		if (subjectPublicKeyAlgID != null && !subjectPublicKeyAlgID.Equals(GetSubjectPublicKey(x509Certificate).AlgorithmID))
		{
			return false;
		}
		return true;
	}

	internal static bool IssuersMatch(X509Name a, X509Name b)
	{
		return a?.Equivalent(b, inOrder: true) ?? (b == null);
	}

	private static bool[] CopyBoolArray(bool[] b)
	{
		if (b != null)
		{
			return (bool[])b.Clone();
		}
		return null;
	}

	private static ISet CopySet(ISet s)
	{
		if (s != null)
		{
			return new HashSet(s);
		}
		return null;
	}

	private static SubjectPublicKeyInfo GetSubjectPublicKey(X509Certificate c)
	{
		return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(c.GetPublicKey());
	}

	private static bool MatchExtension(byte[] b, X509Certificate c, DerObjectIdentifier oid)
	{
		if (b == null)
		{
			return true;
		}
		Asn1OctetString extensionValue = c.GetExtensionValue(oid);
		if (extensionValue == null)
		{
			return false;
		}
		return Arrays.AreEqual(b, extensionValue.GetOctets());
	}
}
