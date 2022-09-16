using System.Collections;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Multiplier;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.GM;

public sealed class GMNamedCurves
{
	internal class SM2P256V1Holder : X9ECParametersHolder
	{
		internal static readonly X9ECParametersHolder Instance = new SM2P256V1Holder();

		private SM2P256V1Holder()
		{
		}

		protected override X9ECParameters CreateParameters()
		{
			BigInteger q = FromHex("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF");
			BigInteger a = FromHex("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFC");
			BigInteger b = FromHex("28E9FA9E9D9F5E344D5A9E4BCF6509A7F39789F515AB8F92DDBCBD414D940E93");
			byte[] seed = null;
			BigInteger bigInteger = FromHex("FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFF7203DF6B21C6052B53BBF40939D54123");
			BigInteger one = BigInteger.One;
			ECCurve curve = ConfigureCurve(new FpCurve(q, a, b, bigInteger, one));
			X9ECPoint g = ConfigureBasepoint(curve, "0432C4AE2C1F1981195F9904466A39C9948FE30BBFF2660BE1715A4589334C74C7BC3736A2F4F6779C59BDCEE36B692153D0A9877CC62A474002DF32E52139F0A0");
			return new X9ECParameters(curve, g, bigInteger, one, seed);
		}
	}

	internal class WapiP192V1Holder : X9ECParametersHolder
	{
		internal static readonly X9ECParametersHolder Instance = new WapiP192V1Holder();

		private WapiP192V1Holder()
		{
		}

		protected override X9ECParameters CreateParameters()
		{
			BigInteger q = FromHex("BDB6F4FE3E8B1D9E0DA8C0D46F4C318CEFE4AFE3B6B8551F");
			BigInteger a = FromHex("BB8E5E8FBC115E139FE6A814FE48AAA6F0ADA1AA5DF91985");
			BigInteger b = FromHex("1854BEBDC31B21B7AEFC80AB0ECD10D5B1B3308E6DBF11C1");
			byte[] seed = null;
			BigInteger bigInteger = FromHex("BDB6F4FE3E8B1D9E0DA8C0D40FC962195DFAE76F56564677");
			BigInteger one = BigInteger.One;
			ECCurve curve = ConfigureCurve(new FpCurve(q, a, b, bigInteger, one));
			X9ECPoint g = ConfigureBasepoint(curve, "044AD5F7048DE709AD51236DE65E4D4B482C836DC6E410664002BB3A02D4AAADACAE24817A4CA3A1B014B5270432DB27D2");
			return new X9ECParameters(curve, g, bigInteger, one, seed);
		}
	}

	private static readonly IDictionary objIds;

	private static readonly IDictionary curves;

	private static readonly IDictionary names;

	public static IEnumerable Names => new EnumerableProxy(names.Values);

	private GMNamedCurves()
	{
	}

	private static X9ECPoint ConfigureBasepoint(ECCurve curve, string encoding)
	{
		X9ECPoint x9ECPoint = new X9ECPoint(curve, Hex.DecodeStrict(encoding));
		WNafUtilities.ConfigureBasepoint(x9ECPoint.Point);
		return x9ECPoint;
	}

	private static ECCurve ConfigureCurve(ECCurve curve)
	{
		return curve;
	}

	private static BigInteger FromHex(string hex)
	{
		return new BigInteger(1, Hex.DecodeStrict(hex));
	}

	private static void DefineCurve(string name, DerObjectIdentifier oid, X9ECParametersHolder holder)
	{
		objIds.Add(Platform.ToUpperInvariant(name), oid);
		names.Add(oid, name);
		curves.Add(oid, holder);
	}

	static GMNamedCurves()
	{
		objIds = Platform.CreateHashtable();
		curves = Platform.CreateHashtable();
		names = Platform.CreateHashtable();
		DefineCurve("wapip192v1", GMObjectIdentifiers.wapip192v1, WapiP192V1Holder.Instance);
		DefineCurve("sm2p256v1", GMObjectIdentifiers.sm2p256v1, SM2P256V1Holder.Instance);
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
		return ((X9ECParametersHolder)curves[oid])?.Parameters;
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
