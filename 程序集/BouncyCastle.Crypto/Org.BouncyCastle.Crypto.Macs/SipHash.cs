using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Macs;

public class SipHash : IMac
{
	protected readonly int c;

	protected readonly int d;

	protected long k0;

	protected long k1;

	protected long v0;

	protected long v1;

	protected long v2;

	protected long v3;

	protected long m = 0L;

	protected int wordPos = 0;

	protected int wordCount = 0;

	public virtual string AlgorithmName => "SipHash-" + c + "-" + d;

	public SipHash()
		: this(2, 4)
	{
	}

	public SipHash(int c, int d)
	{
		this.c = c;
		this.d = d;
	}

	public virtual int GetMacSize()
	{
		return 8;
	}

	public virtual void Init(ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter keyParameter))
		{
			throw new ArgumentException("must be an instance of KeyParameter", "parameters");
		}
		byte[] key = keyParameter.GetKey();
		if (key.Length != 16)
		{
			throw new ArgumentException("must be a 128-bit key", "parameters");
		}
		k0 = (long)Pack.LE_To_UInt64(key, 0);
		k1 = (long)Pack.LE_To_UInt64(key, 8);
		Reset();
	}

	public virtual void Update(byte input)
	{
		m = (long)(((ulong)m >> 8) | ((ulong)input << 56));
		if (++wordPos == 8)
		{
			ProcessMessageWord();
			wordPos = 0;
		}
	}

	public virtual void BlockUpdate(byte[] input, int offset, int length)
	{
		int i = 0;
		int num = length & -8;
		if (wordPos == 0)
		{
			for (; i < num; i += 8)
			{
				m = (long)Pack.LE_To_UInt64(input, offset + i);
				ProcessMessageWord();
			}
			for (; i < length; i++)
			{
				m = (long)(((ulong)m >> 8) | ((ulong)input[offset + i] << 56));
			}
			wordPos = length - num;
			return;
		}
		int num2 = wordPos << 3;
		for (; i < num; i += 8)
		{
			ulong num3 = Pack.LE_To_UInt64(input, offset + i);
			m = (long)((num3 << num2) | ((ulong)m >> -num2));
			ProcessMessageWord();
			m = (long)num3;
		}
		for (; i < length; i++)
		{
			m = (long)(((ulong)m >> 8) | ((ulong)input[offset + i] << 56));
			if (++wordPos == 8)
			{
				ProcessMessageWord();
				wordPos = 0;
			}
		}
	}

	public virtual long DoFinal()
	{
		m = (long)((ulong)m >> (7 - wordPos << 3));
		m = (long)((ulong)m >> 8);
		m |= (long)((wordCount << 3) + wordPos) << 56;
		ProcessMessageWord();
		v2 ^= 255L;
		ApplySipRounds(d);
		long result = v0 ^ v1 ^ v2 ^ v3;
		Reset();
		return result;
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		long n = DoFinal();
		Pack.UInt64_To_LE((ulong)n, output, outOff);
		return 8;
	}

	public virtual void Reset()
	{
		v0 = k0 ^ 0x736F6D6570736575L;
		v1 = k1 ^ 0x646F72616E646F6DL;
		v2 = k0 ^ 0x6C7967656E657261L;
		v3 = k1 ^ 0x7465646279746573L;
		m = 0L;
		wordPos = 0;
		wordCount = 0;
	}

	protected virtual void ProcessMessageWord()
	{
		wordCount++;
		v3 ^= m;
		ApplySipRounds(c);
		v0 ^= m;
	}

	protected virtual void ApplySipRounds(int n)
	{
		long num = v0;
		long num2 = v1;
		long num3 = v2;
		long num4 = v3;
		for (int i = 0; i < n; i++)
		{
			num += num2;
			num3 += num4;
			num2 = RotateLeft(num2, 13);
			num4 = RotateLeft(num4, 16);
			num2 ^= num;
			num4 ^= num3;
			num = RotateLeft(num, 32);
			num3 += num2;
			num += num4;
			num2 = RotateLeft(num2, 17);
			num4 = RotateLeft(num4, 21);
			num2 ^= num3;
			num4 ^= num;
			num3 = RotateLeft(num3, 32);
		}
		v0 = num;
		v1 = num2;
		v2 = num3;
		v3 = num4;
	}

	protected static long RotateLeft(long x, int n)
	{
		return (x << n) | (long)((ulong)x >> -n);
	}
}
