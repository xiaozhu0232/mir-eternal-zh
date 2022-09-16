using System;
using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Serialization;

internal class TraceJsonReader : JsonReader, IJsonLineInfo
{
	private readonly JsonReader _innerReader;

	private readonly JsonTextWriter _textWriter;

	private readonly StringWriter _sw;

	public override int Depth => _innerReader.Depth;

	public override string Path => _innerReader.Path;

	public override char QuoteChar
	{
		get
		{
			return _innerReader.QuoteChar;
		}
		protected internal set
		{
			_innerReader.QuoteChar = value;
		}
	}

	public override JsonToken TokenType => _innerReader.TokenType;

	public override object? Value => _innerReader.Value;

	public override Type? ValueType => _innerReader.ValueType;

	int IJsonLineInfo.LineNumber
	{
		get
		{
			if (!(_innerReader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LineNumber;
		}
	}

	int IJsonLineInfo.LinePosition
	{
		get
		{
			if (!(_innerReader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LinePosition;
		}
	}

	public TraceJsonReader(JsonReader innerReader)
	{
		_innerReader = innerReader;
		_sw = new StringWriter(CultureInfo.InvariantCulture);
		_sw.Write("Deserialized JSON: " + Environment.NewLine);
		_textWriter = new JsonTextWriter(_sw);
		_textWriter.Formatting = Formatting.Indented;
	}

	public string GetDeserializedJsonMessage()
	{
		return _sw.ToString();
	}

	public override bool Read()
	{
		bool result = _innerReader.Read();
		WriteCurrentToken();
		return result;
	}

	public override int? ReadAsInt32()
	{
		int? result = _innerReader.ReadAsInt32();
		WriteCurrentToken();
		return result;
	}

	public override string? ReadAsString()
	{
		string? result = _innerReader.ReadAsString();
		WriteCurrentToken();
		return result;
	}

	public override byte[]? ReadAsBytes()
	{
		byte[]? result = _innerReader.ReadAsBytes();
		WriteCurrentToken();
		return result;
	}

	public override decimal? ReadAsDecimal()
	{
		decimal? result = _innerReader.ReadAsDecimal();
		WriteCurrentToken();
		return result;
	}

	public override double? ReadAsDouble()
	{
		double? result = _innerReader.ReadAsDouble();
		WriteCurrentToken();
		return result;
	}

	public override bool? ReadAsBoolean()
	{
		bool? result = _innerReader.ReadAsBoolean();
		WriteCurrentToken();
		return result;
	}

	public override DateTime? ReadAsDateTime()
	{
		DateTime? result = _innerReader.ReadAsDateTime();
		WriteCurrentToken();
		return result;
	}

	public override DateTimeOffset? ReadAsDateTimeOffset()
	{
		DateTimeOffset? result = _innerReader.ReadAsDateTimeOffset();
		WriteCurrentToken();
		return result;
	}

	public void WriteCurrentToken()
	{
		_textWriter.WriteToken(_innerReader, writeChildren: false, writeDateConstructorAsDate: false, writeComments: true);
	}

	public override void Close()
	{
		_innerReader.Close();
	}

	bool IJsonLineInfo.HasLineInfo()
	{
		if (_innerReader is IJsonLineInfo jsonLineInfo)
		{
			return jsonLineInfo.HasLineInfo();
		}
		return false;
	}
}
