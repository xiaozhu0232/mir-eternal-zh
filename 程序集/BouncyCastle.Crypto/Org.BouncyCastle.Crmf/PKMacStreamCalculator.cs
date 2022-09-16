using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;

namespace Org.BouncyCastle.Crmf;

internal class PKMacStreamCalculator : IStreamCalculator
{
	private readonly MacSink _stream;

	public Stream Stream => _stream;

	public PKMacStreamCalculator(IMac mac)
	{
		_stream = new MacSink(mac);
	}

	public object GetResult()
	{
		return new DefaultPKMacResult(_stream.Mac);
	}
}
