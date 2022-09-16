using System.IO;
using Org.BouncyCastle.Crypto.IO;

namespace Org.BouncyCastle.Crypto.Operators;

public class DefaultVerifierCalculator : IStreamCalculator
{
	private readonly SignerSink mSignerSink;

	public Stream Stream => mSignerSink;

	public DefaultVerifierCalculator(ISigner signer)
	{
		mSignerSink = new SignerSink(signer);
	}

	public object GetResult()
	{
		return new DefaultVerifierResult(mSignerSink.Signer);
	}
}
