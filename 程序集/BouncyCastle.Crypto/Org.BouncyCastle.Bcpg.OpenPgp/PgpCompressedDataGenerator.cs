using System;
using System.IO;
using Org.BouncyCastle.Apache.Bzip2;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpCompressedDataGenerator : IStreamGenerator
{
	private class SafeCBZip2OutputStream : CBZip2OutputStream
	{
		public SafeCBZip2OutputStream(Stream output)
			: base(output)
		{
		}

		public override void Close()
		{
			Finish();
		}
	}

	private class SafeZOutputStream : ZOutputStream
	{
		public SafeZOutputStream(Stream output, int level, bool nowrap)
			: base(output, level, nowrap)
		{
		}

		public override void Close()
		{
			Finish();
			End();
		}
	}

	private readonly CompressionAlgorithmTag algorithm;

	private readonly int compression;

	private Stream dOut;

	private BcpgOutputStream pkOut;

	public PgpCompressedDataGenerator(CompressionAlgorithmTag algorithm)
		: this(algorithm, -1)
	{
	}

	public PgpCompressedDataGenerator(CompressionAlgorithmTag algorithm, int compression)
	{
		switch (algorithm)
		{
		default:
			throw new ArgumentException("unknown compression algorithm", "algorithm");
		case CompressionAlgorithmTag.Uncompressed:
		case CompressionAlgorithmTag.Zip:
		case CompressionAlgorithmTag.ZLib:
		case CompressionAlgorithmTag.BZip2:
			switch (compression)
			{
			default:
				throw new ArgumentException("unknown compression level: " + compression);
			case -1:
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
				this.algorithm = algorithm;
				this.compression = compression;
				break;
			}
			break;
		}
	}

	public Stream Open(Stream outStr)
	{
		if (dOut != null)
		{
			throw new InvalidOperationException("generator already in open state");
		}
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		pkOut = new BcpgOutputStream(outStr, PacketTag.CompressedData);
		doOpen();
		return new WrappedGeneratorStream(this, dOut);
	}

	public Stream Open(Stream outStr, byte[] buffer)
	{
		if (dOut != null)
		{
			throw new InvalidOperationException("generator already in open state");
		}
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		pkOut = new BcpgOutputStream(outStr, PacketTag.CompressedData, buffer);
		doOpen();
		return new WrappedGeneratorStream(this, dOut);
	}

	private void doOpen()
	{
		pkOut.WriteByte((byte)algorithm);
		switch (algorithm)
		{
		case CompressionAlgorithmTag.Uncompressed:
			dOut = pkOut;
			break;
		case CompressionAlgorithmTag.Zip:
			dOut = new SafeZOutputStream(pkOut, compression, nowrap: true);
			break;
		case CompressionAlgorithmTag.ZLib:
			dOut = new SafeZOutputStream(pkOut, compression, nowrap: false);
			break;
		case CompressionAlgorithmTag.BZip2:
			dOut = new SafeCBZip2OutputStream(pkOut);
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	public void Close()
	{
		if (dOut != null)
		{
			if (dOut != pkOut)
			{
				Platform.Dispose(dOut);
			}
			dOut = null;
			pkOut.Finish();
			pkOut.Flush();
			pkOut = null;
		}
	}
}
