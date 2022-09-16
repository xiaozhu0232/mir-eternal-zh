using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.IO;

public class MacSink : BaseOutputStream
{
	private readonly IMac mMac;

	public virtual IMac Mac => mMac;

	public MacSink(IMac mac)
	{
		mMac = mac;
	}

	public override void WriteByte(byte b)
	{
		mMac.Update(b);
	}

	public override void Write(byte[] buf, int off, int len)
	{
		if (len > 0)
		{
			mMac.BlockUpdate(buf, off, len);
		}
	}
}
