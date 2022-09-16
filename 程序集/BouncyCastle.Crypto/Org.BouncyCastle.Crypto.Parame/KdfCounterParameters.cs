using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class KdfCounterParameters : IDerivationParameters
{
	private byte[] ki;

	private byte[] fixedInputDataCounterPrefix;

	private byte[] fixedInputDataCounterSuffix;

	private int r;

	public byte[] Ki => ki;

	public byte[] FixedInputData => Arrays.Clone(fixedInputDataCounterSuffix);

	public byte[] FixedInputDataCounterPrefix => Arrays.Clone(fixedInputDataCounterPrefix);

	public byte[] FixedInputDataCounterSuffix => Arrays.Clone(fixedInputDataCounterSuffix);

	public int R => r;

	public KdfCounterParameters(byte[] ki, byte[] fixedInputDataCounterSuffix, int r)
		: this(ki, null, fixedInputDataCounterSuffix, r)
	{
	}

	public KdfCounterParameters(byte[] ki, byte[] fixedInputDataCounterPrefix, byte[] fixedInputDataCounterSuffix, int r)
	{
		if (ki == null)
		{
			throw new ArgumentException("A KDF requires Ki (a seed) as input");
		}
		this.ki = Arrays.Clone(ki);
		if (fixedInputDataCounterPrefix == null)
		{
			this.fixedInputDataCounterPrefix = new byte[0];
		}
		else
		{
			this.fixedInputDataCounterPrefix = Arrays.Clone(fixedInputDataCounterPrefix);
		}
		if (fixedInputDataCounterSuffix == null)
		{
			this.fixedInputDataCounterSuffix = new byte[0];
		}
		else
		{
			this.fixedInputDataCounterSuffix = Arrays.Clone(fixedInputDataCounterSuffix);
		}
		if (r != 8 && r != 16 && r != 24 && r != 32)
		{
			throw new ArgumentException("Length of counter should be 8, 16, 24 or 32");
		}
		this.r = r;
	}
}
