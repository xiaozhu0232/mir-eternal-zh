using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.IO;

public class DigestSink : BaseOutputStream
{
	private readonly IDigest mDigest;

	public virtual IDigest Digest => mDigest;

	public DigestSink(IDigest digest)
	{
		mDigest = digest;
	}

	public override void WriteByte(byte b)
	{
		mDigest.Update(b);
	}

	public override void Write(byte[] buf, int off, int len)
	{
		if (len > 0)
		{
			mDigest.BlockUpdate(buf, off, len);
		}
	}
}
