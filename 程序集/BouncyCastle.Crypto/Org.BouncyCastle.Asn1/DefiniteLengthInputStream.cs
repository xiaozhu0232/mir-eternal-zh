using System;
using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

internal class DefiniteLengthInputStream : LimitedInputStream
{
	private static readonly byte[] EmptyBytes = new byte[0];

	private readonly int _originalLength;

	private int _remaining;

	internal int Remaining => _remaining;

	internal DefiniteLengthInputStream(Stream inStream, int length, int limit)
		: base(inStream, limit)
	{
		if (length < 0)
		{
			throw new ArgumentException("negative lengths not allowed", "length");
		}
		_originalLength = length;
		_remaining = length;
		if (length == 0)
		{
			SetParentEofDetect(on: true);
		}
	}

	public override int ReadByte()
	{
		if (_remaining == 0)
		{
			return -1;
		}
		int num = _in.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
		}
		if (--_remaining == 0)
		{
			SetParentEofDetect(on: true);
		}
		return num;
	}

	public override int Read(byte[] buf, int off, int len)
	{
		if (_remaining == 0)
		{
			return 0;
		}
		int count = System.Math.Min(len, _remaining);
		int num = _in.Read(buf, off, count);
		if (num < 1)
		{
			throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
		}
		if ((_remaining -= num) == 0)
		{
			SetParentEofDetect(on: true);
		}
		return num;
	}

	internal void ReadAllIntoByteArray(byte[] buf)
	{
		if (_remaining != buf.Length)
		{
			throw new ArgumentException("buffer length not right for data");
		}
		if (_remaining != 0)
		{
			int limit = Limit;
			if (_remaining >= limit)
			{
				throw new IOException("corrupted stream - out of bounds length found: " + _remaining + " >= " + limit);
			}
			if ((_remaining -= Streams.ReadFully(_in, buf)) != 0)
			{
				throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
			}
			SetParentEofDetect(on: true);
		}
	}

	internal byte[] ToArray()
	{
		if (_remaining == 0)
		{
			return EmptyBytes;
		}
		int limit = Limit;
		if (_remaining >= limit)
		{
			throw new IOException("corrupted stream - out of bounds length found: " + _remaining + " >= " + limit);
		}
		byte[] array = new byte[_remaining];
		if ((_remaining -= Streams.ReadFully(_in, array)) != 0)
		{
			throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
		}
		SetParentEofDetect(on: true);
		return array;
	}
}
