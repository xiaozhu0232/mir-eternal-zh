using System;

namespace Org.BouncyCastle.Crypto;

[Serializable]
public class DataLengthException : CryptoException
{
	public DataLengthException()
	{
	}

	public DataLengthException(string message)
		: base(message)
	{
	}

	public DataLengthException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
