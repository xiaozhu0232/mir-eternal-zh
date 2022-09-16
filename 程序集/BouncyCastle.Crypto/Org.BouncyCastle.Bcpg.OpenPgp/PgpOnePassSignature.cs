using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpOnePassSignature
{
	private readonly OnePassSignaturePacket sigPack;

	private readonly int signatureType;

	private ISigner sig;

	private byte lastb;

	public long KeyId => sigPack.KeyId;

	public int SignatureType => sigPack.SignatureType;

	public HashAlgorithmTag HashAlgorithm => sigPack.HashAlgorithm;

	public PublicKeyAlgorithmTag KeyAlgorithm => sigPack.KeyAlgorithm;

	private static OnePassSignaturePacket Cast(Packet packet)
	{
		if (!(packet is OnePassSignaturePacket))
		{
			throw new IOException("unexpected packet in stream: " + packet);
		}
		return (OnePassSignaturePacket)packet;
	}

	internal PgpOnePassSignature(BcpgInputStream bcpgInput)
		: this(Cast(bcpgInput.ReadPacket()))
	{
	}

	internal PgpOnePassSignature(OnePassSignaturePacket sigPack)
	{
		this.sigPack = sigPack;
		signatureType = sigPack.SignatureType;
	}

	public void InitVerify(PgpPublicKey pubKey)
	{
		lastb = 0;
		try
		{
			sig = SignerUtilities.GetSigner(PgpUtilities.GetSignatureName(sigPack.KeyAlgorithm, sigPack.HashAlgorithm));
		}
		catch (Exception exception)
		{
			throw new PgpException("can't set up signature object.", exception);
		}
		try
		{
			sig.Init(forSigning: false, pubKey.GetKey());
		}
		catch (InvalidKeyException exception2)
		{
			throw new PgpException("invalid key.", exception2);
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

	public void Update(byte[] bytes)
	{
		if (signatureType == 1)
		{
			for (int i = 0; i != bytes.Length; i++)
			{
				doCanonicalUpdateByte(bytes[i]);
			}
		}
		else
		{
			sig.BlockUpdate(bytes, 0, bytes.Length);
		}
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

	public bool Verify(PgpSignature pgpSig)
	{
		byte[] signatureTrailer = pgpSig.GetSignatureTrailer();
		sig.BlockUpdate(signatureTrailer, 0, signatureTrailer.Length);
		return sig.VerifySignature(pgpSig.GetSignature());
	}

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public void Encode(Stream outStr)
	{
		BcpgOutputStream.Wrap(outStr).WritePacket(sigPack);
	}
}
