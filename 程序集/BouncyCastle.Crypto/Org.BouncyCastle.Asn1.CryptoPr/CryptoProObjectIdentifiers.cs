namespace Org.BouncyCastle.Asn1.CryptoPro;

public abstract class CryptoProObjectIdentifiers
{
	public const string GostID = "1.2.643.2.2";

	public static readonly DerObjectIdentifier GostR3411 = new DerObjectIdentifier("1.2.643.2.2.9");

	public static readonly DerObjectIdentifier GostR3411Hmac = new DerObjectIdentifier("1.2.643.2.2.10");

	public static readonly DerObjectIdentifier GostR28147Cbc = new DerObjectIdentifier("1.2.643.2.2.21");

	public static readonly DerObjectIdentifier ID_Gost28147_89_CryptoPro_A_ParamSet = new DerObjectIdentifier("1.2.643.2.2.31.1");

	public static readonly DerObjectIdentifier GostR3410x94 = new DerObjectIdentifier("1.2.643.2.2.20");

	public static readonly DerObjectIdentifier GostR3410x2001 = new DerObjectIdentifier("1.2.643.2.2.19");

	public static readonly DerObjectIdentifier GostR3411x94WithGostR3410x94 = new DerObjectIdentifier("1.2.643.2.2.4");

	public static readonly DerObjectIdentifier GostR3411x94WithGostR3410x2001 = new DerObjectIdentifier("1.2.643.2.2.3");

	public static readonly DerObjectIdentifier GostR3411x94CryptoProParamSet = new DerObjectIdentifier("1.2.643.2.2.30.1");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProA = new DerObjectIdentifier("1.2.643.2.2.32.2");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProB = new DerObjectIdentifier("1.2.643.2.2.32.3");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProC = new DerObjectIdentifier("1.2.643.2.2.32.4");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProD = new DerObjectIdentifier("1.2.643.2.2.32.5");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProXchA = new DerObjectIdentifier("1.2.643.2.2.33.1");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProXchB = new DerObjectIdentifier("1.2.643.2.2.33.2");

	public static readonly DerObjectIdentifier GostR3410x94CryptoProXchC = new DerObjectIdentifier("1.2.643.2.2.33.3");

	public static readonly DerObjectIdentifier GostR3410x2001CryptoProA = new DerObjectIdentifier("1.2.643.2.2.35.1");

	public static readonly DerObjectIdentifier GostR3410x2001CryptoProB = new DerObjectIdentifier("1.2.643.2.2.35.2");

	public static readonly DerObjectIdentifier GostR3410x2001CryptoProC = new DerObjectIdentifier("1.2.643.2.2.35.3");

	public static readonly DerObjectIdentifier GostR3410x2001CryptoProXchA = new DerObjectIdentifier("1.2.643.2.2.36.0");

	public static readonly DerObjectIdentifier GostR3410x2001CryptoProXchB = new DerObjectIdentifier("1.2.643.2.2.36.1");

	public static readonly DerObjectIdentifier GostElSgDH3410Default = new DerObjectIdentifier("1.2.643.2.2.36.0");

	public static readonly DerObjectIdentifier GostElSgDH3410x1 = new DerObjectIdentifier("1.2.643.2.2.36.1");
}
