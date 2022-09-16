using System;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpPublicKeyEncryptedData : PgpEncryptedData
{
	private PublicKeyEncSessionPacket keyData;

	public long KeyId => keyData.KeyId;

	internal PgpPublicKeyEncryptedData(PublicKeyEncSessionPacket keyData, InputStreamPacket encData)
		: base(encData)
	{
		this.keyData = keyData;
	}

	private static IBufferedCipher GetKeyCipher(PublicKeyAlgorithmTag algorithm)
	{
		try
		{
			switch (algorithm)
			{
			case PublicKeyAlgorithmTag.RsaGeneral:
			case PublicKeyAlgorithmTag.RsaEncrypt:
				return CipherUtilities.GetCipher("RSA//PKCS1Padding");
			case PublicKeyAlgorithmTag.ElGamalEncrypt:
			case PublicKeyAlgorithmTag.ElGamalGeneral:
				return CipherUtilities.GetCipher("ElGamal/ECB/PKCS1Padding");
			default:
				throw new PgpException("unknown asymmetric algorithm: " + algorithm);
			}
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception creating cipher", exception);
		}
	}

	private bool ConfirmCheckSum(byte[] sessionInfo)
	{
		int num = 0;
		for (int i = 1; i != sessionInfo.Length - 2; i++)
		{
			num += sessionInfo[i] & 0xFF;
		}
		if (sessionInfo[sessionInfo.Length - 2] == (byte)(num >> 8))
		{
			return sessionInfo[sessionInfo.Length - 1] == (byte)num;
		}
		return false;
	}

	public SymmetricKeyAlgorithmTag GetSymmetricAlgorithm(PgpPrivateKey privKey)
	{
		byte[] array = RecoverSessionData(privKey);
		return (SymmetricKeyAlgorithmTag)array[0];
	}

	public Stream GetDataStream(PgpPrivateKey privKey)
	{
		byte[] array = RecoverSessionData(privKey);
		if (!ConfirmCheckSum(array))
		{
			throw new PgpKeyValidationException("key checksum failed");
		}
		SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = (SymmetricKeyAlgorithmTag)array[0];
		if (symmetricKeyAlgorithmTag == SymmetricKeyAlgorithmTag.Null)
		{
			return encData.GetInputStream();
		}
		string symmetricCipherName = PgpUtilities.GetSymmetricCipherName(symmetricKeyAlgorithmTag);
		string text = symmetricCipherName;
		IBufferedCipher cipher;
		try
		{
			text = ((!(encData is SymmetricEncIntegrityPacket)) ? (text + "/OpenPGPCFB/NoPadding") : (text + "/CFB/NoPadding"));
			cipher = CipherUtilities.GetCipher(text);
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("exception creating cipher", exception);
		}
		try
		{
			KeyParameter parameters = ParameterUtilities.CreateKeyParameter(symmetricCipherName, array, 1, array.Length - 3);
			byte[] array2 = new byte[cipher.GetBlockSize()];
			cipher.Init(forEncryption: false, new ParametersWithIV(parameters, array2));
			encStream = BcpgInputStream.Wrap(new CipherStream(encData.GetInputStream(), cipher, null));
			if (encData is SymmetricEncIntegrityPacket)
			{
				truncStream = new TruncatedStream(encStream);
				string digestName = PgpUtilities.GetDigestName(HashAlgorithmTag.Sha1);
				IDigest digest = DigestUtilities.GetDigest(digestName);
				encStream = new DigestStream(truncStream, digest, null);
			}
			if (Streams.ReadFully(encStream, array2, 0, array2.Length) < array2.Length)
			{
				throw new EndOfStreamException("unexpected end of stream.");
			}
			int num = encStream.ReadByte();
			int num2 = encStream.ReadByte();
			if (num < 0 || num2 < 0)
			{
				throw new EndOfStreamException("unexpected end of stream.");
			}
			return encStream;
		}
		catch (PgpException ex2)
		{
			throw ex2;
		}
		catch (Exception exception2)
		{
			throw new PgpException("Exception starting decryption", exception2);
		}
	}

	private byte[] RecoverSessionData(PgpPrivateKey privKey)
	{
		byte[][] encSessionKey = keyData.GetEncSessionKey();
		if (keyData.Algorithm == PublicKeyAlgorithmTag.EC)
		{
			ECDHPublicBcpgKey eCDHPublicBcpgKey = (ECDHPublicBcpgKey)privKey.PublicKeyPacket.Key;
			X9ECParameters x9ECParameters = ECKeyPairGenerator.FindECCurveByOid(eCDHPublicBcpgKey.CurveOid);
			byte[] array = encSessionKey[0];
			int num = (((array[0] & 0xFF) << 8) + (array[1] & 0xFF) + 7) / 8;
			if (2 + num + 1 > array.Length)
			{
				throw new PgpException("encoded length out of range");
			}
			byte[] array2 = new byte[num];
			Array.Copy(array, 2, array2, 0, num);
			int num2 = array[num + 2];
			if (2 + num + 1 + num2 > array.Length)
			{
				throw new PgpException("encoded length out of range");
			}
			byte[] array3 = new byte[num2];
			Array.Copy(array, 2 + num + 1, array3, 0, array3.Length);
			ECPoint eCPoint = x9ECParameters.Curve.DecodePoint(array2);
			ECPrivateKeyParameters eCPrivateKeyParameters = (ECPrivateKeyParameters)privKey.Key;
			ECPoint s = eCPoint.Multiply(eCPrivateKeyParameters.D).Normalize();
			KeyParameter parameters = new KeyParameter(Rfc6637Utilities.CreateKey(privKey.PublicKeyPacket, s));
			IWrapper wrapper = PgpUtilities.CreateWrapper(eCDHPublicBcpgKey.SymmetricKeyAlgorithm);
			wrapper.Init(forWrapping: false, parameters);
			return PgpPad.UnpadSessionData(wrapper.Unwrap(array3, 0, array3.Length));
		}
		IBufferedCipher keyCipher = GetKeyCipher(keyData.Algorithm);
		try
		{
			keyCipher.Init(forEncryption: false, privKey.Key);
		}
		catch (InvalidKeyException exception)
		{
			throw new PgpException("error setting asymmetric cipher", exception);
		}
		if (keyData.Algorithm == PublicKeyAlgorithmTag.RsaEncrypt || keyData.Algorithm == PublicKeyAlgorithmTag.RsaGeneral)
		{
			byte[] array4 = encSessionKey[0];
			keyCipher.ProcessBytes(array4, 2, array4.Length - 2);
		}
		else
		{
			ElGamalPrivateKeyParameters elGamalPrivateKeyParameters = (ElGamalPrivateKeyParameters)privKey.Key;
			int size = (elGamalPrivateKeyParameters.Parameters.P.BitLength + 7) / 8;
			ProcessEncodedMpi(keyCipher, size, encSessionKey[0]);
			ProcessEncodedMpi(keyCipher, size, encSessionKey[1]);
		}
		try
		{
			return keyCipher.DoFinal();
		}
		catch (Exception exception2)
		{
			throw new PgpException("exception decrypting secret key", exception2);
		}
	}

	private static void ProcessEncodedMpi(IBufferedCipher cipher, int size, byte[] mpiEnc)
	{
		if (mpiEnc.Length - 2 > size)
		{
			cipher.ProcessBytes(mpiEnc, 3, mpiEnc.Length - 3);
			return;
		}
		byte[] array = new byte[size];
		Array.Copy(mpiEnc, 2, array, array.Length - (mpiEnc.Length - 2), mpiEnc.Length - 2);
		cipher.ProcessBytes(array, 0, array.Length);
	}
}
