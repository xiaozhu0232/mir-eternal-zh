using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

[Serializable]
public class PgpException : Exception
{
	[Obsolete("Use InnerException property")]
	public Exception UnderlyingException => base.InnerException;

	public PgpException()
	{
	}

	public PgpException(string message)
		: base(message)
	{
	}

	public PgpException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
