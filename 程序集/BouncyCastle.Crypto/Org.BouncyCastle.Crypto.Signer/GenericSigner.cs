using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers;

public class GenericSigner : ISigner
{
	private readonly IAsymmetricBlockCipher engine;

	private readonly IDigest digest;

	private bool forSigning;

	public virtual string AlgorithmName => "Generic(" + engine.AlgorithmName + "/" + digest.AlgorithmName + ")";

	public GenericSigner(IAsymmetricBlockCipher engine, IDigest digest)
	{
		this.engine = engine;
		this.digest = digest;
	}

	public virtual void Init(bool forSigning, ICipherParameters parameters)
	{
		this.forSigning = forSigning;
		AsymmetricKeyParameter asymmetricKeyParameter = ((!(parameters is ParametersWithRandom)) ? ((AsymmetricKeyParameter)parameters) : ((AsymmetricKeyParameter)((ParametersWithRandom)parameters).Parameters));
		if (forSigning && !asymmetricKeyParameter.IsPrivate)
		{
			throw new InvalidKeyException("Signing requires private key.");
		}
		if (!forSigning && asymmetricKeyParameter.IsPrivate)
		{
			throw new InvalidKeyException("Verification requires public key.");
		}
		Reset();
		engine.Init(forSigning, parameters);
	}

	public virtual void Update(byte input)
	{
		digest.Update(input);
	}

	public virtual void BlockUpdate(byte[] input, int inOff, int length)
	{
		digest.BlockUpdate(input, inOff, length);
	}

	public virtual byte[] GenerateSignature()
	{
		if (!forSigning)
		{
			throw new InvalidOperationException("GenericSigner not initialised for signature generation.");
		}
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		return engine.ProcessBlock(array, 0, array.Length);
	}

	public virtual bool VerifySignature(byte[] signature)
	{
		if (forSigning)
		{
			throw new InvalidOperationException("GenericSigner not initialised for verification");
		}
		byte[] array = new byte[digest.GetDigestSize()];
		digest.DoFinal(array, 0);
		try
		{
			byte[] array2 = engine.ProcessBlock(signature, 0, signature.Length);
			if (array2.Length < array.Length)
			{
				byte[] array3 = new byte[array.Length];
				Array.Copy(array2, 0, array3, array3.Length - array2.Length, array2.Length);
				array2 = array3;
			}
			return Arrays.ConstantTimeAreEqual(array2, array);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public virtual void Reset()
	{
		digest.Reset();
	}
}
