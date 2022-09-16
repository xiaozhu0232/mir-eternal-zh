using System.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsDeflateCompression : TlsCompression
{
	protected class DeflateOutputStream : ZOutputStream
	{
		public DeflateOutputStream(Stream output, ZStream z, bool compress)
			: base(output, z)
		{
			base.compress = compress;
			FlushMode = 2;
		}

		public override void Flush()
		{
		}
	}

	public const int LEVEL_NONE = 0;

	public const int LEVEL_FASTEST = 1;

	public const int LEVEL_SMALLEST = 9;

	public const int LEVEL_DEFAULT = -1;

	protected readonly ZStream zIn;

	protected readonly ZStream zOut;

	public TlsDeflateCompression()
		: this(-1)
	{
	}

	public TlsDeflateCompression(int level)
	{
		zIn = new ZStream();
		zIn.inflateInit();
		zOut = new ZStream();
		zOut.deflateInit(level);
	}

	public virtual Stream Compress(Stream output)
	{
		return new DeflateOutputStream(output, zOut, compress: true);
	}

	public virtual Stream Decompress(Stream output)
	{
		return new DeflateOutputStream(output, zIn, compress: false);
	}
}
