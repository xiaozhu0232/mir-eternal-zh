using System;
using System.IO;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Parameters;

public sealed class X448PrivateKeyParameters : AsymmetricKeyParameter
{
	public static readonly int KeySize = 56;

	public static readonly int SecretSize = 56;

	private readonly byte[] data = new byte[KeySize];

	public X448PrivateKeyParameters(SecureRandom random)
		: base(privateKey: true)
	{
		X448.GeneratePrivateKey(random, data);
	}

	public X448PrivateKeyParameters(byte[] buf, int off)
		: base(privateKey: true)
	{
		Array.Copy(buf, off, data, 0, KeySize);
	}

	public X448PrivateKeyParameters(Stream input)
		: base(privateKey: true)
	{
		if (KeySize != Streams.ReadFully(input, data))
		{
			throw new EndOfStreamException("EOF encountered in middle of X448 private key");
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

	public X448PublicKeyParameters GeneratePublicKey()
	{
		byte[] array = new byte[56];
		X448.GeneratePublicKey(data, 0, array, 0);
		return new X448PublicKeyParameters(array, 0);
	}

	public void GenerateSecret(X448PublicKeyParameters publicKey, byte[] buf, int off)
	{
		byte[] array = new byte[56];
		publicKey.Encode(array, 0);
		if (!X448.CalculateAgreement(data, 0, array, 0, buf, off))
		{
			throw new InvalidOperationException("X448 agreement failed");
		}
	}
}
