using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsNoCloseNotifyException : EndOfStreamException
{
	public TlsNoCloseNotifyException()
		: base("No close_notify alert received before connection closed")
	{
	}
}
