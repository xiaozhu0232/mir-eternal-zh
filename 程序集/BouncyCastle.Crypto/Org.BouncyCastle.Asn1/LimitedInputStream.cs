using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

internal abstract class LimitedInputStream : BaseInputStream
{
	protected readonly Stream _in;

	private int _limit;

	internal virtual int Limit => _limit;

	internal LimitedInputStream(Stream inStream, int limit)
	{
		_in = inStream;
		_limit = limit;
	}

	protected virtual void SetParentEofDetect(bool on)
	{
		if (_in is IndefiniteLengthInputStream)
		{
			((IndefiniteLengthInputStream)_in).SetEofOn00(on);
		}
	}
}
