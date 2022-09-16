using System.IO;
using Org.BouncyCastle.Crypto.IO;

namespace Org.BouncyCastle.Crypto.Operators;

public class DefaultSignatureCalculator : IStreamCalculator
{
	private readonly SignerSink mSignerSink;

	public Stream Stream => mSignerSink;

	public DefaultSignatureCalculator(ISigner signer)
	{
		mSignerSink = new SignerSink(signer);
	}

	public object GetResult()
	{
		return new DefaultSignatureResult(mSignerSink.Signer);
	}
}
