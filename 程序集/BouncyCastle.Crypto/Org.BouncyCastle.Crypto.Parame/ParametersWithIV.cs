using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ParametersWithIV : ICipherParameters
{
	private readonly ICipherParameters parameters;

	private readonly byte[] iv;

	public ICipherParameters Parameters => parameters;

	public ParametersWithIV(ICipherParameters parameters, byte[] iv)
		: this(parameters, iv, 0, iv.Length)
	{
	}

	public ParametersWithIV(ICipherParameters parameters, byte[] iv, int ivOff, int ivLen)
	{
		if (iv == null)
		{
			throw new ArgumentNullException("iv");
		}
		this.parameters = parameters;
		this.iv = Arrays.CopyOfRange(iv, ivOff, ivOff + ivLen);
	}

	public byte[] GetIV()
	{
		return (byte[])iv.Clone();
	}
}
