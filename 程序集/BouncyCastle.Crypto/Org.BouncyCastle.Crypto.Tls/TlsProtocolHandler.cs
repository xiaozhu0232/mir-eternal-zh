using System;
using System.IO;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls;

[Obsolete("Use 'TlsClientProtocol' instead")]
public class TlsProtocolHandler : TlsClientProtocol
{
	public TlsProtocolHandler(Stream stream, SecureRandom secureRandom)
		: base(stream, stream, secureRandom)
	{
	}

	public TlsProtocolHandler(Stream input, Stream output, SecureRandom secureRandom)
		: base(input, output, secureRandom)
	{
	}
}
