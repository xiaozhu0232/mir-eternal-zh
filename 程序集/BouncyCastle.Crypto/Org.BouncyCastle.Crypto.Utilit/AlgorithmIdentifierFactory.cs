using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Utilities;

public class AlgorithmIdentifierFactory
{
	public static readonly DerObjectIdentifier IDEA_CBC = new DerObjectIdentifier("1.3.6.1.4.1.188.7.1.1.2");

	public static readonly DerObjectIdentifier CAST5_CBC = new DerObjectIdentifier("1.2.840.113533.7.66.10");

	private static readonly short[] rc2Table = new short[256]
	{
		189, 86, 234, 242, 162, 241, 172, 42, 176, 147,
		209, 156, 27, 51, 253, 208, 48, 4, 182, 220,
		125, 223, 50, 75, 247, 203, 69, 155, 49, 187,
		33, 90, 65, 159, 225, 217, 74, 77, 158, 218,
		160, 104, 44, 195, 39, 95, 128, 54, 62, 238,
		251, 149, 26, 254, 206, 168, 52, 169, 19, 240,
		166, 63, 216, 12, 120, 36, 175, 35, 82, 193,
		103, 23, 245, 102, 144, 231, 232, 7, 184, 96,
		72, 230, 30, 83, 243, 146, 164, 114, 140, 8,
		21, 110, 134, 0, 132, 250, 244, 127, 138, 66,
		25, 246, 219, 205, 20, 141, 80, 18, 186, 60,
		6, 78, 236, 179, 53, 17, 161, 136, 142, 43,
		148, 153, 183, 113, 116, 211, 228, 191, 58, 222,
		150, 14, 188, 10, 237, 119, 252, 55, 107, 3,
		121, 137, 98, 198, 215, 192, 210, 124, 106, 139,
		34, 163, 91, 5, 93, 2, 117, 213, 97, 227,
		24, 143, 85, 81, 173, 31, 11, 94, 133, 229,
		194, 87, 99, 202, 61, 108, 180, 197, 204, 112,
		178, 145, 89, 13, 71, 32, 200, 79, 88, 224,
		1, 226, 22, 56, 196, 111, 59, 15, 101, 70,
		190, 126, 45, 123, 130, 249, 64, 181, 29, 115,
		248, 235, 38, 199, 135, 151, 37, 84, 177, 40,
		170, 152, 157, 165, 100, 109, 122, 212, 16, 129,
		68, 239, 73, 214, 174, 46, 221, 118, 92, 47,
		167, 28, 201, 9, 105, 154, 131, 207, 41, 57,
		185, 233, 76, 255, 67, 171
	};

	public static AlgorithmIdentifier GenerateEncryptionAlgID(DerObjectIdentifier encryptionOID, int keySize, SecureRandom random)
	{
		if (encryptionOID.Equals(NistObjectIdentifiers.IdAes128Cbc) || encryptionOID.Equals(NistObjectIdentifiers.IdAes192Cbc) || encryptionOID.Equals(NistObjectIdentifiers.IdAes256Cbc) || encryptionOID.Equals(NttObjectIdentifiers.IdCamellia128Cbc) || encryptionOID.Equals(NttObjectIdentifiers.IdCamellia192Cbc) || encryptionOID.Equals(NttObjectIdentifiers.IdCamellia256Cbc) || encryptionOID.Equals(KisaObjectIdentifiers.IdSeedCbc))
		{
			byte[] array = new byte[16];
			random.NextBytes(array);
			return new AlgorithmIdentifier(encryptionOID, new DerOctetString(array));
		}
		if (encryptionOID.Equals(PkcsObjectIdentifiers.DesEde3Cbc) || encryptionOID.Equals(IDEA_CBC) || encryptionOID.Equals(OiwObjectIdentifiers.DesCbc))
		{
			byte[] array2 = new byte[8];
			random.NextBytes(array2);
			return new AlgorithmIdentifier(encryptionOID, new DerOctetString(array2));
		}
		if (encryptionOID.Equals(CAST5_CBC))
		{
			byte[] array3 = new byte[8];
			random.NextBytes(array3);
			Cast5CbcParameters parameters = new Cast5CbcParameters(array3, keySize);
			return new AlgorithmIdentifier(encryptionOID, parameters);
		}
		if (encryptionOID.Equals(PkcsObjectIdentifiers.rc4))
		{
			return new AlgorithmIdentifier(encryptionOID, DerNull.Instance);
		}
		if (encryptionOID.Equals(PkcsObjectIdentifiers.RC2Cbc))
		{
			byte[] array4 = new byte[8];
			random.NextBytes(array4);
			RC2CbcParameter parameters2 = new RC2CbcParameter(rc2Table[128], array4);
			return new AlgorithmIdentifier(encryptionOID, parameters2);
		}
		throw new InvalidOperationException("unable to match algorithm");
	}
}
