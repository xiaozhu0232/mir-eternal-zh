using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class DeflaterOutputStream : Stream
{
	protected byte[] buf;

	protected Deflater def;

	protected Stream baseOutputStream;

	private bool isClosed = false;

	private bool isStreamOwner = true;

	private string password = null;

	private uint[] keys = null;

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

	public bool CanPatchEntries => baseOutputStream.CanSeek;

	public override bool CanRead => baseOutputStream.CanRead;

	public override bool CanSeek => false;

	public override bool CanWrite => baseOutputStream.CanWrite;

	public override long Length => baseOutputStream.Length;

	public override long Position
	{
		get
		{
			return baseOutputStream.Position;
		}
		set
		{
			throw new NotSupportedException("DefalterOutputStream Position not supported");
		}
	}

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			if (value != null && value.Length == 0)
			{
				password = null;
			}
			else
			{
				password = value;
			}
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("DeflaterOutputStream Seek not supported");
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
	}

	public override int ReadByte()
	{
		throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
	}

	public override int Read(byte[] b, int off, int len)
	{
		throw new NotSupportedException("DeflaterOutputStream Read not supported");
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException("DeflaterOutputStream BeginWrite not currently supported");
	}

	protected void Deflate()
	{
		while (!def.IsNeedingInput)
		{
			int num = def.Deflate(buf, 0, buf.Length);
			if (num <= 0)
			{
				break;
			}
			if (keys != null)
			{
				EncryptBlock(buf, 0, num);
			}
			baseOutputStream.Write(buf, 0, num);
		}
		if (!def.IsNeedingInput)
		{
			throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
		}
	}

	public DeflaterOutputStream(Stream baseOutputStream)
		: this(baseOutputStream, new Deflater(), 512)
	{
	}

	public DeflaterOutputStream(Stream baseOutputStream, Deflater defl)
		: this(baseOutputStream, defl, 512)
	{
	}

	public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufsize)
	{
		if (!baseOutputStream.CanWrite)
		{
			throw new ArgumentException("baseOutputStream", "must support writing");
		}
		if (deflater == null)
		{
			throw new ArgumentNullException("deflater");
		}
		if (bufsize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufsize");
		}
		this.baseOutputStream = baseOutputStream;
		buf = new byte[bufsize];
		def = deflater;
	}

	public override void Flush()
	{
		def.Flush();
		Deflate();
		baseOutputStream.Flush();
	}

	public virtual void Finish()
	{
		def.Finish();
		while (!def.IsFinished)
		{
			int num = def.Deflate(buf, 0, buf.Length);
			if (num <= 0)
			{
				break;
			}
			if (keys != null)
			{
				EncryptBlock(buf, 0, num);
			}
			baseOutputStream.Write(buf, 0, num);
		}
		if (!def.IsFinished)
		{
			throw new SharpZipBaseException("Can't deflate all input?");
		}
		baseOutputStream.Flush();
		keys = null;
	}

	public override void Close()
	{
		if (!isClosed)
		{
			isClosed = true;
			Finish();
			if (isStreamOwner)
			{
				baseOutputStream.Close();
			}
		}
	}

	public override void WriteByte(byte bval)
	{
		Write(new byte[1] { bval }, 0, 1);
	}

	public override void Write(byte[] buf, int off, int len)
	{
		def.SetInput(buf, off, len);
		Deflate();
	}

	protected byte EncryptByte()
	{
		uint num = (keys[2] & 0xFFFFu) | 2u;
		return (byte)(num * (num ^ 1) >> 8);
	}

	protected void EncryptBlock(byte[] buffer, int offset, int length)
	{
		for (int i = offset; i < offset + length; i++)
		{
			byte ch = buffer[i];
			buffer[i] ^= EncryptByte();
			UpdateKeys(ch);
		}
	}

	protected void InitializePassword(string password)
	{
		keys = new uint[3] { 305419896u, 591751049u, 878082192u };
		for (int i = 0; i < password.Length; i++)
		{
			UpdateKeys((byte)password[i]);
		}
	}

	protected void UpdateKeys(byte ch)
	{
		keys[0] = Crc32.ComputeCrc32(keys[0], ch);
		keys[1] = keys[1] + (byte)keys[0];
		keys[1] = keys[1] * 134775813 + 1;
		keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
	}
}
