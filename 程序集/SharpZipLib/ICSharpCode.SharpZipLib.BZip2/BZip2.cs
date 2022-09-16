using System.IO;

namespace ICSharpCode.SharpZipLib.BZip2;

public sealed class BZip2
{
	public static void Decompress(Stream instream, Stream outstream)
	{
		BZip2InputStream bZip2InputStream = new BZip2InputStream(instream);
		for (int num = bZip2InputStream.ReadByte(); num != -1; num = bZip2InputStream.ReadByte())
		{
			outstream.WriteByte((byte)num);
		}
		outstream.Flush();
	}

	public static void Compress(Stream instream, Stream outstream, int blockSize)
	{
		int num = instream.ReadByte();
		BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(outstream, blockSize);
		while (num != -1)
		{
			bZip2OutputStream.WriteByte((byte)num);
			num = instream.ReadByte();
		}
		instream.Close();
		bZip2OutputStream.Close();
	}
}
