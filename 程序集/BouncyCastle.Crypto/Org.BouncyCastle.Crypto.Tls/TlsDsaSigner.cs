using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class TlsDsaSigner : AbstractTlsSigner
{
	protected abstract byte SignatureAlgorithm { get; }

	public override byte[] GenerateRawSignature(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter privateKey, byte[] hash)
	{
		ISigner signer = MakeSigner(algorithm, raw: true, forSigning: true, new ParametersWithRandom(privateKey, mContext.SecureRandom));
		if (algorithm == null)
		{
			signer.BlockUpdate(hash, 16, 20);
		}
		else
		{
			signer.BlockUpdate(hash, 0, hash.Length);
		}
		return signer.GenerateSignature();
	}

	public override bool VerifyRawSignature(SignatureAndHashAlgorithm algorithm, byte[] sigBytes, AsymmetricKeyParameter publicKey, byte[] hash)
	{
		ISigner signer = MakeSigner(algorithm, raw: true, forSigning: false, publicKey);
		if (algorithm == null)
		{
			signer.BlockUpdate(hash, 16, 20);
		}
		else
		{
			signer.BlockUpdate(hash, 0, hash.Length);
		}
		return signer.VerifySignature(sigBytes);
	}

	public override ISigner CreateSigner(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter privateKey)
	{
		return MakeSigner(algorithm, raw: false, forSigning: true, privateKey);
	}

	public override ISigner CreateVerifyer(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter publicKey)
	{
		return MakeSigner(algorithm, raw: false, forSigning: false, publicKey);
	}

	protected virtual ICipherParameters MakeInitParameters(bool forSigning, ICipherParameters cp)
	{
		return cp;
	}

	protected virtual ISigner MakeSigner(SignatureAndHashAlgorithm algorithm, bool raw, bool forSigning, ICipherParameters cp)
	{
		if (algorithm != null != TlsUtilities.IsTlsV12(mContext))
		{
			throw new InvalidOperationException();
		}
		if (algorithm != null && algorithm.Signature != SignatureAlgorithm)
		{
			throw new InvalidOperationException();
		}
		byte hashAlgorithm = algorithm?.Hash ?? 2;
		IDigest digest = (raw ? new NullDigest() : TlsUtilities.CreateHash(hashAlgorithm));
		ISigner signer = new DsaDigestSigner(CreateDsaImpl(hashAlgorithm), digest);
		signer.Init(forSigning, MakeInitParameters(forSigning, cp));
		return signer;
	}

	protected abstract IDsa CreateDsaImpl(byte hashAlgorithm);
}
