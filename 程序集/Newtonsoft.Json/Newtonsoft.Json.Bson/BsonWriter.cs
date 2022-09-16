using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson;

[Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Newtonsoft.Json.Bson for more details.")]
public class BsonWriter : JsonWriter
{
	private readonly BsonBinaryWriter _writer;

	private BsonToken _root;

	private BsonToken _parent;

	private string _propertyName;

	public DateTimeKind DateTimeKindHandling
	{
		get
		{
			return _writer.DateTimeKindHandling;
		}
		set
		{
			_writer.DateTimeKindHandling = value;
		}
	}

	public BsonWriter(Stream stream)
	{
		ValidationUtils.ArgumentNotNull(stream, "stream");
		_writer = new BsonBinaryWriter(new BinaryWriter(stream));
	}

	public BsonWriter(BinaryWriter writer)
	{
		ValidationUtils.ArgumentNotNull(writer, "writer");
		_writer = new BsonBinaryWriter(writer);
	}

	public override void Flush()
	{
		_writer.Flush();
	}

	protected override void WriteEnd(JsonToken token)
	{
		base.WriteEnd(token);
		RemoveParent();
		if (base.Top == 0)
		{
			_writer.WriteToken(_root);
		}
	}

	public override void WriteComment(string text)
	{
		throw JsonWriterException.Create(this, "Cannot write JSON comment as BSON.", null);
	}

	public override void WriteStartConstructor(string name)
	{
		throw JsonWriterException.Create(this, "Cannot write JSON constructor as BSON.", null);
	}

	public override void WriteRaw(string json)
	{
		throw JsonWriterException.Create(this, "Cannot write raw JSON as BSON.", null);
	}

	public override void WriteRawValue(string json)
	{
		throw JsonWriterException.Create(this, "Cannot write raw JSON as BSON.", null);
	}

	public override void WriteStartArray()
	{
		base.WriteStartArray();
		AddParent(new BsonArray());
	}

	public override void WriteStartObject()
	{
		base.WriteStartObject();
		AddParent(new BsonObject());
	}

	public override void WritePropertyName(string name)
	{
		base.WritePropertyName(name);
		_propertyName = name;
	}

	public override void Close()
	{
		base.Close();
		if (base.CloseOutput)
		{
			_writer?.Close();
		}
	}

	private void AddParent(BsonToken container)
	{
		AddToken(container);
		_parent = container;
	}

	private void RemoveParent()
	{
		_parent = _parent.Parent;
	}

	private void AddValue(object value, BsonType type)
	{
		AddToken(new BsonValue(value, type));
	}

	internal void AddToken(BsonToken token)
	{
		if (_parent != null)
		{
			if (_parent is BsonObject bsonObject)
			{
				bsonObject.Add(_propertyName, token);
				_propertyName = null;
			}
			else
			{
				((BsonArray)_parent).Add(token);
			}
			return;
		}
		if (token.Type != BsonType.Object && token.Type != BsonType.Array)
		{
			throw JsonWriterException.Create(this, "Error writing {0} value. BSON must start with an Object or Array.".FormatWith(CultureInfo.InvariantCulture, token.Type), null);
		}
		_parent = token;
		_root = token;
	}

	public override void WriteValue(object value)
	{
		if (value is BigInteger bigInteger)
		{
			SetWriteState(JsonToken.Integer, null);
			AddToken(new BsonBinary(bigInteger.ToByteArray(), BsonBinaryType.Binary));
		}
		else
		{
			base.WriteValue(value);
		}
	}

	public override void WriteNull()
	{
		base.WriteNull();
		AddToken(BsonEmpty.Null);
	}

	public override void WriteUndefined()
	{
		base.WriteUndefined();
		AddToken(BsonEmpty.Undefined);
	}

	public override void WriteValue(string value)
	{
		base.WriteValue(value);
		AddToken((value == null) ? BsonEmpty.Null : new BsonString(value, includeLength: true));
	}

	public override void WriteValue(int value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	[CLSCompliant(false)]
	public override void WriteValue(uint value)
	{
		if (value > int.MaxValue)
		{
			throw JsonWriterException.Create(this, "Value is too large to fit in a signed 32 bit integer. BSON does not support unsigned values.", null);
		}
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	public override void WriteValue(long value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Long);
	}

	[CLSCompliant(false)]
	public override void WriteValue(ulong value)
	{
		if (value > long.MaxValue)
		{
			throw JsonWriterException.Create(this, "Value is too large to fit in a signed 64 bit integer. BSON does not support unsigned values.", null);
		}
		base.WriteValue(value);
		AddValue(value, BsonType.Long);
	}

	public override void WriteValue(float value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Number);
	}

	public override void WriteValue(double value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Number);
	}

	public override void WriteValue(bool value)
	{
		base.WriteValue(value);
		AddToken(value ? BsonBoolean.True : BsonBoolean.False);
	}

	public override void WriteValue(short value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	[CLSCompliant(false)]
	public override void WriteValue(ushort value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	public override void WriteValue(char value)
	{
		base.WriteValue(value);
		string text = null;
		text = value.ToString(CultureInfo.InvariantCulture);
		AddToken(new BsonString(text, includeLength: true));
	}

	public override void WriteValue(byte value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	[CLSCompliant(false)]
	public override void WriteValue(sbyte value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Integer);
	}

	public override void WriteValue(decimal value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Number);
	}

	public override void WriteValue(DateTime value)
	{
		base.WriteValue(value);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		AddValue(value, BsonType.Date);
	}

	public override void WriteValue(DateTimeOffset value)
	{
		base.WriteValue(value);
		AddValue(value, BsonType.Date);
	}

	public override void WriteValue(byte[] value)
	{
		if (value == null)
		{
			WriteNull();
			return;
		}
		base.WriteValue(value);
		AddToken(new BsonBinary(value, BsonBinaryType.Binary));
	}

	public override void WriteValue(Guid value)
	{
		base.WriteValue(value);
		AddToken(new BsonBinary(value.ToByteArray(), BsonBinaryType.Uuid));
	}

	public override void WriteValue(TimeSpan value)
	{
		base.WriteValue(value);
		AddToken(new BsonString(value.ToString(), includeLength: true));
	}

	public override void WriteValue(Uri value)
	{
		if (value == null)
		{
			WriteNull();
			return;
		}
		base.WriteValue(value);
		AddToken(new BsonString(value.ToString(), includeLength: true));
	}

	public void WriteObjectId(byte[] value)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		if (value.Length != 12)
		{
			throw JsonWriterException.Create(this, "An object id must be 12 bytes", null);
		}
		SetWriteState(JsonToken.Undefined, null);
		AddValue(value, BsonType.Oid);
	}

	public void WriteRegex(string pattern, string options)
	{
		ValidationUtils.ArgumentNotNull(pattern, "pattern");
		SetWriteState(JsonToken.Undefined, null);
		AddToken(new BsonRegex(pattern, options));
	}
}
