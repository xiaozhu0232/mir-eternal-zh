using System;
using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.X509;

public class X509Extensions : Asn1Encodable
{
	public static readonly DerObjectIdentifier SubjectDirectoryAttributes = new DerObjectIdentifier("2.5.29.9");

	public static readonly DerObjectIdentifier SubjectKeyIdentifier = new DerObjectIdentifier("2.5.29.14");

	public static readonly DerObjectIdentifier KeyUsage = new DerObjectIdentifier("2.5.29.15");

	public static readonly DerObjectIdentifier PrivateKeyUsagePeriod = new DerObjectIdentifier("2.5.29.16");

	public static readonly DerObjectIdentifier SubjectAlternativeName = new DerObjectIdentifier("2.5.29.17");

	public static readonly DerObjectIdentifier IssuerAlternativeName = new DerObjectIdentifier("2.5.29.18");

	public static readonly DerObjectIdentifier BasicConstraints = new DerObjectIdentifier("2.5.29.19");

	public static readonly DerObjectIdentifier CrlNumber = new DerObjectIdentifier("2.5.29.20");

	public static readonly DerObjectIdentifier ReasonCode = new DerObjectIdentifier("2.5.29.21");

	public static readonly DerObjectIdentifier InstructionCode = new DerObjectIdentifier("2.5.29.23");

	public static readonly DerObjectIdentifier InvalidityDate = new DerObjectIdentifier("2.5.29.24");

	public static readonly DerObjectIdentifier DeltaCrlIndicator = new DerObjectIdentifier("2.5.29.27");

	public static readonly DerObjectIdentifier IssuingDistributionPoint = new DerObjectIdentifier("2.5.29.28");

	public static readonly DerObjectIdentifier CertificateIssuer = new DerObjectIdentifier("2.5.29.29");

	public static readonly DerObjectIdentifier NameConstraints = new DerObjectIdentifier("2.5.29.30");

	public static readonly DerObjectIdentifier CrlDistributionPoints = new DerObjectIdentifier("2.5.29.31");

	public static readonly DerObjectIdentifier CertificatePolicies = new DerObjectIdentifier("2.5.29.32");

	public static readonly DerObjectIdentifier PolicyMappings = new DerObjectIdentifier("2.5.29.33");

	public static readonly DerObjectIdentifier AuthorityKeyIdentifier = new DerObjectIdentifier("2.5.29.35");

	public static readonly DerObjectIdentifier PolicyConstraints = new DerObjectIdentifier("2.5.29.36");

	public static readonly DerObjectIdentifier ExtendedKeyUsage = new DerObjectIdentifier("2.5.29.37");

	public static readonly DerObjectIdentifier FreshestCrl = new DerObjectIdentifier("2.5.29.46");

	public static readonly DerObjectIdentifier InhibitAnyPolicy = new DerObjectIdentifier("2.5.29.54");

	public static readonly DerObjectIdentifier AuthorityInfoAccess = new DerObjectIdentifier("1.3.6.1.5.5.7.1.1");

	public static readonly DerObjectIdentifier SubjectInfoAccess = new DerObjectIdentifier("1.3.6.1.5.5.7.1.11");

	public static readonly DerObjectIdentifier LogoType = new DerObjectIdentifier("1.3.6.1.5.5.7.1.12");

	public static readonly DerObjectIdentifier BiometricInfo = new DerObjectIdentifier("1.3.6.1.5.5.7.1.2");

	public static readonly DerObjectIdentifier QCStatements = new DerObjectIdentifier("1.3.6.1.5.5.7.1.3");

	public static readonly DerObjectIdentifier AuditIdentity = new DerObjectIdentifier("1.3.6.1.5.5.7.1.4");

	public static readonly DerObjectIdentifier NoRevAvail = new DerObjectIdentifier("2.5.29.56");

	public static readonly DerObjectIdentifier TargetInformation = new DerObjectIdentifier("2.5.29.55");

	public static readonly DerObjectIdentifier ExpiredCertsOnCrl = new DerObjectIdentifier("2.5.29.60");

	private readonly IDictionary extensions = Platform.CreateHashtable();

	private readonly IList ordering;

	public IEnumerable ExtensionOids => new EnumerableProxy(ordering);

	public static X509Extension GetExtension(X509Extensions extensions, DerObjectIdentifier oid)
	{
		return extensions?.GetExtension(oid);
	}

	public static Asn1Encodable GetExtensionParsedValue(X509Extensions extensions, DerObjectIdentifier oid)
	{
		return extensions?.GetExtensionParsedValue(oid);
	}

	public static X509Extensions GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static X509Extensions GetInstance(object obj)
	{
		if (obj == null || obj is X509Extensions)
		{
			return (X509Extensions)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new X509Extensions((Asn1Sequence)obj);
		}
		if (obj is Asn1TaggedObject)
		{
			return GetInstance(((Asn1TaggedObject)obj).GetObject());
		}
		throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
	}

	private X509Extensions(Asn1Sequence seq)
	{
		ordering = Platform.CreateArrayList();
		foreach (Asn1Encodable item in seq)
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(item.ToAsn1Object());
			if (instance.Count < 2 || instance.Count > 3)
			{
				throw new ArgumentException("Bad sequence size: " + instance.Count);
			}
			DerObjectIdentifier instance2 = DerObjectIdentifier.GetInstance(instance[0].ToAsn1Object());
			bool critical = instance.Count == 3 && DerBoolean.GetInstance(instance[1].ToAsn1Object()).IsTrue;
			Asn1OctetString instance3 = Asn1OctetString.GetInstance(instance[instance.Count - 1].ToAsn1Object());
			if (extensions.Contains(instance2))
			{
				throw new ArgumentException("repeated extension found: " + instance2);
			}
			extensions.Add(instance2, new X509Extension(critical, instance3));
			ordering.Add(instance2);
		}
	}

	public X509Extensions(IDictionary extensions)
		: this(null, extensions)
	{
	}

	public X509Extensions(IList ordering, IDictionary extensions)
	{
		if (ordering == null)
		{
			this.ordering = Platform.CreateArrayList(extensions.Keys);
		}
		else
		{
			this.ordering = Platform.CreateArrayList(ordering);
		}
		foreach (DerObjectIdentifier item in this.ordering)
		{
			this.extensions.Add(item, (X509Extension)extensions[item]);
		}
	}

	public X509Extensions(IList oids, IList values)
	{
		ordering = Platform.CreateArrayList(oids);
		int num = 0;
		foreach (DerObjectIdentifier item in ordering)
		{
			extensions.Add(item, (X509Extension)values[num++]);
		}
	}

	[Obsolete]
	public X509Extensions(Hashtable extensions)
		: this(null, extensions)
	{
	}

	[Obsolete]
	public X509Extensions(ArrayList ordering, Hashtable extensions)
	{
		if (ordering == null)
		{
			this.ordering = Platform.CreateArrayList(extensions.Keys);
		}
		else
		{
			this.ordering = Platform.CreateArrayList(ordering);
		}
		foreach (DerObjectIdentifier item in this.ordering)
		{
			this.extensions.Add(item, (X509Extension)extensions[item]);
		}
	}

	[Obsolete]
	public X509Extensions(ArrayList oids, ArrayList values)
	{
		ordering = Platform.CreateArrayList(oids);
		int num = 0;
		foreach (DerObjectIdentifier item in ordering)
		{
			extensions.Add(item, (X509Extension)values[num++]);
		}
	}

	[Obsolete("Use ExtensionOids IEnumerable property")]
	public IEnumerator Oids()
	{
		return ExtensionOids.GetEnumerator();
	}

	public X509Extension GetExtension(DerObjectIdentifier oid)
	{
		return (X509Extension)extensions[oid];
	}

	public Asn1Encodable GetExtensionParsedValue(DerObjectIdentifier oid)
	{
		return GetExtension(oid)?.GetParsedValue();
	}

	public override Asn1Object ToAsn1Object()
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (DerObjectIdentifier item in ordering)
		{
			X509Extension x509Extension = (X509Extension)extensions[item];
			Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector(item);
			if (x509Extension.IsCritical)
			{
				asn1EncodableVector2.Add(DerBoolean.True);
			}
			asn1EncodableVector2.Add(x509Extension.Value);
			asn1EncodableVector.Add(new DerSequence(asn1EncodableVector2));
		}
		return new DerSequence(asn1EncodableVector);
	}

	public bool Equivalent(X509Extensions other)
	{
		if (extensions.Count != other.extensions.Count)
		{
			return false;
		}
		foreach (DerObjectIdentifier key in extensions.Keys)
		{
			if (!extensions[key].Equals(other.extensions[key]))
			{
				return false;
			}
		}
		return true;
	}

	public DerObjectIdentifier[] GetExtensionOids()
	{
		return ToOidArray(ordering);
	}

	public DerObjectIdentifier[] GetNonCriticalExtensionOids()
	{
		return GetExtensionOids(isCritical: false);
	}

	public DerObjectIdentifier[] GetCriticalExtensionOids()
	{
		return GetExtensionOids(isCritical: true);
	}

	private DerObjectIdentifier[] GetExtensionOids(bool isCritical)
	{
		IList list = Platform.CreateArrayList();
		foreach (DerObjectIdentifier item in ordering)
		{
			X509Extension x509Extension = (X509Extension)extensions[item];
			if (x509Extension.IsCritical == isCritical)
			{
				list.Add(item);
			}
		}
		return ToOidArray(list);
	}

	private static DerObjectIdentifier[] ToOidArray(IList oids)
	{
		DerObjectIdentifier[] array = new DerObjectIdentifier[oids.Count];
		oids.CopyTo(array, 0);
		return array;
	}
}
