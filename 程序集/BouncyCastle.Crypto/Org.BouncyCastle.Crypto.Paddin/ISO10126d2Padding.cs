using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Paddings;

public class ISO10126d2Padding : IBlockCipherPadding
{
	private SecureRandom random;

	public string PaddingName => "ISO10126-2";

	public void Init(SecureRandom random)
	{
		this.random = ((random != null) ? random : new SecureRandom());
	}

	public int AddPadding(byte[] input, int inOff)
	{
		byte b = (byte)(input.Length - inOff);
		while (inOff < input.Length - 1)
		{
			input[inOff] = (byte)random.NextInt();
			inOff++;
		}
		input[inOff] = b;
		return b;
	}

	public int PadCount(byte[] input)
	{
		int num = input[input.Length - 1] & 0xFF;
		if (num > input.Length)
		{
			throw new InvalidCipherTextException("pad block corrupted");
		}
		return num;
	}
}
