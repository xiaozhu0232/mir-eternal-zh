namespace Org.BouncyCastle.Crypto.Digests;

internal class XofUtilities
{
	internal static byte[] LeftEncode(long strLen)
	{
		byte b = 1;
		long num = strLen;
		while ((num >>= 8) != 0)
		{
			b = (byte)(b + 1);
		}
		byte[] array = new byte[b + 1];
		array[0] = b;
		for (int i = 1; i <= b; i++)
		{
			array[i] = (byte)(strLen >> 8 * (b - i));
		}
		return array;
	}

	internal static byte[] RightEncode(long strLen)
	{
		byte b = 1;
		long num = strLen;
		while ((num >>= 8) != 0)
		{
			b = (byte)(b + 1);
		}
		byte[] array = new byte[b + 1];
		array[b] = b;
		for (int i = 0; i < b; i++)
		{
			array[i] = (byte)(strLen >> 8 * (b - i - 1));
		}
		return array;
	}
}
