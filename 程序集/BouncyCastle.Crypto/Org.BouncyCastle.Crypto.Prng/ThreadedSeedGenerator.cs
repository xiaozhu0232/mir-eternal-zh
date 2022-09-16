using System;
using System.Threading;

namespace Org.BouncyCastle.Crypto.Prng;

public class ThreadedSeedGenerator
{
	private class SeedGenerator
	{
		private volatile int counter = 0;

		private volatile bool stop = false;

		private void Run(object ignored)
		{
			while (!stop)
			{
				counter++;
			}
		}

		public byte[] GenerateSeed(int numBytes, bool fast)
		{
			ThreadPriority priority = Thread.CurrentThread.Priority;
			try
			{
				Thread.CurrentThread.Priority = ThreadPriority.Normal;
				return DoGenerateSeed(numBytes, fast);
			}
			finally
			{
				Thread.CurrentThread.Priority = priority;
			}
		}

		private byte[] DoGenerateSeed(int numBytes, bool fast)
		{
			counter = 0;
			stop = false;
			byte[] array = new byte[numBytes];
			int num = 0;
			int num2 = (fast ? numBytes : (numBytes * 8));
			ThreadPool.QueueUserWorkItem(Run);
			for (int i = 0; i < num2; i++)
			{
				while (counter == num)
				{
					try
					{
						Thread.Sleep(1);
					}
					catch (Exception)
					{
					}
				}
				num = counter;
				if (fast)
				{
					array[i] = (byte)num;
					continue;
				}
				int num3 = i / 8;
				array[num3] = (byte)((uint)(array[num3] << 1) | ((uint)num & 1u));
			}
			stop = true;
			return array;
		}
	}

	public byte[] GenerateSeed(int numBytes, bool fast)
	{
		return new SeedGenerator().GenerateSeed(numBytes, fast);
	}
}
