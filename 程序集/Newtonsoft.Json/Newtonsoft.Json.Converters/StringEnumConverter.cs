using System;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class StringEnumConverter : JsonConverter
{
	[Obsolete("StringEnumConverter.CamelCaseText is obsolete. Set StringEnumConverter.NamingStrategy with CamelCaseNamingStrategy instead.")]
	public bool CamelCaseText
	{
		get
		{
			if (!(NamingStrategy is CamelCaseNamingStrategy))
			{
				return false;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (!(NamingStrategy is CamelCaseNamingStrategy))
				{
					NamingStrategy = new CamelCaseNamingStrategy();
				}
			}
			else if (NamingStrategy is CamelCaseNamingStrategy)
			{
				NamingStrategy = null;
			}
		}
	}

	public NamingStrategy? NamingStrategy { get; set; }

	public bool AllowIntegerValues { get; set; } = true;


	public StringEnumConverter()
	{
	}

	[Obsolete("StringEnumConverter(bool) is obsolete. Create a converter with StringEnumConverter(NamingStrategy, bool) instead.")]
	public StringEnumConverter(bool camelCaseText)
	{
		if (camelCaseText)
		{
			NamingStrategy = new CamelCaseNamingStrategy();
		}
	}

	public StringEnumConverter(NamingStrategy namingStrategy, bool allowIntegerValues = true)
	{
		NamingStrategy = namingStrategy;
		AllowIntegerValues = allowIntegerValues;
	}

	public StringEnumConverter(Type namingStrategyType)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, null);
	}

	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
	}

	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters, bool allowIntegerValues)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
		AllowIntegerValues = allowIntegerValues;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Enum @enum = (Enum)value;
		if (!EnumUtils.TryToString(@enum.GetType(), value, NamingStrategy, out var name))
		{
			if (!AllowIntegerValues)
			{
				throw JsonSerializationException.Create(null, writer.ContainerPath, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, @enum.ToString("D")), null);
			}
			writer.WriteValue(value);
		}
		else
		{
			writer.WriteValue(name);
		}
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullableType(objectType))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		bool flag = ReflectionUtils.IsNullableType(objectType);
		Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
		try
		{
			if (reader.TokenType == JsonToken.String)
			{
				string value = reader.Value?.ToString();
				if (StringUtils.IsNullOrEmpty(value) && flag)
				{
					return null;
				}
				return EnumUtils.ParseEnum(type, NamingStrategy, value, !AllowIntegerValues);
			}
			if (reader.TokenType == JsonToken.Integer)
			{
				if (!AllowIntegerValues)
				{
					throw JsonSerializationException.Create(reader, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
				}
				return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, type);
			}
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(reader.Value), objectType), ex);
		}
		throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
	}

	public override bool CanConvert(Type objectType)
	{
		return (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType).IsEnum();
	}
}
