using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class SignatureAndHashAlgorithm
{
	protected readonly byte mHash;

	protected readonly byte mSignature;

	public virtual byte Hash => mHash;

	public virtual byte Signature => mSignature;

	public SignatureAndHashAlgorithm(byte hash, byte signature)
	{
		if (!TlsUtilities.IsValidUint8(hash))
		{
			throw new ArgumentException("should be a uint8", "hash");
		}
		if (!TlsUtilities.IsValidUint8(signature))
		{
			throw new ArgumentException("should be a uint8", "signature");
		}
		if (signature == 0)
		{
			throw new ArgumentException("MUST NOT be \"anonymous\"", "signature");
		}
		mHash = hash;
		mSignature = signature;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SignatureAndHashAlgorithm))
		{
			return false;
		}
		SignatureAndHashAlgorithm signatureAndHashAlgorithm = (SignatureAndHashAlgorithm)obj;
		if (signatureAndHashAlgorithm.Hash == Hash)
		{
			return signatureAndHashAlgorithm.Signature == Signature;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Hash << 16) | Signature;
	}

	public virtual void Encode(Stream output)
	{
		TlsUtilities.WriteUint8(Hash, output);
		TlsUtilities.WriteUint8(Signature, output);
	}

	public static SignatureAndHashAlgorithm Parse(Stream input)
	{
		byte hash = TlsUtilities.ReadUint8(input);
		byte signature = TlsUtilities.ReadUint8(input);
		return new SignatureAndHashAlgorithm(hash, signature);
	}
}
