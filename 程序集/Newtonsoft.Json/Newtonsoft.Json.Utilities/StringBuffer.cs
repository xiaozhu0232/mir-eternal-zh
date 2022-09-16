using System;

namespace Newtonsoft.Json.Utilities;

internal struct StringBuffer
{
	private char[]? _buffer;

	private int _position;

	public int Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public bool IsEmpty => _buffer == null;

	public char[]? InternalBuffer => _buffer;

	public StringBuffer(IArrayPool<char>? bufferPool, int initalSize)
		: this(BufferUtils.RentBuffer(bufferPool, initalSize))
	{
	}

	private StringBuffer(char[] buffer)
	{
		_buffer = buffer;
		_position = 0;
	}

	public void Append(IArrayPool<char>? bufferPool, char value)
	{
		if (_position == _buffer!.Length)
		{
			EnsureSize(bufferPool, 1);
		}
		_buffer[_position++] = value;
	}

	public void Append(IArrayPool<char>? bufferPool, char[] buffer, int startIndex, int count)
	{
		if (_position + count >= _buffer!.Length)
		{
			EnsureSize(bufferPool, count);
		}
		Array.Copy(buffer, startIndex, _buffer, _position, count);
		_position += count;
	}

	public void Clear(IArrayPool<char>? bufferPool)
	{
		if (_buffer != null)
		{
			BufferUtils.ReturnBuffer(bufferPool, _buffer);
			_buffer = null;
		}
		_position = 0;
	}

	private void EnsureSize(IArrayPool<char>? bufferPool, int appendLength)
	{
		char[] array = BufferUtils.RentBuffer(bufferPool, (_position + appendLength) * 2);
		if (_buffer != null)
		{
			Array.Copy(_buffer, array, _position);
			BufferUtils.ReturnBuffer(bufferPool, _buffer);
		}
		_buffer = array;
	}

	public override string ToString()
	{
		return ToString(0, _position);
	}

	public string ToString(int start, int length)
	{
		return new string(_buffer, start, length);
	}
}
