using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

public class JsonTextReader : JsonReader, IJsonLineInfo
{
	private readonly bool _safeAsync;

	private const char UnicodeReplacementChar = '\ufffd';

	private const int MaximumJavascriptIntegerCharacterLength = 380;

	private const int LargeBufferLength = 1073741823;

	private readonly TextReader _reader;

	private char[]? _chars;

	private int _charsUsed;

	private int _charPos;

	private int _lineStartPos;

	private int _lineNumber;

	private bool _isEndOfFile;

	private StringBuffer _stringBuffer;

	private StringReference _stringReference;

	private IArrayPool<char>? _arrayPool;

	public JsonNameTable? PropertyNameTable { get; set; }

	public IArrayPool<char>? ArrayPool
	{
		get
		{
			return _arrayPool;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_arrayPool = value;
		}
	}

	public int LineNumber
	{
		get
		{
			if (base.CurrentState == State.Start && LinePosition == 0 && TokenType != JsonToken.Comment)
			{
				return 0;
			}
			return _lineNumber;
		}
	}

	public int LinePosition => _charPos - _lineStartPos;

	public override Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsync(cancellationToken);
		}
		return DoReadAsync(cancellationToken);
	}

	internal Task<bool> DoReadAsync(CancellationToken cancellationToken)
	{
		EnsureBuffer();
		Task<bool> task;
		do
		{
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.ConstructorStart:
			case State.Constructor:
				return ParseValueAsync(cancellationToken);
			case State.ObjectStart:
			case State.Object:
				return ParseObjectAsync(cancellationToken);
			case State.PostValue:
				task = ParsePostValueAsync(ignoreComments: false, cancellationToken);
				if (!task.IsCompletedSucessfully())
				{
					return DoReadAsync(task, cancellationToken);
				}
				break;
			case State.Finished:
				return ReadFromFinishedAsync(cancellationToken);
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}
		while (!task.Result);
		return AsyncUtils.True;
	}

	private async Task<bool> DoReadAsync(Task<bool> task, CancellationToken cancellationToken)
	{
		if (await task.ConfigureAwait(continueOnCapturedContext: false))
		{
			return true;
		}
		return await DoReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<bool> ParsePostValueAsync(bool ignoreComments, CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (await ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						_currentState = State.Finished;
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				continue;
			case '}':
				_charPos++;
				SetToken(JsonToken.EndObject);
				return true;
			case ']':
				_charPos++;
				SetToken(JsonToken.EndArray);
				return true;
			case ')':
				_charPos++;
				SetToken(JsonToken.EndConstructor);
				return true;
			case '/':
				await ParseCommentAsync(!ignoreComments, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!ignoreComments)
				{
					return true;
				}
				continue;
			case ',':
				_charPos++;
				SetStateBasedOnCurrent();
				return false;
			case '\t':
			case ' ':
				_charPos++;
				continue;
			case '\r':
				await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				continue;
			case '\n':
				ProcessLineFeed();
				continue;
			}
			if (char.IsWhiteSpace(c))
			{
				_charPos++;
				continue;
			}
			if (base.SupportMultipleContent && Depth == 0)
			{
				SetStateBasedOnCurrent();
				return false;
			}
			throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}
	}

	private async Task<bool> ReadFromFinishedAsync(CancellationToken cancellationToken)
	{
		if (await EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (_isEndOfFile)
			{
				SetToken(JsonToken.None);
				return false;
			}
			if (_chars[_charPos] == '/')
			{
				await ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
		}
		SetToken(JsonToken.None);
		return false;
	}

	private Task<int> ReadDataAsync(bool append, CancellationToken cancellationToken)
	{
		return ReadDataAsync(append, 0, cancellationToken);
	}

	private async Task<int> ReadDataAsync(bool append, int charsRequired, CancellationToken cancellationToken)
	{
		if (_isEndOfFile)
		{
			return 0;
		}
		PrepareBufferForReadData(append, charsRequired);
		int num = await _reader.ReadAsync(_chars, _charsUsed, _chars!.Length - _charsUsed - 1, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		_charsUsed += num;
		if (num == 0)
		{
			_isEndOfFile = true;
		}
		_chars[_charsUsed] = '\0';
		return num;
	}

	private async Task<bool> ParseValueAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (await ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '"':
			case '\'':
				await ParseStringAsync(c, ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 't':
				await ParseTrueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'f':
				await ParseFalseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'n':
				if (await EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					switch (_chars[_charPos + 1])
					{
					case 'u':
						await ParseNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					case 'e':
						await ParseConstructorAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					default:
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					return true;
				}
				_charPos++;
				throw CreateUnexpectedEndException();
			case 'N':
				await ParseNumberNaNAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'I':
				await ParseNumberPositiveInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '-':
				if (!(await EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) || _chars[_charPos + 1] != 'I')
				{
					await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await ParseNumberNegativeInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				return true;
			case '/':
				await ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'u':
				await ParseUndefinedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '{':
				_charPos++;
				SetToken(JsonToken.StartObject);
				return true;
			case '[':
				_charPos++;
				SetToken(JsonToken.StartArray);
				return true;
			case ']':
				_charPos++;
				SetToken(JsonToken.EndArray);
				return true;
			case ',':
				SetToken(JsonToken.Undefined);
				return true;
			case ')':
				_charPos++;
				SetToken(JsonToken.EndConstructor);
				return true;
			case '\r':
				await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				_charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					_charPos++;
					break;
				}
				if (char.IsNumber(c) || c == '-' || c == '.')
				{
					await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return true;
				}
				throw CreateUnexpectedCharacterException(c);
			}
		}
	}

	private async Task ReadStringIntoBufferAsync(char quote, CancellationToken cancellationToken)
	{
		int charPos = _charPos;
		int initialPosition = _charPos;
		int lastWritePosition = _charPos;
		_stringBuffer.Position = 0;
		while (true)
		{
			switch (_chars[charPos++])
			{
			case '\0':
				if (_charsUsed == charPos - 1)
				{
					charPos--;
					if (await ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						_charPos = charPos;
						throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
					}
				}
				break;
			case '\\':
			{
				_charPos = charPos;
				if (!(await EnsureCharsAsync(0, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
				}
				int escapeStartPos = charPos - 1;
				char c = _chars[charPos];
				charPos++;
				char writeChar;
				switch (c)
				{
				case 'b':
					writeChar = '\b';
					break;
				case 't':
					writeChar = '\t';
					break;
				case 'n':
					writeChar = '\n';
					break;
				case 'f':
					writeChar = '\f';
					break;
				case 'r':
					writeChar = '\r';
					break;
				case '\\':
					writeChar = '\\';
					break;
				case '"':
				case '\'':
				case '/':
					writeChar = c;
					break;
				case 'u':
					_charPos = charPos;
					writeChar = await ParseUnicodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (StringUtils.IsLowSurrogate(writeChar))
					{
						writeChar = '\ufffd';
					}
					else if (StringUtils.IsHighSurrogate(writeChar))
					{
						bool anotherHighSurrogate;
						do
						{
							anotherHighSurrogate = false;
							if (await EnsureCharsAsync(2, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && _chars[_charPos] == '\\' && _chars[_charPos + 1] == 'u')
							{
								char highSurrogate = writeChar;
								_charPos += 2;
								writeChar = await ParseUnicodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
								if (!StringUtils.IsLowSurrogate(writeChar))
								{
									if (StringUtils.IsHighSurrogate(writeChar))
									{
										highSurrogate = '\ufffd';
										anotherHighSurrogate = true;
									}
									else
									{
										highSurrogate = '\ufffd';
									}
								}
								EnsureBufferNotEmpty();
								WriteCharToBuffer(highSurrogate, lastWritePosition, escapeStartPos);
								lastWritePosition = _charPos;
							}
							else
							{
								writeChar = '\ufffd';
							}
						}
						while (anotherHighSurrogate);
					}
					charPos = _charPos;
					break;
				default:
					_charPos = charPos;
					throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, "\\" + c));
				}
				EnsureBufferNotEmpty();
				WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);
				lastWritePosition = charPos;
				break;
			}
			case '\r':
				_charPos = charPos - 1;
				await ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				charPos = _charPos;
				break;
			case '\n':
				_charPos = charPos - 1;
				ProcessLineFeed();
				charPos = _charPos;
				break;
			case '"':
			case '\'':
				if (_chars[charPos - 1] == quote)
				{
					FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
					return;
				}
				break;
			}
		}
	}

	private Task ProcessCarriageReturnAsync(bool append, CancellationToken cancellationToken)
	{
		_charPos++;
		Task<bool> task = EnsureCharsAsync(1, append, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			SetNewLine(task.Result);
			return AsyncUtils.CompletedTask;
		}
		return ProcessCarriageReturnAsync(task);
	}

	private async Task ProcessCarriageReturnAsync(Task<bool> task)
	{
		SetNewLine(await task.ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<char> ParseUnicodeAsync(CancellationToken cancellationToken)
	{
		return ConvertUnicode(await EnsureCharsAsync(4, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private Task<bool> EnsureCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
	{
		if (_charPos + relativePosition < _charsUsed)
		{
			return AsyncUtils.True;
		}
		if (_isEndOfFile)
		{
			return AsyncUtils.False;
		}
		return ReadCharsAsync(relativePosition, append, cancellationToken);
	}

	private async Task<bool> ReadCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
	{
		int charsRequired = _charPos + relativePosition - _charsUsed + 1;
		do
		{
			int num = await ReadDataAsync(append, charsRequired, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == 0)
			{
				return false;
			}
			charsRequired -= num;
		}
		while (charsRequired > 0);
		return true;
	}

	private async Task<bool> ParseObjectAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (await ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '}':
				SetToken(JsonToken.EndObject);
				_charPos++;
				return true;
			case '/':
				await ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '\r':
				await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				_charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					_charPos++;
					break;
				}
				return await ParsePropertyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private async Task ParseCommentAsync(bool setToken, CancellationToken cancellationToken)
	{
		_charPos++;
		if (!(await EnsureCharsAsync(1, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
		}
		bool singlelineComment;
		if (_chars[_charPos] == '*')
		{
			singlelineComment = false;
		}
		else
		{
			if (_chars[_charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			singlelineComment = true;
		}
		_charPos++;
		int initialPosition = _charPos;
		while (true)
		{
			switch (_chars[_charPos])
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (await ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						if (!singlelineComment)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
						}
						EndComment(setToken, initialPosition, _charPos);
						return;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '*':
				_charPos++;
				if (!singlelineComment && await EnsureCharsAsync(0, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && _chars[_charPos] == '/')
				{
					EndComment(setToken, initialPosition, _charPos - 1);
					_charPos++;
					return;
				}
				break;
			case '\r':
				if (singlelineComment)
				{
					EndComment(setToken, initialPosition, _charPos);
					return;
				}
				await ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				if (singlelineComment)
				{
					EndComment(setToken, initialPosition, _charPos);
					return;
				}
				ProcessLineFeed();
				break;
			default:
				_charPos++;
				break;
			}
		}
	}

	private async Task EatWhitespaceAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (await ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '\r':
				await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			default:
				if (!char.IsWhiteSpace(c))
				{
					return;
				}
				goto case ' ';
			case ' ':
				_charPos++;
				break;
			}
		}
	}

	private async Task ParseStringAsync(char quote, ReadType readType, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_charPos++;
		ShiftBufferIfNeeded();
		await ReadStringIntoBufferAsync(quote, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ParseReadString(quote, readType);
	}

	private async Task<bool> MatchValueAsync(string value, CancellationToken cancellationToken)
	{
		return MatchValue(await EnsureCharsAsync(value.Length - 1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), value);
	}

	private async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, CancellationToken cancellationToken)
	{
		if (!(await MatchValueAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return false;
		}
		if (!(await EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return true;
		}
		return IsSeparator(_chars[_charPos]) || _chars[_charPos] == '\0';
	}

	private async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, CancellationToken cancellationToken)
	{
		if (await MatchValueWithTrailingSeparatorAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			SetToken(newToken, tokenValue);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing " + newToken.ToString().ToLowerInvariant() + " value.");
	}

	private Task ParseTrueAsync(CancellationToken cancellationToken)
	{
		return MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancellationToken);
	}

	private Task ParseFalseAsync(CancellationToken cancellationToken)
	{
		return MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancellationToken);
	}

	private Task ParseNullAsync(CancellationToken cancellationToken)
	{
		return MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancellationToken);
	}

	private async Task ParseConstructorAsync(CancellationToken cancellationToken)
	{
		if (await MatchValueWithTrailingSeparatorAsync("new", cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			int initialPosition = _charPos;
			int endPosition;
			while (true)
			{
				char c = _chars[_charPos];
				if (c == '\0')
				{
					if (_charsUsed == _charPos)
					{
						if (await ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
						}
						continue;
					}
					endPosition = _charPos;
					_charPos++;
					break;
				}
				if (char.IsLetterOrDigit(c))
				{
					_charPos++;
					continue;
				}
				switch (c)
				{
				case '\r':
					endPosition = _charPos;
					await ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					endPosition = _charPos;
					ProcessLineFeed();
					break;
				default:
					if (char.IsWhiteSpace(c))
					{
						endPosition = _charPos;
						_charPos++;
						break;
					}
					if (c == '(')
					{
						endPosition = _charPos;
						break;
					}
					throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
				}
				break;
			}
			_stringReference = new StringReference(_chars, initialPosition, endPosition - initialPosition);
			string constructorName = _stringReference.ToString();
			await EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (_chars[_charPos] != '(')
			{
				throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			_charPos++;
			ClearRecentString();
			SetToken(JsonToken.StartConstructor, constructorName);
			return;
		}
		throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
	}

	private async Task<object> ParseNumberNaNAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return ParseNumberNaN(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return ParseNumberPositiveInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return ParseNumberNegativeInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task ParseNumberAsync(ReadType readType, CancellationToken cancellationToken)
	{
		ShiftBufferIfNeeded();
		char firstChar = _chars[_charPos];
		int initialPosition = _charPos;
		await ReadNumberIntoBufferAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ParseReadNumber(readType, firstChar, initialPosition);
	}

	private Task ParseUndefinedAsync(CancellationToken cancellationToken)
	{
		return MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellationToken);
	}

	private async Task<bool> ParsePropertyAsync(CancellationToken cancellationToken)
	{
		char c = _chars[_charPos];
		char quoteChar;
		if (c == '"' || c == '\'')
		{
			_charPos++;
			quoteChar = c;
			ShiftBufferIfNeeded();
			await ReadStringIntoBufferAsync(quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			if (!ValidIdentifierChar(c))
			{
				throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			quoteChar = '\0';
			ShiftBufferIfNeeded();
			await ParseUnquotedPropertyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		string propertyName = ((PropertyNameTable == null) ? _stringReference.ToString() : (PropertyNameTable!.Get(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length) ?? _stringReference.ToString()));
		await EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (_chars[_charPos] != ':')
		{
			throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
		}
		_charPos++;
		SetToken(JsonToken.PropertyName, propertyName);
		_quoteChar = quoteChar;
		ClearRecentString();
		return true;
	}

	private async Task ReadNumberIntoBufferAsync(CancellationToken cancellationToken)
	{
		int charPos = _charPos;
		while (true)
		{
			char c = _chars[charPos];
			if (c == '\0')
			{
				_charPos = charPos;
				if (_charsUsed != charPos || await ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
				{
					break;
				}
			}
			else
			{
				if (ReadNumberCharIntoBuffer(c, charPos))
				{
					break;
				}
				charPos++;
			}
		}
	}

	private async Task ParseUnquotedPropertyAsync(CancellationToken cancellationToken)
	{
		int initialPosition = _charPos;
		while (true)
		{
			char c = _chars[_charPos];
			if (c == '\0')
			{
				if (_charsUsed != _charPos)
				{
					_stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
					break;
				}
				if (await ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
				{
					throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
				}
			}
			else if (ReadUnquotedPropertyReportIfDone(c, initialPosition))
			{
				break;
			}
		}
	}

	private async Task<bool> ReadNullCharAsync(CancellationToken cancellationToken)
	{
		if (_charsUsed == _charPos)
		{
			if (await ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
			{
				_isEndOfFile = true;
				return true;
			}
		}
		else
		{
			_charPos++;
		}
		return false;
	}

	private async Task HandleNullAsync(CancellationToken cancellationToken)
	{
		if (await EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			if (_chars[_charPos + 1] == 'u')
			{
				await ParseNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return;
			}
			_charPos += 2;
			throw CreateUnexpectedCharacterException(_chars[_charPos - 1]);
		}
		_charPos = _charsUsed;
		throw CreateUnexpectedEndException();
	}

	private async Task ReadFinishedAsync(CancellationToken cancellationToken)
	{
		if (await EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (_isEndOfFile)
			{
				SetToken(JsonToken.None);
				return;
			}
			if (_chars[_charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			await ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SetToken(JsonToken.None);
	}

	private async Task<object?> ReadStringValueAsync(ReadType readType, CancellationToken cancellationToken)
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (await ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await ParseStringAsync(c, readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return FinishReadQuotedStringValue(readType);
				case '-':
					if (await EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && _chars[_charPos + 1] == 'I')
					{
						return ParseNumberNegativeInfinity(readType);
					}
					await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (readType != ReadType.ReadAsString)
					{
						_charPos++;
						throw CreateUnexpectedCharacterException(c);
					}
					await ParseNumberAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return Value;
				case 'f':
				case 't':
				{
					if (readType != ReadType.ReadAsString)
					{
						_charPos++;
						throw CreateUnexpectedCharacterException(c);
					}
					string expected = ((c == 't') ? JsonConvert.True : JsonConvert.False);
					if (!(await MatchValueWithTrailingSeparatorAsync(expected, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					SetToken(JsonToken.String, expected);
					return expected;
				}
				case 'I':
					return await ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'N':
					return await ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'n':
					await HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '/':
					await ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private async Task<object?> ReadNumberValueAsync(ReadType readType, CancellationToken cancellationToken)
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (await ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await ParseStringAsync(c, readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return FinishReadQuotedNumber(readType);
				case 'n':
					await HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case 'N':
					return await ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'I':
					return await ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case '-':
					if (await EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && _chars[_charPos + 1] == 'I')
					{
						return await ParseNumberNegativeInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return Value;
				case '/':
					await ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsBooleanAsync(cancellationToken);
		}
		return DoReadAsBooleanAsync(cancellationToken);
	}

	internal async Task<bool?> DoReadAsBooleanAsync(CancellationToken cancellationToken)
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (await ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await ParseStringAsync(c, ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return ReadBooleanString(_stringReference.ToString());
				case 'n':
					await HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					bool flag = ((!(Value is BigInteger i)) ? Convert.ToBoolean(Value, CultureInfo.InvariantCulture) : (i != 0L));
					SetToken(JsonToken.Boolean, flag, updateIndex: false);
					return flag;
				}
				case 'f':
				case 't':
				{
					bool isTrue = c == 't';
					if (!(await MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					SetToken(JsonToken.Boolean, isTrue);
					return isTrue;
				}
				case '/':
					await ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	public override Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsBytesAsync(cancellationToken);
		}
		return DoReadAsBytesAsync(cancellationToken);
	}

	internal async Task<byte[]?> DoReadAsBytesAsync(CancellationToken cancellationToken)
	{
		EnsureBuffer();
		bool isWrapped = false;
		switch (_currentState)
		{
		case State.PostValue:
			if (await ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
				{
					await ParseStringAsync(c, ReadType.ReadAsBytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					byte[] data = (byte[])Value;
					if (isWrapped)
					{
						await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (TokenType != JsonToken.EndObject)
						{
							throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
						}
						SetToken(JsonToken.Bytes, data, updateIndex: false);
					}
					return data;
				}
				case '{':
					_charPos++;
					SetToken(JsonToken.StartObject);
					await ReadIntoWrappedTypeObjectAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					isWrapped = true;
					break;
				case '[':
					_charPos++;
					SetToken(JsonToken.StartArray);
					return await ReadArrayIntoByteArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'n':
					await HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '/':
					await ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					await ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private async Task ReadIntoWrappedTypeObjectAsync(CancellationToken cancellationToken)
	{
		await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (Value != null && Value!.ToString() == "$type")
		{
			await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (Value != null && Value!.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
			{
				await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (Value!.ToString() == "$value")
				{
					return;
				}
			}
		}
		throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
	}

	public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsDateTimeAsync(cancellationToken);
		}
		return DoReadAsDateTimeAsync(cancellationToken);
	}

	internal async Task<DateTime?> DoReadAsDateTimeAsync(CancellationToken cancellationToken)
	{
		return (DateTime?)(await ReadStringValueAsync(ReadType.ReadAsDateTime, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsDateTimeOffsetAsync(cancellationToken);
		}
		return DoReadAsDateTimeOffsetAsync(cancellationToken);
	}

	internal async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(CancellationToken cancellationToken)
	{
		return (DateTimeOffset?)(await ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsDecimalAsync(cancellationToken);
		}
		return DoReadAsDecimalAsync(cancellationToken);
	}

	internal async Task<decimal?> DoReadAsDecimalAsync(CancellationToken cancellationToken)
	{
		return (decimal?)(await ReadNumberValueAsync(ReadType.ReadAsDecimal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsDoubleAsync(cancellationToken);
		}
		return DoReadAsDoubleAsync(cancellationToken);
	}

	internal async Task<double?> DoReadAsDoubleAsync(CancellationToken cancellationToken)
	{
		return (double?)(await ReadNumberValueAsync(ReadType.ReadAsDouble, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsInt32Async(cancellationToken);
		}
		return DoReadAsInt32Async(cancellationToken);
	}

	internal async Task<int?> DoReadAsInt32Async(CancellationToken cancellationToken)
	{
		return (int?)(await ReadNumberValueAsync(ReadType.ReadAsInt32, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.ReadAsStringAsync(cancellationToken);
		}
		return DoReadAsStringAsync(cancellationToken);
	}

	internal async Task<string?> DoReadAsStringAsync(CancellationToken cancellationToken)
	{
		return (string)(await ReadStringValueAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public JsonTextReader(TextReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		_reader = reader;
		_lineNumber = 1;
		_safeAsync = GetType() == typeof(JsonTextReader);
	}

	private void EnsureBufferNotEmpty()
	{
		if (_stringBuffer.IsEmpty)
		{
			_stringBuffer = new StringBuffer(_arrayPool, 1024);
		}
	}

	private void SetNewLine(bool hasNextChar)
	{
		if (hasNextChar && _chars[_charPos] == '\n')
		{
			_charPos++;
		}
		OnNewLine(_charPos);
	}

	private void OnNewLine(int pos)
	{
		_lineNumber++;
		_lineStartPos = pos;
	}

	private void ParseString(char quote, ReadType readType)
	{
		_charPos++;
		ShiftBufferIfNeeded();
		ReadStringIntoBuffer(quote);
		ParseReadString(quote, readType);
	}

	private void ParseReadString(char quote, ReadType readType)
	{
		SetPostValueState(updateIndex: true);
		switch (readType)
		{
		case ReadType.ReadAsBytes:
		{
			Guid g;
			byte[] value2 = ((_stringReference.Length == 0) ? CollectionUtils.ArrayEmpty<byte>() : ((_stringReference.Length != 36 || !ConvertUtils.TryConvertGuid(_stringReference.ToString(), out g)) ? Convert.FromBase64CharArray(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length) : g.ToByteArray()));
			SetToken(JsonToken.Bytes, value2, updateIndex: false);
			return;
		}
		case ReadType.ReadAsString:
		{
			string value = _stringReference.ToString();
			SetToken(JsonToken.String, value, updateIndex: false);
			_quoteChar = quote;
			return;
		}
		case ReadType.ReadAsInt32:
		case ReadType.ReadAsDecimal:
		case ReadType.ReadAsBoolean:
			return;
		}
		if (_dateParseHandling != 0)
		{
			DateTimeOffset dt2;
			if (readType switch
			{
				ReadType.ReadAsDateTime => 1, 
				ReadType.ReadAsDateTimeOffset => 2, 
				_ => (int)_dateParseHandling, 
			} == 1)
			{
				if (DateTimeUtils.TryParseDateTime(_stringReference, base.DateTimeZoneHandling, base.DateFormatString, base.Culture, out var dt))
				{
					SetToken(JsonToken.Date, dt, updateIndex: false);
					return;
				}
			}
			else if (DateTimeUtils.TryParseDateTimeOffset(_stringReference, base.DateFormatString, base.Culture, out dt2))
			{
				SetToken(JsonToken.Date, dt2, updateIndex: false);
				return;
			}
		}
		SetToken(JsonToken.String, _stringReference.ToString(), updateIndex: false);
		_quoteChar = quote;
	}

	private static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
	{
		Buffer.BlockCopy(src, srcOffset * 2, dst, dstOffset * 2, count * 2);
	}

	private void ShiftBufferIfNeeded()
	{
		int num = _chars!.Length;
		if ((double)(num - _charPos) <= (double)num * 0.1 || num >= 1073741823)
		{
			int num2 = _charsUsed - _charPos;
			if (num2 > 0)
			{
				BlockCopyChars(_chars, _charPos, _chars, 0, num2);
			}
			_lineStartPos -= _charPos;
			_charPos = 0;
			_charsUsed = num2;
			_chars[_charsUsed] = '\0';
		}
	}

	private int ReadData(bool append)
	{
		return ReadData(append, 0);
	}

	private void PrepareBufferForReadData(bool append, int charsRequired)
	{
		if (_charsUsed + charsRequired < _chars!.Length - 1)
		{
			return;
		}
		if (append)
		{
			int num = _chars!.Length * 2;
			int minSize = Math.Max((num < 0) ? int.MaxValue : num, _charsUsed + charsRequired + 1);
			char[] array = BufferUtils.RentBuffer(_arrayPool, minSize);
			BlockCopyChars(_chars, 0, array, 0, _chars!.Length);
			BufferUtils.ReturnBuffer(_arrayPool, _chars);
			_chars = array;
			return;
		}
		int num2 = _charsUsed - _charPos;
		if (num2 + charsRequired + 1 >= _chars!.Length)
		{
			char[] array2 = BufferUtils.RentBuffer(_arrayPool, num2 + charsRequired + 1);
			if (num2 > 0)
			{
				BlockCopyChars(_chars, _charPos, array2, 0, num2);
			}
			BufferUtils.ReturnBuffer(_arrayPool, _chars);
			_chars = array2;
		}
		else if (num2 > 0)
		{
			BlockCopyChars(_chars, _charPos, _chars, 0, num2);
		}
		_lineStartPos -= _charPos;
		_charPos = 0;
		_charsUsed = num2;
	}

	private int ReadData(bool append, int charsRequired)
	{
		if (_isEndOfFile)
		{
			return 0;
		}
		PrepareBufferForReadData(append, charsRequired);
		int count = _chars!.Length - _charsUsed - 1;
		int num = _reader.Read(_chars, _charsUsed, count);
		_charsUsed += num;
		if (num == 0)
		{
			_isEndOfFile = true;
		}
		_chars[_charsUsed] = '\0';
		return num;
	}

	private bool EnsureChars(int relativePosition, bool append)
	{
		if (_charPos + relativePosition >= _charsUsed)
		{
			return ReadChars(relativePosition, append);
		}
		return true;
	}

	private bool ReadChars(int relativePosition, bool append)
	{
		if (_isEndOfFile)
		{
			return false;
		}
		int num = _charPos + relativePosition - _charsUsed + 1;
		int num2 = 0;
		do
		{
			int num3 = ReadData(append, num - num2);
			if (num3 == 0)
			{
				break;
			}
			num2 += num3;
		}
		while (num2 < num);
		if (num2 < num)
		{
			return false;
		}
		return true;
	}

	public override bool Read()
	{
		EnsureBuffer();
		do
		{
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.ConstructorStart:
			case State.Constructor:
				return ParseValue();
			case State.ObjectStart:
			case State.Object:
				return ParseObject();
			case State.PostValue:
				break;
			case State.Finished:
				if (EnsureChars(0, append: false))
				{
					EatWhitespace();
					if (_isEndOfFile)
					{
						SetToken(JsonToken.None);
						return false;
					}
					if (_chars[_charPos] == '/')
					{
						ParseComment(setToken: true);
						return true;
					}
					throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
				}
				SetToken(JsonToken.None);
				return false;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}
		while (!ParsePostValue(ignoreComments: false));
		return true;
	}

	public override int? ReadAsInt32()
	{
		return (int?)ReadNumberValue(ReadType.ReadAsInt32);
	}

	public override DateTime? ReadAsDateTime()
	{
		return (DateTime?)ReadStringValue(ReadType.ReadAsDateTime);
	}

	public override string? ReadAsString()
	{
		return (string)ReadStringValue(ReadType.ReadAsString);
	}

	public override byte[]? ReadAsBytes()
	{
		EnsureBuffer();
		bool flag = false;
		switch (_currentState)
		{
		case State.PostValue:
			if (ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (ReadNullChar())
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
				{
					ParseString(c, ReadType.ReadAsBytes);
					byte[] array = (byte[])Value;
					if (flag)
					{
						ReaderReadAndAssert();
						if (TokenType != JsonToken.EndObject)
						{
							throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
						}
						SetToken(JsonToken.Bytes, array, updateIndex: false);
					}
					return array;
				}
				case '{':
					_charPos++;
					SetToken(JsonToken.StartObject);
					ReadIntoWrappedTypeObject();
					flag = true;
					break;
				case '[':
					_charPos++;
					SetToken(JsonToken.StartArray);
					return ReadArrayIntoByteArray();
				case 'n':
					HandleNull();
					return null;
				case '/':
					ParseComment(setToken: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					ProcessCarriageReturn(append: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? ReadStringValue(ReadType readType)
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (ReadNullChar())
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					ParseString(c, readType);
					return FinishReadQuotedStringValue(readType);
				case '-':
					if (EnsureChars(1, append: true) && _chars[_charPos + 1] == 'I')
					{
						return ParseNumberNegativeInfinity(readType);
					}
					ParseNumber(readType);
					return Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (readType != ReadType.ReadAsString)
					{
						_charPos++;
						throw CreateUnexpectedCharacterException(c);
					}
					ParseNumber(ReadType.ReadAsString);
					return Value;
				case 'f':
				case 't':
				{
					if (readType != ReadType.ReadAsString)
					{
						_charPos++;
						throw CreateUnexpectedCharacterException(c);
					}
					string text = ((c == 't') ? JsonConvert.True : JsonConvert.False);
					if (!MatchValueWithTrailingSeparator(text))
					{
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					SetToken(JsonToken.String, text);
					return text;
				}
				case 'I':
					return ParseNumberPositiveInfinity(readType);
				case 'N':
					return ParseNumberNaN(readType);
				case 'n':
					HandleNull();
					return null;
				case '/':
					ParseComment(setToken: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					ProcessCarriageReturn(append: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? FinishReadQuotedStringValue(ReadType readType)
	{
		switch (readType)
		{
		case ReadType.ReadAsBytes:
		case ReadType.ReadAsString:
			return Value;
		case ReadType.ReadAsDateTime:
			if (Value is DateTime dateTime)
			{
				return dateTime;
			}
			return ReadDateTimeString((string)Value);
		case ReadType.ReadAsDateTimeOffset:
			if (Value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset;
			}
			return ReadDateTimeOffsetString((string)Value);
		default:
			throw new ArgumentOutOfRangeException("readType");
		}
	}

	private JsonReaderException CreateUnexpectedCharacterException(char c)
	{
		return JsonReaderException.Create(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
	}

	public override bool? ReadAsBoolean()
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (ReadNullChar())
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					ParseString(c, ReadType.Read);
					return ReadBooleanString(_stringReference.ToString());
				case 'n':
					HandleNull();
					return null;
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					ParseNumber(ReadType.Read);
					bool flag2 = ((!(Value is BigInteger bigInteger)) ? Convert.ToBoolean(Value, CultureInfo.InvariantCulture) : (bigInteger != 0L));
					SetToken(JsonToken.Boolean, flag2, updateIndex: false);
					return flag2;
				}
				case 'f':
				case 't':
				{
					bool flag = c == 't';
					string value = (flag ? JsonConvert.True : JsonConvert.False);
					if (!MatchValueWithTrailingSeparator(value))
					{
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					SetToken(JsonToken.Boolean, flag);
					return flag;
				}
				case '/':
					ParseComment(setToken: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					ProcessCarriageReturn(append: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private void ProcessValueComma()
	{
		_charPos++;
		if (_currentState != State.PostValue)
		{
			SetToken(JsonToken.Undefined);
			JsonReaderException ex = CreateUnexpectedCharacterException(',');
			_charPos--;
			throw ex;
		}
		SetStateBasedOnCurrent();
	}

	private object? ReadNumberValue(ReadType readType)
	{
		EnsureBuffer();
		switch (_currentState)
		{
		case State.PostValue:
			if (ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (ReadNullChar())
					{
						SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					ParseString(c, readType);
					return FinishReadQuotedNumber(readType);
				case 'n':
					HandleNull();
					return null;
				case 'N':
					return ParseNumberNaN(readType);
				case 'I':
					return ParseNumberPositiveInfinity(readType);
				case '-':
					if (EnsureChars(1, append: true) && _chars[_charPos + 1] == 'I')
					{
						return ParseNumberNegativeInfinity(readType);
					}
					ParseNumber(readType);
					return Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					ParseNumber(readType);
					return Value;
				case '/':
					ParseComment(setToken: false);
					break;
				case ',':
					ProcessValueComma();
					break;
				case ']':
					_charPos++;
					if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
					{
						SetToken(JsonToken.EndArray);
						return null;
					}
					throw CreateUnexpectedCharacterException(c);
				case '\r':
					ProcessCarriageReturn(append: false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					_charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? FinishReadQuotedNumber(ReadType readType)
	{
		return readType switch
		{
			ReadType.ReadAsInt32 => ReadInt32String(_stringReference.ToString()), 
			ReadType.ReadAsDecimal => ReadDecimalString(_stringReference.ToString()), 
			ReadType.ReadAsDouble => ReadDoubleString(_stringReference.ToString()), 
			_ => throw new ArgumentOutOfRangeException("readType"), 
		};
	}

	public override DateTimeOffset? ReadAsDateTimeOffset()
	{
		return (DateTimeOffset?)ReadStringValue(ReadType.ReadAsDateTimeOffset);
	}

	public override decimal? ReadAsDecimal()
	{
		return (decimal?)ReadNumberValue(ReadType.ReadAsDecimal);
	}

	public override double? ReadAsDouble()
	{
		return (double?)ReadNumberValue(ReadType.ReadAsDouble);
	}

	private void HandleNull()
	{
		if (EnsureChars(1, append: true))
		{
			if (_chars[_charPos + 1] == 'u')
			{
				ParseNull();
				return;
			}
			_charPos += 2;
			throw CreateUnexpectedCharacterException(_chars[_charPos - 1]);
		}
		_charPos = _charsUsed;
		throw CreateUnexpectedEndException();
	}

	private void ReadFinished()
	{
		if (EnsureChars(0, append: false))
		{
			EatWhitespace();
			if (_isEndOfFile)
			{
				return;
			}
			if (_chars[_charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			ParseComment(setToken: false);
		}
		SetToken(JsonToken.None);
	}

	private bool ReadNullChar()
	{
		if (_charsUsed == _charPos)
		{
			if (ReadData(append: false) == 0)
			{
				_isEndOfFile = true;
				return true;
			}
		}
		else
		{
			_charPos++;
		}
		return false;
	}

	private void EnsureBuffer()
	{
		if (_chars == null)
		{
			_chars = BufferUtils.RentBuffer(_arrayPool, 1024);
			_chars[0] = '\0';
		}
	}

	private void ReadStringIntoBuffer(char quote)
	{
		int num = _charPos;
		int charPos = _charPos;
		int lastWritePosition = _charPos;
		_stringBuffer.Position = 0;
		while (true)
		{
			switch (_chars[num++])
			{
			case '\0':
				if (_charsUsed == num - 1)
				{
					num--;
					if (ReadData(append: true) == 0)
					{
						_charPos = num;
						throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
					}
				}
				break;
			case '\\':
			{
				_charPos = num;
				if (!EnsureChars(0, append: true))
				{
					throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
				}
				int writeToPosition = num - 1;
				char c = _chars[num];
				num++;
				char c2;
				switch (c)
				{
				case 'b':
					c2 = '\b';
					break;
				case 't':
					c2 = '\t';
					break;
				case 'n':
					c2 = '\n';
					break;
				case 'f':
					c2 = '\f';
					break;
				case 'r':
					c2 = '\r';
					break;
				case '\\':
					c2 = '\\';
					break;
				case '"':
				case '\'':
				case '/':
					c2 = c;
					break;
				case 'u':
					_charPos = num;
					c2 = ParseUnicode();
					if (StringUtils.IsLowSurrogate(c2))
					{
						c2 = '\ufffd';
					}
					else if (StringUtils.IsHighSurrogate(c2))
					{
						bool flag;
						do
						{
							flag = false;
							if (EnsureChars(2, append: true) && _chars[_charPos] == '\\' && _chars[_charPos + 1] == 'u')
							{
								char writeChar = c2;
								_charPos += 2;
								c2 = ParseUnicode();
								if (!StringUtils.IsLowSurrogate(c2))
								{
									if (StringUtils.IsHighSurrogate(c2))
									{
										writeChar = '\ufffd';
										flag = true;
									}
									else
									{
										writeChar = '\ufffd';
									}
								}
								EnsureBufferNotEmpty();
								WriteCharToBuffer(writeChar, lastWritePosition, writeToPosition);
								lastWritePosition = _charPos;
							}
							else
							{
								c2 = '\ufffd';
							}
						}
						while (flag);
					}
					num = _charPos;
					break;
				default:
					_charPos = num;
					throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, "\\" + c));
				}
				EnsureBufferNotEmpty();
				WriteCharToBuffer(c2, lastWritePosition, writeToPosition);
				lastWritePosition = num;
				break;
			}
			case '\r':
				_charPos = num - 1;
				ProcessCarriageReturn(append: true);
				num = _charPos;
				break;
			case '\n':
				_charPos = num - 1;
				ProcessLineFeed();
				num = _charPos;
				break;
			case '"':
			case '\'':
				if (_chars[num - 1] == quote)
				{
					FinishReadStringIntoBuffer(num - 1, charPos, lastWritePosition);
					return;
				}
				break;
			}
		}
	}

	private void FinishReadStringIntoBuffer(int charPos, int initialPosition, int lastWritePosition)
	{
		if (initialPosition == lastWritePosition)
		{
			_stringReference = new StringReference(_chars, initialPosition, charPos - initialPosition);
		}
		else
		{
			EnsureBufferNotEmpty();
			if (charPos > lastWritePosition)
			{
				_stringBuffer.Append(_arrayPool, _chars, lastWritePosition, charPos - lastWritePosition);
			}
			_stringReference = new StringReference(_stringBuffer.InternalBuffer, 0, _stringBuffer.Position);
		}
		_charPos = charPos + 1;
	}

	private void WriteCharToBuffer(char writeChar, int lastWritePosition, int writeToPosition)
	{
		if (writeToPosition > lastWritePosition)
		{
			_stringBuffer.Append(_arrayPool, _chars, lastWritePosition, writeToPosition - lastWritePosition);
		}
		_stringBuffer.Append(_arrayPool, writeChar);
	}

	private char ConvertUnicode(bool enoughChars)
	{
		if (enoughChars)
		{
			if (ConvertUtils.TryHexTextToInt(_chars, _charPos, _charPos + 4, out var value))
			{
				char result = Convert.ToChar(value);
				_charPos += 4;
				return result;
			}
			throw JsonReaderException.Create(this, "Invalid Unicode escape sequence: \\u{0}.".FormatWith(CultureInfo.InvariantCulture, new string(_chars, _charPos, 4)));
		}
		throw JsonReaderException.Create(this, "Unexpected end while parsing Unicode escape sequence.");
	}

	private char ParseUnicode()
	{
		return ConvertUnicode(EnsureChars(4, append: true));
	}

	private void ReadNumberIntoBuffer()
	{
		int num = _charPos;
		while (true)
		{
			char c = _chars[num];
			if (c == '\0')
			{
				_charPos = num;
				if (_charsUsed != num || ReadData(append: true) == 0)
				{
					break;
				}
			}
			else
			{
				if (ReadNumberCharIntoBuffer(c, num))
				{
					break;
				}
				num++;
			}
		}
	}

	private bool ReadNumberCharIntoBuffer(char currentChar, int charPos)
	{
		switch (currentChar)
		{
		case '+':
		case '-':
		case '.':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'X':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'x':
			return false;
		default:
			_charPos = charPos;
			if (char.IsWhiteSpace(currentChar) || currentChar == ',' || currentChar == '}' || currentChar == ']' || currentChar == ')' || currentChar == '/')
			{
				return true;
			}
			throw JsonReaderException.Create(this, "Unexpected character encountered while parsing number: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
		}
	}

	private void ClearRecentString()
	{
		_stringBuffer.Position = 0;
		_stringReference = default(StringReference);
	}

	private bool ParsePostValue(bool ignoreComments)
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (ReadData(append: false) == 0)
					{
						_currentState = State.Finished;
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				continue;
			case '}':
				_charPos++;
				SetToken(JsonToken.EndObject);
				return true;
			case ']':
				_charPos++;
				SetToken(JsonToken.EndArray);
				return true;
			case ')':
				_charPos++;
				SetToken(JsonToken.EndConstructor);
				return true;
			case '/':
				ParseComment(!ignoreComments);
				if (!ignoreComments)
				{
					return true;
				}
				continue;
			case ',':
				_charPos++;
				SetStateBasedOnCurrent();
				return false;
			case '\t':
			case ' ':
				_charPos++;
				continue;
			case '\r':
				ProcessCarriageReturn(append: false);
				continue;
			case '\n':
				ProcessLineFeed();
				continue;
			}
			if (char.IsWhiteSpace(c))
			{
				_charPos++;
				continue;
			}
			if (base.SupportMultipleContent && Depth == 0)
			{
				SetStateBasedOnCurrent();
				return false;
			}
			throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}
	}

	private bool ParseObject()
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (ReadData(append: false) == 0)
					{
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '}':
				SetToken(JsonToken.EndObject);
				_charPos++;
				return true;
			case '/':
				ParseComment(setToken: true);
				return true;
			case '\r':
				ProcessCarriageReturn(append: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				_charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					_charPos++;
					break;
				}
				return ParseProperty();
			}
		}
	}

	private bool ParseProperty()
	{
		char c = _chars[_charPos];
		char c2;
		if (c == '"' || c == '\'')
		{
			_charPos++;
			c2 = c;
			ShiftBufferIfNeeded();
			ReadStringIntoBuffer(c2);
		}
		else
		{
			if (!ValidIdentifierChar(c))
			{
				throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			c2 = '\0';
			ShiftBufferIfNeeded();
			ParseUnquotedProperty();
		}
		string text;
		if (PropertyNameTable != null)
		{
			text = PropertyNameTable!.Get(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length);
			if (text == null)
			{
				text = _stringReference.ToString();
			}
		}
		else
		{
			text = _stringReference.ToString();
		}
		EatWhitespace();
		if (_chars[_charPos] != ':')
		{
			throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
		}
		_charPos++;
		SetToken(JsonToken.PropertyName, text);
		_quoteChar = c2;
		ClearRecentString();
		return true;
	}

	private bool ValidIdentifierChar(char value)
	{
		if (!char.IsLetterOrDigit(value) && value != '_')
		{
			return value == '$';
		}
		return true;
	}

	private void ParseUnquotedProperty()
	{
		int charPos = _charPos;
		while (true)
		{
			char c = _chars[_charPos];
			if (c == '\0')
			{
				if (_charsUsed != _charPos)
				{
					_stringReference = new StringReference(_chars, charPos, _charPos - charPos);
					break;
				}
				if (ReadData(append: true) == 0)
				{
					throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
				}
			}
			else if (ReadUnquotedPropertyReportIfDone(c, charPos))
			{
				break;
			}
		}
	}

	private bool ReadUnquotedPropertyReportIfDone(char currentChar, int initialPosition)
	{
		if (ValidIdentifierChar(currentChar))
		{
			_charPos++;
			return false;
		}
		if (char.IsWhiteSpace(currentChar) || currentChar == ':')
		{
			_stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
			return true;
		}
		throw JsonReaderException.Create(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
	}

	private bool ParseValue()
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (ReadData(append: false) == 0)
					{
						return false;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '"':
			case '\'':
				ParseString(c, ReadType.Read);
				return true;
			case 't':
				ParseTrue();
				return true;
			case 'f':
				ParseFalse();
				return true;
			case 'n':
				if (EnsureChars(1, append: true))
				{
					switch (_chars[_charPos + 1])
					{
					case 'u':
						ParseNull();
						break;
					case 'e':
						ParseConstructor();
						break;
					default:
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					return true;
				}
				_charPos++;
				throw CreateUnexpectedEndException();
			case 'N':
				ParseNumberNaN(ReadType.Read);
				return true;
			case 'I':
				ParseNumberPositiveInfinity(ReadType.Read);
				return true;
			case '-':
				if (EnsureChars(1, append: true) && _chars[_charPos + 1] == 'I')
				{
					ParseNumberNegativeInfinity(ReadType.Read);
				}
				else
				{
					ParseNumber(ReadType.Read);
				}
				return true;
			case '/':
				ParseComment(setToken: true);
				return true;
			case 'u':
				ParseUndefined();
				return true;
			case '{':
				_charPos++;
				SetToken(JsonToken.StartObject);
				return true;
			case '[':
				_charPos++;
				SetToken(JsonToken.StartArray);
				return true;
			case ']':
				_charPos++;
				SetToken(JsonToken.EndArray);
				return true;
			case ',':
				SetToken(JsonToken.Undefined);
				return true;
			case ')':
				_charPos++;
				SetToken(JsonToken.EndConstructor);
				return true;
			case '\r':
				ProcessCarriageReturn(append: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				_charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					_charPos++;
					break;
				}
				if (char.IsNumber(c) || c == '-' || c == '.')
				{
					ParseNumber(ReadType.Read);
					return true;
				}
				throw CreateUnexpectedCharacterException(c);
			}
		}
	}

	private void ProcessLineFeed()
	{
		_charPos++;
		OnNewLine(_charPos);
	}

	private void ProcessCarriageReturn(bool append)
	{
		_charPos++;
		SetNewLine(EnsureChars(1, append));
	}

	private void EatWhitespace()
	{
		while (true)
		{
			char c = _chars[_charPos];
			switch (c)
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (ReadData(append: false) == 0)
					{
						return;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '\r':
				ProcessCarriageReturn(append: false);
				break;
			case '\n':
				ProcessLineFeed();
				break;
			default:
				if (!char.IsWhiteSpace(c))
				{
					return;
				}
				goto case ' ';
			case ' ':
				_charPos++;
				break;
			}
		}
	}

	private void ParseConstructor()
	{
		if (MatchValueWithTrailingSeparator("new"))
		{
			EatWhitespace();
			int charPos = _charPos;
			int charPos2;
			while (true)
			{
				char c = _chars[_charPos];
				if (c == '\0')
				{
					if (_charsUsed == _charPos)
					{
						if (ReadData(append: true) == 0)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
						}
						continue;
					}
					charPos2 = _charPos;
					_charPos++;
					break;
				}
				if (char.IsLetterOrDigit(c))
				{
					_charPos++;
					continue;
				}
				switch (c)
				{
				case '\r':
					charPos2 = _charPos;
					ProcessCarriageReturn(append: true);
					break;
				case '\n':
					charPos2 = _charPos;
					ProcessLineFeed();
					break;
				default:
					if (char.IsWhiteSpace(c))
					{
						charPos2 = _charPos;
						_charPos++;
						break;
					}
					if (c == '(')
					{
						charPos2 = _charPos;
						break;
					}
					throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
				}
				break;
			}
			_stringReference = new StringReference(_chars, charPos, charPos2 - charPos);
			string value = _stringReference.ToString();
			EatWhitespace();
			if (_chars[_charPos] != '(')
			{
				throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			_charPos++;
			ClearRecentString();
			SetToken(JsonToken.StartConstructor, value);
			return;
		}
		throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
	}

	private void ParseNumber(ReadType readType)
	{
		ShiftBufferIfNeeded();
		char firstChar = _chars[_charPos];
		int charPos = _charPos;
		ReadNumberIntoBuffer();
		ParseReadNumber(readType, firstChar, charPos);
	}

	private void ParseReadNumber(ReadType readType, char firstChar, int initialPosition)
	{
		SetPostValueState(updateIndex: true);
		_stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
		bool flag = char.IsDigit(firstChar) && _stringReference.Length == 1;
		bool flag2 = firstChar == '0' && _stringReference.Length > 1 && _stringReference.Chars[_stringReference.StartIndex + 1] != '.' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'e' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'E';
		JsonToken newToken;
		object value;
		switch (readType)
		{
		case ReadType.ReadAsString:
		{
			string text5 = _stringReference.ToString();
			double result3;
			if (flag2)
			{
				try
				{
					if (text5.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					{
						Convert.ToInt64(text5, 16);
					}
					else
					{
						Convert.ToInt64(text5, 8);
					}
				}
				catch (Exception ex4)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text5), ex4);
				}
			}
			else if (!double.TryParse(text5, NumberStyles.Float, CultureInfo.InvariantCulture, out result3))
			{
				throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
			}
			newToken = JsonToken.String;
			value = text5;
			break;
		}
		case ReadType.ReadAsInt32:
			if (flag)
			{
				value = firstChar - 48;
			}
			else if (flag2)
			{
				string text6 = _stringReference.ToString();
				try
				{
					value = (text6.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(text6, 16) : Convert.ToInt32(text6, 8));
				}
				catch (Exception ex5)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, text6), ex5);
				}
			}
			else
			{
				int value5;
				switch (ConvertUtils.Int32TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value5))
				{
				case ParseResult.Success:
					break;
				case ParseResult.Overflow:
					throw ThrowReaderError("JSON integer {0} is too large or small for an Int32.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				default:
					throw ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				}
				value = value5;
			}
			newToken = JsonToken.Integer;
			break;
		case ReadType.ReadAsDecimal:
			if (flag)
			{
				value = (decimal)firstChar - 48m;
			}
			else if (flag2)
			{
				string text3 = _stringReference.ToString();
				try
				{
					value = Convert.ToDecimal(text3.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text3, 16) : Convert.ToInt64(text3, 8));
				}
				catch (Exception ex2)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, text3), ex2);
				}
			}
			else
			{
				if (ConvertUtils.DecimalTryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out var value4) != ParseResult.Success)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				}
				value = value4;
			}
			newToken = JsonToken.Float;
			break;
		case ReadType.ReadAsDouble:
			if (flag)
			{
				value = (double)(int)firstChar - 48.0;
			}
			else if (flag2)
			{
				string text4 = _stringReference.ToString();
				try
				{
					value = Convert.ToDouble(text4.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text4, 16) : Convert.ToInt64(text4, 8));
				}
				catch (Exception ex3)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, text4), ex3);
				}
			}
			else
			{
				if (!double.TryParse(_stringReference.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					throw ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				}
				value = result2;
			}
			newToken = JsonToken.Float;
			break;
		case ReadType.Read:
		case ReadType.ReadAsInt64:
		{
			if (flag)
			{
				value = (long)firstChar - 48L;
				newToken = JsonToken.Integer;
				break;
			}
			if (flag2)
			{
				string text = _stringReference.ToString();
				try
				{
					value = (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8));
				}
				catch (Exception ex)
				{
					throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text), ex);
				}
				newToken = JsonToken.Integer;
				break;
			}
			long value2;
			switch (ConvertUtils.Int64TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value2))
			{
			case ParseResult.Success:
				value = value2;
				newToken = JsonToken.Integer;
				break;
			case ParseResult.Overflow:
			{
				string text2 = _stringReference.ToString();
				if (text2.Length > 380)
				{
					throw ThrowReaderError("JSON integer {0} is too large to parse.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				}
				value = BigIntegerParse(text2, CultureInfo.InvariantCulture);
				newToken = JsonToken.Integer;
				break;
			}
			default:
				if (_floatParseHandling == FloatParseHandling.Decimal)
				{
					decimal value3;
					ParseResult parseResult = ConvertUtils.DecimalTryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value3);
					if (parseResult != ParseResult.Success)
					{
						throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					}
					value = value3;
				}
				else
				{
					if (!double.TryParse(_stringReference.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
					{
						throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					}
					value = result;
				}
				newToken = JsonToken.Float;
				break;
			}
			break;
		}
		default:
			throw JsonReaderException.Create(this, "Cannot read number value as type.");
		}
		ClearRecentString();
		SetToken(newToken, value, updateIndex: false);
	}

	private JsonReaderException ThrowReaderError(string message, Exception? ex = null)
	{
		SetToken(JsonToken.Undefined, null, updateIndex: false);
		return JsonReaderException.Create(this, message, ex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static object BigIntegerParse(string number, CultureInfo culture)
	{
		return BigInteger.Parse(number, culture);
	}

	private void ParseComment(bool setToken)
	{
		_charPos++;
		if (!EnsureChars(1, append: false))
		{
			throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
		}
		bool flag;
		if (_chars[_charPos] == '*')
		{
			flag = false;
		}
		else
		{
			if (_chars[_charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			flag = true;
		}
		_charPos++;
		int charPos = _charPos;
		while (true)
		{
			switch (_chars[_charPos])
			{
			case '\0':
				if (_charsUsed == _charPos)
				{
					if (ReadData(append: true) == 0)
					{
						if (!flag)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
						}
						EndComment(setToken, charPos, _charPos);
						return;
					}
				}
				else
				{
					_charPos++;
				}
				break;
			case '*':
				_charPos++;
				if (!flag && EnsureChars(0, append: true) && _chars[_charPos] == '/')
				{
					EndComment(setToken, charPos, _charPos - 1);
					_charPos++;
					return;
				}
				break;
			case '\r':
				if (flag)
				{
					EndComment(setToken, charPos, _charPos);
					return;
				}
				ProcessCarriageReturn(append: true);
				break;
			case '\n':
				if (flag)
				{
					EndComment(setToken, charPos, _charPos);
					return;
				}
				ProcessLineFeed();
				break;
			default:
				_charPos++;
				break;
			}
		}
	}

	private void EndComment(bool setToken, int initialPosition, int endPosition)
	{
		if (setToken)
		{
			SetToken(JsonToken.Comment, new string(_chars, initialPosition, endPosition - initialPosition));
		}
	}

	private bool MatchValue(string value)
	{
		return MatchValue(EnsureChars(value.Length - 1, append: true), value);
	}

	private bool MatchValue(bool enoughChars, string value)
	{
		if (!enoughChars)
		{
			_charPos = _charsUsed;
			throw CreateUnexpectedEndException();
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (_chars[_charPos + i] != value[i])
			{
				_charPos += i;
				return false;
			}
		}
		_charPos += value.Length;
		return true;
	}

	private bool MatchValueWithTrailingSeparator(string value)
	{
		if (!MatchValue(value))
		{
			return false;
		}
		if (!EnsureChars(0, append: false))
		{
			return true;
		}
		if (!IsSeparator(_chars[_charPos]))
		{
			return _chars[_charPos] == '\0';
		}
		return true;
	}

	private bool IsSeparator(char c)
	{
		switch (c)
		{
		case ',':
		case ']':
		case '}':
			return true;
		case '/':
		{
			if (!EnsureChars(1, append: false))
			{
				return false;
			}
			char c2 = _chars[_charPos + 1];
			if (c2 != '*')
			{
				return c2 == '/';
			}
			return true;
		}
		case ')':
			if (base.CurrentState == State.Constructor || base.CurrentState == State.ConstructorStart)
			{
				return true;
			}
			break;
		case '\t':
		case '\n':
		case '\r':
		case ' ':
			return true;
		default:
			if (char.IsWhiteSpace(c))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void ParseTrue()
	{
		if (MatchValueWithTrailingSeparator(JsonConvert.True))
		{
			SetToken(JsonToken.Boolean, true);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing boolean value.");
	}

	private void ParseNull()
	{
		if (MatchValueWithTrailingSeparator(JsonConvert.Null))
		{
			SetToken(JsonToken.Null);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing null value.");
	}

	private void ParseUndefined()
	{
		if (MatchValueWithTrailingSeparator(JsonConvert.Undefined))
		{
			SetToken(JsonToken.Undefined);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing undefined value.");
	}

	private void ParseFalse()
	{
		if (MatchValueWithTrailingSeparator(JsonConvert.False))
		{
			SetToken(JsonToken.Boolean, false);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing boolean value.");
	}

	private object ParseNumberNegativeInfinity(ReadType readType)
	{
		return ParseNumberNegativeInfinity(readType, MatchValueWithTrailingSeparator(JsonConvert.NegativeInfinity));
	}

	private object ParseNumberNegativeInfinity(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (_floatParseHandling == FloatParseHandling.Double)
				{
					SetToken(JsonToken.Float, double.NegativeInfinity);
					return double.NegativeInfinity;
				}
				break;
			case ReadType.ReadAsString:
				SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
				return JsonConvert.NegativeInfinity;
			}
			throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
		}
		throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
	}

	private object ParseNumberPositiveInfinity(ReadType readType)
	{
		return ParseNumberPositiveInfinity(readType, MatchValueWithTrailingSeparator(JsonConvert.PositiveInfinity));
	}

	private object ParseNumberPositiveInfinity(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (_floatParseHandling == FloatParseHandling.Double)
				{
					SetToken(JsonToken.Float, double.PositiveInfinity);
					return double.PositiveInfinity;
				}
				break;
			case ReadType.ReadAsString:
				SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
				return JsonConvert.PositiveInfinity;
			}
			throw JsonReaderException.Create(this, "Cannot read Infinity value.");
		}
		throw JsonReaderException.Create(this, "Error parsing Infinity value.");
	}

	private object ParseNumberNaN(ReadType readType)
	{
		return ParseNumberNaN(readType, MatchValueWithTrailingSeparator(JsonConvert.NaN));
	}

	private object ParseNumberNaN(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (_floatParseHandling == FloatParseHandling.Double)
				{
					SetToken(JsonToken.Float, double.NaN);
					return double.NaN;
				}
				break;
			case ReadType.ReadAsString:
				SetToken(JsonToken.String, JsonConvert.NaN);
				return JsonConvert.NaN;
			}
			throw JsonReaderException.Create(this, "Cannot read NaN value.");
		}
		throw JsonReaderException.Create(this, "Error parsing NaN value.");
	}

	public override void Close()
	{
		base.Close();
		if (_chars != null)
		{
			BufferUtils.ReturnBuffer(_arrayPool, _chars);
			_chars = null;
		}
		if (base.CloseInput)
		{
			_reader?.Close();
		}
		_stringBuffer.Clear(_arrayPool);
	}

	public bool HasLineInfo()
	{
		return true;
	}
}
