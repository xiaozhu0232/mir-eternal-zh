using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsMac
{
	protected readonly TlsContext context;

	protected readonly byte[] secret;

	protected readonly IMac mac;

	protected readonly int digestBlockSize;

	protected readonly int digestOverhead;

	protected readonly int macLength;

	public virtual byte[] MacSecret => secret;

	public virtual int Size => macLength;

	public TlsMac(TlsContext context, IDigest digest, byte[] key, int keyOff, int keyLen)
	{
		this.context = context;
		KeyParameter keyParameter = new KeyParameter(key, keyOff, keyLen);
		secret = Arrays.Clone(keyParameter.GetKey());
		if (digest is LongDigest)
		{
			digestBlockSize = 128;
			digestOverhead = 16;
		}
		else
		{
			digestBlockSize = 64;
			digestOverhead = 8;
		}
		if (TlsUtilities.IsSsl(context))
		{
			mac = new Ssl3Mac(digest);
			if (digest.GetDigestSize() == 20)
			{
				digestOverhead = 4;
			}
		}
		else
		{
			mac = new HMac(digest);
		}
		mac.Init(keyParameter);
		macLength = mac.GetMacSize();
		if (context.SecurityParameters.truncatedHMac)
		{
			macLength = System.Math.Min(macLength, 10);
		}
	}

	public virtual byte[] CalculateMac(long seqNo, byte type, byte[] message, int offset, int length)
	{
		ProtocolVersion serverVersion = context.ServerVersion;
		bool isSsl = serverVersion.IsSsl;
		byte[] array = new byte[isSsl ? 11 : 13];
		TlsUtilities.WriteUint64(seqNo, array, 0);
		TlsUtilities.WriteUint8(type, array, 8);
		if (!isSsl)
		{
			TlsUtilities.WriteVersion(serverVersion, array, 9);
		}
		TlsUtilities.WriteUint16(length, array, array.Length - 2);
		mac.BlockUpdate(array, 0, array.Length);
		mac.BlockUpdate(message, offset, length);
		return Truncate(MacUtilities.DoFinal(mac));
	}

	public virtual byte[] CalculateMacConstantTime(long seqNo, byte type, byte[] message, int offset, int length, int fullLength, byte[] dummyData)
	{
		byte[] result = CalculateMac(seqNo, type, message, offset, length);
		int num = (TlsUtilities.IsSsl(context) ? 11 : 13);
		int num2 = GetDigestBlockCount(num + fullLength) - GetDigestBlockCount(num + length);
		while (--num2 >= 0)
		{
			mac.BlockUpdate(dummyData, 0, digestBlockSize);
		}
		mac.Update(dummyData[0]);
		mac.Reset();
		return result;
	}

	protected virtual int GetDigestBlockCount(int inputLength)
	{
		return (inputLength + digestOverhead) / digestBlockSize;
	}

	protected virtual byte[] Truncate(byte[] bs)
	{
		if (bs.Length <= macLength)
		{
			return bs;
		}
		return Arrays.CopyOf(bs, macLength);
	}
}
