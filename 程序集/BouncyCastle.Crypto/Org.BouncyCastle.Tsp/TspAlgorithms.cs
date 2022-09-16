using System.Collections;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Rosstandart;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Tsp;

public abstract class TspAlgorithms
{
	public static readonly string MD5;

	public static readonly string Sha1;

	public static readonly string Sha224;

	public static readonly string Sha256;

	public static readonly string Sha384;

	public static readonly string Sha512;

	public static readonly string RipeMD128;

	public static readonly string RipeMD160;

	public static readonly string RipeMD256;

	public static readonly string Gost3411;

	public static readonly string Gost3411_2012_256;

	public static readonly string Gost3411_2012_512;

	public static readonly string SM3;

	public static readonly IList Allowed;

	static TspAlgorithms()
	{
		MD5 = PkcsObjectIdentifiers.MD5.Id;
		Sha1 = OiwObjectIdentifiers.IdSha1.Id;
		Sha224 = NistObjectIdentifiers.IdSha224.Id;
		Sha256 = NistObjectIdentifiers.IdSha256.Id;
		Sha384 = NistObjectIdentifiers.IdSha384.Id;
		Sha512 = NistObjectIdentifiers.IdSha512.Id;
		RipeMD128 = TeleTrusTObjectIdentifiers.RipeMD128.Id;
		RipeMD160 = TeleTrusTObjectIdentifiers.RipeMD160.Id;
		RipeMD256 = TeleTrusTObjectIdentifiers.RipeMD256.Id;
		Gost3411 = CryptoProObjectIdentifiers.GostR3411.Id;
		Gost3411_2012_256 = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_256.Id;
		Gost3411_2012_512 = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_512.Id;
		SM3 = GMObjectIdentifiers.sm3.Id;
		string[] array = new string[13]
		{
			Gost3411, Gost3411_2012_256, Gost3411_2012_512, MD5, RipeMD128, RipeMD160, RipeMD256, Sha1, Sha224, Sha256,
			Sha384, Sha512, SM3
		};
		Allowed = Platform.CreateArrayList();
		string[] array2 = array;
		foreach (string value in array2)
		{
			Allowed.Add(value);
		}
	}
}
