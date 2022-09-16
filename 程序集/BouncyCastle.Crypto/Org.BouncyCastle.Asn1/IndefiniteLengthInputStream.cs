using System.IO;

namespace Org.BouncyCastle.Asn1;

internal class IndefiniteLengthInputStream : LimitedInputStream
{
	private int _lookAhead;

	private bool _eofOn00 = true;

	internal IndefiniteLengthInputStream(Stream inStream, int limit)
		: base(inStream, limit)
	{
		_lookAhead = RequireByte();
		CheckForEof();
	}

	internal void SetEofOn00(bool eofOn00)
	{
		_eofOn00 = eofOn00;
		if (_eofOn00)
		{
			CheckForEof();
		}
	}

	private bool CheckForEof()
	{
		if (_lookAhead == 0)
		{
			if (RequireByte() != 0)
			{
				throw new IOException("malformed end-of-contents marker");
			}
			_lookAhead = -1;
			SetParentEofDetect(on: true);
			return true;
		}
		return _lookAhead < 0;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_eofOn00 || count <= 1)
		{
			return base.Read(buffer, offset, count);
		}
		if (_lookAhead < 0)
		{
			return 0;
		}
		int num = _in.Read(buffer, offset + 1, count - 1);
		if (num <= 0)
		{
			throw new EndOfStreamException();
		}
		buffer[offset] = (byte)_lookAhead;
		_lookAhead = RequireByte();
		return num + 1;
	}

	public override int ReadByte()
	{
		if (_eofOn00 && CheckForEof())
		{
			return -1;
		}
		int lookAhead = _lookAhead;
		_lookAhead = RequireByte();
		return lookAhead;
	}

	private int RequireByte()
	{
		int num = _in.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException();
		}
		return num;
	}
}
