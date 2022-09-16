using System;

namespace Org.BouncyCastle.Security.Certificates;

[Serializable]
public class CertificateException : GeneralSecurityException
{
	public CertificateException()
	{
	}

	public CertificateException(string message)
		: base(message)
	{
	}

	public CertificateException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
