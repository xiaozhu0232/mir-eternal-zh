using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms;

public class CmsProcessableFile : CmsProcessable, CmsReadable
{
	private const int DefaultBufSize = 32768;

	private readonly FileInfo _file;

	private readonly int _bufSize;

	public CmsProcessableFile(FileInfo file)
		: this(file, 32768)
	{
	}

	public CmsProcessableFile(FileInfo file, int bufSize)
	{
		_file = file;
		_bufSize = bufSize;
	}

	public virtual Stream GetInputStream()
	{
		return new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, _bufSize);
	}

	public virtual void Write(Stream zOut)
	{
		Stream inputStream = GetInputStream();
		Streams.PipeAll(inputStream, zOut);
		Platform.Dispose(inputStream);
	}

	[Obsolete]
	public virtual object GetContent()
	{
		return _file;
	}
}
