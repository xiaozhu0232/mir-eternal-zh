using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Pkix;

[Serializable]
public class PkixCertPathBuilderException : GeneralSecurityException
{
	public PkixCertPathBuilderException()
	{
	}

	public PkixCertPathBuilderException(string message)
		: base(message)
	{
	}

	public PkixCertPathBuilderException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
