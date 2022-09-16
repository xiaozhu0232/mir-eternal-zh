using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class Ed448phSigner : ISigner
{
	private readonly IXof prehash = Ed448.CreatePrehash();

	private readonly byte[] context;

	private bool forSigning;

	private Ed448PrivateKeyParameters privateKey;

	private Ed448PublicKeyParameters publicKey;

	public virtual string AlgorithmName => "Ed448ph";

	public Ed448phSigner(byte[] context)
	{
		this.context = Arrays.Clone(context);
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		this.forSigning = forSigning;
		if (forSigning)
		{
			privateKey = (Ed448PrivateKeyParameters)parameters;
			publicKey = null;
		}
		else
		{
			privateKey = null;
			publicKey = (Ed448PublicKeyParameters)parameters;
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
			throw new InvalidOperationException("Ed448phSigner not initialised for signature generation.");
		}
		byte[] array = new byte[Ed448.PrehashSize];
		if (Ed448.PrehashSize != prehash.DoFinal(array, 0, Ed448.PrehashSize))
		{
			throw new InvalidOperationException("Prehash digest failed");
		}
		byte[] array2 = new byte[Ed448PrivateKeyParameters.SignatureSize];
		privateKey.Sign(Ed448.Algorithm.Ed448ph, context, array, 0, Ed448.PrehashSize, array2, 0);
		return array2;
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		if (forSigning || publicKey == null)
		{
			throw new InvalidOperationException("Ed448phSigner not initialised for verification");
		}
		if (Ed448.SignatureSize != signature.Length)
		{
			prehash.Reset();
			return false;
		}
		byte[] encoded = publicKey.GetEncoded();
		return Ed448.VerifyPrehash(signature, 0, encoded, 0, context, prehash);
	}

	public void Reset()
	{
		prehash.Reset();
	}
}
