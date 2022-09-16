using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Paddings;

public class ZeroBytePadding : IBlockCipherPadding
{
	public string PaddingName => "ZeroBytePadding";

	public void Init(SecureRandom random)
	{
	}

	public int AddPadding(byte[] input, int inOff)
	{
		int result = input.Length - inOff;
		while (inOff < input.Length)
		{
			input[inOff] = 0;
			inOff++;
		}
		return result;
	}

	public int PadCount(byte[] input)
	{
		int num = input.Length;
		while (num > 0 && input[num - 1] == 0)
		{
			num--;
		}
		return input.Length - num;
	}
}
