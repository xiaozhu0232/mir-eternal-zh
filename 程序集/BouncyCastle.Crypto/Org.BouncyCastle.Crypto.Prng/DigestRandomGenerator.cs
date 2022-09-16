using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Prng;

public class DigestRandomGenerator : IRandomGenerator
{
	private const long CYCLE_COUNT = 10L;

	private long stateCounter;

	private long seedCounter;

	private IDigest digest;

	private byte[] state;

	private byte[] seed;

	public DigestRandomGenerator(IDigest digest)
	{
		this.digest = digest;
		seed = new byte[digest.GetDigestSize()];
		seedCounter = 1L;
		state = new byte[digest.GetDigestSize()];
		stateCounter = 1L;
	}

	public void AddSeedMaterial(byte[] inSeed)
	{
		lock (this)
		{
			DigestUpdate(inSeed);
			DigestUpdate(seed);
			DigestDoFinal(seed);
		}
	}

	public void AddSeedMaterial(long rSeed)
	{
		lock (this)
		{
			DigestAddCounter(rSeed);
			DigestUpdate(seed);
			DigestDoFinal(seed);
		}
	}

	public void NextBytes(byte[] bytes)
	{
		NextBytes(bytes, 0, bytes.Length);
	}

	public void NextBytes(byte[] bytes, int start, int len)
	{
		lock (this)
		{
			int num = 0;
			GenerateState();
			int num2 = start + len;
			for (int i = start; i < num2; i++)
			{
				if (num == state.Length)
				{
					GenerateState();
					num = 0;
				}
				bytes[i] = state[num++];
			}
		}
	}

	private void CycleSeed()
	{
		DigestUpdate(seed);
		DigestAddCounter(seedCounter++);
		DigestDoFinal(seed);
	}

	private void GenerateState()
	{
		DigestAddCounter(stateCounter++);
		DigestUpdate(state);
		DigestUpdate(seed);
		DigestDoFinal(state);
		if (stateCounter % 10 == 0)
		{
			CycleSeed();
		}
	}

	private void DigestAddCounter(long seedVal)
	{
		byte[] array = new byte[8];
		Pack.UInt64_To_LE((ulong)seedVal, array);
		digest.BlockUpdate(array, 0, array.Length);
	}

	private void DigestUpdate(byte[] inSeed)
	{
		digest.BlockUpdate(inSeed, 0, inSeed.Length);
	}

	private void DigestDoFinal(byte[] result)
	{
		digest.DoFinal(result, 0);
	}
}
