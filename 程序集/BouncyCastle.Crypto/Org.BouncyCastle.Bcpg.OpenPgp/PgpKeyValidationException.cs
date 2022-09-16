using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

[Serializable]
public class PgpKeyValidationException : PgpException
{
	public PgpKeyValidationException()
	{
	}

	public PgpKeyValidationException(string message)
		: base(message)
	{
	}

	public PgpKeyValidationException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
