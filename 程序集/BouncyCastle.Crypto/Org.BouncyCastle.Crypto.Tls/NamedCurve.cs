namespace Org.BouncyCastle.Crypto.Tls;

public abstract class NamedCurve
{
	public const int sect163k1 = 1;

	public const int sect163r1 = 2;

	public const int sect163r2 = 3;

	public const int sect193r1 = 4;

	public const int sect193r2 = 5;

	public const int sect233k1 = 6;

	public const int sect233r1 = 7;

	public const int sect239k1 = 8;

	public const int sect283k1 = 9;

	public const int sect283r1 = 10;

	public const int sect409k1 = 11;

	public const int sect409r1 = 12;

	public const int sect571k1 = 13;

	public const int sect571r1 = 14;

	public const int secp160k1 = 15;

	public const int secp160r1 = 16;

	public const int secp160r2 = 17;

	public const int secp192k1 = 18;

	public const int secp192r1 = 19;

	public const int secp224k1 = 20;

	public const int secp224r1 = 21;

	public const int secp256k1 = 22;

	public const int secp256r1 = 23;

	public const int secp384r1 = 24;

	public const int secp521r1 = 25;

	public const int brainpoolP256r1 = 26;

	public const int brainpoolP384r1 = 27;

	public const int brainpoolP512r1 = 28;

	public const int arbitrary_explicit_prime_curves = 65281;

	public const int arbitrary_explicit_char2_curves = 65282;

	public static bool IsValid(int namedCurve)
	{
		if (namedCurve < 1 || namedCurve > 28)
		{
			if (namedCurve >= 65281)
			{
				return namedCurve <= 65282;
			}
			return false;
		}
		return true;
	}

	public static bool RefersToASpecificNamedCurve(int namedCurve)
	{
		switch (namedCurve)
		{
		case 65281:
		case 65282:
			return false;
		default:
			return true;
		}
	}
}
