using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

[Serializable]
public class PgpDataValidationException : PgpException
{
	public PgpDataValidationException()
	{
	}

	public PgpDataValidationException(string message)
		: base(message)
	{
	}

	public PgpDataValidationException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
