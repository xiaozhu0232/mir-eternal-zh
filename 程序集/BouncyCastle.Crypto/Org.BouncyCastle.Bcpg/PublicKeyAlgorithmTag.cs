using System;

namespace Org.BouncyCastle.Bcpg;

public enum PublicKeyAlgorithmTag
{
	RsaGeneral = 1,
	RsaEncrypt = 2,
	RsaSign = 3,
	ElGamalEncrypt = 16,
	Dsa = 17,
	[Obsolete("Use 'ECDH' instead")]
	EC = 18,
	ECDH = 18,
	ECDsa = 19,
	ElGamalGeneral = 20,
	DiffieHellman = 21,
	EdDsa = 22,
	Experimental_1 = 100,
	Experimental_2 = 101,
	Experimental_3 = 102,
	Experimental_4 = 103,
	Experimental_5 = 104,
	Experimental_6 = 105,
	Experimental_7 = 106,
	Experimental_8 = 107,
	Experimental_9 = 108,
	Experimental_10 = 109,
	Experimental_11 = 110
}
