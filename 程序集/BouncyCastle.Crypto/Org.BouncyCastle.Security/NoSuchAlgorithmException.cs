using System;

namespace Org.BouncyCastle.Security;

[Serializable]
[Obsolete("Never thrown")]
public class NoSuchAlgorithmException : GeneralSecurityException
{
	public NoSuchAlgorithmException()
	{
	}

	public NoSuchAlgorithmException(string message)
		: base(message)
	{
	}

	public NoSuchAlgorithmException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
