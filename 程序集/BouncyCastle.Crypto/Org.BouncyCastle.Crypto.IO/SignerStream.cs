using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.IO;

public class SignerStream : Stream
{
	protected readonly Stream stream;

	protected readonly ISigner inSigner;

	protected readonly ISigner outSigner;

	public override bool CanRead => stream.CanRead;

	public override bool CanWrite => stream.CanWrite;

	public override bool CanSeek => stream.CanSeek;

	public override long Length => stream.Length;

	public override long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			stream.Position = value;
		}
	}

	public SignerStream(Stream stream, ISigner readSigner, ISigner writeSigner)
	{
		this.stream = stream;
		inSigner = readSigner;
		outSigner = writeSigner;
	}

	public virtual ISigner ReadSigner()
	{
		return inSigner;
	}

	public virtual ISigner WriteSigner()
	{
		return outSigner;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = stream.Read(buffer, offset, count);
		if (inSigner != null && num > 0)
		{
			inSigner.BlockUpdate(buffer, offset, num);
		}
		return num;
	}

	public override int ReadByte()
	{
		int num = stream.ReadByte();
		if (inSigner != null && num >= 0)
		{
			inSigner.Update((byte)num);
		}
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (outSigner != null && count > 0)
		{
			outSigner.BlockUpdate(buffer, offset, count);
		}
		stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte b)
	{
		if (outSigner != null)
		{
			outSigner.Update(b);
		}
		stream.WriteByte(b);
	}

	public override void Close()
	{
		Platform.Dispose(stream);
		base.Close();
	}

	public override void Flush()
	{
		stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long length)
	{
		stream.SetLength(length);
	}
}
