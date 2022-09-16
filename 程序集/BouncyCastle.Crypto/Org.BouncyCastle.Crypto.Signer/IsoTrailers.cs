using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Crypto.Signers;

public class IsoTrailers
{
	public const int TRAILER_IMPLICIT = 188;

	public const int TRAILER_RIPEMD160 = 12748;

	public const int TRAILER_RIPEMD128 = 13004;

	public const int TRAILER_SHA1 = 13260;

	public const int TRAILER_SHA256 = 13516;

	public const int TRAILER_SHA512 = 13772;

	public const int TRAILER_SHA384 = 14028;

	public const int TRAILER_WHIRLPOOL = 14284;

	public const int TRAILER_SHA224 = 14540;

	public const int TRAILER_SHA512_224 = 14796;

	public const int TRAILER_SHA512_256 = 16588;

	private static readonly IDictionary trailerMap = CreateTrailerMap();

	private static IDictionary CreateTrailerMap()
	{
		IDictionary dictionary = Platform.CreateHashtable();
		dictionary.Add("RIPEMD128", 13004);
		dictionary.Add("RIPEMD160", 12748);
		dictionary.Add("SHA-1", 13260);
		dictionary.Add("SHA-224", 14540);
		dictionary.Add("SHA-256", 13516);
		dictionary.Add("SHA-384", 14028);
		dictionary.Add("SHA-512", 13772);
		dictionary.Add("SHA-512/224", 14796);
		dictionary.Add("SHA-512/256", 16588);
		dictionary.Add("Whirlpool", 14284);
		return CollectionUtilities.ReadOnly(dictionary);
	}

	public static int GetTrailer(IDigest digest)
	{
		return (int)trailerMap[digest.AlgorithmName];
	}

	public static bool NoTrailerAvailable(IDigest digest)
	{
		return !trailerMap.Contains(digest.AlgorithmName);
	}
}
