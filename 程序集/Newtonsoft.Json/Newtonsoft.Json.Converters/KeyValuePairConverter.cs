using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class KeyValuePairConverter : JsonConverter
{
	private const string KeyName = "Key";

	private const string ValueName = "Value";

	private static readonly ThreadSafeStore<Type, ReflectionObject> ReflectionObjectPerType = new ThreadSafeStore<Type, ReflectionObject>(InitializeReflectionObject);

	private static ReflectionObject InitializeReflectionObject(Type t)
	{
		Type[] genericArguments = t.GetGenericArguments();
		Type type = ((IList<Type>)genericArguments)[0];
		Type type2 = ((IList<Type>)genericArguments)[1];
		return ReflectionObject.Create(t, t.GetConstructor(new Type[2] { type, type2 }), "Key", "Value");
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		ReflectionObject reflectionObject = ReflectionObjectPerType.Get(value!.GetType());
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		writer.WriteStartObject();
		writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Key") : "Key");
		serializer.Serialize(writer, reflectionObject.GetValue(value, "Key"), reflectionObject.GetType("Key"));
		writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Value") : "Value");
		serializer.Serialize(writer, reflectionObject.GetValue(value, "Value"), reflectionObject.GetType("Value"));
		writer.WriteEndObject();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullableType(objectType))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
			}
			return null;
		}
		object obj = null;
		object obj2 = null;
		reader.ReadAndAssert();
		Type key = (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType);
		ReflectionObject reflectionObject = ReflectionObjectPerType.Get(key);
		JsonContract jsonContract = serializer.ContractResolver.ResolveContract(reflectionObject.GetType("Key"));
		JsonContract jsonContract2 = serializer.ContractResolver.ResolveContract(reflectionObject.GetType("Value"));
		while (reader.TokenType == JsonToken.PropertyName)
		{
			string a = reader.Value!.ToString();
			if (string.Equals(a, "Key", StringComparison.OrdinalIgnoreCase))
			{
				reader.ReadForTypeAndAssert(jsonContract, hasConverter: false);
				obj = serializer.Deserialize(reader, jsonContract.UnderlyingType);
			}
			else if (string.Equals(a, "Value", StringComparison.OrdinalIgnoreCase))
			{
				reader.ReadForTypeAndAssert(jsonContract2, hasConverter: false);
				obj2 = serializer.Deserialize(reader, jsonContract2.UnderlyingType);
			}
			else
			{
				reader.Skip();
			}
			reader.ReadAndAssert();
		}
		return reflectionObject.Creator!(obj, obj2);
	}

	public override bool CanConvert(Type objectType)
	{
		Type type = (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType);
		if (type.IsValueType() && type.IsGenericType())
		{
			return type.GetGenericTypeDefinition() == typeof(KeyValuePair<, >);
		}
		return false;
	}
}
