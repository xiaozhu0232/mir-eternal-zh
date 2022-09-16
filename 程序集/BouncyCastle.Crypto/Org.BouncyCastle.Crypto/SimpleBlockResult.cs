using System;

namespace Org.BouncyCastle.Crypto;

public class SimpleBlockResult : IBlockResult
{
	private readonly byte[] result;

	public int Length => result.Length;

	public SimpleBlockResult(byte[] result)
	{
		this.result = result;
	}

	public byte[] Collect()
	{
		return result;
	}

	public int Collect(byte[] destination, int offset)
	{
		Array.Copy(result, 0, destination, offset, result.Length);
		return result.Length;
	}
}
