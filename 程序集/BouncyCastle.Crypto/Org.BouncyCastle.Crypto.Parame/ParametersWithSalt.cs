using System;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ParametersWithSalt : ICipherParameters
{
	private byte[] salt;

	private ICipherParameters parameters;

	public ICipherParameters Parameters => parameters;

	public ParametersWithSalt(ICipherParameters parameters, byte[] salt)
		: this(parameters, salt, 0, salt.Length)
	{
	}

	public ParametersWithSalt(ICipherParameters parameters, byte[] salt, int saltOff, int saltLen)
	{
		this.salt = new byte[saltLen];
		this.parameters = parameters;
		Array.Copy(salt, saltOff, this.salt, 0, saltLen);
	}

	public byte[] GetSalt()
	{
		return salt;
	}
}
