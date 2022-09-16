using System.IO;
using Org.BouncyCastle.Crypto.IO;

namespace Org.BouncyCastle.Crypto.Operators;

internal class DfDigestStream : IStreamCalculator
{
	private readonly DigestSink mStream;

	public Stream Stream => mStream;

	public DfDigestStream(IDigest digest)
	{
		mStream = new DigestSink(digest);
	}

	public object GetResult()
	{
		byte[] array = new byte[mStream.Digest.GetDigestSize()];
		mStream.Digest.DoFinal(array, 0);
		return new SimpleBlockResult(array);
	}
}
