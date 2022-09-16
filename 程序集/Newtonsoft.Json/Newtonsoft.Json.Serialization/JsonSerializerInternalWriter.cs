using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
{
	private Type? _rootType;

	private int _rootLevel;

	private readonly List<object> _serializeStack = new List<object>();

	public JsonSerializerInternalWriter(JsonSerializer serializer)
		: base(serializer)
	{
	}

	public void Serialize(JsonWriter jsonWriter, object? value, Type? objectType)
	{
		if (jsonWriter == null)
		{
			throw new ArgumentNullException("jsonWriter");
		}
		_rootType = objectType;
		_rootLevel = _serializeStack.Count + 1;
		JsonContract contractSafe = GetContractSafe(value);
		try
		{
			if (ShouldWriteReference(value, null, contractSafe, null, null))
			{
				WriteReference(jsonWriter, value);
			}
			else
			{
				SerializeValue(jsonWriter, value, contractSafe, null, null, null);
			}
		}
		catch (Exception ex)
		{
			if (IsErrorHandled(null, contractSafe, null, null, jsonWriter.Path, ex))
			{
				HandleError(jsonWriter, 0);
				return;
			}
			ClearErrorContext();
			throw;
		}
		finally
		{
			_rootType = null;
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

	private JsonContract? GetContractSafe(object? value)
	{
		if (value == null)
		{
			return null;
		}
		return GetContract(value);
	}

	private JsonContract GetContract(object value)
	{
		return Serializer._contractResolver.ResolveContract(value.GetType());
	}

	private void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (contract.TypeCode == PrimitiveTypeCode.Bytes && ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty))
		{
			writer.WriteStartObject();
			WriteTypeProperty(writer, contract.CreatedType);
			writer.WritePropertyName("$value", escape: false);
			JsonWriter.WriteValue(writer, contract.TypeCode, value);
			writer.WriteEndObject();
		}
		else
		{
			JsonWriter.WriteValue(writer, contract.TypeCode, value);
		}
	}

	private void SerializeValue(JsonWriter writer, object? value, JsonContract? valueContract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		JsonConverter jsonConverter = member?.Converter ?? containerProperty?.ItemConverter ?? containerContract?.ItemConverter ?? valueContract!.Converter ?? Serializer.GetMatchingConverter(valueContract!.UnderlyingType) ?? valueContract!.InternalConverter;
		if (jsonConverter != null && jsonConverter.CanWrite)
		{
			SerializeConvertable(writer, jsonConverter, value, valueContract, containerContract, containerProperty);
			return;
		}
		switch (valueContract!.ContractType)
		{
		case JsonContractType.Object:
			SerializeObject(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Array:
		{
			JsonArrayContract jsonArrayContract = (JsonArrayContract)valueContract;
			if (!jsonArrayContract.IsMultidimensionalArray)
			{
				SerializeList(writer, (IEnumerable)value, jsonArrayContract, member, containerContract, containerProperty);
			}
			else
			{
				SerializeMultidimensionalArray(writer, (Array)value, jsonArrayContract, member, containerContract, containerProperty);
			}
			break;
		}
		case JsonContractType.Primitive:
			SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.String:
			SerializeString(writer, value, (JsonStringContract)valueContract);
			break;
		case JsonContractType.Dictionary:
		{
			JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)valueContract;
			IDictionary values;
			if (!(value is IDictionary dictionary))
			{
				IDictionary dictionary2 = jsonDictionaryContract.CreateWrapper(value);
				values = dictionary2;
			}
			else
			{
				values = dictionary;
			}
			SerializeDictionary(writer, values, jsonDictionaryContract, member, containerContract, containerProperty);
			break;
		}
		case JsonContractType.Dynamic:
			SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Serializable:
			SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Linq:
			((JToken)value).WriteTo(writer, Serializer.Converters.ToArray());
			break;
		}
	}

	private bool? ResolveIsReference(JsonContract contract, JsonProperty? property, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		bool? result = null;
		if (property != null)
		{
			result = property!.IsReference;
		}
		if (!result.HasValue && containerProperty != null)
		{
			result = containerProperty!.ItemIsReference;
		}
		if (!result.HasValue && collectionContract != null)
		{
			result = collectionContract!.ItemIsReference;
		}
		if (!result.HasValue)
		{
			result = contract.IsReference;
		}
		return result;
	}

	private bool ShouldWriteReference(object? value, JsonProperty? property, JsonContract? valueContract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			return false;
		}
		if (valueContract!.ContractType == JsonContractType.Primitive || valueContract!.ContractType == JsonContractType.String)
		{
			return false;
		}
		bool? flag = ResolveIsReference(valueContract, property, collectionContract, containerProperty);
		if (!flag.HasValue)
		{
			flag = ((valueContract!.ContractType != JsonContractType.Array) ? new bool?(HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects)) : new bool?(HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays)));
		}
		if (!flag.GetValueOrDefault())
		{
			return false;
		}
		return Serializer.GetReferenceResolver().IsReferenced(this, value);
	}

	private bool ShouldWriteProperty(object? memberValue, JsonObjectContract? containerContract, JsonProperty property)
	{
		if (memberValue == null && ResolvedNullValueHandling(containerContract, property) == NullValueHandling.Ignore)
		{
			return false;
		}
		if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && MiscellaneousUtils.ValueEquals(memberValue, property.GetResolvedDefaultValue()))
		{
			return false;
		}
		return true;
	}

	private bool CheckForCircularReference(JsonWriter writer, object? value, JsonProperty? property, JsonContract? contract, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			return true;
		}
		if (contract!.ContractType == JsonContractType.Primitive || contract!.ContractType == JsonContractType.String)
		{
			return true;
		}
		ReferenceLoopHandling? referenceLoopHandling = null;
		if (property != null)
		{
			referenceLoopHandling = property!.ReferenceLoopHandling;
		}
		if (!referenceLoopHandling.HasValue && containerProperty != null)
		{
			referenceLoopHandling = containerProperty!.ItemReferenceLoopHandling;
		}
		if (!referenceLoopHandling.HasValue && containerContract != null)
		{
			referenceLoopHandling = containerContract!.ItemReferenceLoopHandling;
		}
		if ((Serializer._equalityComparer != null) ? _serializeStack.Contains(value, Serializer._equalityComparer) : _serializeStack.Contains(value))
		{
			string text = "Self referencing loop detected";
			if (property != null)
			{
				text += " for property '{0}'".FormatWith(CultureInfo.InvariantCulture, property!.PropertyName);
			}
			text += " with type '{0}'.".FormatWith(CultureInfo.InvariantCulture, value!.GetType());
			switch (referenceLoopHandling.GetValueOrDefault(Serializer._referenceLoopHandling))
			{
			case ReferenceLoopHandling.Error:
				throw JsonSerializationException.Create(null, writer.ContainerPath, text, null);
			case ReferenceLoopHandling.Ignore:
				if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
				{
					TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, text + ". Skipping serializing self referenced value."), null);
				}
				return false;
			case ReferenceLoopHandling.Serialize:
				if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
				{
					TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, text + ". Serializing self referenced value."), null);
				}
				return true;
			}
		}
		return true;
	}

	private void WriteReference(JsonWriter writer, object value)
	{
		string reference = GetReference(writer, value);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference to Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, value.GetType())), null);
		}
		writer.WriteStartObject();
		writer.WritePropertyName("$ref", escape: false);
		writer.WriteValue(reference);
		writer.WriteEndObject();
	}

	private string GetReference(JsonWriter writer, object value)
	{
		try
		{
			return Serializer.GetReferenceResolver().GetReference(this, value);
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(null, writer.ContainerPath, "Error writing object reference for '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), ex);
		}
	}

	internal static bool TryConvertToString(object value, Type type, [NotNullWhen(true)] out string? s)
	{
		if (JsonTypeReflector.CanTypeDescriptorConvertString(type, out var typeConverter))
		{
			s = typeConverter.ConvertToInvariantString(value);
			return true;
		}
		if (value is Type type2)
		{
			s = type2.AssemblyQualifiedName;
			return true;
		}
		s = null;
		return false;
	}

	private void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
	{
		OnSerializing(writer, contract, value);
		TryConvertToString(value, contract.UnderlyingType, out var s);
		writer.WriteValue(s);
		OnSerialized(writer, contract, value);
	}

	private void OnSerializing(JsonWriter writer, JsonContract contract, object value)
	{
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnSerializing(value, Serializer._context);
	}

	private void OnSerialized(JsonWriter writer, JsonContract contract, object value)
	{
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
		{
			TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnSerialized(value, Serializer._context);
	}

	private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		OnSerializing(writer, contract, value);
		_serializeStack.Add(value);
		WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		int top = writer.Top;
		for (int i = 0; i < contract.Properties.Count; i++)
		{
			JsonProperty jsonProperty = contract.Properties[i];
			try
			{
				if (CalculatePropertyValues(writer, value, contract, member, jsonProperty, out var memberContract, out var memberValue))
				{
					jsonProperty.WritePropertyName(writer);
					SerializeValue(writer, memberValue, memberContract, jsonProperty, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (IsErrorHandled(value, contract, jsonProperty.PropertyName, null, writer.ContainerPath, ex))
				{
					HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		IEnumerable<KeyValuePair<object, object>> enumerable = contract.ExtensionDataGetter?.Invoke(value);
		if (enumerable != null)
		{
			foreach (KeyValuePair<object, object> item in enumerable)
			{
				JsonContract contract2 = GetContract(item.Key);
				JsonContract contractSafe = GetContractSafe(item.Value);
				string propertyName = GetPropertyName(writer, item.Key, contract2, out var _);
				propertyName = ((contract.ExtensionDataNameResolver != null) ? contract.ExtensionDataNameResolver!(propertyName) : propertyName);
				if (ShouldWriteReference(item.Value, null, contractSafe, contract, member))
				{
					writer.WritePropertyName(propertyName);
					WriteReference(writer, item.Value);
				}
				else if (CheckForCircularReference(writer, item.Value, null, contractSafe, contract, member))
				{
					writer.WritePropertyName(propertyName);
					SerializeValue(writer, item.Value, contractSafe, null, contract, member);
				}
			}
		}
		writer.WriteEndObject();
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, value);
	}

	private bool CalculatePropertyValues(JsonWriter writer, object value, JsonContainerContract contract, JsonProperty? member, JsonProperty property, [NotNullWhen(true)] out JsonContract? memberContract, out object? memberValue)
	{
		if (!property.Ignored && property.Readable && ShouldSerialize(writer, property, value) && IsSpecified(writer, property, value))
		{
			if (property.PropertyContract == null)
			{
				property.PropertyContract = Serializer._contractResolver.ResolveContract(property.PropertyType);
			}
			memberValue = property.ValueProvider!.GetValue(value);
			memberContract = (property.PropertyContract!.IsSealed ? property.PropertyContract : GetContractSafe(memberValue));
			if (ShouldWriteProperty(memberValue, contract as JsonObjectContract, property))
			{
				if (ShouldWriteReference(memberValue, property, memberContract, contract, member))
				{
					property.WritePropertyName(writer);
					WriteReference(writer, memberValue);
					return false;
				}
				if (!CheckForCircularReference(writer, memberValue, property, memberContract, contract, member))
				{
					return false;
				}
				if (memberValue == null)
				{
					JsonObjectContract jsonObjectContract = contract as JsonObjectContract;
					switch (property._required ?? (jsonObjectContract?.ItemRequired).GetValueOrDefault())
					{
					case Required.Always:
						throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
					case Required.DisallowNull:
						throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
					}
				}
				return true;
			}
		}
		memberContract = null;
		memberValue = null;
		return false;
	}

	private void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		writer.WriteStartObject();
		if ((ResolveIsReference(contract, member, collectionContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects)) && (member == null || member!.Writable || HasCreatorParameter(collectionContract, member)))
		{
			WriteReferenceIdProperty(writer, contract.UnderlyingType, value);
		}
		if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
		{
			WriteTypeProperty(writer, contract.UnderlyingType);
		}
	}

	private bool HasCreatorParameter(JsonContainerContract? contract, JsonProperty property)
	{
		if (!(contract is JsonObjectContract jsonObjectContract))
		{
			return false;
		}
		return jsonObjectContract.CreatorParameters.Contains(property.PropertyName);
	}

	private void WriteReferenceIdProperty(JsonWriter writer, Type type, object value)
	{
		string reference = GetReference(writer, value);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
		{
			TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, type)), null);
		}
		writer.WritePropertyName("$id", escape: false);
		writer.WriteValue(reference);
	}

	private void WriteTypeProperty(JsonWriter writer, Type type)
	{
		string typeName = ReflectionUtils.GetTypeName(type, Serializer._typeNameAssemblyFormatHandling, Serializer._serializationBinder);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
		{
			TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing type name '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, typeName, type)), null);
		}
		writer.WritePropertyName("$type", escape: false);
		writer.WriteValue(typeName);
	}

	private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
	{
		return (value & flag) == flag;
	}

	private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
	{
		return (value & flag) == flag;
	}

	private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
	{
		return (value & flag) == flag;
	}

	private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
		{
			WriteReference(writer, value);
		}
		else if (CheckForCircularReference(writer, value, null, contract, collectionContract, containerProperty))
		{
			_serializeStack.Add(value);
			if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
			}
			converter.WriteJson(writer, value, GetInternalSerializer());
			if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter!.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
			}
			_serializeStack.RemoveAt(_serializeStack.Count - 1);
		}
	}

	private void SerializeList(JsonWriter writer, IEnumerable values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		object obj = ((values is IWrappedCollection wrappedCollection) ? wrappedCollection.UnderlyingCollection : values);
		OnSerializing(writer, contract, obj);
		_serializeStack.Add(obj);
		bool flag = WriteStartArray(writer, obj, contract, member, collectionContract, containerProperty);
		writer.WriteStartArray();
		int top = writer.Top;
		int num = 0;
		foreach (object value in values)
		{
			try
			{
				JsonContract jsonContract = contract.FinalItemContract ?? GetContractSafe(value);
				if (ShouldWriteReference(value, null, jsonContract, contract, member))
				{
					WriteReference(writer, value);
				}
				else if (CheckForCircularReference(writer, value, null, jsonContract, contract, member))
				{
					SerializeValue(writer, value, jsonContract, null, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (IsErrorHandled(obj, contract, num, null, writer.ContainerPath, ex))
				{
					HandleError(writer, top);
					continue;
				}
				throw;
			}
			finally
			{
				num++;
			}
		}
		writer.WriteEndArray();
		if (flag)
		{
			writer.WriteEndObject();
		}
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, obj);
	}

	private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		OnSerializing(writer, contract, values);
		_serializeStack.Add(values);
		bool num = WriteStartArray(writer, values, contract, member, collectionContract, containerProperty);
		SerializeMultidimensionalArray(writer, values, contract, member, writer.Top, CollectionUtils.ArrayEmpty<int>());
		if (num)
		{
			writer.WriteEndObject();
		}
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, values);
	}

	private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, int initialDepth, int[] indices)
	{
		int num = indices.Length;
		int[] array = new int[num + 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = indices[i];
		}
		writer.WriteStartArray();
		for (int j = values.GetLowerBound(num); j <= values.GetUpperBound(num); j++)
		{
			array[num] = j;
			if (array.Length == values.Rank)
			{
				object value = values.GetValue(array);
				try
				{
					JsonContract jsonContract = contract.FinalItemContract ?? GetContractSafe(value);
					if (ShouldWriteReference(value, null, jsonContract, contract, member))
					{
						WriteReference(writer, value);
					}
					else if (CheckForCircularReference(writer, value, null, jsonContract, contract, member))
					{
						SerializeValue(writer, value, jsonContract, null, contract, member);
					}
				}
				catch (Exception ex)
				{
					if (IsErrorHandled(values, contract, j, null, writer.ContainerPath, ex))
					{
						HandleError(writer, initialDepth + 1);
						continue;
					}
					throw;
				}
			}
			else
			{
				SerializeMultidimensionalArray(writer, values, contract, member, initialDepth + 1, array);
			}
		}
		writer.WriteEndArray();
	}

	private bool WriteStartArray(JsonWriter writer, object values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		bool flag = (ResolveIsReference(contract, member, containerContract, containerProperty) ?? HasFlag(Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays)) && (member == null || member!.Writable || HasCreatorParameter(containerContract, member));
		bool flag2 = ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
		bool num = flag || flag2;
		if (num)
		{
			writer.WriteStartObject();
			if (flag)
			{
				WriteReferenceIdProperty(writer, contract.UnderlyingType, values);
			}
			if (flag2)
			{
				WriteTypeProperty(writer, values.GetType());
			}
			writer.WritePropertyName("$values", escape: false);
		}
		if (contract.ItemContract == null)
		{
			contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));
		}
		return num;
	}

	[SecuritySafeCritical]
	private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (!JsonTypeReflector.FullyTrusted)
		{
			string format = "Type '{0}' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine + "To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
			format = format.FormatWith(CultureInfo.InvariantCulture, value.GetType());
			throw JsonSerializationException.Create(null, writer.ContainerPath, format, null);
		}
		OnSerializing(writer, contract, value);
		_serializeStack.Add(value);
		WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
		value.GetObjectData(serializationInfo, Serializer._context);
		SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			JsonContract contractSafe = GetContractSafe(current.Value);
			if (ShouldWriteReference(current.Value, null, contractSafe, contract, member))
			{
				writer.WritePropertyName(current.Name);
				WriteReference(writer, current.Value);
			}
			else if (CheckForCircularReference(writer, current.Value, null, contractSafe, contract, member))
			{
				writer.WritePropertyName(current.Name);
				SerializeValue(writer, current.Value, contractSafe, null, contract, member);
			}
		}
		writer.WriteEndObject();
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, value);
	}

	private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		OnSerializing(writer, contract, value);
		_serializeStack.Add(value);
		WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		int top = writer.Top;
		for (int i = 0; i < contract.Properties.Count; i++)
		{
			JsonProperty jsonProperty = contract.Properties[i];
			if (!jsonProperty.HasMemberAttribute)
			{
				continue;
			}
			try
			{
				if (CalculatePropertyValues(writer, value, contract, member, jsonProperty, out var memberContract, out var memberValue))
				{
					jsonProperty.WritePropertyName(writer);
					SerializeValue(writer, memberValue, memberContract, jsonProperty, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (IsErrorHandled(value, contract, jsonProperty.PropertyName, null, writer.ContainerPath, ex))
				{
					HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		foreach (string dynamicMemberName in value.GetDynamicMemberNames())
		{
			if (!contract.TryGetMember(value, dynamicMemberName, out var value2))
			{
				continue;
			}
			try
			{
				JsonContract contractSafe = GetContractSafe(value2);
				if (ShouldWriteDynamicProperty(value2) && CheckForCircularReference(writer, value2, null, contractSafe, contract, member))
				{
					string name = ((contract.PropertyNameResolver != null) ? contract.PropertyNameResolver!(dynamicMemberName) : dynamicMemberName);
					writer.WritePropertyName(name);
					SerializeValue(writer, value2, contractSafe, null, contract, member);
				}
			}
			catch (Exception ex2)
			{
				if (IsErrorHandled(value, contract, dynamicMemberName, null, writer.ContainerPath, ex2))
				{
					HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		writer.WriteEndObject();
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, value);
	}

	private bool ShouldWriteDynamicProperty(object? memberValue)
	{
		if (Serializer._nullValueHandling == NullValueHandling.Ignore && memberValue == null)
		{
			return false;
		}
		if (HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Ignore) && (memberValue == null || MiscellaneousUtils.ValueEquals(memberValue, ReflectionUtils.GetDefaultValue(memberValue!.GetType()))))
		{
			return false;
		}
		return true;
	}

	private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		TypeNameHandling value = member?.TypeNameHandling ?? containerProperty?.ItemTypeNameHandling ?? containerContract?.ItemTypeNameHandling ?? Serializer._typeNameHandling;
		if (HasFlag(value, typeNameHandlingFlag))
		{
			return true;
		}
		if (HasFlag(value, TypeNameHandling.Auto))
		{
			if (member != null)
			{
				if (contract.NonNullableUnderlyingType != member!.PropertyContract!.CreatedType)
				{
					return true;
				}
			}
			else if (containerContract != null)
			{
				if (containerContract!.ItemContract == null || contract.NonNullableUnderlyingType != containerContract!.ItemContract!.CreatedType)
				{
					return true;
				}
			}
			else if (_rootType != null && _serializeStack.Count == _rootLevel)
			{
				JsonContract jsonContract = Serializer._contractResolver.ResolveContract(_rootType);
				if (contract.NonNullableUnderlyingType != jsonContract.CreatedType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		object obj = ((values is IWrappedDictionary wrappedDictionary) ? wrappedDictionary.UnderlyingDictionary : values);
		OnSerializing(writer, contract, obj);
		_serializeStack.Add(obj);
		WriteObjectStart(writer, obj, contract, member, collectionContract, containerProperty);
		if (contract.ItemContract == null)
		{
			contract.ItemContract = Serializer._contractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));
		}
		if (contract.KeyContract == null)
		{
			contract.KeyContract = Serializer._contractResolver.ResolveContract(contract.DictionaryKeyType ?? typeof(object));
		}
		int top = writer.Top;
		foreach (DictionaryEntry value2 in values)
		{
			string propertyName = GetPropertyName(writer, value2.Key, contract.KeyContract, out var escape);
			propertyName = ((contract.DictionaryKeyResolver != null) ? contract.DictionaryKeyResolver!(propertyName) : propertyName);
			try
			{
				object value = value2.Value;
				JsonContract jsonContract = contract.FinalItemContract ?? GetContractSafe(value);
				if (ShouldWriteReference(value, null, jsonContract, contract, member))
				{
					writer.WritePropertyName(propertyName, escape);
					WriteReference(writer, value);
				}
				else if (CheckForCircularReference(writer, value, null, jsonContract, contract, member))
				{
					writer.WritePropertyName(propertyName, escape);
					SerializeValue(writer, value, jsonContract, null, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (IsErrorHandled(obj, contract, propertyName, null, writer.ContainerPath, ex))
				{
					HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		writer.WriteEndObject();
		_serializeStack.RemoveAt(_serializeStack.Count - 1);
		OnSerialized(writer, contract, obj);
	}

	private string GetPropertyName(JsonWriter writer, object name, JsonContract contract, out bool escape)
	{
		if (contract.ContractType == JsonContractType.Primitive)
		{
			JsonPrimitiveContract jsonPrimitiveContract = (JsonPrimitiveContract)contract;
			switch (jsonPrimitiveContract.TypeCode)
			{
			case PrimitiveTypeCode.DateTime:
			case PrimitiveTypeCode.DateTimeNullable:
			{
				DateTime value = DateTimeUtils.EnsureDateTime((DateTime)name, writer.DateTimeZoneHandling);
				escape = false;
				StringWriter stringWriter2 = new StringWriter(CultureInfo.InvariantCulture);
				DateTimeUtils.WriteDateTimeString(stringWriter2, value, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
				return stringWriter2.ToString();
			}
			case PrimitiveTypeCode.DateTimeOffset:
			case PrimitiveTypeCode.DateTimeOffsetNullable:
			{
				escape = false;
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				DateTimeUtils.WriteDateTimeOffsetString(stringWriter, (DateTimeOffset)name, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
				return stringWriter.ToString();
			}
			case PrimitiveTypeCode.Double:
			case PrimitiveTypeCode.DoubleNullable:
			{
				double num = (double)name;
				escape = false;
				return num.ToString("R", CultureInfo.InvariantCulture);
			}
			case PrimitiveTypeCode.Single:
			case PrimitiveTypeCode.SingleNullable:
			{
				float num2 = (float)name;
				escape = false;
				return num2.ToString("R", CultureInfo.InvariantCulture);
			}
			default:
			{
				escape = true;
				if (jsonPrimitiveContract.IsEnum && EnumUtils.TryToString(jsonPrimitiveContract.NonNullableUnderlyingType, name, null, out var name2))
				{
					return name2;
				}
				return Convert.ToString(name, CultureInfo.InvariantCulture);
			}
			}
		}
		if (TryConvertToString(name, name.GetType(), out var s))
		{
			escape = true;
			return s;
		}
		escape = true;
		return name.ToString();
	}

	private void HandleError(JsonWriter writer, int initialDepth)
	{
		ClearErrorContext();
		if (writer.WriteState == WriteState.Property)
		{
			writer.WriteNull();
		}
		while (writer.Top > initialDepth)
		{
			writer.WriteEnd();
		}
	}

	private bool ShouldSerialize(JsonWriter writer, JsonProperty property, object target)
	{
		if (property.ShouldSerialize == null)
		{
			return true;
		}
		bool flag = property.ShouldSerialize!(target);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
		{
			TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "ShouldSerialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
		}
		return flag;
	}

	private bool IsSpecified(JsonWriter writer, JsonProperty property, object target)
	{
		if (property.GetIsSpecified == null)
		{
			return true;
		}
		bool flag = property.GetIsSpecified!(target);
		if (TraceWriter != null && TraceWriter!.LevelFilter >= TraceLevel.Verbose)
		{
			TraceWriter!.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "IsSpecified result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
		}
		return flag;
	}
}
