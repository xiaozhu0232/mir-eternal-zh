using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

public abstract class JsonConverter
{
	public virtual bool CanRead => true;

	public virtual bool CanWrite => true;

	public abstract void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer);

	public abstract object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer);

	public abstract bool CanConvert(Type objectType);
}
public abstract class JsonConverter<T> : JsonConverter
{
	public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (!((value != null) ? (value is T) : ReflectionUtils.IsNullable(typeof(T))))
		{
			throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		WriteJson(writer, (T)value, serializer);
	}

	public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);

	public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		bool flag = existingValue == null;
		if (!flag && !(existingValue is T))
		{
			throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		return ReadJson(reader, objectType, flag ? default(T) : ((T)existingValue), !flag, serializer);
	}

	public abstract T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer);

	public sealed override bool CanConvert(Type objectType)
	{
		return typeof(T).IsAssignableFrom(objectType);
	}
}
