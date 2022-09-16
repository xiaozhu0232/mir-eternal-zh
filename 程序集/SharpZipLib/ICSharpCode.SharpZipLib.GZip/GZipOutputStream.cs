using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.GZip;

public class GZipOutputStream : DeflaterOutputStream
{
	protected Crc32 crc = new Crc32();

	public GZipOutputStream(Stream baseOutputStream)
		: this(baseOutputStream, 4096)
	{
	}

	public GZipOutputStream(Stream baseOutputStream, int size)
		: base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, noZlibHeaderOrFooter: true), size)
	{
		WriteHeader();
	}

	private void WriteHeader()
	{
		int num = (int)(DateTime.Now.Ticks / 10000);
		byte[] array = new byte[10]
		{
			(byte)(GZipConstants.GZIP_MAGIC >> 8),
			(byte)GZipConstants.GZIP_MAGIC,
			(byte)Deflater.DEFLATED,
			0,
			(byte)num,
			(byte)(num >> 8),
			(byte)(num >> 16),
			(byte)(num >> 24),
			0,
			255
		};
		baseOutputStream.Write(array, 0, array.Length);
	}

	public override void Write(byte[] buf, int off, int len)
	{
		crc.Update(buf, off, len);
		base.Write(buf, off, len);
	}

	public override void Close()
	{
		Finish();
		if (base.IsStreamOwner)
		{
			baseOutputStream.Close();
		}
	}

	public void SetLevel(int level)
	{
		if (level < Deflater.BEST_SPEED)
		{
			throw new ArgumentOutOfRangeException("level");
		}
		def.SetLevel(level);
	}

	public int GetLevel()
	{
		return def.GetLevel();
	}

	public override void Finish()
	{
		base.Finish();
		int totalIn = def.TotalIn;
		int num = (int)(crc.Value & 0xFFFFFFFFu);
		byte[] array = new byte[8]
		{
			(byte)num,
			(byte)(num >> 8),
			(byte)(num >> 16),
			(byte)(num >> 24),
			(byte)totalIn,
			(byte)(totalIn >> 8),
			(byte)(totalIn >> 16),
			(byte)(totalIn >> 24)
		};
		baseOutputStream.Write(array, 0, array.Length);
	}
}
