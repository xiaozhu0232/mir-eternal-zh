using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpEncryptedDataGenerator : IStreamGenerator
{
	private abstract class EncMethod : ContainedPacket
	{
		protected byte[] sessionInfo;

		protected SymmetricKeyAlgorithmTag encAlgorithm;

		protected KeyParameter key;

		public abstract void AddSessionInfo(byte[] si, SecureRandom random);
	}

	private class PbeMethod : EncMethod
	{
		private S2k s2k;

		internal PbeMethod(SymmetricKeyAlgorithmTag encAlgorithm, S2k s2k, KeyParameter key)
		{
			base.encAlgorithm = encAlgorithm;
			this.s2k = s2k;
			base.key = key;
		}

		public KeyParameter GetKey()
		{
			return key;
		}

		public override void AddSessionInfo(byte[] si, SecureRandom random)
		{
			string symmetricCipherName = PgpUtilities.GetSymmetricCipherName(encAlgorithm);
			IBufferedCipher cipher = CipherUtilities.GetCipher(symmetricCipherName + "/CFB/NoPadding");
			byte[] iv = new byte[cipher.GetBlockSize()];
			cipher.Init(forEncryption: true, new ParametersWithRandom(new ParametersWithIV(key, iv), random));
			sessionInfo = cipher.DoFinal(si, 0, si.Length - 2);
		}

		public override void Encode(BcpgOutputStream pOut)
		{
			SymmetricKeyEncSessionPacket p = new SymmetricKeyEncSessionPacket(encAlgorithm, s2k, sessionInfo);
			pOut.WritePacket(p);
		}
	}

	private class PubMethod : EncMethod
	{
		internal PgpPublicKey pubKey;

		internal bool sessionKeyObfuscation;

		internal byte[][] data;

		internal PubMethod(PgpPublicKey pubKey, bool sessionKeyObfuscation)
		{
			this.pubKey = pubKey;
			this.sessionKeyObfuscation = sessionKeyObfuscation;
		}

		public override void AddSessionInfo(byte[] sessionInfo, SecureRandom random)
		{
			byte[] encryptedSessionInfo = EncryptSessionInfo(sessionInfo, random);
			data = ProcessSessionInfo(encryptedSessionInfo);
		}

		private byte[] EncryptSessionInfo(byte[] sessionInfo, SecureRandom random)
		{
			if (pubKey.Algorithm != PublicKeyAlgorithmTag.EC)
			{
				IBufferedCipher cipher;
				switch (pubKey.Algorithm)
				{
				case PublicKeyAlgorithmTag.RsaGeneral:
				case PublicKeyAlgorithmTag.RsaEncrypt:
					cipher = CipherUtilities.GetCipher("RSA//PKCS1Padding");
					break;
				case PublicKeyAlgorithmTag.ElGamalEncrypt:
				case PublicKeyAlgorithmTag.ElGamalGeneral:
					cipher = CipherUtilities.GetCipher("ElGamal/ECB/PKCS1Padding");
					break;
				case PublicKeyAlgorithmTag.Dsa:
					throw new PgpException("Can't use DSA for encryption.");
				case PublicKeyAlgorithmTag.ECDsa:
					throw new PgpException("Can't use ECDSA for encryption.");
				default:
					throw new PgpException("unknown asymmetric algorithm: " + pubKey.Algorithm);
				}
				AsymmetricKeyParameter parameters = pubKey.GetKey();
				cipher.Init(forEncryption: true, new ParametersWithRandom(parameters, random));
				return cipher.DoFinal(sessionInfo);
			}
			ECDHPublicBcpgKey eCDHPublicBcpgKey = (ECDHPublicBcpgKey)pubKey.PublicKeyPacket.Key;
			IAsymmetricCipherKeyPairGenerator keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("ECDH");
			keyPairGenerator.Init(new ECKeyGenerationParameters(eCDHPublicBcpgKey.CurveOid, random));
			AsymmetricCipherKeyPair asymmetricCipherKeyPair = keyPairGenerator.GenerateKeyPair();
			ECPrivateKeyParameters eCPrivateKeyParameters = (ECPrivateKeyParameters)asymmetricCipherKeyPair.Private;
			ECPublicKeyParameters eCPublicKeyParameters = (ECPublicKeyParameters)asymmetricCipherKeyPair.Public;
			ECPublicKeyParameters eCPublicKeyParameters2 = (ECPublicKeyParameters)pubKey.GetKey();
			ECPoint s = eCPublicKeyParameters2.Q.Multiply(eCPrivateKeyParameters.D).Normalize();
			KeyParameter parameters2 = new KeyParameter(Rfc6637Utilities.CreateKey(pubKey.PublicKeyPacket, s));
			IWrapper wrapper = PgpUtilities.CreateWrapper(eCDHPublicBcpgKey.SymmetricKeyAlgorithm);
			wrapper.Init(forWrapping: true, new ParametersWithRandom(parameters2, random));
			byte[] array = PgpPad.PadSessionData(sessionInfo, sessionKeyObfuscation);
			byte[] array2 = wrapper.Wrap(array, 0, array.Length);
			byte[] encoded = new MPInteger(new BigInteger(1, eCPublicKeyParameters.Q.GetEncoded(compressed: false))).GetEncoded();
			byte[] array3 = new byte[encoded.Length + 1 + array2.Length];
			Array.Copy(encoded, 0, array3, 0, encoded.Length);
			array3[encoded.Length] = (byte)array2.Length;
			Array.Copy(array2, 0, array3, encoded.Length + 1, array2.Length);
			return array3;
		}

		private byte[][] ProcessSessionInfo(byte[] encryptedSessionInfo)
		{
			switch (pubKey.Algorithm)
			{
			case PublicKeyAlgorithmTag.RsaGeneral:
			case PublicKeyAlgorithmTag.RsaEncrypt:
				return new byte[1][] { ConvertToEncodedMpi(encryptedSessionInfo) };
			case PublicKeyAlgorithmTag.ElGamalEncrypt:
			case PublicKeyAlgorithmTag.ElGamalGeneral:
			{
				int num = encryptedSessionInfo.Length / 2;
				byte[] array = new byte[num];
				byte[] array2 = new byte[num];
				Array.Copy(encryptedSessionInfo, 0, array, 0, num);
				Array.Copy(encryptedSessionInfo, num, array2, 0, num);
				return new byte[2][]
				{
					ConvertToEncodedMpi(array),
					ConvertToEncodedMpi(array2)
				};
			}
			case PublicKeyAlgorithmTag.EC:
				return new byte[1][] { encryptedSessionInfo };
			default:
				throw new PgpException("unknown asymmetric algorithm: " + pubKey.Algorithm);
			}
		}

		private byte[] ConvertToEncodedMpi(byte[] encryptedSessionInfo)
		{
			try
			{
				return new MPInteger(new BigInteger(1, encryptedSessionInfo)).GetEncoded();
			}
			catch (IOException ex)
			{
				throw new PgpException("Invalid MPI encoding: " + ex.Message, ex);
			}
		}

		public override void Encode(BcpgOutputStream pOut)
		{
			PublicKeyEncSessionPacket p = new PublicKeyEncSessionPacket(pubKey.KeyId, pubKey.Algorithm, data);
			pOut.WritePacket(p);
		}
	}

	private BcpgOutputStream pOut;

	private CipherStream cOut;

	private IBufferedCipher c;

	private bool withIntegrityPacket;

	private bool oldFormat;

	private DigestStream digestOut;

	private readonly IList methods = Platform.CreateArrayList();

	private readonly SymmetricKeyAlgorithmTag defAlgorithm;

	private readonly SecureRandom rand;

	public PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag encAlgorithm)
	{
		defAlgorithm = encAlgorithm;
		rand = new SecureRandom();
	}

	public PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag encAlgorithm, bool withIntegrityPacket)
	{
		defAlgorithm = encAlgorithm;
		this.withIntegrityPacket = withIntegrityPacket;
		rand = new SecureRandom();
	}

	public PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag encAlgorithm, SecureRandom rand)
	{
		defAlgorithm = encAlgorithm;
		this.rand = rand;
	}

	public PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag encAlgorithm, bool withIntegrityPacket, SecureRandom rand)
	{
		defAlgorithm = encAlgorithm;
		this.rand = rand;
		this.withIntegrityPacket = withIntegrityPacket;
	}

	public PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag encAlgorithm, SecureRandom rand, bool oldFormat)
	{
		defAlgorithm = encAlgorithm;
		this.rand = rand;
		this.oldFormat = oldFormat;
	}

	[Obsolete("Use version that takes an explicit s2kDigest parameter")]
	public void AddMethod(char[] passPhrase)
	{
		AddMethod(passPhrase, HashAlgorithmTag.Sha1);
	}

	public void AddMethod(char[] passPhrase, HashAlgorithmTag s2kDigest)
	{
		DoAddMethod(PgpUtilities.EncodePassPhrase(passPhrase, utf8: false), clearPassPhrase: true, s2kDigest);
	}

	public void AddMethodUtf8(char[] passPhrase, HashAlgorithmTag s2kDigest)
	{
		DoAddMethod(PgpUtilities.EncodePassPhrase(passPhrase, utf8: true), clearPassPhrase: true, s2kDigest);
	}

	public void AddMethodRaw(byte[] rawPassPhrase, HashAlgorithmTag s2kDigest)
	{
		DoAddMethod(rawPassPhrase, clearPassPhrase: false, s2kDigest);
	}

	internal void DoAddMethod(byte[] rawPassPhrase, bool clearPassPhrase, HashAlgorithmTag s2kDigest)
	{
		S2k s2k = PgpUtilities.GenerateS2k(s2kDigest, 96, rand);
		methods.Add(new PbeMethod(defAlgorithm, s2k, PgpUtilities.DoMakeKeyFromPassPhrase(defAlgorithm, s2k, rawPassPhrase, clearPassPhrase)));
	}

	public void AddMethod(PgpPublicKey key)
	{
		AddMethod(key, sessionKeyObfuscation: true);
	}

	public void AddMethod(PgpPublicKey key, bool sessionKeyObfuscation)
	{
		if (!key.IsEncryptionKey)
		{
			throw new ArgumentException("passed in key not an encryption key!");
		}
		methods.Add(new PubMethod(key, sessionKeyObfuscation));
	}

	private void AddCheckSum(byte[] sessionInfo)
	{
		int num = 0;
		for (int i = 1; i < sessionInfo.Length - 2; i++)
		{
			num += sessionInfo[i];
		}
		sessionInfo[sessionInfo.Length - 2] = (byte)(num >> 8);
		sessionInfo[sessionInfo.Length - 1] = (byte)num;
	}

	private byte[] CreateSessionInfo(SymmetricKeyAlgorithmTag algorithm, KeyParameter key)
	{
		byte[] key2 = key.GetKey();
		byte[] array = new byte[key2.Length + 3];
		array[0] = (byte)algorithm;
		key2.CopyTo(array, 1);
		AddCheckSum(array);
		return array;
	}

	private Stream Open(Stream outStr, long length, byte[] buffer)
	{
		if (cOut != null)
		{
			throw new InvalidOperationException("generator already in open state");
		}
		if (methods.Count == 0)
		{
			throw new InvalidOperationException("No encryption methods specified");
		}
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		pOut = new BcpgOutputStream(outStr);
		KeyParameter keyParameter;
		if (methods.Count == 1)
		{
			if (methods[0] is PbeMethod)
			{
				PbeMethod pbeMethod = (PbeMethod)methods[0];
				keyParameter = pbeMethod.GetKey();
			}
			else
			{
				keyParameter = PgpUtilities.MakeRandomKey(defAlgorithm, rand);
				byte[] si = CreateSessionInfo(defAlgorithm, keyParameter);
				PubMethod pubMethod = (PubMethod)methods[0];
				try
				{
					pubMethod.AddSessionInfo(si, rand);
				}
				catch (Exception exception)
				{
					throw new PgpException("exception encrypting session key", exception);
				}
			}
			pOut.WritePacket((ContainedPacket)methods[0]);
		}
		else
		{
			keyParameter = PgpUtilities.MakeRandomKey(defAlgorithm, rand);
			byte[] si2 = CreateSessionInfo(defAlgorithm, keyParameter);
			for (int i = 0; i != methods.Count; i++)
			{
				EncMethod encMethod = (EncMethod)methods[i];
				try
				{
					encMethod.AddSessionInfo(si2, rand);
				}
				catch (Exception exception2)
				{
					throw new PgpException("exception encrypting session key", exception2);
				}
				pOut.WritePacket(encMethod);
			}
		}
		string symmetricCipherName = PgpUtilities.GetSymmetricCipherName(defAlgorithm);
		if (symmetricCipherName == null)
		{
			throw new PgpException("null cipher specified");
		}
		try
		{
			symmetricCipherName = ((!withIntegrityPacket) ? (symmetricCipherName + "/OpenPGPCFB/NoPadding") : (symmetricCipherName + "/CFB/NoPadding"));
			c = CipherUtilities.GetCipher(symmetricCipherName);
			byte[] iv = new byte[c.GetBlockSize()];
			c.Init(forEncryption: true, new ParametersWithRandom(new ParametersWithIV(keyParameter, iv), rand));
			if (buffer == null)
			{
				if (withIntegrityPacket)
				{
					pOut = new BcpgOutputStream(outStr, PacketTag.SymmetricEncryptedIntegrityProtected, length + c.GetBlockSize() + 2 + 1 + 22);
					pOut.WriteByte(1);
				}
				else
				{
					pOut = new BcpgOutputStream(outStr, PacketTag.SymmetricKeyEncrypted, length + c.GetBlockSize() + 2, oldFormat);
				}
			}
			else if (withIntegrityPacket)
			{
				pOut = new BcpgOutputStream(outStr, PacketTag.SymmetricEncryptedIntegrityProtected, buffer);
				pOut.WriteByte(1);
			}
			else
			{
				pOut = new BcpgOutputStream(outStr, PacketTag.SymmetricKeyEncrypted, buffer);
			}
			int blockSize = c.GetBlockSize();
			byte[] array = new byte[blockSize + 2];
			rand.NextBytes(array, 0, blockSize);
			Array.Copy(array, array.Length - 4, array, array.Length - 2, 2);
			Stream stream = (cOut = new CipherStream(pOut, null, c));
			if (withIntegrityPacket)
			{
				string digestName = PgpUtilities.GetDigestName(HashAlgorithmTag.Sha1);
				IDigest digest = DigestUtilities.GetDigest(digestName);
				stream = (digestOut = new DigestStream(stream, null, digest));
			}
			stream.Write(array, 0, array.Length);
			return new WrappedGeneratorStream(this, stream);
		}
		catch (Exception exception3)
		{
			throw new PgpException("Exception creating cipher", exception3);
		}
	}

	public Stream Open(Stream outStr, long length)
	{
		return Open(outStr, length, null);
	}

	public Stream Open(Stream outStr, byte[] buffer)
	{
		return Open(outStr, 0L, buffer);
	}

	public void Close()
	{
		if (cOut != null)
		{
			if (digestOut != null)
			{
				BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(digestOut, PacketTag.ModificationDetectionCode, 20L);
				bcpgOutputStream.Flush();
				digestOut.Flush();
				byte[] array = DigestUtilities.DoFinal(digestOut.WriteDigest());
				cOut.Write(array, 0, array.Length);
			}
			cOut.Flush();
			try
			{
				pOut.Write(c.DoFinal());
				pOut.Finish();
			}
			catch (Exception ex)
			{
				throw new IOException(ex.Message, ex);
			}
			cOut = null;
			pOut = null;
		}
	}
}
