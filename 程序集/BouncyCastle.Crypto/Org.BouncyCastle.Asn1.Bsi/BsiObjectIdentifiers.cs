namespace Org.BouncyCastle.Asn1.Bsi;

public abstract class BsiObjectIdentifiers
{
	public static readonly DerObjectIdentifier bsi_de = new DerObjectIdentifier("0.4.0.127.0.7");

	public static readonly DerObjectIdentifier id_ecc = bsi_de.Branch("1.1");

	public static readonly DerObjectIdentifier ecdsa_plain_signatures = id_ecc.Branch("4.1");

	public static readonly DerObjectIdentifier ecdsa_plain_SHA1 = ecdsa_plain_signatures.Branch("1");

	public static readonly DerObjectIdentifier ecdsa_plain_SHA224 = ecdsa_plain_signatures.Branch("2");

	public static readonly DerObjectIdentifier ecdsa_plain_SHA256 = ecdsa_plain_signatures.Branch("3");

	public static readonly DerObjectIdentifier ecdsa_plain_SHA384 = ecdsa_plain_signatures.Branch("4");

	public static readonly DerObjectIdentifier ecdsa_plain_SHA512 = ecdsa_plain_signatures.Branch("5");

	public static readonly DerObjectIdentifier ecdsa_plain_RIPEMD160 = ecdsa_plain_signatures.Branch("6");

	public static readonly DerObjectIdentifier algorithm = bsi_de.Branch("1");

	public static readonly DerObjectIdentifier ecka_eg = id_ecc.Branch("5.1");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf = ecka_eg.Branch("1");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_SHA1 = ecka_eg_X963kdf.Branch("1");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_SHA224 = ecka_eg_X963kdf.Branch("2");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_SHA256 = ecka_eg_X963kdf.Branch("3");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_SHA384 = ecka_eg_X963kdf.Branch("4");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_SHA512 = ecka_eg_X963kdf.Branch("5");

	public static readonly DerObjectIdentifier ecka_eg_X963kdf_RIPEMD160 = ecka_eg_X963kdf.Branch("6");

	public static readonly DerObjectIdentifier ecka_eg_SessionKDF = ecka_eg.Branch("2");

	public static readonly DerObjectIdentifier ecka_eg_SessionKDF_3DES = ecka_eg_SessionKDF.Branch("1");

	public static readonly DerObjectIdentifier ecka_eg_SessionKDF_AES128 = ecka_eg_SessionKDF.Branch("2");

	public static readonly DerObjectIdentifier ecka_eg_SessionKDF_AES192 = ecka_eg_SessionKDF.Branch("3");

	public static readonly DerObjectIdentifier ecka_eg_SessionKDF_AES256 = ecka_eg_SessionKDF.Branch("4");
}
