using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class UnixDateTimeConverter : DateTimeConverterBase
{
	internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		long num;
		if (value is DateTime dateTime)
		{
			num = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Expected date object value.");
			}
			num = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
		}
		if (num < 0)
		{
			throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
		}
		writer.WriteValue(num);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		bool flag = ReflectionUtils.IsNullable(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			if (!flag)
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		long result;
		if (reader.TokenType == JsonToken.Integer)
		{
			result = (long)reader.Value;
		}
		else
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			if (!long.TryParse((string)reader.Value, out result))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
		}
		if (result >= 0)
		{
			DateTime dateTime = UnixEpoch.AddSeconds(result);
			if ((flag ? Nullable.GetUnderlyingType(objectType) : objectType) == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(dateTime, TimeSpan.Zero);
			}
			return dateTime;
		}
		throw JsonSerializationException.Create(reader, "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
	}
}
