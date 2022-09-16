using System.IO;

namespace Org.BouncyCastle.Utilities.IO;

public class MemoryInputStream : MemoryStream
{
	public sealed override bool CanWrite => false;

	public MemoryInputStream(byte[] buffer)
		: base(buffer, writable: false)
	{
	}
}
