using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Utilities;

public class CipherFactory
{
	private static readonly short[] rc2Ekb = new short[256]
	{
		93, 190, 155, 139, 17, 153, 110, 77, 89, 243,
		133, 166, 63, 183, 131, 197, 228, 115, 107, 58,
		104, 90, 192, 71, 160, 100, 52, 12, 241, 208,
		82, 165, 185, 30, 150, 67, 65, 216, 212, 44,
		219, 248, 7, 119, 42, 202, 235, 239, 16, 28,
		22, 13, 56, 114, 47, 137, 193, 249, 128, 196,
		109, 174, 48, 61, 206, 32, 99, 254, 230, 26,
		199, 184, 80, 232, 36, 23, 252, 37, 111, 187,
		106, 163, 68, 83, 217, 162, 1, 171, 188, 182,
		31, 152, 238, 154, 167, 45, 79, 158, 142, 172,
		224, 198, 73, 70, 41, 244, 148, 138, 175, 225,
		91, 195, 179, 123, 87, 209, 124, 156, 237, 135,
		64, 140, 226, 203, 147, 20, 201, 97, 46, 229,
		204, 246, 94, 168, 92, 214, 117, 141, 98, 149,
		88, 105, 118, 161, 74, 181, 85, 9, 120, 51,
		130, 215, 221, 121, 245, 27, 11, 222, 38, 33,
		40, 116, 4, 151, 86, 223, 60, 240, 55, 57,
		220, 255, 6, 164, 234, 66, 8, 218, 180, 113,
		176, 207, 18, 122, 78, 250, 108, 29, 132, 0,
		200, 127, 145, 69, 170, 43, 194, 177, 143, 213,
		186, 242, 173, 25, 178, 103, 54, 247, 15, 10,
		146, 125, 227, 157, 233, 144, 62, 35, 39, 102,
		19, 236, 129, 21, 189, 34, 191, 159, 126, 169,
		81, 75, 76, 251, 2, 211, 112, 134, 49, 231,
		59, 5, 3, 84, 96, 72, 101, 24, 210, 205,
		95, 50, 136, 14, 53, 253
	};

	private CipherFactory()
	{
	}

	public static object CreateContentCipher(bool forEncryption, ICipherParameters encKey, AlgorithmIdentifier encryptionAlgID)
	{
		DerObjectIdentifier algorithm = encryptionAlgID.Algorithm;
		if (algorithm.Equals(PkcsObjectIdentifiers.rc4))
		{
			IStreamCipher streamCipher = new RC4Engine();
			streamCipher.Init(forEncryption, encKey);
			return streamCipher;
		}
		BufferedBlockCipher bufferedBlockCipher = CreateCipher(encryptionAlgID.Algorithm);
		Asn1Object asn1Object = encryptionAlgID.Parameters.ToAsn1Object();
		if (asn1Object != null && !(asn1Object is DerNull))
		{
			if (algorithm.Equals(PkcsObjectIdentifiers.DesEde3Cbc) || algorithm.Equals(AlgorithmIdentifierFactory.IDEA_CBC) || algorithm.Equals(NistObjectIdentifiers.IdAes128Cbc) || algorithm.Equals(NistObjectIdentifiers.IdAes192Cbc) || algorithm.Equals(NistObjectIdentifiers.IdAes256Cbc) || algorithm.Equals(NttObjectIdentifiers.IdCamellia128Cbc) || algorithm.Equals(NttObjectIdentifiers.IdCamellia192Cbc) || algorithm.Equals(NttObjectIdentifiers.IdCamellia256Cbc) || algorithm.Equals(KisaObjectIdentifiers.IdSeedCbc) || algorithm.Equals(OiwObjectIdentifiers.DesCbc))
			{
				bufferedBlockCipher.Init(forEncryption, new ParametersWithIV(encKey, Asn1OctetString.GetInstance(asn1Object).GetOctets()));
			}
			else if (algorithm.Equals(AlgorithmIdentifierFactory.CAST5_CBC))
			{
				Cast5CbcParameters instance = Cast5CbcParameters.GetInstance(asn1Object);
				bufferedBlockCipher.Init(forEncryption, new ParametersWithIV(encKey, instance.GetIV()));
			}
			else
			{
				if (!algorithm.Equals(PkcsObjectIdentifiers.RC2Cbc))
				{
					throw new InvalidOperationException("cannot match parameters");
				}
				RC2CbcParameter instance2 = RC2CbcParameter.GetInstance(asn1Object);
				bufferedBlockCipher.Init(forEncryption, new ParametersWithIV(new RC2Parameters(((KeyParameter)encKey).GetKey(), rc2Ekb[instance2.RC2ParameterVersion.IntValue]), instance2.GetIV()));
			}
		}
		else if (algorithm.Equals(PkcsObjectIdentifiers.DesEde3Cbc) || algorithm.Equals(AlgorithmIdentifierFactory.IDEA_CBC) || algorithm.Equals(AlgorithmIdentifierFactory.CAST5_CBC))
		{
			bufferedBlockCipher.Init(forEncryption, new ParametersWithIV(encKey, new byte[8]));
		}
		else
		{
			bufferedBlockCipher.Init(forEncryption, encKey);
		}
		return bufferedBlockCipher;
	}

	private static BufferedBlockCipher CreateCipher(DerObjectIdentifier algorithm)
	{
		IBlockCipher cipher;
		if (NistObjectIdentifiers.IdAes128Cbc.Equals(algorithm) || NistObjectIdentifiers.IdAes192Cbc.Equals(algorithm) || NistObjectIdentifiers.IdAes256Cbc.Equals(algorithm))
		{
			cipher = new CbcBlockCipher(new AesEngine());
		}
		else if (PkcsObjectIdentifiers.DesEde3Cbc.Equals(algorithm))
		{
			cipher = new CbcBlockCipher(new DesEdeEngine());
		}
		else if (OiwObjectIdentifiers.DesCbc.Equals(algorithm))
		{
			cipher = new CbcBlockCipher(new DesEngine());
		}
		else if (PkcsObjectIdentifiers.RC2Cbc.Equals(algorithm))
		{
			cipher = new CbcBlockCipher(new RC2Engine());
		}
		else
		{
			if (!MiscObjectIdentifiers.cast5CBC.Equals(algorithm))
			{
				throw new InvalidOperationException("cannot recognise cipher: " + algorithm);
			}
			cipher = new CbcBlockCipher(new Cast5Engine());
		}
		return new PaddedBufferedBlockCipher(cipher, new Pkcs7Padding());
	}
}
