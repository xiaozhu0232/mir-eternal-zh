using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

public class JsonTextWriter : JsonWriter
{
	private readonly bool _safeAsync;

	private const int IndentCharBufferSize = 12;

	private readonly TextWriter _writer;

	private Base64Encoder? _base64Encoder;

	private char _indentChar;

	private int _indentation;

	private char _quoteChar;

	private bool _quoteName;

	private bool[]? _charEscapeFlags;

	private char[]? _writeBuffer;

	private IArrayPool<char>? _arrayPool;

	private char[]? _indentChars;

	private Base64Encoder Base64Encoder
	{
		get
		{
			if (_base64Encoder == null)
			{
				_base64Encoder = new Base64Encoder(_writer);
			}
			return _base64Encoder;
		}
	}

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

	public int Indentation
	{
		get
		{
			return _indentation;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Indentation value must be greater than 0.");
			}
			_indentation = value;
		}
	}

	public char QuoteChar
	{
		get
		{
			return _quoteChar;
		}
		set
		{
			if (value != '"' && value != '\'')
			{
				throw new ArgumentException("Invalid JavaScript string quote character. Valid quote characters are ' and \".");
			}
			_quoteChar = value;
			UpdateCharEscapeFlags();
		}
	}

	public char IndentChar
	{
		get
		{
			return _indentChar;
		}
		set
		{
			if (value != _indentChar)
			{
				_indentChar = value;
				_indentChars = null;
			}
		}
	}

	public bool QuoteName
	{
		get
		{
			return _quoteName;
		}
		set
		{
			_quoteName = value;
		}
	}

	public override Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.FlushAsync(cancellationToken);
		}
		return DoFlushAsync(cancellationToken);
	}

	internal Task DoFlushAsync(CancellationToken cancellationToken)
	{
		return cancellationToken.CancelIfRequestedAsync() ?? _writer.FlushAsync();
	}

	protected override Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
	{
		if (!_safeAsync)
		{
			return base.WriteValueDelimiterAsync(cancellationToken);
		}
		return DoWriteValueDelimiterAsync(cancellationToken);
	}

	internal Task DoWriteValueDelimiterAsync(CancellationToken cancellationToken)
	{
		return _writer.WriteAsync(',', cancellationToken);
	}

	protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
	{
		if (!_safeAsync)
		{
			return base.WriteEndAsync(token, cancellationToken);
		}
		return DoWriteEndAsync(token, cancellationToken);
	}

	internal Task DoWriteEndAsync(JsonToken token, CancellationToken cancellationToken)
	{
		return token switch
		{
			JsonToken.EndObject => _writer.WriteAsync('}', cancellationToken), 
			JsonToken.EndArray => _writer.WriteAsync(']', cancellationToken), 
			JsonToken.EndConstructor => _writer.WriteAsync(')', cancellationToken), 
			_ => throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null), 
		};
	}

	public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.CloseAsync(cancellationToken);
		}
		return DoCloseAsync(cancellationToken);
	}

	internal async Task DoCloseAsync(CancellationToken cancellationToken)
	{
		if (base.Top == 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
		}
		while (base.Top > 0)
		{
			await WriteEndAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		CloseBufferAndWriter();
	}

	public override Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteEndAsync(cancellationToken);
		}
		return WriteEndInternalAsync(cancellationToken);
	}

	protected override Task WriteIndentAsync(CancellationToken cancellationToken)
	{
		if (!_safeAsync)
		{
			return base.WriteIndentAsync(cancellationToken);
		}
		return DoWriteIndentAsync(cancellationToken);
	}

	internal Task DoWriteIndentAsync(CancellationToken cancellationToken)
	{
		int num = base.Top * _indentation;
		int num2 = SetIndentChars();
		if (num <= 12)
		{
			return _writer.WriteAsync(_indentChars, 0, num2 + num, cancellationToken);
		}
		return WriteIndentAsync(num, num2, cancellationToken);
	}

	private async Task WriteIndentAsync(int currentIndentCount, int newLineLen, CancellationToken cancellationToken)
	{
		await _writer.WriteAsync(_indentChars, 0, newLineLen + Math.Min(currentIndentCount, 12), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		while (true)
		{
			int num;
			currentIndentCount = (num = currentIndentCount - 12);
			if (num <= 0)
			{
				break;
			}
			await _writer.WriteAsync(_indentChars, newLineLen, Math.Min(currentIndentCount, 12), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private Task WriteValueInternalAsync(JsonToken token, string value, CancellationToken cancellationToken)
	{
		Task task = InternalWriteValueAsync(token, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return _writer.WriteAsync(value, cancellationToken);
		}
		return WriteValueInternalAsync(task, value, cancellationToken);
	}

	private async Task WriteValueInternalAsync(Task task, string value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
	{
		if (!_safeAsync)
		{
			return base.WriteIndentSpaceAsync(cancellationToken);
		}
		return DoWriteIndentSpaceAsync(cancellationToken);
	}

	internal Task DoWriteIndentSpaceAsync(CancellationToken cancellationToken)
	{
		return _writer.WriteAsync(' ', cancellationToken);
	}

	public override Task WriteRawAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteRawAsync(json, cancellationToken);
		}
		return DoWriteRawAsync(json, cancellationToken);
	}

	internal Task DoWriteRawAsync(string? json, CancellationToken cancellationToken)
	{
		return _writer.WriteAsync(json, cancellationToken);
	}

	public override Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteNullAsync(cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	internal Task DoWriteNullAsync(CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Null, JsonConvert.Null, cancellationToken);
	}

	private Task WriteDigitsAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		if (uvalue <= 9 && !negative)
		{
			return _writer.WriteAsync((char)(48 + uvalue), cancellationToken);
		}
		int count = WriteNumberToBuffer(uvalue, negative);
		return _writer.WriteAsync(_writeBuffer, 0, count, cancellationToken);
	}

	private Task WriteIntegerValueAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		Task task = InternalWriteValueAsync(JsonToken.Integer, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return WriteDigitsAsync(uvalue, negative, cancellationToken);
		}
		return WriteIntegerValueAsync(task, uvalue, negative, cancellationToken);
	}

	private async Task WriteIntegerValueAsync(Task task, ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await WriteDigitsAsync(uvalue, negative, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal Task WriteIntegerValueAsync(long value, CancellationToken cancellationToken)
	{
		bool flag = value < 0;
		if (flag)
		{
			value = -value;
		}
		return WriteIntegerValueAsync((ulong)value, flag, cancellationToken);
	}

	internal Task WriteIntegerValueAsync(ulong uvalue, CancellationToken cancellationToken)
	{
		return WriteIntegerValueAsync(uvalue, negative: false, cancellationToken);
	}

	private Task WriteEscapedStringAsync(string value, bool quote, CancellationToken cancellationToken)
	{
		return JavaScriptUtils.WriteEscapedJavaScriptStringAsync(_writer, value, _quoteChar, quote, _charEscapeFlags, base.StringEscapeHandling, this, _writeBuffer, cancellationToken);
	}

	public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WritePropertyNameAsync(name, cancellationToken);
		}
		return DoWritePropertyNameAsync(name, cancellationToken);
	}

	internal Task DoWritePropertyNameAsync(string name, CancellationToken cancellationToken)
	{
		Task task = InternalWritePropertyNameAsync(name, cancellationToken);
		if (!task.IsCompletedSucessfully())
		{
			return DoWritePropertyNameAsync(task, name, cancellationToken);
		}
		task = WriteEscapedStringAsync(name, _quoteName, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return _writer.WriteAsync(':', cancellationToken);
		}
		return JavaScriptUtils.WriteCharAsync(task, _writer, ':', cancellationToken);
	}

	private async Task DoWritePropertyNameAsync(Task task, string name, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(':').ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WritePropertyNameAsync(name, escape, cancellationToken);
		}
		return DoWritePropertyNameAsync(name, escape, cancellationToken);
	}

	internal async Task DoWritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken)
	{
		await InternalWritePropertyNameAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (escape)
		{
			await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			if (_quoteName)
			{
				await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			}
			await _writer.WriteAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (_quoteName)
			{
				await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		await _writer.WriteAsync(':').ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteStartArrayAsync(cancellationToken);
		}
		return DoWriteStartArrayAsync(cancellationToken);
	}

	internal Task DoWriteStartArrayAsync(CancellationToken cancellationToken)
	{
		Task task = InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return _writer.WriteAsync('[', cancellationToken);
		}
		return DoWriteStartArrayAsync(task, cancellationToken);
	}

	internal async Task DoWriteStartArrayAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync('[', cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteStartObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteStartObjectAsync(cancellationToken);
		}
		return DoWriteStartObjectAsync(cancellationToken);
	}

	internal Task DoWriteStartObjectAsync(CancellationToken cancellationToken)
	{
		Task task = InternalWriteStartAsync(JsonToken.StartObject, JsonContainerType.Object, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return _writer.WriteAsync('{', cancellationToken);
		}
		return DoWriteStartObjectAsync(task, cancellationToken);
	}

	internal async Task DoWriteStartObjectAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync('{', cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteStartConstructorAsync(name, cancellationToken);
		}
		return DoWriteStartConstructorAsync(name, cancellationToken);
	}

	internal async Task DoWriteStartConstructorAsync(string name, CancellationToken cancellationToken)
	{
		await InternalWriteStartAsync(JsonToken.StartConstructor, JsonContainerType.Constructor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync("new ", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync('(').ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteUndefinedAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteUndefinedAsync(cancellationToken);
		}
		return DoWriteUndefinedAsync(cancellationToken);
	}

	internal Task DoWriteUndefinedAsync(CancellationToken cancellationToken)
	{
		Task task = InternalWriteValueAsync(JsonToken.Undefined, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return _writer.WriteAsync(JsonConvert.Undefined, cancellationToken);
		}
		return DoWriteUndefinedAsync(task, cancellationToken);
	}

	private async Task DoWriteUndefinedAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(JsonConvert.Undefined, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteWhitespaceAsync(ws, cancellationToken);
		}
		return DoWriteWhitespaceAsync(ws, cancellationToken);
	}

	internal Task DoWriteWhitespaceAsync(string ws, CancellationToken cancellationToken)
	{
		InternalWriteWhitespace(ws);
		return _writer.WriteAsync(ws, cancellationToken);
	}

	public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(bool value, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Boolean, JsonConvert.ToString(value), cancellationToken);
	}

	public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(bool? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(byte? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (value != null)
		{
			return WriteValueNonNullAsync(value, cancellationToken);
		}
		return WriteNullAsync(cancellationToken);
	}

	internal async Task WriteValueNonNullAsync(byte[] value, CancellationToken cancellationToken)
	{
		await InternalWriteValueAsync(JsonToken.Bytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		await Base64Encoder.EncodeAsync(value, 0, value.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await Base64Encoder.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(char value, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.String, JsonConvert.ToString(value), cancellationToken);
	}

	public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(char? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(DateTime value, CancellationToken cancellationToken)
	{
		await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = WriteValueToBuffer(value);
			await _writer.WriteAsync(_writeBuffer, 0, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			await _writer.WriteAsync(value.ToString(base.DateFormatString, base.Culture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(DateTime? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken)
	{
		await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = WriteValueToBuffer(value);
			await _writer.WriteAsync(_writeBuffer, 0, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			await _writer.WriteAsync(value.ToString(base.DateFormatString, base.Culture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(decimal value, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value), cancellationToken);
	}

	public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(decimal? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteValueAsync(value, nullable: false, cancellationToken);
	}

	internal Task WriteValueAsync(double value, bool nullable, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, base.FloatFormatHandling, QuoteChar, nullable), cancellationToken);
	}

	public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!value.HasValue)
		{
			return WriteNullAsync(cancellationToken);
		}
		return WriteValueAsync(value.GetValueOrDefault(), nullable: true, cancellationToken);
	}

	public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteValueAsync(value, nullable: false, cancellationToken);
	}

	internal Task WriteValueAsync(float value, bool nullable, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, base.FloatFormatHandling, QuoteChar, nullable), cancellationToken);
	}

	public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!value.HasValue)
		{
			return WriteNullAsync(cancellationToken);
		}
		return WriteValueAsync(value.GetValueOrDefault(), nullable: true, cancellationToken);
	}

	public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(Guid value, CancellationToken cancellationToken)
	{
		await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(value.ToString("D", CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(Guid? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(int? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(long? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	internal Task WriteValueAsync(BigInteger value, CancellationToken cancellationToken)
	{
		return WriteValueInternalAsync(JsonToken.Integer, value.ToString(CultureInfo.InvariantCulture), cancellationToken);
	}

	public override Task WriteValueAsync(object? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_safeAsync)
		{
			if (value == null)
			{
				return WriteNullAsync(cancellationToken);
			}
			if (value is BigInteger value2)
			{
				return WriteValueAsync(value2, cancellationToken);
			}
			return JsonWriter.WriteValueAsync(this, ConvertUtils.GetTypeCode(value!.GetType()), value, cancellationToken);
		}
		return base.WriteValueAsync(value, cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(sbyte? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(short? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(string? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(string? value, CancellationToken cancellationToken)
	{
		Task task = InternalWriteValueAsync(JsonToken.String, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			if (value != null)
			{
				return WriteEscapedStringAsync(value, quote: true, cancellationToken);
			}
			return _writer.WriteAsync(JsonConvert.Null, cancellationToken);
		}
		return DoWriteValueAsync(task, value, cancellationToken);
	}

	private async Task DoWriteValueAsync(Task task, string? value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await ((value == null) ? _writer.WriteAsync(JsonConvert.Null, cancellationToken) : WriteEscapedStringAsync(value, quote: true, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(TimeSpan value, CancellationToken cancellationToken)
	{
		await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(value.ToString(null, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(TimeSpan? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(uint? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(ulong? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!(value == null))
		{
			return WriteValueNotNullAsync(value, cancellationToken);
		}
		return WriteNullAsync(cancellationToken);
	}

	internal Task WriteValueNotNullAsync(Uri value, CancellationToken cancellationToken)
	{
		Task task = InternalWriteValueAsync(JsonToken.String, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return WriteEscapedStringAsync(value.OriginalString, quote: true, cancellationToken);
		}
		return WriteValueNotNullAsync(task, value, cancellationToken);
	}

	internal async Task WriteValueNotNullAsync(Task task, Uri value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await WriteEscapedStringAsync(value.OriginalString, quote: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return WriteIntegerValueAsync(value, cancellationToken);
	}

	[CLSCompliant(false)]
	public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(ushort? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return DoWriteNullAsync(cancellationToken);
	}

	public override Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteCommentAsync(text, cancellationToken);
		}
		return DoWriteCommentAsync(text, cancellationToken);
	}

	internal async Task DoWriteCommentAsync(string? text, CancellationToken cancellationToken)
	{
		await InternalWriteCommentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync("/*", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync(text ?? string.Empty, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _writer.WriteAsync("*/", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteEndArrayAsync(cancellationToken);
		}
		return InternalWriteEndAsync(JsonContainerType.Array, cancellationToken);
	}

	public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteEndConstructorAsync(cancellationToken);
		}
		return InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken);
	}

	public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteEndObjectAsync(cancellationToken);
		}
		return InternalWriteEndAsync(JsonContainerType.Object, cancellationToken);
	}

	public override Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_safeAsync)
		{
			return base.WriteRawValueAsync(json, cancellationToken);
		}
		return DoWriteRawValueAsync(json, cancellationToken);
	}

	internal Task DoWriteRawValueAsync(string? json, CancellationToken cancellationToken)
	{
		UpdateScopeWithFinishedValue();
		Task task = AutoCompleteAsync(JsonToken.Undefined, cancellationToken);
		if (task.IsCompletedSucessfully())
		{
			return WriteRawAsync(json, cancellationToken);
		}
		return DoWriteRawValueAsync(task, json, cancellationToken);
	}

	private async Task DoWriteRawValueAsync(Task task, string? json, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await WriteRawAsync(json, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal char[] EnsureWriteBuffer(int length, int copyTo)
	{
		if (length < 35)
		{
			length = 35;
		}
		char[] writeBuffer = _writeBuffer;
		if (writeBuffer == null)
		{
			return _writeBuffer = BufferUtils.RentBuffer(_arrayPool, length);
		}
		if (writeBuffer.Length >= length)
		{
			return writeBuffer;
		}
		char[] array = BufferUtils.RentBuffer(_arrayPool, length);
		if (copyTo != 0)
		{
			Array.Copy(writeBuffer, array, copyTo);
		}
		BufferUtils.ReturnBuffer(_arrayPool, writeBuffer);
		_writeBuffer = array;
		return array;
	}

	public JsonTextWriter(TextWriter textWriter)
	{
		if (textWriter == null)
		{
			throw new ArgumentNullException("textWriter");
		}
		_writer = textWriter;
		_quoteChar = '"';
		_quoteName = true;
		_indentChar = ' ';
		_indentation = 2;
		UpdateCharEscapeFlags();
		_safeAsync = GetType() == typeof(JsonTextWriter);
	}

	public override void Flush()
	{
		_writer.Flush();
	}

	public override void Close()
	{
		base.Close();
		CloseBufferAndWriter();
	}

	private void CloseBufferAndWriter()
	{
		if (_writeBuffer != null)
		{
			BufferUtils.ReturnBuffer(_arrayPool, _writeBuffer);
			_writeBuffer = null;
		}
		if (base.CloseOutput)
		{
			_writer?.Close();
		}
	}

	public override void WriteStartObject()
	{
		InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
		_writer.Write('{');
	}

	public override void WriteStartArray()
	{
		InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
		_writer.Write('[');
	}

	public override void WriteStartConstructor(string name)
	{
		InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
		_writer.Write("new ");
		_writer.Write(name);
		_writer.Write('(');
	}

	protected override void WriteEnd(JsonToken token)
	{
		switch (token)
		{
		case JsonToken.EndObject:
			_writer.Write('}');
			break;
		case JsonToken.EndArray:
			_writer.Write(']');
			break;
		case JsonToken.EndConstructor:
			_writer.Write(')');
			break;
		default:
			throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
		}
	}

	public override void WritePropertyName(string name)
	{
		InternalWritePropertyName(name);
		WriteEscapedString(name, _quoteName);
		_writer.Write(':');
	}

	public override void WritePropertyName(string name, bool escape)
	{
		InternalWritePropertyName(name);
		if (escape)
		{
			WriteEscapedString(name, _quoteName);
		}
		else
		{
			if (_quoteName)
			{
				_writer.Write(_quoteChar);
			}
			_writer.Write(name);
			if (_quoteName)
			{
				_writer.Write(_quoteChar);
			}
		}
		_writer.Write(':');
	}

	internal override void OnStringEscapeHandlingChanged()
	{
		UpdateCharEscapeFlags();
	}

	private void UpdateCharEscapeFlags()
	{
		_charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(base.StringEscapeHandling, _quoteChar);
	}

	protected override void WriteIndent()
	{
		int num = base.Top * _indentation;
		int num2 = SetIndentChars();
		_writer.Write(_indentChars, 0, num2 + Math.Min(num, 12));
		while ((num -= 12) > 0)
		{
			_writer.Write(_indentChars, num2, Math.Min(num, 12));
		}
	}

	private int SetIndentChars()
	{
		string newLine = _writer.NewLine;
		int length = newLine.Length;
		bool flag = _indentChars != null && _indentChars!.Length == 12 + length;
		if (flag)
		{
			for (int i = 0; i != length; i++)
			{
				if (newLine[i] != _indentChars[i])
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			_indentChars = (newLine + new string(_indentChar, 12)).ToCharArray();
		}
		return length;
	}

	protected override void WriteValueDelimiter()
	{
		_writer.Write(',');
	}

	protected override void WriteIndentSpace()
	{
		_writer.Write(' ');
	}

	private void WriteValueInternal(string value, JsonToken token)
	{
		_writer.Write(value);
	}

	public override void WriteValue(object? value)
	{
		if (value is BigInteger bigInteger)
		{
			InternalWriteValue(JsonToken.Integer);
			WriteValueInternal(bigInteger.ToString(CultureInfo.InvariantCulture), JsonToken.String);
		}
		else
		{
			base.WriteValue(value);
		}
	}

	public override void WriteNull()
	{
		InternalWriteValue(JsonToken.Null);
		WriteValueInternal(JsonConvert.Null, JsonToken.Null);
	}

	public override void WriteUndefined()
	{
		InternalWriteValue(JsonToken.Undefined);
		WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
	}

	public override void WriteRaw(string? json)
	{
		InternalWriteRaw();
		_writer.Write(json);
	}

	public override void WriteValue(string? value)
	{
		InternalWriteValue(JsonToken.String);
		if (value == null)
		{
			WriteValueInternal(JsonConvert.Null, JsonToken.Null);
		}
		else
		{
			WriteEscapedString(value, quote: true);
		}
	}

	private void WriteEscapedString(string value, bool quote)
	{
		EnsureWriteBuffer();
		JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, quote, _charEscapeFlags, base.StringEscapeHandling, _arrayPool, ref _writeBuffer);
	}

	public override void WriteValue(int value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	[CLSCompliant(false)]
	public override void WriteValue(uint value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	public override void WriteValue(long value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	[CLSCompliant(false)]
	public override void WriteValue(ulong value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value, negative: false);
	}

	public override void WriteValue(float value)
	{
		InternalWriteValue(JsonToken.Float);
		WriteValueInternal(JsonConvert.ToString(value, base.FloatFormatHandling, QuoteChar, nullable: false), JsonToken.Float);
	}

	public override void WriteValue(float? value)
	{
		if (!value.HasValue)
		{
			WriteNull();
			return;
		}
		InternalWriteValue(JsonToken.Float);
		WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), base.FloatFormatHandling, QuoteChar, nullable: true), JsonToken.Float);
	}

	public override void WriteValue(double value)
	{
		InternalWriteValue(JsonToken.Float);
		WriteValueInternal(JsonConvert.ToString(value, base.FloatFormatHandling, QuoteChar, nullable: false), JsonToken.Float);
	}

	public override void WriteValue(double? value)
	{
		if (!value.HasValue)
		{
			WriteNull();
			return;
		}
		InternalWriteValue(JsonToken.Float);
		WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), base.FloatFormatHandling, QuoteChar, nullable: true), JsonToken.Float);
	}

	public override void WriteValue(bool value)
	{
		InternalWriteValue(JsonToken.Boolean);
		WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
	}

	public override void WriteValue(short value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	[CLSCompliant(false)]
	public override void WriteValue(ushort value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	public override void WriteValue(char value)
	{
		InternalWriteValue(JsonToken.String);
		WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
	}

	public override void WriteValue(byte value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	[CLSCompliant(false)]
	public override void WriteValue(sbyte value)
	{
		InternalWriteValue(JsonToken.Integer);
		WriteIntegerValue(value);
	}

	public override void WriteValue(decimal value)
	{
		InternalWriteValue(JsonToken.Float);
		WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
	}

	public override void WriteValue(DateTime value)
	{
		InternalWriteValue(JsonToken.Date);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = WriteValueToBuffer(value);
			_writer.Write(_writeBuffer, 0, count);
		}
		else
		{
			_writer.Write(_quoteChar);
			_writer.Write(value.ToString(base.DateFormatString, base.Culture));
			_writer.Write(_quoteChar);
		}
	}

	private int WriteValueToBuffer(DateTime value)
	{
		EnsureWriteBuffer();
		int start = 0;
		_writeBuffer[start++] = _quoteChar;
		start = DateTimeUtils.WriteDateTimeString(_writeBuffer, start, value, null, value.Kind, base.DateFormatHandling);
		_writeBuffer[start++] = _quoteChar;
		return start;
	}

	public override void WriteValue(byte[]? value)
	{
		if (value == null)
		{
			WriteNull();
			return;
		}
		InternalWriteValue(JsonToken.Bytes);
		_writer.Write(_quoteChar);
		Base64Encoder.Encode(value, 0, value!.Length);
		Base64Encoder.Flush();
		_writer.Write(_quoteChar);
	}

	public override void WriteValue(DateTimeOffset value)
	{
		InternalWriteValue(JsonToken.Date);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = WriteValueToBuffer(value);
			_writer.Write(_writeBuffer, 0, count);
		}
		else
		{
			_writer.Write(_quoteChar);
			_writer.Write(value.ToString(base.DateFormatString, base.Culture));
			_writer.Write(_quoteChar);
		}
	}

	private int WriteValueToBuffer(DateTimeOffset value)
	{
		EnsureWriteBuffer();
		int start = 0;
		_writeBuffer[start++] = _quoteChar;
		start = DateTimeUtils.WriteDateTimeString(_writeBuffer, start, (base.DateFormatHandling == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, base.DateFormatHandling);
		_writeBuffer[start++] = _quoteChar;
		return start;
	}

	public override void WriteValue(Guid value)
	{
		InternalWriteValue(JsonToken.String);
		string value2 = value.ToString("D", CultureInfo.InvariantCulture);
		_writer.Write(_quoteChar);
		_writer.Write(value2);
		_writer.Write(_quoteChar);
	}

	public override void WriteValue(TimeSpan value)
	{
		InternalWriteValue(JsonToken.String);
		string value2 = value.ToString(null, CultureInfo.InvariantCulture);
		_writer.Write(_quoteChar);
		_writer.Write(value2);
		_writer.Write(_quoteChar);
	}

	public override void WriteValue(Uri? value)
	{
		if (value == null)
		{
			WriteNull();
			return;
		}
		InternalWriteValue(JsonToken.String);
		WriteEscapedString(value!.OriginalString, quote: true);
	}

	public override void WriteComment(string? text)
	{
		InternalWriteComment();
		_writer.Write("/*");
		_writer.Write(text);
		_writer.Write("*/");
	}

	public override void WriteWhitespace(string ws)
	{
		InternalWriteWhitespace(ws);
		_writer.Write(ws);
	}

	private void EnsureWriteBuffer()
	{
		if (_writeBuffer == null)
		{
			_writeBuffer = BufferUtils.RentBuffer(_arrayPool, 35);
		}
	}

	private void WriteIntegerValue(long value)
	{
		if (value >= 0 && value <= 9)
		{
			_writer.Write((char)(48 + value));
			return;
		}
		bool flag = value < 0;
		WriteIntegerValue((ulong)(flag ? (-value) : value), flag);
	}

	private void WriteIntegerValue(ulong value, bool negative)
	{
		if (!negative && value <= 9)
		{
			_writer.Write((char)(48 + value));
			return;
		}
		int count = WriteNumberToBuffer(value, negative);
		_writer.Write(_writeBuffer, 0, count);
	}

	private int WriteNumberToBuffer(ulong value, bool negative)
	{
		if (value <= uint.MaxValue)
		{
			return WriteNumberToBuffer((uint)value, negative);
		}
		EnsureWriteBuffer();
		int num = MathUtils.IntLength(value);
		if (negative)
		{
			num++;
			_writeBuffer[0] = '-';
		}
		int num2 = num;
		do
		{
			ulong num3 = value / 10uL;
			ulong num4 = value - num3 * 10;
			_writeBuffer[--num2] = (char)(48 + num4);
			value = num3;
		}
		while (value != 0L);
		return num;
	}

	private void WriteIntegerValue(int value)
	{
		if (value >= 0 && value <= 9)
		{
			_writer.Write((char)(48 + value));
			return;
		}
		bool flag = value < 0;
		WriteIntegerValue((uint)(flag ? (-value) : value), flag);
	}

	private void WriteIntegerValue(uint value, bool negative)
	{
		if (!negative && value <= 9)
		{
			_writer.Write((char)(48 + value));
			return;
		}
		int count = WriteNumberToBuffer(value, negative);
		_writer.Write(_writeBuffer, 0, count);
	}

	private int WriteNumberToBuffer(uint value, bool negative)
	{
		EnsureWriteBuffer();
		int num = MathUtils.IntLength(value);
		if (negative)
		{
			num++;
			_writeBuffer[0] = '-';
		}
		int num2 = num;
		do
		{
			uint num3 = value / 10u;
			uint num4 = value - num3 * 10;
			_writeBuffer[--num2] = (char)(48 + num4);
			value = num3;
		}
		while (value != 0);
		return num;
	}
}
