using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpLiteralDataGenerator : IStreamGenerator
{
	public const char Binary = 'b';

	public const char Text = 't';

	public const char Utf8 = 'u';

	public const string Console = "_CONSOLE";

	private BcpgOutputStream pkOut;

	private bool oldFormat;

	public PgpLiteralDataGenerator()
	{
	}

	public PgpLiteralDataGenerator(bool oldFormat)
	{
		this.oldFormat = oldFormat;
	}

	private void WriteHeader(BcpgOutputStream outStr, char format, byte[] encName, long modificationTime)
	{
		outStr.Write((byte)format, (byte)encName.Length);
		outStr.Write(encName);
		long num = modificationTime / 1000;
		outStr.Write((byte)(num >> 24), (byte)(num >> 16), (byte)(num >> 8), (byte)num);
	}

	public Stream Open(Stream outStr, char format, string name, long length, DateTime modificationTime)
	{
		if (pkOut != null)
		{
			throw new InvalidOperationException("generator already in open state");
		}
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		long modificationTime2 = DateTimeUtilities.DateTimeToUnixMs(modificationTime);
		byte[] array = Strings.ToUtf8ByteArray(name);
		pkOut = new BcpgOutputStream(outStr, PacketTag.LiteralData, length + 2 + array.Length + 4, oldFormat);
		WriteHeader(pkOut, format, array, modificationTime2);
		return new WrappedGeneratorStream(this, pkOut);
	}

	public Stream Open(Stream outStr, char format, string name, DateTime modificationTime, byte[] buffer)
	{
		if (pkOut != null)
		{
			throw new InvalidOperationException("generator already in open state");
		}
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		long modificationTime2 = DateTimeUtilities.DateTimeToUnixMs(modificationTime);
		byte[] encName = Strings.ToUtf8ByteArray(name);
		pkOut = new BcpgOutputStream(outStr, PacketTag.LiteralData, buffer);
		WriteHeader(pkOut, format, encName, modificationTime2);
		return new WrappedGeneratorStream(this, pkOut);
	}

	public Stream Open(Stream outStr, char format, FileInfo file)
	{
		return Open(outStr, format, file.Name, file.Length, file.LastWriteTime);
	}

	public void Close()
	{
		if (pkOut != null)
		{
			pkOut.Finish();
			pkOut.Flush();
			pkOut = null;
		}
	}
}
