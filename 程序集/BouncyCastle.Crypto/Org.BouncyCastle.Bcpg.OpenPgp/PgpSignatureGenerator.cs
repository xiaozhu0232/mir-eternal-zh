using System;
using System.IO;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSignatureGenerator
{
	private static readonly SignatureSubpacket[] EmptySignatureSubpackets = new SignatureSubpacket[0];

	private PublicKeyAlgorithmTag keyAlgorithm;

	private HashAlgorithmTag hashAlgorithm;

	private PgpPrivateKey privKey;

	private ISigner sig;

	private IDigest dig;

	private int signatureType;

	private byte lastb;

	private SignatureSubpacket[] unhashed = EmptySignatureSubpackets;

	private SignatureSubpacket[] hashed = EmptySignatureSubpackets;

	public PgpSignatureGenerator(PublicKeyAlgorithmTag keyAlgorithm, HashAlgorithmTag hashAlgorithm)
	{
		this.keyAlgorithm = keyAlgorithm;
		this.hashAlgorithm = hashAlgorithm;
		dig = DigestUtilities.GetDigest(PgpUtilities.GetDigestName(hashAlgorithm));
		sig = SignerUtilities.GetSigner(PgpUtilities.GetSignatureName(keyAlgorithm, hashAlgorithm));
	}

	public void InitSign(int sigType, PgpPrivateKey key)
	{
		InitSign(sigType, key, null);
	}

	public void InitSign(int sigType, PgpPrivateKey key, SecureRandom random)
	{
		privKey = key;
		signatureType = sigType;
		try
		{
			ICipherParameters parameters = key.Key;
			if (random != null)
			{
				parameters = new ParametersWithRandom(key.Key, random);
			}
			sig.Init(forSigning: true, parameters);
		}
		catch (InvalidKeyException exception)
		{
			throw new PgpException("invalid key.", exception);
		}
		dig.Reset();
		lastb = 0;
	}

	public void Update(byte b)
	{
		if (signatureType == 1)
		{
			doCanonicalUpdateByte(b);
		}
		else
		{
			doUpdateByte(b);
		}
	}

	private void doCanonicalUpdateByte(byte b)
	{
		switch (b)
		{
		case 13:
			doUpdateCRLF();
			break;
		case 10:
			if (lastb != 13)
			{
				doUpdateCRLF();
			}
			break;
		default:
			doUpdateByte(b);
			break;
		}
		lastb = b;
	}

	private void doUpdateCRLF()
	{
		doUpdateByte(13);
		doUpdateByte(10);
	}

	private void doUpdateByte(byte b)
	{
		sig.Update(b);
		dig.Update(b);
	}

	public void Update(params byte[] b)
	{
		Update(b, 0, b.Length);
	}

	public void Update(byte[] b, int off, int len)
	{
		if (signatureType == 1)
		{
			int num = off + len;
			for (int i = off; i != num; i++)
			{
				doCanonicalUpdateByte(b[i]);
			}
		}
		else
		{
			sig.BlockUpdate(b, off, len);
			dig.BlockUpdate(b, off, len);
		}
	}

	public void SetHashedSubpackets(PgpSignatureSubpacketVector hashedPackets)
	{
		hashed = ((hashedPackets == null) ? EmptySignatureSubpackets : hashedPackets.ToSubpacketArray());
	}

	public void SetUnhashedSubpackets(PgpSignatureSubpacketVector unhashedPackets)
	{
		unhashed = ((unhashedPackets == null) ? EmptySignatureSubpackets : unhashedPackets.ToSubpacketArray());
	}

	public PgpOnePassSignature GenerateOnePassVersion(bool isNested)
	{
		return new PgpOnePassSignature(new OnePassSignaturePacket(signatureType, hashAlgorithm, keyAlgorithm, privKey.KeyId, isNested));
	}

	public PgpSignature Generate()
	{
		SignatureSubpacket[] array = hashed;
		SignatureSubpacket[] array2 = unhashed;
		if (!packetPresent(hashed, SignatureSubpacketTag.CreationTime))
		{
			array = insertSubpacket(array, new SignatureCreationTime(critical: false, DateTime.UtcNow));
		}
		if (!packetPresent(hashed, SignatureSubpacketTag.IssuerKeyId) && !packetPresent(unhashed, SignatureSubpacketTag.IssuerKeyId))
		{
			array2 = insertSubpacket(array2, new IssuerKeyId(critical: false, privKey.KeyId));
		}
		int num = 4;
		byte[] array4;
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			for (int i = 0; i != array.Length; i++)
			{
				array[i].Encode(memoryStream);
			}
			byte[] array3 = memoryStream.ToArray();
			MemoryStream memoryStream2 = new MemoryStream(array3.Length + 6);
			memoryStream2.WriteByte((byte)num);
			memoryStream2.WriteByte((byte)signatureType);
			memoryStream2.WriteByte((byte)keyAlgorithm);
			memoryStream2.WriteByte((byte)hashAlgorithm);
			memoryStream2.WriteByte((byte)(array3.Length >> 8));
			memoryStream2.WriteByte((byte)array3.Length);
			memoryStream2.Write(array3, 0, array3.Length);
			array4 = memoryStream2.ToArray();
		}
		catch (IOException exception)
		{
			throw new PgpException("exception encoding hashed data.", exception);
		}
		sig.BlockUpdate(array4, 0, array4.Length);
		dig.BlockUpdate(array4, 0, array4.Length);
		array4 = new byte[6]
		{
			(byte)num,
			255,
			(byte)(array4.Length >> 24),
			(byte)(array4.Length >> 16),
			(byte)(array4.Length >> 8),
			(byte)array4.Length
		};
		sig.BlockUpdate(array4, 0, array4.Length);
		dig.BlockUpdate(array4, 0, array4.Length);
		byte[] encoding = sig.GenerateSignature();
		byte[] array5 = DigestUtilities.DoFinal(dig);
		byte[] fingerprint = new byte[2]
		{
			array5[0],
			array5[1]
		};
		MPInteger[] signature = ((keyAlgorithm == PublicKeyAlgorithmTag.RsaSign || keyAlgorithm == PublicKeyAlgorithmTag.RsaGeneral) ? PgpUtilities.RsaSigToMpi(encoding) : PgpUtilities.DsaSigToMpi(encoding));
		return new PgpSignature(new SignaturePacket(signatureType, privKey.KeyId, keyAlgorithm, hashAlgorithm, array, array2, fingerprint, signature));
	}

	public PgpSignature GenerateCertification(string id, PgpPublicKey pubKey)
	{
		UpdateWithPublicKey(pubKey);
		UpdateWithIdData(180, Strings.ToUtf8ByteArray(id));
		return Generate();
	}

	public PgpSignature GenerateCertification(PgpUserAttributeSubpacketVector userAttributes, PgpPublicKey pubKey)
	{
		UpdateWithPublicKey(pubKey);
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			UserAttributeSubpacket[] array = userAttributes.ToSubpacketArray();
			foreach (UserAttributeSubpacket userAttributeSubpacket in array)
			{
				userAttributeSubpacket.Encode(memoryStream);
			}
			UpdateWithIdData(209, memoryStream.ToArray());
		}
		catch (IOException exception)
		{
			throw new PgpException("cannot encode subpacket array", exception);
		}
		return Generate();
	}

	public PgpSignature GenerateCertification(PgpPublicKey masterKey, PgpPublicKey pubKey)
	{
		UpdateWithPublicKey(masterKey);
		UpdateWithPublicKey(pubKey);
		return Generate();
	}

	public PgpSignature GenerateCertification(PgpPublicKey pubKey)
	{
		UpdateWithPublicKey(pubKey);
		return Generate();
	}

	private byte[] GetEncodedPublicKey(PgpPublicKey pubKey)
	{
		try
		{
			return pubKey.publicPk.GetEncodedContents();
		}
		catch (IOException exception)
		{
			throw new PgpException("exception preparing key.", exception);
		}
	}

	private bool packetPresent(SignatureSubpacket[] packets, SignatureSubpacketTag type)
	{
		for (int i = 0; i != packets.Length; i++)
		{
			if (packets[i].SubpacketType == type)
			{
				return true;
			}
		}
		return false;
	}

	private SignatureSubpacket[] insertSubpacket(SignatureSubpacket[] packets, SignatureSubpacket subpacket)
	{
		SignatureSubpacket[] array = new SignatureSubpacket[packets.Length + 1];
		array[0] = subpacket;
		packets.CopyTo(array, 1);
		return array;
	}

	private void UpdateWithIdData(int header, byte[] idBytes)
	{
		Update((byte)header, (byte)(idBytes.Length >> 24), (byte)(idBytes.Length >> 16), (byte)(idBytes.Length >> 8), (byte)idBytes.Length);
		Update(idBytes);
	}

	private void UpdateWithPublicKey(PgpPublicKey key)
	{
		byte[] encodedPublicKey = GetEncodedPublicKey(key);
		Update(153, (byte)(encodedPublicKey.Length >> 8), (byte)encodedPublicKey.Length);
		Update(encodedPublicKey);
	}
}
