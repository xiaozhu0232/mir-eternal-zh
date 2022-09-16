using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsRsaSigner : AbstractTlsSigner
{
	public override byte[] GenerateRawSignature(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter privateKey, byte[] hash)
	{
		ISigner signer = MakeSigner(algorithm, raw: true, forSigning: true, new ParametersWithRandom(privateKey, mContext.SecureRandom));
		signer.BlockUpdate(hash, 0, hash.Length);
		return signer.GenerateSignature();
	}

	public override bool VerifyRawSignature(SignatureAndHashAlgorithm algorithm, byte[] sigBytes, AsymmetricKeyParameter publicKey, byte[] hash)
	{
		ISigner signer = MakeSigner(algorithm, raw: true, forSigning: false, publicKey);
		signer.BlockUpdate(hash, 0, hash.Length);
		return signer.VerifySignature(sigBytes);
	}

	public override ISigner CreateSigner(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter privateKey)
	{
		return MakeSigner(algorithm, raw: false, forSigning: true, new ParametersWithRandom(privateKey, mContext.SecureRandom));
	}

	public override ISigner CreateVerifyer(SignatureAndHashAlgorithm algorithm, AsymmetricKeyParameter publicKey)
	{
		return MakeSigner(algorithm, raw: false, forSigning: false, publicKey);
	}

	public override bool IsValidPublicKey(AsymmetricKeyParameter publicKey)
	{
		if (publicKey is RsaKeyParameters)
		{
			return !publicKey.IsPrivate;
		}
		return false;
	}

	protected virtual ISigner MakeSigner(SignatureAndHashAlgorithm algorithm, bool raw, bool forSigning, ICipherParameters cp)
	{
		if (algorithm != null != TlsUtilities.IsTlsV12(mContext))
		{
			throw new InvalidOperationException();
		}
		if (algorithm != null && algorithm.Signature != 1)
		{
			throw new InvalidOperationException();
		}
		IDigest digest = (raw ? new NullDigest() : ((algorithm != null) ? TlsUtilities.CreateHash(algorithm.Hash) : new CombinedHash()));
		ISigner signer = ((algorithm == null) ? ((ISigner)new GenericSigner(CreateRsaImpl(), digest)) : ((ISigner)new RsaDigestSigner(digest, TlsUtilities.GetOidForHashAlgorithm(algorithm.Hash))));
		signer.Init(forSigning, cp);
		return signer;
	}

	protected virtual IAsymmetricBlockCipher CreateRsaImpl()
	{
		return new Pkcs1Encoding(new RsaBlindedEngine());
	}
}
