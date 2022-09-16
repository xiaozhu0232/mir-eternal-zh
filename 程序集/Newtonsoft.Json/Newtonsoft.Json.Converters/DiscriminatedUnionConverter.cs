using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class DiscriminatedUnionConverter : JsonConverter
{
	internal class Union
	{
		public readonly FSharpFunction TagReader;

		public readonly List<UnionCase> Cases;

		public Union(FSharpFunction tagReader, List<UnionCase> cases)
		{
			TagReader = tagReader;
			Cases = cases;
		}
	}

	internal class UnionCase
	{
		public readonly int Tag;

		public readonly string Name;

		public readonly PropertyInfo[] Fields;

		public readonly FSharpFunction FieldReader;

		public readonly FSharpFunction Constructor;

		public UnionCase(int tag, string name, PropertyInfo[] fields, FSharpFunction fieldReader, FSharpFunction constructor)
		{
			Tag = tag;
			Name = name;
			Fields = fields;
			FieldReader = fieldReader;
			Constructor = constructor;
		}
	}

	private const string CasePropertyName = "Case";

	private const string FieldsPropertyName = "Fields";

	private static readonly ThreadSafeStore<Type, Union> UnionCache = new ThreadSafeStore<Type, Union>(CreateUnion);

	private static readonly ThreadSafeStore<Type, Type> UnionTypeLookupCache = new ThreadSafeStore<Type, Type>(CreateUnionTypeLookup);

	private static Type CreateUnionTypeLookup(Type t)
	{
		object arg = ((object[])FSharpUtils.Instance.GetUnionCases(null, t, null)).First();
		return (Type)FSharpUtils.Instance.GetUnionCaseInfoDeclaringType(arg);
	}

	private static Union CreateUnion(Type t)
	{
		Union union = new Union((FSharpFunction)FSharpUtils.Instance.PreComputeUnionTagReader(null, t, null), new List<UnionCase>());
		object[] array = (object[])FSharpUtils.Instance.GetUnionCases(null, t, null);
		foreach (object obj in array)
		{
			UnionCase item = new UnionCase((int)FSharpUtils.Instance.GetUnionCaseInfoTag(obj), (string)FSharpUtils.Instance.GetUnionCaseInfoName(obj), (PropertyInfo[])FSharpUtils.Instance.GetUnionCaseInfoFields(obj), (FSharpFunction)FSharpUtils.Instance.PreComputeUnionReader(null, obj, null), (FSharpFunction)FSharpUtils.Instance.PreComputeUnionConstructor(null, obj, null));
			union.Cases.Add(item);
		}
		return union;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		Type key = UnionTypeLookupCache.Get(value!.GetType());
		Union union = UnionCache.Get(key);
		int tag = (int)union.TagReader.Invoke(value);
		UnionCase unionCase = union.Cases.Single((UnionCase c) => c.Tag == tag);
		writer.WriteStartObject();
		writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Case") : "Case");
		writer.WriteValue(unionCase.Name);
		if (unionCase.Fields != null && unionCase.Fields.Length != 0)
		{
			object[] obj = (object[])unionCase.FieldReader.Invoke(value);
			writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Fields") : "Fields");
			writer.WriteStartArray();
			object[] array = obj;
			foreach (object value2 in array)
			{
				serializer.Serialize(writer, value2);
			}
			writer.WriteEndArray();
		}
		writer.WriteEndObject();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		UnionCase unionCase = null;
		string caseName = null;
		JArray jArray = null;
		reader.ReadAndAssert();
		while (reader.TokenType == JsonToken.PropertyName)
		{
			string text = reader.Value!.ToString();
			if (string.Equals(text, "Case", StringComparison.OrdinalIgnoreCase))
			{
				reader.ReadAndAssert();
				Union union = UnionCache.Get(objectType);
				caseName = reader.Value!.ToString();
				unionCase = union.Cases.SingleOrDefault((UnionCase c) => c.Name == caseName);
				if (unionCase == null)
				{
					throw JsonSerializationException.Create(reader, "No union type found with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
				}
			}
			else
			{
				if (!string.Equals(text, "Fields", StringComparison.OrdinalIgnoreCase))
				{
					throw JsonSerializationException.Create(reader, "Unexpected property '{0}' found when reading union.".FormatWith(CultureInfo.InvariantCulture, text));
				}
				reader.ReadAndAssert();
				if (reader.TokenType != JsonToken.StartArray)
				{
					throw JsonSerializationException.Create(reader, "Union fields must been an array.");
				}
				jArray = (JArray)JToken.ReadFrom(reader);
			}
			reader.ReadAndAssert();
		}
		if (unionCase == null)
		{
			throw JsonSerializationException.Create(reader, "No '{0}' property with union name found.".FormatWith(CultureInfo.InvariantCulture, "Case"));
		}
		object[] array = new object[unionCase.Fields.Length];
		if (unionCase.Fields.Length != 0 && jArray == null)
		{
			throw JsonSerializationException.Create(reader, "No '{0}' property with union fields found.".FormatWith(CultureInfo.InvariantCulture, "Fields"));
		}
		if (jArray != null)
		{
			if (unionCase.Fields.Length != jArray.Count)
			{
				throw JsonSerializationException.Create(reader, "The number of field values does not match the number of properties defined by union '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
			}
			for (int i = 0; i < jArray.Count; i++)
			{
				JToken jToken = jArray[i];
				PropertyInfo propertyInfo = unionCase.Fields[i];
				array[i] = jToken.ToObject(propertyInfo.PropertyType, serializer);
			}
		}
		object[] args = new object[1] { array };
		return unionCase.Constructor.Invoke(args);
	}

	public override bool CanConvert(Type objectType)
	{
		if (typeof(IEnumerable).IsAssignableFrom(objectType))
		{
			return false;
		}
		object[] customAttributes = objectType.GetCustomAttributes(inherit: true);
		bool flag = false;
		object[] array = customAttributes;
		for (int i = 0; i < array.Length; i++)
		{
			Type type = array[i].GetType();
			if (type.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
			{
				FSharpUtils.EnsureInitialized(type.Assembly());
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return (bool)FSharpUtils.Instance.IsUnion(null, objectType, null);
	}
}
