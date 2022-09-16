using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class JsonSerializerInternalReader : JsonSerializerInternalBase
{
	internal enum PropertyPresence
	{
		None,
		Null,
		Value
	}

	internal class CreatorPropertyContext
	{
		public readonly string Name;

		public JsonProperty? Property;

		public JsonProperty? ConstructorProperty;

		public PropertyPresence? Presence;

		public object? Value;

		public bool Used;

		public CreatorPropertyContext(string name)
		{
			Name = name;
		}
	}

	public JsonSerializerInternalReader(JsonSerializer serializer)
		: base(serializer)
	{
	}

	public void Populate(JsonReader reader, object target)
	{
		ValidationUtils.ArgumentNotNull(target, "target");
		Type type = target.GetType();
		JsonContract jsonContract = Serializer._contractResolver.ResolveContract(type);
		if (!reader.MoveToContent())
		{
			throw JsonSerializationException.Create(reader, "No JSON content found.");
		}
		if (reader.TokenType == JsonToken.StartArray)
		{
			if (jsonContract.ContractType == JsonContractType.Array)
			{
				JsonArrayContract jsonArrayContract = (JsonArrayContract)jsonContract;
				object list;
				if (!jsonArrayContract.ShouldCreateWrapper)
				{
					list = (IList)target;
				}
				else
				{
					IList list2 = jsonArrayContract.CreateWrapper(target);
					list = list2;
				}
				PopulateList((IList)list, reader, jsonArrayContract, null, null);
				return;
			}
			throw JsonSerializationException.Create(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, type));
		}
		if (reader.TokenType == JsonToken.StartObject)
		{
			reader.ReadAndAssert();
			string id = null;
			if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore && reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value!.ToString(), "$id", StringComparison.Ordinal))
			{
				reader.ReadAndAssert();
				id = reader.Value?.ToString();
				reader.ReadAndAssert();
			}
			if (jsonContract.ContractType == JsonContractType.Dictionary)
			{
				JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)jsonContract;
				object dictionary;
				if (!jsonDictionaryContract.ShouldCreateWrapper)
				{
					dictionary = (IDictionary)target;
				}
				else
				{
					IDictionary dictionary2 = jsonDictionaryContract.CreateWrapper(target);
					dictionary = dictionary2;
				}
				PopulateDictionary((IDictionary)dictionary, reader, jsonDictionaryContract, null, id);
			}
			else
			{
				if (jsonContract.ContractType != JsonContractType.Object)
				{
					throw JsonSerializationException.Create(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, type));
				}
				PopulateObject(target, reader, (JsonObjectContract)jsonContract, null, id);
			}
			return;
		}
		throw JsonSerializationException.Create(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
	}

	private JsonContract? GetContractSafe(Type? type)
	{
		if (type == null)
		{
			return null;
		}
		return GetContract(type);
	}

	private JsonContract GetContract(Type type)
	{
		return Serializer._contractResolver.ResolveContract(type);
	}

	public object? Deserialize(JsonReader reader, Type? objectType, bool checkAdditionalContent)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		JsonContract contractSafe = GetContractSafe(objectType);
		try
		{
			JsonConverter converter = GetConverter(contractSafe, null, null, null);
			if (reader.TokenType == JsonToken.None && !reader.ReadForType(contractSafe, converter != null))
			{
				if (contractSafe != null && !contractSafe.IsNullable)
				{
					throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contractSafe.UnderlyingType));
				}
				return null;
			}
			object result = ((converter == null || !converter.CanRead) ? CreateValueInternal(reader, objectType, contractSafe, null, null, null, null) : DeserializeConvertable(converter, reader, objectType, null));
			if (checkAdditionalContent)
			{
				while (reader.Read())
				{
					if (reader.TokenType != JsonToken.Comment)
					{
						throw JsonSerializationException.Create(reader, "Additional text found in JSON string after finishing deserializing object.");
					}
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			if (IsErrorHandled(null, contractSafe, null, reader as IJsonLineInfo, reader.Path, ex))
			{
				HandleError(reader, readPastError: false, 0);
				return null;
			}
			ClearErrorContext();
			throw;
		}
	}

	private JsonSerializerProxy GetInternalSerializer()
	{
		if (InternalSerializer == null)
		{
			InternalSerializer = new JsonSerializerProxy(this);
		}
		return InternalSerializer;
	}

	private JToken? CreateJToken(JsonReader reader, JsonContract? contract)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (contract != null)
		{
			if (contract!.UnderlyingType == typeof(JRaw))
			{
				return JRaw.Create(reader);
			}
			if (reader.TokenType == JsonToken.Null && !(contract!.UnderlyingType == typeof(JValue)) && !(contract!.UnderlyingType == typeof(JToken)))
			{
				return null;
			}
		}
		JToken token;
		using (JTokenWriter jTokenWriter = new JTokenWriter())
		{
			jTokenWriter.WriteToken(reader);
			token = jTokenWriter.Token;
		}
		if (contract != null && token != null && !contract!.UnderlyingType.IsAssignableFrom(token.GetType()))
		{
			throw JsonSerializationException.Create(reader, "Deserialized JSON type '{0}' is not compatible with expected type '{1}'.".FormatWith(CultureInfo.InvariantCulture, token.GetType().FullName, contract!.UnderlyingType.FullName));
		}
		return token;
	}

	private JToken CreateJObject(JsonReader reader)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		using JTokenWriter jTokenWriter = new JTokenWriter();
		jTokenWriter.WriteStartObject();
		do
		{
			if (reader.TokenType == JsonToken.PropertyName)
			{
				string text = (string)reader.Value;
				if (!reader.ReadAndMoveToContent())
				{
					break;
				}
				if (!CheckPropertyName(reader, text))
				{
					jTokenWriter.WritePropertyName(text);
					jTokenWriter.WriteToken(reader, writeChildren: true, writeDateConstructorAsDate: true, writeComments: false);
				}
			}
			else if (reader.TokenType != JsonToken.Comment)
			{
				jTokenWriter.WriteEndObject();
				return jTokenWriter.Token;
			}
		}
		while (reader.Read());
		throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
	}

	private object? CreateValueInternal(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue)
	{
		if (contract != null && contract!.ContractType == JsonContractType.Linq)
		{
			return CreateJToken(reader, contract);
		}
		do
		{
			switch (reader.TokenType)
			{
			case JsonToken.StartObject:
				return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
			case JsonToken.StartArray:
				return CreateList(reader, objectType, contract, member, existingValue, null);
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.Boolean:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
			case JsonToken.String:
			{
				string text = (string)reader.Value;
				if (objectType == typeof(byte[]))
				{
					return Convert.FromBase64String(text);
				}
				if (CoerceEmptyStringToNull(objectType, contract, text))
				{
					return null;
				}
				return EnsureType(reader, text, CultureInfo.InvariantCulture, contract, objectType);
			}
			case JsonToken.StartConstructor:
			{
				string value = reader.Value!.ToString();
				return EnsureType(reader, value, CultureInfo.InvariantCulture, contract, objectType);
			}
			case JsonToken.Null:
			case JsonToken.Undefined:
				if (objectType == typeof(DBNull))
				{
					return DBNull.Value;
				}
				return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
			case JsonToken.Raw:
				return new JRaw((string)reader.Value);
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (reader.Read());
		throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
	}

	private static bool CoerceEmptyStringToNull(Type? objectType, JsonContract? contract, string s)
	{
		if (StringUtils.IsNullOrEmpty(s) && objectType != null && objectType != typeof(string) && objectType != typeof(object) && contract != null)
		{
			return contract!.IsNullable;
		}
		return false;
	}

	internal string GetExpectedDescription(JsonContract contract)
	{
		switch (contract.ContractType)
		{
		case JsonContractType.Object:
		case JsonContractType.Dictionary:
		case JsonContractType.Dynamic:
		case JsonContractType.Serializable:
			return "JSON object (e.g. {\"name\":\"value\"})";
		case JsonContractType.Array:
			return "JSON array (e.g. [1,2,3])";
		case JsonContractType.Primitive:
			return "JSON primitive value (e.g. string, number, boolean, null)";
		case JsonContractType.String:
			return "JSON string value";
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private JsonConverter? GetConverter(JsonContract? contract, JsonConverter? memberConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		JsonConverter result = null;
		if (memberConverter != null)
		{
			result = memberConverter;
		}
		else if (containerProperty?.ItemConverter != null)
		{
			result = containerProperty!.ItemConverter;
		}
		else if (containerContract?.ItemConverter != null)
		{
			result = containerContract!.ItemConverter;
		}
		else if (contract != null)
		{
			if (contract!.Converter != null)
			{
				result = contract!.Converter;
			}
			else
			{
				JsonConverter matchingConverter = Serializer.GetMatchingConverter(contract!.UnderlyingType);
				if (matchingConverter != null)
				{
					result = matchingConverter;
				}
				else if (contract!.InternalConverter != null)
				{
					result = contract!.InternalConverter;
				}
			}
		}
		return result;
	}

	private object? CreateObject(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue)
	{
		Type objectType2 = objectType;
		string id;
		if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
		{
			reader.ReadAndAssert();
			id = null;
		}
		else if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
		{
			JTokenReader jTokenReader = reader as JTokenReader;
			if (jTokenReader == null)
			{
				jTokenReader = (JTokenReader)JToken.ReadFrom(reader).CreateReader();
				jTokenReader.Culture = reader.Culture;
				jTokenReader.DateFormatString = reader.DateFormatString;
				jTokenReader.DateParseHandling = reader.DateParseHandling;
				jTokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
				jTokenReader.FloatParseHandling = reader.FloatParseHandling;
				jTokenReader.SupportMultipleContent = reader.SupportMultipleContent;
				jTokenReader.ReadAndAssert();
				reader = jTokenReader;
			}
			if (ReadMetadataPropertiesToken(jTokenReader, ref objectType2, ref contract, member, containerContract, containerMember, existingValue, out var newValue, out id))
			{
				return newValue;
			}
		}
		else
		{
			reader.ReadAndAssert();
			if (ReadMetadataProperties(reader, ref objectType2, ref contract, member, containerContract, containerMember, existingValue, out var newValue2, out id))
			{
				return newValue2;
			}
		}
		if (HasNoDefinedType(contract))
		{
			return CreateJObject(reader);
		}
		switch (contract!.ContractType)
		{
		case JsonContractType.Object:
		{
			bool createdFromNonDefaultCreator2 = false;
			JsonObjectContract jsonObjectContract = (JsonObjectContract)contract;
			object obj = ((existingValue == null || (!(objectType2 == objectType) && !objectType2.IsAssignableFrom(existingValue!.GetType()))) ? CreateNewObject(reader, jsonObjectContract, member, containerMember, id, out createdFromNonDefaultCreator2) : existingValue);
			if (createdFromNonDefaultCreator2)
			{
				return obj;
			}
			return PopulateObject(obj, reader, jsonObjectContract, member, id);
		}
		case JsonContractType.Primitive:
		{
			JsonPrimitiveContract contract4 = (JsonPrimitiveContract)contract;
			if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore && reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value!.ToString(), "$value", StringComparison.Ordinal))
			{
				reader.ReadAndAssert();
				if (reader.TokenType == JsonToken.StartObject)
				{
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
				}
				object? result = CreateValueInternal(reader, objectType2, contract4, member, null, null, existingValue);
				reader.ReadAndAssert();
				return result;
			}
			break;
		}
		case JsonContractType.Dictionary:
		{
			JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)contract;
			if (existingValue == null)
			{
				bool createdFromNonDefaultCreator;
				IDictionary dictionary = CreateNewDictionary(reader, jsonDictionaryContract, out createdFromNonDefaultCreator);
				if (createdFromNonDefaultCreator)
				{
					if (id != null)
					{
						throw JsonSerializationException.Create(reader, "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
					}
					if (contract!.OnSerializingCallbacks.Count > 0)
					{
						throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
					}
					if (contract!.OnErrorCallbacks.Count > 0)
					{
						throw JsonSerializationException.Create(reader, "Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
					}
					if (!jsonDictionaryContract.HasParameterizedCreatorInternal)
					{
						throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size dictionary: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
					}
				}
				PopulateDictionary(dictionary, reader, jsonDictionaryContract, member, id);
				if (createdFromNonDefaultCreator)
				{
					return (jsonDictionaryContract.OverrideCreator ?? jsonDictionaryContract.ParameterizedCreator)!(dictionary);
				}
				if (dictionary is IWrappedDictionary wrappedDictionary)
				{
					return wrappedDictionary.UnderlyingDictionary;
				}
				return dictionary;
			}
			object dictionary2;
			if (!jsonDictionaryContract.ShouldCreateWrapper && existingValue is IDictionary)
			{
				dictionary2 = (IDictionary)existingValue;
			}
			else
			{
				IDictionary dictionary3 = jsonDictionaryContract.CreateWrapper(existingValue);
				dictionary2 = dictionary3;
			}
			return PopulateDictionary((IDictionary)dictionary2, reader, jsonDictionaryContract, member, id);
		}
		case JsonContractType.Dynamic:
		{
			JsonDynamicContract contract3 = (JsonDynamicContract)contract;
			return CreateDynamic(reader, contract3, member, id);
		}
		case JsonContractType.Serializable:
		{
			JsonISerializableContract contract2 = (JsonISerializableContract)contract;
			return CreateISerializable(reader, contract2, member, id);
		}
		}
		string format = "Cannot deserialize the current JSON object (e.g. {{\"name\":\"value\"}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine + "To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
		format = format.FormatWith(CultureInfo.InvariantCulture, objectType2, GetExpectedDescription(contract));
		throw JsonSerializationException.Create(reader, format);
	}

	private bool ReadMetadataPropertiesToken(JTokenReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue, out object? newValue, out string? id)
	{
		id = null;
		newValue = null;
		if (reader.TokenType == JsonToken.StartObject)
		{
			JObject jObject = (JObject)reader.CurrentToken;
			JProperty jProperty = jObject.Property("$ref", StringComparison.Ordinal);
			if (jProperty != null)
			{
				JToken value = jProperty.Value;
				if (value.Type != JTokenType.String && value.Type != JTokenType.Null)
				{
					throw JsonSerializationException.Create(value, value.Path, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, "$ref"), null);
				}
				string text = (string?)(JToken?)jProperty;
				if (text != null)
				{
					JToken jToken = jProperty.Next ?? jProperty.Previous;
					if (jToken != null)
					{
						throw JsonSerializationException.Create(jToken, jToken.Path, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, "$ref"), null);
					}
					newValue = Serializer.GetReferenceResolver().ResolveReference(this, text);
					if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
					{
						TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, text, newValue!.GetType())), null);
					}
					reader.Skip();
					return true;
				}
			}
			JToken jToken2 = jObject["$type"];
			if (jToken2 != null)
			{
				string qualifiedTypeName = (string?)jToken2;
				JsonReader jsonReader = jToken2.CreateReader();
				jsonReader.ReadAndAssert();
				ResolveTypeName(jsonReader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);
				if (jObject["$value"] != null)
				{
					while (true)
					{
						reader.ReadAndAssert();
						if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "$value")
						{
							break;
						}
						reader.ReadAndAssert();
						reader.Skip();
					}
					return false;
				}
			}
			JToken jToken3 = jObject["$id"];
			if (jToken3 != null)
			{
				id = (string?)jToken3;
			}
			JToken jToken4 = jObject["$values"];
			if (jToken4 != null)
			{
				JsonReader jsonReader2 = jToken4.CreateReader();
				jsonReader2.ReadAndAssert();
				newValue = CreateList(jsonReader2, objectType, contract, member, existingValue, id);
				reader.Skip();
				return true;
			}
		}
		reader.ReadAndAssert();
		return false;
	}

	private bool ReadMetadataProperties(JsonReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue, out object? newValue, out string? id)
	{
		id = null;
		newValue = null;
		if (reader.TokenType == JsonToken.PropertyName)
		{
			string text = reader.Value!.ToString();
			if (text.Length > 0 && text[0] == '$')
			{
				bool flag;
				do
				{
					text = reader.Value!.ToString();
					if (string.Equals(text, "$ref", StringComparison.Ordinal))
					{
						reader.ReadAndAssert();
						if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
						{
							throw JsonSerializationException.Create(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, "$ref"));
						}
						string text2 = reader.Value?.ToString();
						reader.ReadAndAssert();
						if (text2 != null)
						{
							if (reader.TokenType == JsonToken.PropertyName)
							{
								throw JsonSerializationException.Create(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, "$ref"));
							}
							newValue = Serializer.GetReferenceResolver().ResolveReference(this, text2);
							if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
							{
								TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, text2, newValue!.GetType())), null);
							}
							return true;
						}
						flag = true;
					}
					else if (string.Equals(text, "$type", StringComparison.Ordinal))
					{
						reader.ReadAndAssert();
						string qualifiedTypeName = reader.Value!.ToString();
						ResolveTypeName(reader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);
						reader.ReadAndAssert();
						flag = true;
					}
					else if (string.Equals(text, "$id", StringComparison.Ordinal))
					{
						reader.ReadAndAssert();
						id = reader.Value?.ToString();
						reader.ReadAndAssert();
						flag = true;
					}
					else
					{
						if (string.Equals(text, "$values", StringComparison.Ordinal))
						{
							reader.ReadAndAssert();
							object obj = CreateList(reader, objectType, contract, member, existingValue, id);
							reader.ReadAndAssert();
							newValue = obj;
							return true;
						}
						flag = false;
					}
				}
				while (flag && reader.TokenType == JsonToken.PropertyName);
			}
		}
		return false;
	}

	private void ResolveTypeName(JsonReader reader, ref Type? objectType, ref JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, string qualifiedTypeName)
	{
		if ((member?.TypeNameHandling ?? containerContract?.ItemTypeNameHandling ?? containerMember?.ItemTypeNameHandling ?? Serializer._typeNameHandling) != 0)
		{
			StructMultiKey<string, string> structMultiKey = ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName);
			Type type;
			try
			{
				type = Serializer._serializationBinder.BindToType(structMultiKey.Value1, structMultiKey.Value2);
			}
			catch (Exception ex)
			{
				throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
			}
			if (type == null)
			{
				throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));
			}
			if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
			{
				TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved type '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName, type)), null);
			}
			if (objectType != null && objectType != typeof(IDynamicMetaObjectProvider) && !objectType!.IsAssignableFrom(type))
			{
				throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, type.AssemblyQualifiedName, objectType!.AssemblyQualifiedName));
			}
			objectType = type;
			contract = GetContract(type);
		}
	}

	private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
	{
		if (contract == null)
		{
			throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));
		}
		JsonArrayContract obj = contract as JsonArrayContract;
		if (obj == null)
		{
			string format = "Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine + "To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array." + Environment.NewLine;
			format = format.FormatWith(CultureInfo.InvariantCulture, objectType, GetExpectedDescription(contract));
			throw JsonSerializationException.Create(reader, format);
		}
		return obj;
	}

	private object? CreateList(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, object? existingValue, string? id)
	{
		if (HasNoDefinedType(contract))
		{
			return CreateJToken(reader, contract);
		}
		JsonArrayContract jsonArrayContract = EnsureArrayContract(reader, objectType, contract);
		if (existingValue == null)
		{
			bool createdFromNonDefaultCreator;
			IList list = CreateNewList(reader, jsonArrayContract, out createdFromNonDefaultCreator);
			if (createdFromNonDefaultCreator)
			{
				if (id != null)
				{
					throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
				}
				if (contract!.OnSerializingCallbacks.Count > 0)
				{
					throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
				}
				if (contract!.OnErrorCallbacks.Count > 0)
				{
					throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
				}
				if (!jsonArrayContract.HasParameterizedCreatorInternal && !jsonArrayContract.IsArray)
				{
					throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.UnderlyingType));
				}
			}
			if (!jsonArrayContract.IsMultidimensionalArray)
			{
				PopulateList(list, reader, jsonArrayContract, member, id);
			}
			else
			{
				PopulateMultidimensionalArray(list, reader, jsonArrayContract, member, id);
			}
			if (createdFromNonDefaultCreator)
			{
				if (jsonArrayContract.IsMultidimensionalArray)
				{
					list = CollectionUtils.ToMultidimensionalArray(list, jsonArrayContract.CollectionItemType, contract!.CreatedType.GetArrayRank());
				}
				else
				{
					if (!jsonArrayContract.IsArray)
					{
						return (jsonArrayContract.OverrideCreator ?? jsonArrayContract.ParameterizedCreator)!(list);
					}
					Array array = Array.CreateInstance(jsonArrayContract.CollectionItemType, list.Count);
					list.CopyTo(array, 0);
					list = array;
				}
			}
			else if (list is IWrappedCollection wrappedCollection)
			{
				return wrappedCollection.UnderlyingCollection;
			}
			return list;
		}
		if (!jsonArrayContract.CanDeserialize)
		{
			throw JsonSerializationException.Create(reader, "Cannot populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract!.CreatedType));
		}
		IList list3;
		if (!jsonArrayContract.ShouldCreateWrapper && existingValue is IList list2)
		{
			list3 = list2;
		}
		else
		{
			IList list4 = jsonArrayContract.CreateWrapper(existingValue);
			list3 = list4;
		}
		return PopulateList(list3, reader, jsonArrayContract, member, id);
	}

	private bool HasNoDefinedType(JsonContract? contract)
	{
		if (contract != null && !(contract!.UnderlyingType == typeof(object)) && contract!.ContractType != JsonContractType.Linq)
		{
			return contract!.UnderlyingType == typeof(IDynamicMetaObjectProvider);
		}
		return true;
	}

	private object? EnsureType(JsonReader reader, object? value, CultureInfo culture, JsonContract? contract, Type? targetType)
	{
		if (targetType == null)
		{
			return value;
		}
		if (ReflectionUtils.GetObjectType(value) != targetType)
		{
			if (value == null && contract!.IsNullable)
			{
				return null;
			}
			try
			{
				if (contract!.IsConvertable)
				{
					JsonPrimitiveContract jsonPrimitiveContract = (JsonPrimitiveContract)contract;
					DateTime dt;
					if (contract!.IsEnum)
					{
						if (value is string value2)
						{
							return EnumUtils.ParseEnum(contract!.NonNullableUnderlyingType, null, value2, disallowNumber: false);
						}
						if (ConvertUtils.IsInteger(jsonPrimitiveContract.TypeCode))
						{
							return Enum.ToObject(contract!.NonNullableUnderlyingType, value);
						}
					}
					else if (contract!.NonNullableUnderlyingType == typeof(DateTime) && value is string s && DateTimeUtils.TryParseDateTime(s, reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out dt))
					{
						return DateTimeUtils.EnsureDateTime(dt, reader.DateTimeZoneHandling);
					}
					if (!(value is BigInteger i))
					{
						return Convert.ChangeType(value, contract!.NonNullableUnderlyingType, culture);
					}
					return ConvertUtils.FromBigInteger(i, contract!.NonNullableUnderlyingType);
				}
				return ConvertUtils.ConvertOrCast(value, culture, contract!.NonNullableUnderlyingType);
			}
			catch (Exception ex)
			{
				throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(value), targetType), ex);
			}
		}
		return value;
	}

	private bool SetPropertyValue(JsonProperty property, JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target)
	{
		if (CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out var useExistingValue, out var currentValue, out var propertyContract, out var gottenCurrentValue, out var ignoredValue))
		{
			if (ignoredValue)
			{
				return true;
			}
			return false;
		}
		object obj;
		if (propertyConverter != null && propertyConverter!.CanRead)
		{
			if (!gottenCurrentValue && property.Readable)
			{
				currentValue = property.ValueProvider!.GetValue(target);
			}
			obj = DeserializeConvertable(propertyConverter, reader, property.PropertyType, currentValue);
		}
		else
		{
			obj = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, useExistingValue ? currentValue : null);
		}
		if ((!useExistingValue || obj != currentValue) && ShouldSetPropertyValue(property, containerContract as JsonObjectContract, obj))
		{
			property.ValueProvider!.SetValue(target, obj);
			if (property.SetIsSpecified != null)
			{
				if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
				{
					TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
				}
				property.SetIsSpecified!(target, true);
			}
			return true;
		}
		return useExistingValue;
	}

	private bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target, out bool useExistingValue, out object? currentValue, out JsonContract? propertyContract, out bool gottenCurrentValue, out bool ignoredValue)
	{
		currentValue = null;
		useExistingValue = false;
		propertyContract = null;
		gottenCurrentValue = false;
		ignoredValue = false;
		if (property.Ignored)
		{
			return true;
		}
		JsonToken tokenType = reader.TokenType;
		if (property.PropertyContract == null)
		{
			property.PropertyContract = GetContractSafe(property.PropertyType);
		}
		if (property.ObjectCreationHandling.GetValueOrDefault(Serializer._objectCreationHandling) != ObjectCreationHandling.Replace && (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject || propertyConverter != null) && property.Readable)
		{
			currentValue = property.ValueProvider!.GetValue(target);
			gottenCurrentValue = true;
			if (currentValue != null)
			{
				propertyContract = GetContract(currentValue!.GetType());
				useExistingValue = !propertyContract!.IsReadOnlyOrFixedSize && !propertyContract!.UnderlyingType.IsValueType();
			}
		}
		if (!property.Writable && !useExistingValue)
		{
			if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Unable to deserialize value to non-writable property '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
			}
			return true;
		}
		if (tokenType == JsonToken.Null && ResolvedNullValueHandling(containerContract as JsonObjectContract, property) == NullValueHandling.Ignore)
		{
			ignoredValue = true;
			return true;
		}
		if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && JsonTokenUtils.IsPrimitiveToken(tokenType) && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
		{
			ignoredValue = true;
			return true;
		}
		if (currentValue == null)
		{
			propertyContract = property.PropertyContract;
		}
		else
		{
			propertyContract = GetContract(currentValue!.GetType());
			if (propertyContract != property.PropertyContract)
			{
				propertyConverter = GetConverter(propertyContract, property.Converter, containerContract, containerProperty);
			}
		}
		return false;
	}

	private void AddReference(JsonReader reader, string id, object value)
	{
		try
		{
			if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
			{
				TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Read object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, id, value.GetType())), null);
			}
			Serializer.GetReferenceResolver().AddReference(this, id, value);
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(reader, "Error reading object reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, id), ex);
		}
	}

	private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
	{
		return (value & flag) == flag;
	}

	private bool ShouldSetPropertyValue(JsonProperty property, JsonObjectContract? contract, object? value)
	{
		if (value == null && ResolvedNullValueHandling(contract, property) == NullValueHandling.Ignore)
		{
			return false;
		}
		if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
		{
			return false;
		}
		if (!property.Writable)
		{
			return false;
		}
		return true;
	}

	private IList CreateNewList(JsonReader reader, JsonArrayContract contract, out bool createdFromNonDefaultCreator)
	{
		if (!contract.CanDeserialize)
		{
			throw JsonSerializationException.Create(reader, "Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
		}
		if (contract.OverrideCreator != null)
		{
			if (contract.HasParameterizedCreator)
			{
				createdFromNonDefaultCreator = true;
				return contract.CreateTemporaryCollection();
			}
			object obj = contract.OverrideCreator!();
			if (contract.ShouldCreateWrapper)
			{
				obj = contract.CreateWrapper(obj);
			}
			createdFromNonDefaultCreator = false;
			return (IList)obj;
		}
		if (contract.IsReadOnlyOrFixedSize)
		{
			createdFromNonDefaultCreator = true;
			IList list = contract.CreateTemporaryCollection();
			if (contract.ShouldCreateWrapper)
			{
				list = contract.CreateWrapper(list);
			}
			return list;
		}
		if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
		{
			object obj2 = contract.DefaultCreator!();
			if (contract.ShouldCreateWrapper)
			{
				obj2 = contract.CreateWrapper(obj2);
			}
			createdFromNonDefaultCreator = false;
			return (IList)obj2;
		}
		if (contract.HasParameterizedCreatorInternal)
		{
			createdFromNonDefaultCreator = true;
			return contract.CreateTemporaryCollection();
		}
		if (!contract.IsInstantiable)
		{
			throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}
		throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
	}

	private IDictionary CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool createdFromNonDefaultCreator)
	{
		if (contract.OverrideCreator != null)
		{
			if (contract.HasParameterizedCreator)
			{
				createdFromNonDefaultCreator = true;
				return contract.CreateTemporaryDictionary();
			}
			createdFromNonDefaultCreator = false;
			return (IDictionary)contract.OverrideCreator!();
		}
		if (contract.IsReadOnlyOrFixedSize)
		{
			createdFromNonDefaultCreator = true;
			return contract.CreateTemporaryDictionary();
		}
		if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
		{
			object obj = contract.DefaultCreator!();
			if (contract.ShouldCreateWrapper)
			{
				obj = contract.CreateWrapper(obj);
			}
			createdFromNonDefaultCreator = false;
			return (IDictionary)obj;
		}
		if (contract.HasParameterizedCreatorInternal)
		{
			createdFromNonDefaultCreator = true;
			return contract.CreateTemporaryDictionary();
		}
		if (!contract.IsInstantiable)
		{
			throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}
		throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
	}

	private void OnDeserializing(JsonReader reader, JsonContract contract, object value)
	{
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnDeserializing(value, Serializer._context);
	}

	private void OnDeserialized(JsonReader reader, JsonContract contract, object value)
	{
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnDeserialized(value, Serializer._context);
	}

	private object PopulateDictionary(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty? containerProperty, string? id)
	{
		object obj = ((dictionary is IWrappedDictionary wrappedDictionary) ? wrappedDictionary.UnderlyingDictionary : dictionary);
		if (id != null)
		{
			AddReference(reader, id, obj);
		}
		OnDeserializing(reader, contract, obj);
		int depth = reader.Depth;
		if (contract.KeyContract == null)
		{
			contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);
		}
		if (contract.ItemContract == null)
		{
			contract.ItemContract = GetContractSafe(contract.DictionaryValueType);
		}
		JsonConverter jsonConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);
		PrimitiveTypeCode primitiveTypeCode = ((contract.KeyContract is JsonPrimitiveContract jsonPrimitiveContract) ? jsonPrimitiveContract.TypeCode : PrimitiveTypeCode.Empty);
		bool flag = false;
		do
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				object obj2 = reader.Value;
				if (CheckPropertyName(reader, obj2.ToString()))
				{
					break;
				}
				try
				{
					try
					{
						switch (primitiveTypeCode)
						{
						case PrimitiveTypeCode.DateTime:
						case PrimitiveTypeCode.DateTimeNullable:
						{
							obj2 = (DateTimeUtils.TryParseDateTime(obj2.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out var dt2) ? ((object)dt2) : EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType));
							break;
						}
						case PrimitiveTypeCode.DateTimeOffset:
						case PrimitiveTypeCode.DateTimeOffsetNullable:
						{
							obj2 = (DateTimeUtils.TryParseDateTimeOffset(obj2.ToString(), reader.DateFormatString, reader.Culture, out var dt) ? ((object)dt) : EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType));
							break;
						}
						default:
							obj2 = ((contract.KeyContract != null && contract.KeyContract!.IsEnum) ? EnumUtils.ParseEnum(contract.KeyContract!.NonNullableUnderlyingType, (Serializer._contractResolver as DefaultContractResolver)?.NamingStrategy, obj2.ToString(), disallowNumber: false) : EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType));
							break;
						}
					}
					catch (Exception ex)
					{
						throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
					}
					if (!reader.ReadForType(contract.ItemContract, jsonConverter != null))
					{
						throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
					}
					object obj4 = (dictionary[obj2] = ((jsonConverter == null || !jsonConverter.CanRead) ? CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null) : DeserializeConvertable(jsonConverter, reader, contract.DictionaryValueType, null)));
				}
				catch (Exception ex2)
				{
					if (IsErrorHandled(obj, contract, obj2, reader as IJsonLineInfo, reader.Path, ex2))
					{
						HandleError(reader, readPastError: true, depth);
						break;
					}
					throw;
				}
				break;
			}
			case JsonToken.EndObject:
				flag = true;
				break;
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (!flag && reader.Read());
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, obj, "Unexpected end when deserializing object.");
		}
		OnDeserialized(reader, contract, obj);
		return obj;
	}

	private object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty? containerProperty, string? id)
	{
		int arrayRank = contract.UnderlyingType.GetArrayRank();
		if (id != null)
		{
			AddReference(reader, id, list);
		}
		OnDeserializing(reader, contract, list);
		JsonContract contractSafe = GetContractSafe(contract.CollectionItemType);
		JsonConverter converter = GetConverter(contractSafe, null, contract, containerProperty);
		int? num = null;
		Stack<IList> stack = new Stack<IList>();
		stack.Push(list);
		IList list2 = list;
		bool flag = false;
		do
		{
			int depth = reader.Depth;
			if (stack.Count == arrayRank)
			{
				try
				{
					if (reader.ReadForType(contractSafe, converter != null))
					{
						switch (reader.TokenType)
						{
						case JsonToken.EndArray:
							stack.Pop();
							list2 = stack.Peek();
							num = null;
							break;
						default:
						{
							object value = ((converter == null || !converter.CanRead) ? CreateValueInternal(reader, contract.CollectionItemType, contractSafe, null, contract, containerProperty, null) : DeserializeConvertable(converter, reader, contract.CollectionItemType, null));
							list2.Add(value);
							break;
						}
						case JsonToken.Comment:
							break;
						}
						continue;
					}
				}
				catch (Exception ex)
				{
					JsonPosition position = reader.GetPosition(depth);
					if (IsErrorHandled(list, contract, position.Position, reader as IJsonLineInfo, reader.Path, ex))
					{
						HandleError(reader, readPastError: true, depth + 1);
						if (num.HasValue && num == position.Position)
						{
							throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
						}
						num = position.Position;
						continue;
					}
					throw;
				}
				break;
			}
			if (!reader.Read())
			{
				break;
			}
			switch (reader.TokenType)
			{
			case JsonToken.StartArray:
			{
				IList list3 = new List<object>();
				list2.Add(list3);
				stack.Push(list3);
				list2 = list3;
				break;
			}
			case JsonToken.EndArray:
				stack.Pop();
				if (stack.Count > 0)
				{
					list2 = stack.Peek();
				}
				else
				{
					flag = true;
				}
				break;
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (!flag);
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
		}
		OnDeserialized(reader, contract, list);
		return list;
	}

	private void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object? currentObject, string message)
	{
		try
		{
			throw JsonSerializationException.Create(reader, message);
		}
		catch (Exception ex)
		{
			if (IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
			{
				HandleError(reader, readPastError: false, 0);
				return;
			}
			throw;
		}
	}

	private object PopulateList(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty? containerProperty, string? id)
	{
		object obj = ((list is IWrappedCollection wrappedCollection) ? wrappedCollection.UnderlyingCollection : list);
		if (id != null)
		{
			AddReference(reader, id, obj);
		}
		if (list.IsFixedSize)
		{
			reader.Skip();
			return obj;
		}
		OnDeserializing(reader, contract, obj);
		int depth = reader.Depth;
		if (contract.ItemContract == null)
		{
			contract.ItemContract = GetContractSafe(contract.CollectionItemType);
		}
		JsonConverter converter = GetConverter(contract.ItemContract, null, contract, containerProperty);
		int? num = null;
		bool flag = false;
		do
		{
			try
			{
				if (reader.ReadForType(contract.ItemContract, converter != null))
				{
					switch (reader.TokenType)
					{
					case JsonToken.EndArray:
						flag = true;
						break;
					default:
					{
						object value = ((converter == null || !converter.CanRead) ? CreateValueInternal(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null) : DeserializeConvertable(converter, reader, contract.CollectionItemType, null));
						list.Add(value);
						break;
					}
					case JsonToken.Comment:
						break;
					}
					continue;
				}
			}
			catch (Exception ex)
			{
				JsonPosition position = reader.GetPosition(depth);
				if (IsErrorHandled(obj, contract, position.Position, reader as IJsonLineInfo, reader.Path, ex))
				{
					HandleError(reader, readPastError: true, depth + 1);
					if (num.HasValue && num == position.Position)
					{
						throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
					}
					num = position.Position;
					continue;
				}
				throw;
			}
			break;
		}
		while (!flag);
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, obj, "Unexpected end when deserializing array.");
		}
		OnDeserialized(reader, contract, obj);
		return obj;
	}

	private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, JsonProperty? member, string? id)
	{
		Type underlyingType = contract.UnderlyingType;
		if (!JsonTypeReflector.FullyTrusted)
		{
			string format = "Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine + "To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
			format = format.FormatWith(CultureInfo.InvariantCulture, underlyingType);
			throw JsonSerializationException.Create(reader, format);
		}
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));
		bool flag = false;
		do
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string text = reader.Value!.ToString();
				if (!reader.Read())
				{
					throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
				}
				serializationInfo.AddValue(text, JToken.ReadFrom(reader));
				break;
			}
			case JsonToken.EndObject:
				flag = true;
				break;
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (!flag && reader.Read());
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
		}
		if (!contract.IsInstantiable)
		{
			throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}
		if (contract.ISerializableCreator == null)
		{
			throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, underlyingType));
		}
		object obj = contract.ISerializableCreator!(serializationInfo, Serializer._context);
		if (id != null)
		{
			AddReference(reader, id, obj);
		}
		OnDeserializing(reader, contract, obj);
		OnDeserialized(reader, contract, obj);
		return obj;
	}

	internal object? CreateISerializableItem(JToken token, Type type, JsonISerializableContract contract, JsonProperty? member)
	{
		JsonContract contractSafe = GetContractSafe(type);
		JsonConverter converter = GetConverter(contractSafe, null, contract, member);
		JsonReader jsonReader = token.CreateReader();
		jsonReader.ReadAndAssert();
		if (converter != null && converter.CanRead)
		{
			return DeserializeConvertable(converter, jsonReader, type, null);
		}
		return CreateValueInternal(jsonReader, type, contractSafe, null, contract, member, null);
	}

	private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, JsonProperty? member, string? id)
	{
		if (!contract.IsInstantiable)
		{
			throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}
		if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
		{
			IDynamicMetaObjectProvider dynamicMetaObjectProvider = (IDynamicMetaObjectProvider)contract.DefaultCreator!();
			if (id != null)
			{
				AddReference(reader, id, dynamicMetaObjectProvider);
			}
			OnDeserializing(reader, contract, dynamicMetaObjectProvider);
			int depth = reader.Depth;
			bool flag = false;
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string text = reader.Value!.ToString();
					try
					{
						if (!reader.Read())
						{
							throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
						}
						JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
						if (closestMatchProperty != null && closestMatchProperty.Writable && !closestMatchProperty.Ignored)
						{
							if (closestMatchProperty.PropertyContract == null)
							{
								closestMatchProperty.PropertyContract = GetContractSafe(closestMatchProperty.PropertyType);
							}
							JsonConverter converter = GetConverter(closestMatchProperty.PropertyContract, closestMatchProperty.Converter, null, null);
							if (!SetPropertyValue(closestMatchProperty, converter, null, member, reader, dynamicMetaObjectProvider))
							{
								reader.Skip();
							}
						}
						else
						{
							Type type = (JsonTokenUtils.IsPrimitiveToken(reader.TokenType) ? reader.ValueType : typeof(IDynamicMetaObjectProvider));
							JsonContract contractSafe = GetContractSafe(type);
							JsonConverter converter2 = GetConverter(contractSafe, null, null, member);
							object value = ((converter2 == null || !converter2.CanRead) ? CreateValueInternal(reader, type, contractSafe, null, null, member, null) : DeserializeConvertable(converter2, reader, type, null));
							contract.TrySetMember(dynamicMetaObjectProvider, text, value);
						}
					}
					catch (Exception ex)
					{
						if (IsErrorHandled(dynamicMetaObjectProvider, contract, text, reader as IJsonLineInfo, reader.Path, ex))
						{
							HandleError(reader, readPastError: true, depth);
							break;
						}
						throw;
					}
					break;
				}
				case JsonToken.EndObject:
					flag = true;
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
				}
			}
			while (!flag && reader.Read());
			if (!flag)
			{
				ThrowUnexpectedEndException(reader, contract, dynamicMetaObjectProvider, "Unexpected end when deserializing object.");
			}
			OnDeserialized(reader, contract, dynamicMetaObjectProvider);
			return dynamicMetaObjectProvider;
		}
		throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
	}

	private object CreateObjectUsingCreatorWithParameters(JsonReader reader, JsonObjectContract contract, JsonProperty? containerProperty, ObjectConstructor<object> creator, string? id)
	{
		ValidationUtils.ArgumentNotNull(creator, "creator");
		bool flag = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate);
		Type underlyingType = contract.UnderlyingType;
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			string arg = string.Join(", ", contract.CreatorParameters.Select((JsonProperty p) => p.PropertyName));
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using creator with parameters: {1}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, arg)), null);
		}
		List<CreatorPropertyContext> list = ResolvePropertyAndCreatorValues(contract, containerProperty, reader, underlyingType);
		if (flag)
		{
			foreach (JsonProperty property in contract.Properties)
			{
				if (!property.Ignored && list.All((CreatorPropertyContext p) => p.Property != property))
				{
					list.Add(new CreatorPropertyContext(property.PropertyName)
					{
						Property = property,
						Presence = PropertyPresence.None
					});
				}
			}
		}
		object[] array = new object[contract.CreatorParameters.Count];
		foreach (CreatorPropertyContext item in list)
		{
			if (flag && item.Property != null && !item.Presence.HasValue)
			{
				object value = item.Value;
				PropertyPresence value2 = ((value == null) ? PropertyPresence.Null : ((!(value is string s)) ? PropertyPresence.Value : (CoerceEmptyStringToNull(item.Property!.PropertyType, item.Property!.PropertyContract, s) ? PropertyPresence.Null : PropertyPresence.Value)));
				item.Presence = value2;
			}
			JsonProperty jsonProperty = item.ConstructorProperty;
			if (jsonProperty == null && item.Property != null)
			{
				jsonProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind((JsonProperty p) => p.PropertyName, item.Property!.UnderlyingName);
			}
			if (jsonProperty == null || jsonProperty.Ignored)
			{
				continue;
			}
			if (flag && (item.Presence == PropertyPresence.None || item.Presence == PropertyPresence.Null))
			{
				if (jsonProperty.PropertyContract == null)
				{
					jsonProperty.PropertyContract = GetContractSafe(jsonProperty.PropertyType);
				}
				if (HasFlag(jsonProperty.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate))
				{
					item.Value = EnsureType(reader, jsonProperty.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, jsonProperty.PropertyContract, jsonProperty.PropertyType);
				}
			}
			int num = contract.CreatorParameters.IndexOf(jsonProperty);
			array[num] = item.Value;
			item.Used = true;
		}
		object obj = creator(array);
		if (id != null)
		{
			AddReference(reader, id, obj);
		}
		OnDeserializing(reader, contract, obj);
		foreach (CreatorPropertyContext item2 in list)
		{
			if (item2.Used || item2.Property == null || item2.Property!.Ignored || item2.Presence == PropertyPresence.None)
			{
				continue;
			}
			JsonProperty property2 = item2.Property;
			object value3 = item2.Value;
			if (ShouldSetPropertyValue(property2, contract, value3))
			{
				property2.ValueProvider!.SetValue(obj, value3);
				item2.Used = true;
			}
			else
			{
				if (property2.Writable || value3 == null)
				{
					continue;
				}
				JsonContract jsonContract = Serializer._contractResolver.ResolveContract(property2.PropertyType);
				if (jsonContract.ContractType == JsonContractType.Array)
				{
					JsonArrayContract jsonArrayContract = (JsonArrayContract)jsonContract;
					if (jsonArrayContract.CanDeserialize && !jsonArrayContract.IsReadOnlyOrFixedSize)
					{
						object value4 = property2.ValueProvider!.GetValue(obj);
						if (value4 != null)
						{
							jsonArrayContract = (JsonArrayContract)GetContract(value4.GetType());
							object obj2;
							if (!jsonArrayContract.ShouldCreateWrapper)
							{
								obj2 = (IList)value4;
							}
							else
							{
								IList list2 = jsonArrayContract.CreateWrapper(value4);
								obj2 = list2;
							}
							IList list3 = (IList)obj2;
							if (!list3.IsFixedSize)
							{
								object obj3;
								if (!jsonArrayContract.ShouldCreateWrapper)
								{
									obj3 = (IList)value3;
								}
								else
								{
									IList list2 = jsonArrayContract.CreateWrapper(value3);
									obj3 = list2;
								}
								foreach (object item3 in (IEnumerable)obj3)
								{
									list3.Add(item3);
								}
							}
						}
					}
				}
				else if (jsonContract.ContractType == JsonContractType.Dictionary)
				{
					JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)jsonContract;
					if (!jsonDictionaryContract.IsReadOnlyOrFixedSize)
					{
						object value5 = property2.ValueProvider!.GetValue(obj);
						if (value5 != null)
						{
							object obj4;
							if (!jsonDictionaryContract.ShouldCreateWrapper)
							{
								obj4 = (IDictionary)value5;
							}
							else
							{
								IDictionary dictionary = jsonDictionaryContract.CreateWrapper(value5);
								obj4 = dictionary;
							}
							IDictionary dictionary2 = (IDictionary)obj4;
							object obj5;
							if (!jsonDictionaryContract.ShouldCreateWrapper)
							{
								obj5 = (IDictionary)value3;
							}
							else
							{
								IDictionary dictionary = jsonDictionaryContract.CreateWrapper(value3);
								obj5 = dictionary;
							}
							foreach (DictionaryEntry item4 in (IDictionary)obj5)
							{
								dictionary2[item4.Key] = item4.Value;
							}
						}
					}
				}
				item2.Used = true;
			}
		}
		if (contract.ExtensionDataSetter != null)
		{
			foreach (CreatorPropertyContext item5 in list)
			{
				if (!item5.Used && item5.Presence != PropertyPresence.None)
				{
					contract.ExtensionDataSetter!(obj, item5.Name, item5.Value);
				}
			}
		}
		if (flag)
		{
			foreach (CreatorPropertyContext item6 in list)
			{
				if (item6.Property != null)
				{
					EndProcessProperty(obj, reader, contract, reader.Depth, item6.Property, item6.Presence.GetValueOrDefault(), !item6.Used);
				}
			}
		}
		OnDeserialized(reader, contract, obj);
		return obj;
	}

	private object? DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object? existingValue)
	{
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
		}
		object? result = converter.ReadJson(reader, objectType, existingValue, GetInternalSerializer());
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
		}
		return result;
	}

	private List<CreatorPropertyContext> ResolvePropertyAndCreatorValues(JsonObjectContract contract, JsonProperty? containerProperty, JsonReader reader, Type objectType)
	{
		List<CreatorPropertyContext> list = new List<CreatorPropertyContext>();
		bool flag = false;
		do
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string text = reader.Value!.ToString();
				CreatorPropertyContext creatorPropertyContext = new CreatorPropertyContext(text)
				{
					ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(text),
					Property = contract.Properties.GetClosestMatchProperty(text)
				};
				list.Add(creatorPropertyContext);
				JsonProperty jsonProperty = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
				if (jsonProperty != null)
				{
					if (!jsonProperty.Ignored)
					{
						if (jsonProperty.PropertyContract == null)
						{
							jsonProperty.PropertyContract = GetContractSafe(jsonProperty.PropertyType);
						}
						JsonConverter converter = GetConverter(jsonProperty.PropertyContract, jsonProperty.Converter, contract, containerProperty);
						if (!reader.ReadForType(jsonProperty.PropertyContract, converter != null))
						{
							throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
						}
						if (converter != null && converter.CanRead)
						{
							creatorPropertyContext.Value = DeserializeConvertable(converter, reader, jsonProperty.PropertyType, null);
						}
						else
						{
							creatorPropertyContext.Value = CreateValueInternal(reader, jsonProperty.PropertyType, jsonProperty.PropertyContract, jsonProperty, contract, containerProperty, null);
						}
						break;
					}
				}
				else
				{
					if (!reader.Read())
					{
						throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
					}
					if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
					{
						TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType)), null);
					}
					if ((contract.MissingMemberHandling ?? Serializer._missingMemberHandling) == MissingMemberHandling.Error)
					{
						throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, text, objectType.Name));
					}
				}
				if (contract.ExtensionDataSetter != null)
				{
					creatorPropertyContext.Value = ReadExtensionDataValue(contract, containerProperty, reader);
				}
				else
				{
					reader.Skip();
				}
				break;
			}
			case JsonToken.EndObject:
				flag = true;
				break;
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (!flag && reader.Read());
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, null, "Unexpected end when deserializing object.");
		}
		return list;
	}

	public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty? containerMember, JsonProperty? containerProperty, string? id, out bool createdFromNonDefaultCreator)
	{
		object obj = null;
		if (objectContract.OverrideCreator != null)
		{
			if (objectContract.CreatorParameters.Count > 0)
			{
				createdFromNonDefaultCreator = true;
				return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.OverrideCreator, id);
			}
			obj = objectContract.OverrideCreator!(CollectionUtils.ArrayEmpty<object>());
		}
		else if (objectContract.DefaultCreator != null && (!objectContract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
		{
			obj = objectContract.DefaultCreator!();
		}
		else if (objectContract.ParameterizedCreator != null)
		{
			createdFromNonDefaultCreator = true;
			return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id);
		}
		if (obj == null)
		{
			if (!objectContract.IsInstantiable)
			{
				throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
			}
			throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
		}
		createdFromNonDefaultCreator = false;
		return obj;
	}

	private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty? member, string? id)
	{
		OnDeserializing(reader, contract, newObject);
		Dictionary<JsonProperty, PropertyPresence> dictionary = ((contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate)) ? contract.Properties.ToDictionary((JsonProperty m) => m, (JsonProperty m) => PropertyPresence.None) : null);
		if (id != null)
		{
			AddReference(reader, id, newObject);
		}
		int depth = reader.Depth;
		bool flag = false;
		do
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string text = reader.Value!.ToString();
				if (CheckPropertyName(reader, text))
				{
					break;
				}
				try
				{
					JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
					if (closestMatchProperty == null)
					{
						if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
						{
							TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType)), null);
						}
						if ((contract.MissingMemberHandling ?? Serializer._missingMemberHandling) == MissingMemberHandling.Error)
						{
							throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType.Name));
						}
						if (reader.Read())
						{
							SetExtensionData(contract, member, reader, text, newObject);
						}
						break;
					}
					if (closestMatchProperty.Ignored || !ShouldDeserialize(reader, closestMatchProperty, newObject))
					{
						if (reader.Read())
						{
							SetPropertyPresence(reader, closestMatchProperty, dictionary);
							SetExtensionData(contract, member, reader, text, newObject);
						}
						break;
					}
					if (closestMatchProperty.PropertyContract == null)
					{
						closestMatchProperty.PropertyContract = GetContractSafe(closestMatchProperty.PropertyType);
					}
					JsonConverter converter = GetConverter(closestMatchProperty.PropertyContract, closestMatchProperty.Converter, contract, member);
					if (!reader.ReadForType(closestMatchProperty.PropertyContract, converter != null))
					{
						throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
					}
					SetPropertyPresence(reader, closestMatchProperty, dictionary);
					if (!SetPropertyValue(closestMatchProperty, converter, contract, member, reader, newObject))
					{
						SetExtensionData(contract, member, reader, text, newObject);
					}
				}
				catch (Exception ex)
				{
					if (IsErrorHandled(newObject, contract, text, reader as IJsonLineInfo, reader.Path, ex))
					{
						HandleError(reader, readPastError: true, depth);
						break;
					}
					throw;
				}
				break;
			}
			case JsonToken.EndObject:
				flag = true;
				break;
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
			case JsonToken.Comment:
				break;
			}
		}
		while (!flag && reader.Read());
		if (!flag)
		{
			ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
		}
		if (dictionary != null)
		{
			foreach (KeyValuePair<JsonProperty, PropertyPresence> item in dictionary)
			{
				JsonProperty key = item.Key;
				PropertyPresence value = item.Value;
				EndProcessProperty(newObject, reader, contract, depth, key, value, setDefaultValue: true);
			}
		}
		OnDeserialized(reader, contract, newObject);
		return newObject;
	}

	private bool ShouldDeserialize(JsonReader reader, JsonProperty property, object target)
	{
		if (property.ShouldDeserialize == null)
		{
			return true;
		}
		bool flag = property.ShouldDeserialize!(target);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
		{
			TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, reader.Path, "ShouldDeserialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
		}
		return flag;
	}

	private bool CheckPropertyName(JsonReader reader, string memberName)
	{
		if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
		{
			switch (memberName)
			{
			case "$id":
			case "$ref":
			case "$type":
			case "$values":
				reader.Skip();
				return true;
			}
		}
		return false;
	}

	private void SetExtensionData(JsonObjectContract contract, JsonProperty? member, JsonReader reader, string memberName, object o)
	{
		if (contract.ExtensionDataSetter != null)
		{
			try
			{
				object value = ReadExtensionDataValue(contract, member, reader);
				contract.ExtensionDataSetter!(o, memberName, value);
				return;
			}
			catch (Exception ex)
			{
				throw JsonSerializationException.Create(reader, "Error setting value in extension data for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType), ex);
			}
		}
		reader.Skip();
	}

	private object? ReadExtensionDataValue(JsonObjectContract contract, JsonProperty? member, JsonReader reader)
	{
		if (contract.ExtensionDataIsJToken)
		{
			return JToken.ReadFrom(reader);
		}
		return CreateValueInternal(reader, null, null, null, contract, member, null);
	}

	private void EndProcessProperty(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue)
	{
		if (presence != 0 && presence != PropertyPresence.Null)
		{
			return;
		}
		try
		{
			Required required = ((!property.Ignored) ? (property._required ?? contract.ItemRequired.GetValueOrDefault()) : Required.Default);
			switch (presence)
			{
			case PropertyPresence.None:
				if (required == Required.AllowNull || required == Required.Always)
				{
					throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
				}
				if (setDefaultValue && !property.Ignored)
				{
					if (property.PropertyContract == null)
					{
						property.PropertyContract = GetContractSafe(property.PropertyType);
					}
					if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
					{
						property.ValueProvider!.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
					}
				}
				break;
			case PropertyPresence.Null:
				switch (required)
				{
				case Required.Always:
					throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
				case Required.DisallowNull:
					throw JsonSerializationException.Create(reader, "Required property '{0}' expects a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
				}
				break;
			}
		}
		catch (Exception ex)
		{
			if (IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
			{
				HandleError(reader, readPastError: true, initialDepth);
				return;
			}
			throw;
		}
	}

	private void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence>? requiredProperties)
	{
		if (property != null && requiredProperties != null)
		{
			PropertyPresence value;
			switch (reader.TokenType)
			{
			case JsonToken.String:
				value = (CoerceEmptyStringToNull(property.PropertyType, property.PropertyContract, (string)reader.Value) ? PropertyPresence.Null : PropertyPresence.Value);
				break;
			case JsonToken.Null:
			case JsonToken.Undefined:
				value = PropertyPresence.Null;
				break;
			default:
				value = PropertyPresence.Value;
				break;
			}
			requiredProperties![property] = value;
		}
	}

	private void HandleError(JsonReader reader, bool readPastError, int initialDepth)
	{
		ClearErrorContext();
		if (readPastError)
		{
			reader.Skip();
			while (reader.Depth > initialDepth && reader.Read())
			{
			}
		}
	}
}
