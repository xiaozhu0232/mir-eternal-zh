using System;

namespace Org.BouncyCastle.Crypto.Prng;

public class ReversedWindowGenerator : IRandomGenerator
{
	private readonly IRandomGenerator generator;

	private byte[] window;

	private int windowCount;

	public ReversedWindowGenerator(IRandomGenerator generator, int windowSize)
	{
		if (generator == null)
		{
			throw new ArgumentNullException("generator");
		}
		if (windowSize < 2)
		{
			throw new ArgumentException("Window size must be at least 2", "windowSize");
		}
		this.generator = generator;
		window = new byte[windowSize];
	}

	public virtual void AddSeedMaterial(byte[] seed)
	{
		lock (this)
		{
			windowCount = 0;
			generator.AddSeedMaterial(seed);
		}
	}

	public virtual void AddSeedMaterial(long seed)
	{
		lock (this)
		{
			windowCount = 0;
			generator.AddSeedMaterial(seed);
		}
	}

	public virtual void NextBytes(byte[] bytes)
	{
		doNextBytes(bytes, 0, bytes.Length);
	}

	public virtual void NextBytes(byte[] bytes, int start, int len)
	{
		doNextBytes(bytes, start, len);
	}

	private void doNextBytes(byte[] bytes, int start, int len)
	{
		lock (this)
		{
			int num = 0;
			while (num < len)
			{
				if (windowCount < 1)
				{
					generator.NextBytes(window, 0, window.Length);
					windowCount = window.Length;
				}
				bytes[start + num++] = window[--windowCount];
			}
		}
	}
}
