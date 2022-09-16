using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class Ed25519phSigner : ISigner
{
	private readonly IDigest prehash = Ed25519.CreatePrehash();

	private readonly byte[] context;

	private bool forSigning;

	private Ed25519PrivateKeyParameters privateKey;

	private Ed25519PublicKeyParameters publicKey;

	public virtual string AlgorithmName => "Ed25519ph";

	public Ed25519phSigner(byte[] context)
	{
		this.context = Arrays.Clone(context);
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		this.forSigning = forSigning;
		if (forSigning)
		{
			privateKey = (Ed25519PrivateKeyParameters)parameters;
			publicKey = null;
		}
		else
		{
			privateKey = null;
			publicKey = (Ed25519PublicKeyParameters)parameters;
		}
		Reset();
	}

	public virtual void Update(byte b)
	{
		prehash.Update(b);
	}

	public virtual void BlockUpdate(byte[] buf, int off, int len)
	{
		prehash.BlockUpdate(buf, off, len);
	}

	public virtual byte[] GenerateSignature()
	{
		if (!forSigning || privateKey == null)
		{
			throw new InvalidOperationException("Ed25519phSigner not initialised for signature generation.");
		}
		byte[] array = new byte[Ed25519.PrehashSize];
		if (Ed25519.PrehashSize != prehash.DoFinal(array, 0))
		{
			throw new InvalidOperationException("Prehash digest failed");
		}
		byte[] array2 = new byte[Ed25519PrivateKeyParameters.SignatureSize];
		privateKey.Sign(Ed25519.Algorithm.Ed25519ph, context, array, 0, Ed25519.PrehashSize, array2, 0);
		return array2;
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		if (forSigning || publicKey == null)
		{
			throw new InvalidOperationException("Ed25519phSigner not initialised for verification");
		}
		if (Ed25519.SignatureSize != signature.Length)
		{
			prehash.Reset();
			return false;
		}
		byte[] encoded = publicKey.GetEncoded();
		return Ed25519.VerifyPrehash(signature, 0, encoded, 0, context, prehash);
	}

	public void Reset()
	{
		prehash.Reset();
	}
}
