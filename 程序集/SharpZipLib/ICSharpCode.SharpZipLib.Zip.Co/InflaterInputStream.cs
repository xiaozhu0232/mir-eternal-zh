using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class InflaterInputStream : Stream
{
	protected Inflater inf;

	protected InflaterInputBuffer inputBuffer;

	protected Stream baseInputStream;

	protected long csize;

	private bool isClosed = false;

	private bool isStreamOwner = true;

	public bool IsStreamOwner
	{
		get
		{
			return isStreamOwner;
		}
		set
		{
			isStreamOwner = value;
		}
	}

	public override bool CanRead => baseInputStream.CanRead;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => inputBuffer.RawLength;

	public override long Position
	{
		get
		{
			return baseInputStream.Position;
		}
		set
		{
			throw new NotSupportedException("InflaterInputStream Position not supported");
		}
	}

	public virtual int Available => (!inf.IsFinished) ? 1 : 0;

	public override void Flush()
	{
		baseInputStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("Seek not supported");
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException("InflaterInputStream SetLength not supported");
	}

	public override void Write(byte[] array, int offset, int count)
	{
		throw new NotSupportedException("InflaterInputStream Write not supported");
	}

	public override void WriteByte(byte val)
	{
		throw new NotSupportedException("InflaterInputStream WriteByte not supported");
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException("InflaterInputStream BeginWrite not supported");
	}

	public InflaterInputStream(Stream baseInputStream)
		: this(baseInputStream, new Inflater(), 4096)
	{
	}

	public InflaterInputStream(Stream baseInputStream, Inflater inf)
		: this(baseInputStream, inf, 4096)
	{
	}

	public InflaterInputStream(Stream baseInputStream, Inflater inflater, int bufferSize)
	{
		if (baseInputStream == null)
		{
			throw new ArgumentNullException("InflaterInputStream baseInputStream is null");
		}
		if (inflater == null)
		{
			throw new ArgumentNullException("InflaterInputStream Inflater is null");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize");
		}
		this.baseInputStream = baseInputStream;
		inf = inflater;
		inputBuffer = new InflaterInputBuffer(baseInputStream);
	}

	public override void Close()
	{
		if (!isClosed)
		{
			isClosed = true;
			if (isStreamOwner)
			{
				baseInputStream.Close();
			}
		}
	}

	protected void Fill()
	{
		inputBuffer.Fill();
		inputBuffer.SetInflaterInput(inf);
	}

	public override int Read(byte[] b, int off, int len)
	{
		while (true)
		{
			bool flag = true;
			int num;
			try
			{
				num = inf.Inflate(b, off, len);
			}
			catch (Exception ex)
			{
				throw new SharpZipBaseException(ex.ToString());
			}
			if (num > 0)
			{
				return num;
			}
			if (inf.IsNeedingDictionary)
			{
				throw new SharpZipBaseException("Need a dictionary");
			}
			if (inf.IsFinished)
			{
				return 0;
			}
			if (!inf.IsNeedingInput)
			{
				break;
			}
			Fill();
		}
		throw new InvalidOperationException("Don't know what to do");
	}

	public long Skip(long n)
	{
		if (n <= 0)
		{
			throw new ArgumentOutOfRangeException("n");
		}
		if (baseInputStream.CanSeek)
		{
			baseInputStream.Seek(n, SeekOrigin.Current);
			return n;
		}
		int num = 2048;
		if (n < num)
		{
			num = (int)n;
		}
		byte[] array = new byte[num];
		return baseInputStream.Read(array, 0, array.Length);
	}

	protected void StopDecrypting()
	{
		inputBuffer.CryptoTransform = null;
	}
}
