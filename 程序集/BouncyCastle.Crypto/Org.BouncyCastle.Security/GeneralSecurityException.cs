using System;

namespace Org.BouncyCastle.Security;

[Serializable]
public class GeneralSecurityException : Exception
{
	public GeneralSecurityException()
	{
	}

	public GeneralSecurityException(string message)
		: base(message)
	{
	}

	public GeneralSecurityException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
