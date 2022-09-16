using System.Collections;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Nist;

public sealed class NistNamedCurves
{
	private static readonly IDictionary objIds;

	private static readonly IDictionary names;

	public static IEnumerable Names => new EnumerableProxy(names.Values);

	private NistNamedCurves()
	{
	}

	private static void DefineCurveAlias(string name, DerObjectIdentifier oid)
	{
		objIds.Add(Platform.ToUpperInvariant(name), oid);
		names.Add(oid, name);
	}

	static NistNamedCurves()
	{
		objIds = Platform.CreateHashtable();
		names = Platform.CreateHashtable();
		DefineCurveAlias("B-163", SecObjectIdentifiers.SecT163r2);
		DefineCurveAlias("B-233", SecObjectIdentifiers.SecT233r1);
		DefineCurveAlias("B-283", SecObjectIdentifiers.SecT283r1);
		DefineCurveAlias("B-409", SecObjectIdentifiers.SecT409r1);
		DefineCurveAlias("B-571", SecObjectIdentifiers.SecT571r1);
		DefineCurveAlias("K-163", SecObjectIdentifiers.SecT163k1);
		DefineCurveAlias("K-233", SecObjectIdentifiers.SecT233k1);
		DefineCurveAlias("K-283", SecObjectIdentifiers.SecT283k1);
		DefineCurveAlias("K-409", SecObjectIdentifiers.SecT409k1);
		DefineCurveAlias("K-571", SecObjectIdentifiers.SecT571k1);
		DefineCurveAlias("P-192", SecObjectIdentifiers.SecP192r1);
		DefineCurveAlias("P-224", SecObjectIdentifiers.SecP224r1);
		DefineCurveAlias("P-256", SecObjectIdentifiers.SecP256r1);
		DefineCurveAlias("P-384", SecObjectIdentifiers.SecP384r1);
		DefineCurveAlias("P-521", SecObjectIdentifiers.SecP521r1);
	}

	public static X9ECParameters GetByName(string name)
	{
		DerObjectIdentifier oid = GetOid(name);
		if (oid != null)
		{
			return GetByOid(oid);
		}
		return null;
	}

	public static X9ECParameters GetByOid(DerObjectIdentifier oid)
	{
		return SecNamedCurves.GetByOid(oid);
	}

	public static DerObjectIdentifier GetOid(string name)
	{
		return (DerObjectIdentifier)objIds[Platform.ToUpperInvariant(name)];
	}

	public static string GetName(DerObjectIdentifier oid)
	{
		return (string)names[oid];
	}
}
