using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpV3SignatureGenerator
{
	private PublicKeyAlgorithmTag keyAlgorithm;

	private HashAlgorithmTag hashAlgorithm;

	private PgpPrivateKey privKey;

	private ISigner sig;

	private IDigest dig;

	private int signatureType;

	private byte lastb;

	public PgpV3SignatureGenerator(PublicKeyAlgorithmTag keyAlgorithm, HashAlgorithmTag hashAlgorithm)
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

	public void Update(byte[] b)
	{
		if (signatureType == 1)
		{
			for (int i = 0; i != b.Length; i++)
			{
				doCanonicalUpdateByte(b[i]);
			}
		}
		else
		{
			sig.BlockUpdate(b, 0, b.Length);
			dig.BlockUpdate(b, 0, b.Length);
		}
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

	public PgpOnePassSignature GenerateOnePassVersion(bool isNested)
	{
		return new PgpOnePassSignature(new OnePassSignaturePacket(signatureType, hashAlgorithm, keyAlgorithm, privKey.KeyId, isNested));
	}

	public PgpSignature Generate()
	{
		long num = DateTimeUtilities.CurrentUnixMs() / 1000;
		byte[] array = new byte[5]
		{
			(byte)signatureType,
			(byte)(num >> 24),
			(byte)(num >> 16),
			(byte)(num >> 8),
			(byte)num
		};
		sig.BlockUpdate(array, 0, array.Length);
		dig.BlockUpdate(array, 0, array.Length);
		byte[] encoding = sig.GenerateSignature();
		byte[] array2 = DigestUtilities.DoFinal(dig);
		byte[] fingerprint = new byte[2]
		{
			array2[0],
			array2[1]
		};
		MPInteger[] signature = ((keyAlgorithm == PublicKeyAlgorithmTag.RsaSign || keyAlgorithm == PublicKeyAlgorithmTag.RsaGeneral) ? PgpUtilities.RsaSigToMpi(encoding) : PgpUtilities.DsaSigToMpi(encoding));
		return new PgpSignature(new SignaturePacket(3, signatureType, privKey.KeyId, keyAlgorithm, hashAlgorithm, num * 1000, fingerprint, signature));
	}
}
