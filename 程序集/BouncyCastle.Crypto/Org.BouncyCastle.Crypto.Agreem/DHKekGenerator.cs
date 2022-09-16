using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Kdf;

public class DHKekGenerator : IDerivationFunction
{
	private readonly IDigest digest;

	private DerObjectIdentifier algorithm;

	private int keySize;

	private byte[] z;

	private byte[] partyAInfo;

	public virtual IDigest Digest => digest;

	public DHKekGenerator(IDigest digest)
	{
		this.digest = digest;
	}

	public virtual void Init(IDerivationParameters param)
	{
		DHKdfParameters dHKdfParameters = (DHKdfParameters)param;
		algorithm = dHKdfParameters.Algorithm;
		keySize = dHKdfParameters.KeySize;
		z = dHKdfParameters.GetZ();
		partyAInfo = dHKdfParameters.GetExtraInfo();
	}

	public virtual int GenerateBytes(byte[] outBytes, int outOff, int len)
	{
		if (outBytes.Length - len < outOff)
		{
			throw new DataLengthException("output buffer too small");
		}
		long num = len;
		int digestSize = digest.GetDigestSize();
		if (num > 8589934591L)
		{
			throw new ArgumentException("Output length too large");
		}
		int num2 = (int)((num + digestSize - 1) / digestSize);
		byte[] array = new byte[digest.GetDigestSize()];
		uint num3 = 1u;
		for (int i = 0; i < num2; i++)
		{
			digest.BlockUpdate(z, 0, z.Length);
			DerSequence derSequence = new DerSequence(algorithm, new DerOctetString(Pack.UInt32_To_BE(num3)));
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(derSequence);
			if (partyAInfo != null)
			{
				asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 0, new DerOctetString(partyAInfo)));
			}
			asn1EncodableVector.Add(new DerTaggedObject(explicitly: true, 2, new DerOctetString(Pack.UInt32_To_BE((uint)keySize))));
			byte[] derEncoded = new DerSequence(asn1EncodableVector).GetDerEncoded();
			digest.BlockUpdate(derEncoded, 0, derEncoded.Length);
			digest.DoFinal(array, 0);
			if (len > digestSize)
			{
				Array.Copy(array, 0, outBytes, outOff, digestSize);
				outOff += digestSize;
				len -= digestSize;
			}
			else
			{
				Array.Copy(array, 0, outBytes, outOff, len);
			}
			num3++;
		}
		digest.Reset();
		return (int)num;
	}
}
