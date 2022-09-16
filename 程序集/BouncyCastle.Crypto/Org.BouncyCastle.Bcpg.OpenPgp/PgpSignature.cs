using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSignature
{
	public const int BinaryDocument = 0;

	public const int CanonicalTextDocument = 1;

	public const int StandAlone = 2;

	public const int DefaultCertification = 16;

	public const int NoCertification = 17;

	public const int CasualCertification = 18;

	public const int PositiveCertification = 19;

	public const int SubkeyBinding = 24;

	public const int PrimaryKeyBinding = 25;

	public const int DirectKey = 31;

	public const int KeyRevocation = 32;

	public const int SubkeyRevocation = 40;

	public const int CertificationRevocation = 48;

	public const int Timestamp = 64;

	private readonly SignaturePacket sigPck;

	private readonly int signatureType;

	private readonly TrustPacket trustPck;

	private ISigner sig;

	private byte lastb;

	public int Version => sigPck.Version;

	public PublicKeyAlgorithmTag KeyAlgorithm => sigPck.KeyAlgorithm;

	public HashAlgorithmTag HashAlgorithm => sigPck.HashAlgorithm;

	public int SignatureType => sigPck.SignatureType;

	public long KeyId => sigPck.KeyId;

	public DateTime CreationTime => DateTimeUtilities.UnixMsToDateTime(sigPck.CreationTime);

	public bool HasSubpackets
	{
		get
		{
			if (sigPck.GetHashedSubPackets() == null)
			{
				return sigPck.GetUnhashedSubPackets() != null;
			}
			return true;
		}
	}

	private static SignaturePacket Cast(Packet packet)
	{
		if (!(packet is SignaturePacket))
		{
			throw new IOException("unexpected packet in stream: " + packet);
		}
		return (SignaturePacket)packet;
	}

	internal PgpSignature(BcpgInputStream bcpgInput)
		: this(Cast(bcpgInput.ReadPacket()))
	{
	}

	internal PgpSignature(SignaturePacket sigPacket)
		: this(sigPacket, null)
	{
	}

	internal PgpSignature(SignaturePacket sigPacket, TrustPacket trustPacket)
	{
		if (sigPacket == null)
		{
			throw new ArgumentNullException("sigPacket");
		}
		sigPck = sigPacket;
		signatureType = sigPck.SignatureType;
		trustPck = trustPacket;
	}

	private void GetSig()
	{
		sig = SignerUtilities.GetSigner(PgpUtilities.GetSignatureName(sigPck.KeyAlgorithm, sigPck.HashAlgorithm));
	}

	public bool IsCertification()
	{
		return IsCertification(SignatureType);
	}

	public void InitVerify(PgpPublicKey pubKey)
	{
		lastb = 0;
		if (sig == null)
		{
			GetSig();
		}
		try
		{
			sig.Init(forSigning: false, pubKey.GetKey());
		}
		catch (InvalidKeyException exception)
		{
			throw new PgpException("invalid key.", exception);
		}
	}

	public void Update(byte b)
	{
		if (signatureType == 1)
		{
			doCanonicalUpdateByte(b);
		}
		else
		{
			sig.Update(b);
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
			sig.Update(b);
			break;
		}
		lastb = b;
	}

	private void doUpdateCRLF()
	{
		sig.Update(13);
		sig.Update(10);
	}

	public void Update(params byte[] bytes)
	{
		Update(bytes, 0, bytes.Length);
	}

	public void Update(byte[] bytes, int off, int length)
	{
		if (signatureType == 1)
		{
			int num = off + length;
			for (int i = off; i != num; i++)
			{
				doCanonicalUpdateByte(bytes[i]);
			}
		}
		else
		{
			sig.BlockUpdate(bytes, off, length);
		}
	}

	public bool Verify()
	{
		byte[] signatureTrailer = GetSignatureTrailer();
		sig.BlockUpdate(signatureTrailer, 0, signatureTrailer.Length);
		return sig.VerifySignature(GetSignature());
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

	public bool VerifyCertification(PgpUserAttributeSubpacketVector userAttributes, PgpPublicKey key)
	{
		UpdateWithPublicKey(key);
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
		Update(sigPck.GetSignatureTrailer());
		return sig.VerifySignature(GetSignature());
	}

	public bool VerifyCertification(string id, PgpPublicKey key)
	{
		UpdateWithPublicKey(key);
		UpdateWithIdData(180, Strings.ToUtf8ByteArray(id));
		Update(sigPck.GetSignatureTrailer());
		return sig.VerifySignature(GetSignature());
	}

	public bool VerifyCertification(PgpPublicKey masterKey, PgpPublicKey pubKey)
	{
		UpdateWithPublicKey(masterKey);
		UpdateWithPublicKey(pubKey);
		Update(sigPck.GetSignatureTrailer());
		return sig.VerifySignature(GetSignature());
	}

	public bool VerifyCertification(PgpPublicKey pubKey)
	{
		if (SignatureType != 32 && SignatureType != 40)
		{
			throw new InvalidOperationException("signature is not a key signature");
		}
		UpdateWithPublicKey(pubKey);
		Update(sigPck.GetSignatureTrailer());
		return sig.VerifySignature(GetSignature());
	}

	[Obsolete("Use 'CreationTime' property instead")]
	public DateTime GetCreationTime()
	{
		return CreationTime;
	}

	public byte[] GetSignatureTrailer()
	{
		return sigPck.GetSignatureTrailer();
	}

	public PgpSignatureSubpacketVector GetHashedSubPackets()
	{
		return createSubpacketVector(sigPck.GetHashedSubPackets());
	}

	public PgpSignatureSubpacketVector GetUnhashedSubPackets()
	{
		return createSubpacketVector(sigPck.GetUnhashedSubPackets());
	}

	private PgpSignatureSubpacketVector createSubpacketVector(SignatureSubpacket[] pcks)
	{
		if (pcks != null)
		{
			return new PgpSignatureSubpacketVector(pcks);
		}
		return null;
	}

	public byte[] GetSignature()
	{
		MPInteger[] signature = sigPck.GetSignature();
		if (signature != null)
		{
			if (signature.Length == 1)
			{
				return signature[0].Value.ToByteArrayUnsigned();
			}
			try
			{
				return new DerSequence(new DerInteger(signature[0].Value), new DerInteger(signature[1].Value)).GetEncoded();
			}
			catch (IOException exception)
			{
				throw new PgpException("exception encoding DSA sig.", exception);
			}
		}
		return sigPck.GetSignatureBytes();
	}

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public void Encode(Stream outStream)
	{
		BcpgOutputStream bcpgOutputStream = BcpgOutputStream.Wrap(outStream);
		bcpgOutputStream.WritePacket(sigPck);
		if (trustPck != null)
		{
			bcpgOutputStream.WritePacket(trustPck);
		}
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

	public static bool IsCertification(int signatureType)
	{
		switch (signatureType)
		{
		case 16:
		case 17:
		case 18:
		case 19:
			return true;
		default:
			return false;
		}
	}
}
