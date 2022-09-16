using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Paddings;

public class ISO7816d4Padding : IBlockCipherPadding
{
	public string PaddingName => "ISO7816-4";

	public void Init(SecureRandom random)
	{
	}

	public int AddPadding(byte[] input, int inOff)
	{
		int result = input.Length - inOff;
		input[inOff] = 128;
		for (inOff++; inOff < input.Length; inOff++)
		{
			input[inOff] = 0;
		}
		return result;
	}

	public int PadCount(byte[] input)
	{
		int num = input.Length - 1;
		while (num > 0 && input[num] == 0)
		{
			num--;
		}
		if (input[num] != 128)
		{
			throw new InvalidCipherTextException("pad block corrupted");
		}
		return input.Length - num;
	}
}
