namespace Org.BouncyCastle.Asn1.Gnu;

public abstract class GnuObjectIdentifiers
{
	public static readonly DerObjectIdentifier Gnu = new DerObjectIdentifier("1.3.6.1.4.1.11591.1");

	public static readonly DerObjectIdentifier GnuPG = new DerObjectIdentifier("1.3.6.1.4.1.11591.2");

	public static readonly DerObjectIdentifier Notation = new DerObjectIdentifier("1.3.6.1.4.1.11591.2.1");

	public static readonly DerObjectIdentifier PkaAddress = new DerObjectIdentifier("1.3.6.1.4.1.11591.2.1.1");

	public static readonly DerObjectIdentifier GnuRadar = new DerObjectIdentifier("1.3.6.1.4.1.11591.3");

	public static readonly DerObjectIdentifier DigestAlgorithm = new DerObjectIdentifier("1.3.6.1.4.1.11591.12");

	public static readonly DerObjectIdentifier Tiger192 = new DerObjectIdentifier("1.3.6.1.4.1.11591.12.2");

	public static readonly DerObjectIdentifier EncryptionAlgorithm = new DerObjectIdentifier("1.3.6.1.4.1.11591.13");

	public static readonly DerObjectIdentifier Serpent = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2");

	public static readonly DerObjectIdentifier Serpent128Ecb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.1");

	public static readonly DerObjectIdentifier Serpent128Cbc = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.2");

	public static readonly DerObjectIdentifier Serpent128Ofb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.3");

	public static readonly DerObjectIdentifier Serpent128Cfb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.4");

	public static readonly DerObjectIdentifier Serpent192Ecb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.21");

	public static readonly DerObjectIdentifier Serpent192Cbc = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.22");

	public static readonly DerObjectIdentifier Serpent192Ofb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.23");

	public static readonly DerObjectIdentifier Serpent192Cfb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.24");

	public static readonly DerObjectIdentifier Serpent256Ecb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.41");

	public static readonly DerObjectIdentifier Serpent256Cbc = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.42");

	public static readonly DerObjectIdentifier Serpent256Ofb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.43");

	public static readonly DerObjectIdentifier Serpent256Cfb = new DerObjectIdentifier("1.3.6.1.4.1.11591.13.2.44");

	public static readonly DerObjectIdentifier Crc = new DerObjectIdentifier("1.3.6.1.4.1.11591.14");

	public static readonly DerObjectIdentifier Crc32 = new DerObjectIdentifier("1.3.6.1.4.1.11591.14.1");

	public static readonly DerObjectIdentifier EllipticCurve = new DerObjectIdentifier("1.3.6.1.4.1.11591.15");

	public static readonly DerObjectIdentifier Ed25519 = EllipticCurve.Branch("1");
}
