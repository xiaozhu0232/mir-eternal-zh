using System.Collections;
using Org.BouncyCastle.Asn1.Anssi;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.X9;

public class ECNamedCurveTable
{
	public static IEnumerable Names
	{
		get
		{
			IList list = Platform.CreateArrayList();
			CollectionUtilities.AddRange(list, X962NamedCurves.Names);
			CollectionUtilities.AddRange(list, SecNamedCurves.Names);
			CollectionUtilities.AddRange(list, NistNamedCurves.Names);
			CollectionUtilities.AddRange(list, TeleTrusTNamedCurves.Names);
			CollectionUtilities.AddRange(list, AnssiNamedCurves.Names);
			CollectionUtilities.AddRange(list, ECGost3410NamedCurves.Names);
			CollectionUtilities.AddRange(list, GMNamedCurves.Names);
			return list;
		}
	}

	public static X9ECParameters GetByName(string name)
	{
		X9ECParameters x9ECParameters = X962NamedCurves.GetByName(name);
		if (x9ECParameters == null)
		{
			x9ECParameters = SecNamedCurves.GetByName(name);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = NistNamedCurves.GetByName(name);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = TeleTrusTNamedCurves.GetByName(name);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = AnssiNamedCurves.GetByName(name);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = ECGost3410NamedCurves.GetByNameX9(name);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = GMNamedCurves.GetByName(name);
		}
		return x9ECParameters;
	}

	public static string GetName(DerObjectIdentifier oid)
	{
		string name = X962NamedCurves.GetName(oid);
		if (name == null)
		{
			name = SecNamedCurves.GetName(oid);
		}
		if (name == null)
		{
			name = NistNamedCurves.GetName(oid);
		}
		if (name == null)
		{
			name = TeleTrusTNamedCurves.GetName(oid);
		}
		if (name == null)
		{
			name = AnssiNamedCurves.GetName(oid);
		}
		if (name == null)
		{
			name = ECGost3410NamedCurves.GetName(oid);
		}
		if (name == null)
		{
			name = GMNamedCurves.GetName(oid);
		}
		return name;
	}

	public static DerObjectIdentifier GetOid(string name)
	{
		DerObjectIdentifier oid = X962NamedCurves.GetOid(name);
		if (oid == null)
		{
			oid = SecNamedCurves.GetOid(name);
		}
		if (oid == null)
		{
			oid = NistNamedCurves.GetOid(name);
		}
		if (oid == null)
		{
			oid = TeleTrusTNamedCurves.GetOid(name);
		}
		if (oid == null)
		{
			oid = AnssiNamedCurves.GetOid(name);
		}
		if (oid == null)
		{
			oid = ECGost3410NamedCurves.GetOid(name);
		}
		if (oid == null)
		{
			oid = GMNamedCurves.GetOid(name);
		}
		return oid;
	}

	public static X9ECParameters GetByOid(DerObjectIdentifier oid)
	{
		X9ECParameters x9ECParameters = X962NamedCurves.GetByOid(oid);
		if (x9ECParameters == null)
		{
			x9ECParameters = SecNamedCurves.GetByOid(oid);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = TeleTrusTNamedCurves.GetByOid(oid);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = AnssiNamedCurves.GetByOid(oid);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = ECGost3410NamedCurves.GetByOidX9(oid);
		}
		if (x9ECParameters == null)
		{
			x9ECParameters = GMNamedCurves.GetByOid(oid);
		}
		return x9ECParameters;
	}
}
