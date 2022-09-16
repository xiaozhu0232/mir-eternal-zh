using System;
using System.IO;

namespace Org.BouncyCastle.Pkcs;

public class PkcsIOException : IOException
{
	public PkcsIOException(string message)
		: base(message)
	{
	}

	public PkcsIOException(string message, Exception underlying)
		: base(message, underlying)
	{
	}
}
