using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class CShakeDigest : ShakeDigest
{
	private static readonly byte[] padding = new byte[100];

	private readonly byte[] diff;

	public override string AlgorithmName => "CSHAKE" + fixedOutputLength;

	public CShakeDigest(int bitLength, byte[] N, byte[] S)
		: base(bitLength)
	{
		if ((N == null || N.Length == 0) && (S == null || S.Length == 0))
		{
			diff = null;
			return;
		}
		diff = Arrays.ConcatenateAll(XofUtilities.LeftEncode(rate / 8), encodeString(N), encodeString(S));
		DiffPadAndAbsorb();
	}

	private void DiffPadAndAbsorb()
	{
		int num = rate / 8;
		Absorb(diff, 0, diff.Length);
		int num2 = diff.Length % num;
		if (num2 != 0)
		{
			int num3;
			for (num3 = num - num2; num3 > padding.Length; num3 -= padding.Length)
			{
				Absorb(padding, 0, padding.Length);
			}
			Absorb(padding, 0, num3);
		}
	}

	private byte[] encodeString(byte[] str)
	{
		if (str == null || str.Length == 0)
		{
			return XofUtilities.LeftEncode(0L);
		}
		return Arrays.Concatenate(XofUtilities.LeftEncode((long)str.Length * 8L), str);
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		return DoFinal(output, outOff, GetDigestSize());
	}

	public override int DoFinal(byte[] output, int outOff, int outLen)
	{
		int result = DoOutput(output, outOff, outLen);
		Reset();
		return result;
	}

	public override int DoOutput(byte[] output, int outOff, int outLen)
	{
		if (diff != null)
		{
			if (!squeezing)
			{
				AbsorbBits(0, 2);
			}
			Squeeze(output, outOff, (long)outLen * 8L);
			return outLen;
		}
		return base.DoOutput(output, outOff, outLen);
	}

	public override void Reset()
	{
		base.Reset();
		if (diff != null)
		{
			DiffPadAndAbsorb();
		}
	}
}
