using System;
using System.IO;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Parameters;

public sealed class X25519PrivateKeyParameters : AsymmetricKeyParameter
{
	public static readonly int KeySize = 32;

	public static readonly int SecretSize = 32;

	private readonly byte[] data = new byte[KeySize];

	public X25519PrivateKeyParameters(SecureRandom random)
		: base(privateKey: true)
	{
		X25519.GeneratePrivateKey(random, data);
	}

	public X25519PrivateKeyParameters(byte[] buf, int off)
		: base(privateKey: true)
	{
		Array.Copy(buf, off, data, 0, KeySize);
	}

	public X25519PrivateKeyParameters(Stream input)
		: base(privateKey: true)
	{
		if (KeySize != Streams.ReadFully(input, data))
		{
			throw new EndOfStreamException("EOF encountered in middle of X25519 private key");
		}
	}

	public void Encode(byte[] buf, int off)
	{
		Array.Copy(data, 0, buf, off, KeySize);
	}

	public byte[] GetEncoded()
	{
		return Arrays.Clone(data);
	}

	public X25519PublicKeyParameters GeneratePublicKey()
	{
		byte[] array = new byte[32];
		X25519.GeneratePublicKey(data, 0, array, 0);
		return new X25519PublicKeyParameters(array, 0);
	}

	public void GenerateSecret(X25519PublicKeyParameters publicKey, byte[] buf, int off)
	{
		byte[] array = new byte[32];
		publicKey.Encode(array, 0);
		if (!X25519.CalculateAgreement(data, 0, array, 0, buf, off))
		{
			throw new InvalidOperationException("X25519 agreement failed");
		}
	}
}
